using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Recaps;
using Repository;
using Services.Interface;
using Services.Service.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Apis.Books.v1.Data;
using Microsoft.EntityFrameworkCore;
using BusinessObject.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BusinessObject.ViewModels.Contents;
using Services.Responses;
using Hangfire;

namespace Services.Service
{
    public class RecapVersionService : IRecapVersionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GoogleCloudService _googleCloudService;
        private readonly TextToSpeechService _textToSpeechService;
        private readonly HttpClient _httpClient;
        private readonly SpeechToTextService _speechToTextService;
        private readonly ApiSettings _transcriptApiSettings;
        private readonly IConfiguration _configuration;

        public RecapVersionService(
         IUnitOfWork unitOfWork,
         IMapper mapper,
         GoogleCloudService googleCloudService,
         IConfiguration configuration, // Đảm bảo IConfiguration được truyền vào
         IOptions<ApiSettings> transcriptApiSettingsOptions,
         HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
            _configuration = configuration; // Lưu lại IConfiguration
            _googleCloudService = googleCloudService;
            _transcriptApiSettings = transcriptApiSettingsOptions.Value;

            // Khởi tạo TextToSpeechService với GoogleCloudService và IConfiguration
            _textToSpeechService = new TextToSpeechService(_googleCloudService, _configuration); // Truyền thêm IConfiguration vào
            _speechToTextService = new SpeechToTextService(_googleCloudService); // Giữ nguyên nếu chỉ cần GoogleCloudService
        }

        public async Task<ApiResponse<RecapVersion>> CreatePrepareRecapVersion(CreatePrepareVersion request)
        {
            var recapVersion = _mapper.Map<RecapVersion>(request);
            await _unitOfWork.RecapVersionRepository.AddAsync(recapVersion);
            await _unitOfWork.SaveChangesAsync();
            return new ApiResponse<RecapVersion>
            {
                Succeeded = true,
                Message = "Tạo thành công.",
                Errors = new[] { "No data available." },
                Data = recapVersion
            };
        }
        public async Task<ApiResponse<RecapVersion>> CreateRecapVersionAsync(CreateRecapVersion request)
        {
            // Lấy Recap với Id được cung cấp
            var recap = await _unitOfWork.RecapRepository
                .QueryWithIncludes(r => r.RecapVersions)
                .FirstOrDefaultAsync(r => r.Id == request.RecapId);

            if (recap == null)
            {
                return new ApiResponse<RecapVersion>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                    Errors = new[] { "Recap not found." }
                };
            }
            // Kiểm tra xem contributor có phải là người đã tạo recap này không
            if (recap.UserId != request.ContributorId)
            {
                return new ApiResponse<RecapVersion>
                {
                    Succeeded = false,
                    Message = "Bạn không có quyền tạo phiên bản cho recap này."
                };
            }
                var latestVersion = recap.RecapVersions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
                var newVersionNumber = (latestVersion?.VersionNumber ?? 0) + 1;

                // Sử dụng AutoMapper để tạo đối tượng RecapVersion mới từ request
                var newRecapVersion = _mapper.Map<RecapVersion>(request);
                newRecapVersion.VersionNumber = newRecapVersion.VersionNumber ?? newVersionNumber;
                newRecapVersion.RecapId = recap.Id;
                newRecapVersion.Status = RecapStatus.Draft; // Đặt trạng thái mặc định là Draft

                // Thêm RecapVersion mới vào cơ sở dữ liệu
                await _unitOfWork.RecapVersionRepository.AddAsync(newRecapVersion);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<RecapVersion>
                {
                    Succeeded = true,
                    Message = "Tạo mới phiên bản thành công.",
                    Data = newRecapVersion
                };        
            
        }


        public async Task<ApiResponse<TranscriptStatus>> GenerateRecapVersionAsync(UpdateRecapVersion request)
        {
            var errors = new List<string>();

            if (errors.Any())
            {
                return new ApiResponse<TranscriptStatus>
                {
                    Succeeded = false,
                    Message = "Validation failed.",
                    Errors = errors.ToArray()
                };
            }

            var existingDraftRecapVersion = await _unitOfWork.RecapVersionRepository
                .QueryWithIncludes(cv => cv.KeyIdeas)
                .FirstOrDefaultAsync(cv => cv.Id == request.RecapVersionId && cv.Status == RecapStatus.Draft);

            if (existingDraftRecapVersion == null)
            {
                return new ApiResponse<TranscriptStatus>
                {
                    Succeeded = false,
                    Message = "Không tìm thấy bản nháp.",
                    Errors = new[] { "Draft RecapVersion with the given ID does not exist." }
                };
            }

            // **Cập nhật trạng thái sang AudioProcessing**
            existingDraftRecapVersion.TranscriptStatus = TranscriptStatus.AudioProcessing;
            _unitOfWork.RecapVersionRepository.Update(existingDraftRecapVersion);
            await _unitOfWork.SaveChangesAsync();

            // **Đẩy job xuống background để tạo file audio**
            BackgroundJob.Enqueue(() => ProcessAudioAsync(existingDraftRecapVersion.Id));

            return new ApiResponse<TranscriptStatus>
            {
                Succeeded = true,
                Message = "Đã bắt đầu xử lý audio trong chế độ nền.",
                Data = TranscriptStatus.AudioProcessing
            };
        }

        // **Hàm xử lý trong background**
        public async Task ProcessAudioAsync(Guid recapVersionId)
        {
            var recapVersion = await _unitOfWork.RecapVersionRepository
                .QueryWithIncludes(cv => cv.KeyIdeas)
                .FirstOrDefaultAsync(cv => cv.Id == recapVersionId);

            if (recapVersion == null) return;

            var keyIdeas = await _unitOfWork.KeyIdeaRepository
                .GetAllAsync(ki => ki.RecapVersionId == recapVersion.Id);
            var fullText = string.Join(" ", keyIdeas.OrderBy(ki => ki.Order).Select(ki => ki.Body));
            var audioStream = await _textToSpeechService.GenerateAudioAsync(fullText, 0.9);
            string folderName = $"recap_audio/";
            string newAudioFileName = $"{folderName}audio_{Guid.NewGuid()}.wav";

            string publicAudioUrl = await _googleCloudService.UploadFileAsync(newAudioFileName, audioStream);
            recapVersion.AudioURL = publicAudioUrl;
            recapVersion.isGenAudio = true;

            // **Cập nhật trạng thái sang AudioDone**

            recapVersion.TranscriptStatus = TranscriptStatus.AudioDone;
            _unitOfWork.RecapVersionRepository.Update(recapVersion);
            await _unitOfWork.SaveChangesAsync();
            string recapVersionstringId = recapVersionId.ToString();
            string transcriptApiUrl = $"{_transcriptApiSettings.BaseUrl}/mfa/{recapVersionstringId}";

            var transcriptApiResponse = await CallTranscriptApiAsync(transcriptApiUrl);
        }


        public async Task<ApiResponse<TranscriptStatus>> UploadAudioByContributorAsync(Guid recapVersionId, IFormFile audioFile)
        {
            // Kiểm tra validation cho file audio
            if (audioFile == null || audioFile.Length == 0)
            {
                return new ApiResponse<TranscriptStatus>
                {
                    Succeeded = false,
                    Message = "Audio file cannot be null or empty.",
                    Errors = new[] { "Audio file is required." }
                };
            }

            // Kiểm tra xem RecapVersion có tồn tại và đang ở trạng thái Draft hay không
            var existingDraftRecapVersion = await _unitOfWork.RecapVersionRepository
                .QueryWithIncludes(cv => cv.KeyIdeas)
                .FirstOrDefaultAsync(cv => cv.Id == recapVersionId && cv.Status == RecapStatus.Draft);

            if (existingDraftRecapVersion == null)
            {
                return new ApiResponse<TranscriptStatus>
                {
                    Succeeded = false,
                    Message = "No draft version found for the provided RecapVersionId.",
                    Errors = new[] { "Draft RecapVersion with the given ID does not exist." }
                };
            }

            // Lấy URL audio cũ nếu có
            //string oldAudioUrl = existingDraftRecapVersion.AudioURL;

            //// Xử lý xóa tệp âm thanh cũ nếu tồn tại
            //if (!string.IsNullOrEmpty(oldAudioUrl))
            //{
            //    Uri oldUri = new Uri(oldAudioUrl);
            //    string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
            //    await _googleCloudService.DeleteFileAsync(oldObjectName);
            //}

            // Tạo tên tệp mới cho file audio được upload
            string folderName = $"recap_audio/"; // Bạn có thể thay đổi folder tùy ý
            string newAudioFileName = $"{folderName}audio_{Guid.NewGuid()}.wav";

            // Upload tệp audio mới lên cloud
            string publicAudioUrl = await _googleCloudService.UploadFileAsync(newAudioFileName, audioFile.OpenReadStream());

            // **Gán URL của tệp âm thanh mới vào đối tượng RecapVersion**
            existingDraftRecapVersion.AudioURL = publicAudioUrl; // Gán public URL cho thuộc tính AudioURL
            existingDraftRecapVersion.isGenAudio = false;
            // Tạo fullText từ KeyIdeas để lưu giữ
            var keyIdeas = await _unitOfWork.KeyIdeaRepository
                .GetAllAsync(ki => ki.RecapVersionId == existingDraftRecapVersion.Id);
            var fullText = string.Join(" ", keyIdeas.OrderBy(ki => ki.Order).Select(ki => ki.Body));

            // Cập nhật các thay đổi vào cơ sở dữ liệu
            _unitOfWork.RecapVersionRepository.Update(existingDraftRecapVersion);
            await _unitOfWork.SaveChangesAsync();

            // Gọi Transcript API để tạo bản dịch cho file audio
            string transcriptApiUrl = $"{_transcriptApiSettings.BaseUrl}/mfa/{recapVersionId}";
            var transcriptApiResponse = await CallTranscriptApiAsync(transcriptApiUrl);
            var transcriptStatus = existingDraftRecapVersion.TranscriptStatus;
            return new ApiResponse<TranscriptStatus>
            {
                Succeeded = true,
                Message = "Cập nhật phiên bản thành công",
                Data = (TranscriptStatus)transcriptStatus
            };
        }



        public async Task<TranscriptResponse> CallTranscriptApiAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return new TranscriptResponse
                    {
                        Succeeded = true,
                        ErrorMessage = null,
                        TranscriptUrl = url 
                    };
                }
                else
                {
                    throw new Exception($"Error calling Transcript API: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        public async Task<ApiResponse<List<RecapVersion>>> GetAllRecapVersions(Guid userId)
        {
            // Lấy tất cả RecapVersion có UserId tương ứng trong bảng Recap
            var recapVersions = await _unitOfWork.RecapVersionRepository
                .QueryWithIncludes(rv => rv.Recap)
                .Where(rv => rv.Recap.UserId == userId)
                .ToListAsync();

            // Kiểm tra nếu không tìm thấy bất kỳ RecapVersion nào
            if (!recapVersions.Any())
            {
                return new ApiResponse<List<RecapVersion>>
                {
                    Succeeded = false,
                    Message = "No RecapVersions found for the provided UserId.",
                    Errors = new[] { "No data available." },
                    Data = new List<RecapVersion>()
                };
            }

            // Trả về danh sách RecapVersion tìm thấy
            return new ApiResponse<List<RecapVersion>>
            {
                Succeeded = true,
                Message = "RecapVersions retrieved successfully.",
                Data = recapVersions
            };
        }




        #region
        //public async Task UpdateAudioStartForKeyIdeas(Guid recapVersionId, string vttFilePath)
        //{
        //    var vttRecap = await ReadVttFileAsync(vttFilePath);
        //    var timeRanges = ParseVttRecap(vttRecap);

        //    // Tạo bảng băm cho các đoạn văn bản và thời gian tương ứng
        //    var timeRangeDict = CreateTimeRangeDictionary(timeRanges);

        //    var keyIdeas = await _unitOfWork.KeyIdeaRepository
        //         .GetKeyIdeasAsNoTracking(recapVersionId)
        //         .ToListAsync();

        //    foreach (var keyIdea in keyIdeas)
        //    {
        //        var timeRange = FindTimeRangeForKeyIdea(keyIdea.Body, timeRangeDict);
        //        if (timeRange != null)
        //        {
        //            _unitOfWork.KeyIdeaRepository.DetachEntity(keyIdea);
        //            // Retrieve the entity from the database and update it
        //            var keyIdeaToUpdate = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(keyIdea.Id);
        //            keyIdeaToUpdate.AudioStart = timeRange.Start;
        //            _unitOfWork.KeyIdeaRepository.Update(keyIdeaToUpdate); // Update entity
        //        }
        //    }

        //    await _unitOfWork.SaveChangesAsync();
        //}

        //private TimeRange FindTimeRangeForKeyIdea(string keyIdeaBody, Dictionary<string, TimeRange> timeRangeDict)
        //{
        //    // Chia văn bản keyIdea thành các đoạn nhỏ hơn để so sánh
        //    var keyIdeaChunks = SplitKeyIdeaIntoChunks(keyIdeaBody, 8);
        //    var foundRange = default(TimeRange);

        //    foreach (var chunk in keyIdeaChunks)
        //    {
        //        // Tìm kiếm đoạn văn bản trong bảng băm
        //        if (timeRangeDict.TryGetValue(chunk, out var timeRange))
        //        {
        //            foundRange = timeRange;
        //        }
        //        else
        //        {
        //            // Nếu không tìm thấy chính xác, tìm đoạn gần nhất
        //            var closestMatch = FindClosestMatch(chunk, timeRangeDict.Keys);
        //            if (closestMatch != null)
        //            {
        //                foundRange = timeRangeDict[closestMatch];
        //            }
        //        }
        //        return foundRange;
        //    }

        //    return foundRange;
        //}

        //private string FindClosestMatch(string chunk, IEnumerable<string> candidates)
        //{
        //    // Tìm đoạn văn bản gần nhất trong các ứng viên
        //    return candidates.OrderBy(candidate => LevenshteinDistance(chunk, candidate)).FirstOrDefault();
        //}

        //private int LevenshteinDistance(string s, string t)
        //{
        //    var d = new int[s.Length + 1, t.Length + 1];

        //    for (int i = 0; i <= s.Length; i++)
        //        d[i, 0] = i;

        //    for (int j = 0; j <= t.Length; j++)
        //        d[0, j] = j;

        //    for (int i = 1; i <= s.Length; i++)
        //    {
        //        for (int j = 1; j <= t.Length; j++)
        //        {
        //            var cost = s[i - 1] == t[j - 1] ? 0 : 1;
        //            d[i, j] = Math.Min(
        //                Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
        //                d[i - 1, j - 1] + cost
        //            );
        //        }
        //    }

        //    return d[s.Length, t.Length];
        //}


        //private async Task<string> ReadVttFileAsync(string filePath)
        //{
        //    return await File.ReadAllTextAsync(filePath);
        //}

        //private List<TimeRange> ParseVttRecap(string vttRecap)
        //{
        //    var timeRanges = new List<TimeRange>();
        //    var lines = vttRecap.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        //    TimeRange currentRange = null;

        //    foreach (var line in lines)
        //    {
        //        // Kiểm tra nếu dòng chứa thời gian
        //        if (line.Contains("-->"))
        //        {
        //            // Xử lý thời gian bắt đầu và kết thúc
        //            var timeParts = line.Split(new[] { " --> " }, StringSplitOptions.None);
        //            if (timeParts.Length == 2 && TimeSpan.TryParse(timeParts[0], out var startTime) && TimeSpan.TryParse(timeParts[1], out var endTime))
        //            {
        //                // Nếu có TimeRange hiện tại, thêm vào danh sách trước khi tạo mới
        //                if (currentRange != null)
        //                {
        //                    timeRanges.Add(currentRange);
        //                }
        //                // Khởi tạo TimeRange mới
        //                currentRange = new TimeRange
        //                {
        //                    Start = startTime,
        //                    End = endTime,
        //                    Text = string.Empty // Bắt đầu với văn bản trống
        //                };
        //            }
        //        }
        //        else if (currentRange != null)
        //        {
        //            // Gắn thêm đoạn văn bản vào TimeRange hiện tại
        //            currentRange.Text += line + " ";
        //        }
        //    }

        //    // Thêm TimeRange cuối cùng nếu có
        //    if (currentRange != null)
        //    {
        //        timeRanges.Add(currentRange);
        //    }

        //    return timeRanges;
        //}

        //private Dictionary<string, TimeRange> CreateTimeRangeDictionary(List<TimeRange> timeRanges)
        //{
        //    var timeRangeDict = new Dictionary<string, TimeRange>();
        //    foreach (var range in timeRanges)
        //    {
        //        var chunks = SplitTextIntoChunks(range.Text, 8);
        //        foreach (var chunk in chunks)
        //        {
        //            if (!timeRangeDict.ContainsKey(chunk))
        //            {
        //                timeRangeDict[chunk] = range;
        //            }
        //        }
        //    }
        //    return timeRangeDict;
        //}

        //private TimeRange FindClosestTimeRangeForKeyIdea(string keyIdeaBody, Dictionary<string, TimeRange> timeRangeDict)
        //{
        //    // Chia văn bản keyIdea thành các đoạn nhỏ hơn để so sánh (mỗi đoạn 8 chữ)
        //    var keyIdeaChunks = SplitKeyIdeaIntoChunks(keyIdeaBody, 8);

        //    foreach (var chunk in keyIdeaChunks)
        //    {
        //        if (timeRangeDict.TryGetValue(chunk, out var timeRange))
        //        {
        //            return timeRange;
        //        }
        //    }
        //    return null;
        //}

        //private IEnumerable<string> SplitTextIntoChunks(string text, int wordsPerChunk)
        //{
        //    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //    var chunks = new List<string>();

        //    for (int i = 0; i < words.Length; i += wordsPerChunk)
        //    {
        //        chunks.Add(string.Join(' ', words.Skip(i).Take(wordsPerChunk)));
        //    }

        //    return chunks;
        //}

        //private IEnumerable<string> SplitKeyIdeaIntoChunks(string text, int wordsPerChunk)
        //{
        //    return SplitTextIntoChunks(text, wordsPerChunk);
        //}
        #endregion
        public async Task<ApiResponse<RecapVersion>> GetVersionById(Guid versionId)
        {
            var version = await _unitOfWork.RecapVersionRepository.GetByIdAsync(versionId);

            if (version == null)
            {
                return new ApiResponse<RecapVersion>
                {
                    Succeeded = false,
                    Message = $"Version with ID {versionId} not found.",
                    Errors = new[] { "Version not found." },
                    Data = null
                };
            }

            return new ApiResponse<RecapVersion>(version);
        }


        public async Task<ApiResponse<RecapVersion>> ChangeVersionStatus(ChangeVersionStatus request)
        {
            // Lấy phiên bản RecapVersion với ID được cung cấp
            var existingRecapVersion = await _unitOfWork.RecapVersionRepository
                .FirstOrDefaultAsync(cv => cv.Id == request.RecapVersionId);

            if (existingRecapVersion == null)
            {
                return new ApiResponse<RecapVersion>
                {
                    Succeeded = false,
                    Message = $"RecapVersion with ID {request.RecapVersionId} not found.",
                    Errors = new[] { "RecapVersion not found." }
                };
            }

            // Lấy Recap liên quan đến phiên bản này
            var recap = await _unitOfWork.RecapRepository
                .QueryWithIncludes(r => r.RecapVersions)
                .FirstOrDefaultAsync(r => r.Id == existingRecapVersion.RecapId);


            // Kiểm tra nếu có phiên bản khác đang ở trạng thái Pending
            if (request.Status == RecapStatus.Pending)
            {
                var hasPendingVersion = recap.RecapVersions
                    .Any(rv => rv.Status == RecapStatus.Pending && rv.Id != request.RecapVersionId);

                if (hasPendingVersion)
                {
                    return new ApiResponse<RecapVersion>
                    {
                        Succeeded = false,
                        Message = "Recap đang có phiên bản ở trạng thái chờ xử lý.",
                        Errors = new[] { "Another version is already Pending." }
                    };
                }
            }

            // Cập nhật trạng thái của phiên bản hiện tại
            existingRecapVersion.Status = request.Status;

            // Lưu các thay đổi vào cơ sở dữ liệu
            _unitOfWork.RecapVersionRepository.Update(existingRecapVersion);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<RecapVersion>
            {
                Succeeded = true,
                Message = "Version status updated successfully",
                Data = existingRecapVersion
            };
        }

        public async Task<ApiResponse<bool>> DeleteRecapVersion(Guid recapId)
        {
            var recap = await _unitOfWork.RecapVersionRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = $"Recap with ID {recapId} not found.",
                    Errors = new[] { "Recap not found." },
                    Data = false
                };
            }

            _unitOfWork.RecapVersionRepository.Delete(recap);
            var result = await _unitOfWork.SaveChangesAsync();

            if (!result)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa phiên bản thất bại.",
                    Errors = new[] { "Failed to delete recap." },
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa phiên bản thành công.",
                Data = true
            };
        }
        public async Task<ApiResponse<bool>> SoftDeleteRecapVersion(Guid recapId)
        {
            // Lấy RecapVersion từ repository
            var recapVersion = await _unitOfWork.RecapVersionRepository.GetByIdAsync(recapId);

            // Kiểm tra nếu RecapVersion không tồn tại
            if (recapVersion == null)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = $"Recap version with ID {recapId} not found.",
                    Errors = new[] { "Recap version not found." },
                    Data = false
                };
            }
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapVersion.RecapId);
            if (recap != null && recap.CurrentVersionId == recapVersion.Id)
            {
                recap.CurrentVersionId = null;
                _unitOfWork.RecapRepository.Update(recap);
            }

            _unitOfWork.RecapVersionRepository.SoftDelete(recapVersion);
            var result = await _unitOfWork.SaveChangesAsync();

            // Kiểm tra kết quả của thao tác lưu
            if (!result)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = "Xóa phiên bản thất bại.",
                    Errors = new[] { "Failed to soft delete recap version." },
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Succeeded = true,
                Message = "Xóa phiên bản thành công.",
                Data = true
            };
        }

        public async Task<ApiResponse<List<RecapVersion>>> GetVersionApprovedByRecapId(Guid recapId)
        {
            // Lấy Recap từ database theo recapId
            var recap = await _unitOfWork.RecapRepository.GetByIdAsync(recapId);

            if (recap == null)
            {
                return new ApiResponse<List<RecapVersion>>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                    Errors = new[] { "Recap not found." },
                    Data = null
                };
            }
            var approvedVersions = await _unitOfWork.RecapVersionRepository.GetVersionApprovedbyRecapId(recapId);
            if (approvedVersions == null || !approvedVersions.Any())
            {
                return new ApiResponse<List<RecapVersion>>
                {
                    Succeeded = false,
                    Message = "No approved versions found.",
                    Errors = new[] { "No approved versions found." },
                    Data = null
                };
            }
            return new ApiResponse<List<RecapVersion>>
            {
                Succeeded = true,
                Message = "Approved versions fetched successfully.",
                Data = approvedVersions
            };
        }
        public async Task<ApiResponse<List<ListRecapNotDraft>>> GetListRecapVersionNotDraft()
        {
            // Lấy danh sách các RecapVersion không có status là Draft
            var recapVersions = await _unitOfWork.RecapVersionRepository.GetAllRecapVersionNotDraft();

            // Kiểm tra nếu không có phiên bản nào được trả về
            if (recapVersions == null || !recapVersions.Any())
            {
                return new ApiResponse<List<ListRecapNotDraft>>
                {
                    Succeeded = false,
                    Message = "No approved versions found.",
                    Errors = new[] { "No approved versions found." },
                    Data = null
                };
            }

            // Lấy danh sách RecapVersionId từ recapVersions
            var recapVersionIds = recapVersions.Select(rv => rv.Id).ToList();

            // Lấy danh sách Review tương ứng với các RecapVersionId
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByRecapVersionIds(recapVersionIds);

            // Tạo dictionary để tra cứu Review nhanh hơn
            var reviewDictionary = reviews.ToDictionary(r => r.RecapVersionId, r => r);

            // Map dữ liệu từ RecapVersion sang ListRecapNotDraft
            var listRecapNotDraft = recapVersions.Select(version =>
            {
                var review = reviewDictionary.GetValueOrDefault(version.Id);

                return new ListRecapNotDraft
                {
                    RecapVersionId = version.Id,
                    VersionName = version.VersionName,
                    RecapName = version.Recap?.Name,
                    BookTitle = version.Recap?.Book?.Title,
                    BookDescription = version.Recap?.Book?.Description,
                    ContributorName = version.Recap?.Contributor?.FullName,
                    CreateAt = version.CreatedAt,
                    ReviewId = review?.Id, // Trả về null nếu không có Review
                    StaffName = review?.Staff?.FullName ?? null,
                    Status = version.Status.ToString()
                };
            }).ToList();

            return new ApiResponse<List<ListRecapNotDraft>>
            {
                Succeeded = true,
                Message = "Approved versions fetched successfully.",
                Data = listRecapNotDraft
            };
        }


    }
}
