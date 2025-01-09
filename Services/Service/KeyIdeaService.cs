using AutoMapper;
using BusinessObject.Enums;
using BusinessObject.Models;
using BusinessObject.ViewModels.Recaps;
using BusinessObject.ViewModels.KeyIdea;
using Google.Apis.Books.v1.Data;
using Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Service.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Services.Responses;

namespace Services.Service
{
    public class KeyIdeaService : IKeyIdeaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRecapVersionService _recapVersionService;
        private readonly GoogleCloudService _googleCloudService;

        public KeyIdeaService(IUnitOfWork unitOfWork, IMapper mapper
            , IRecapVersionService recapVersionService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _recapVersionService = recapVersionService;
            _googleCloudService = new GoogleCloudService(configuration);
        }

        public async Task<ApiResponse<KeyIdea>> CreatePrepareKeyIdea(PrepareIdea idea, Stream? ImageStream, string? ImageContentType)
        {
            // Lấy RecapVersion từ RecapVersionId của idea
            var recapVersion = _unitOfWork.RecapVersionRepository
                .QueryWithIncludes(cv => cv.Recap, cv => cv.KeyIdeas) // Bao gồm KeyIdeas nếu cần xử lý thêm
                .FirstOrDefault(cv => cv.Id == idea.RecapVersionId);

            if (recapVersion == null)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = "RecapVersion not found.",
                };
            }

            // Lấy Recap từ RecapVersion
            var recap = recapVersion.Recap;

            if (recap == null)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = "Recap not found.",
                };
            }

            // Kiểm tra nếu RecapVersion hiện tại không phải Draft, tạo mới RecapVersion
            if (recapVersion.Status != RecapStatus.Draft)
            {
                var newVersionNumber = (recap.RecapVersions.Max(cv => cv.VersionNumber) ?? 0) + 1;

                var newRecapVersion = new RecapVersion
                {
                    VersionNumber = newVersionNumber,
                    Status = RecapStatus.Draft,
                    RecapId = recap.Id
                };

                // Thêm RecapVersion mới vào cơ sở dữ liệu
                await _unitOfWork.RecapVersionRepository.AddAsync(newRecapVersion);
                await _unitOfWork.SaveChangesAsync();


                // Cập nhật Recap trong cơ sở dữ liệu
                _unitOfWork.RecapRepository.Update(recap);
                await _unitOfWork.SaveChangesAsync();

                recapVersion = newRecapVersion; // Gán RecapVersion mới để thêm KeyIdea
            }

            // Kiểm tra và xử lý ảnh nếu có
            if (ImageStream != null && !string.IsNullOrEmpty(ImageContentType))
            {
                // Tạo thư mục động cho ảnh bìa dựa theo BookId
                string authorFolderName = $"keyidea_image/";
                string ImageFileName = $"{authorFolderName}image_keyidea_{Guid.NewGuid()}.jpg";

                var ImageUrl = await _googleCloudService.UploadImageAsync(ImageFileName, ImageStream, ImageContentType);
                idea.Image = ImageUrl;  // Lưu URL ảnh bìa
            }

            // Tạo KeyIdea và gán vào RecapVersion đã xác định
            var keyIdea = _mapper.Map<KeyIdea>(idea);
            keyIdea.RecapVersionId = recapVersion.Id; // Gắn RecapVersionId vào KeyIdea
            var version = await _unitOfWork.RecapVersionRepository.GetByIdAsync(keyIdea.RecapVersionId);
            if (version == null)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = "RecapVersion not found.",
                    Errors = new[] { "RecapVersion not found." },
                    Data = null
                };
            }

            var keyIdeasInVersion = await _unitOfWork.KeyIdeaRepository.GetAllAsync(k => k.RecapVersionId == version.Id);

            // Kiểm tra trùng Order
            if (idea.Order != 0)
            {
                if (keyIdeasInVersion.Any(k => k.Order == idea.Order))
                {
                    return new ApiResponse<KeyIdea>
                    {
                        Succeeded = false,
                        Message = "Order already exists for another KeyIdea in the same version.",
                        Errors = new[] { "Duplicate order in the same RecapVersion." },
                        Data = null
                    };
                }
                keyIdea.Order = idea.Order;
            }

            await _unitOfWork.KeyIdeaRepository.AddAsync(keyIdea);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResponse<KeyIdea>
            {
                Succeeded = true,
                Message = "Create Success.",
                Data = keyIdea
            };
        }

        public async Task<ApiResponse<IEnumerable<KeyIdea>>> CreateMultiplePrepareKeyIdeas(IEnumerable<PrepareIdea> ideas, IEnumerable<IFormFile>? imageFiles)
        {
            var createdKeyIdeas = new List<KeyIdea>();

            // Chuyển đổi danh sách ảnh thành list, mặc định là null nếu không có ảnh
            var imageList = imageFiles?.ToList() ?? new List<IFormFile>();

            for (int i = 0; i < ideas.Count(); i++)
            {
                var idea = ideas.ElementAt(i);
                var image = imageList.ElementAtOrDefault(i); // Lấy ảnh tương ứng nếu có

                // Lấy RecapVersion từ RecapVersionId của idea
                var recapVersion = _unitOfWork.RecapVersionRepository
                    .QueryWithIncludes(cv => cv.Recap, cv => cv.KeyIdeas)
                    .FirstOrDefault(cv => cv.Id == idea.RecapVersionId);

                if (recapVersion == null)
                {
                    return new ApiResponse<IEnumerable<KeyIdea>>
                    {
                        Succeeded = false,
                        Message = $"RecapVersion not found for KeyIdea with RecapVersionId {idea.RecapVersionId}.",
                    };
                }

                // Nếu RecapVersion không ở trạng thái Draft, tạo RecapVersion mới
                if (recapVersion.Status != RecapStatus.Draft)
                {
                    var newVersionNumber = (recapVersion.Recap.RecapVersions.Max(cv => cv.VersionNumber) ?? 0) + 1;

                    var newRecapVersion = new RecapVersion
                    {
                        VersionNumber = newVersionNumber,
                        Status = RecapStatus.Draft,
                        RecapId = recapVersion.RecapId
                    };

                    await _unitOfWork.RecapVersionRepository.AddAsync(newRecapVersion);
                    await _unitOfWork.SaveChangesAsync();

                    recapVersion = newRecapVersion;
                }

                // Xử lý ảnh nếu có ảnh trong request
                if (image != null && image.Length > 0)
                {
                    using var imageStream = image.OpenReadStream();
                    var imageContentType = image.ContentType;
                    string imageFileName = $"keyidea_image/image_keyidea_{Guid.NewGuid()}.jpg";
                    var imageUrl = await _googleCloudService.UploadImageAsync(imageFileName, imageStream, imageContentType);
                    idea.Image = imageUrl; // Lưu URL của ảnh vào idea
                }

                var keyIdea = _mapper.Map<KeyIdea>(idea); // Map PrepareIdea sang KeyIdea
                keyIdea.RecapVersionId = recapVersion.Id; // Gán RecapVersionId cho KeyIdea

                await _unitOfWork.KeyIdeaRepository.AddAsync(keyIdea); // Lưu vào database
                createdKeyIdeas.Add(keyIdea); // Thêm vào danh sách đã tạo
            }

            await _unitOfWork.SaveChangesAsync(); // Lưu toàn bộ thay đổi

            return new ApiResponse<IEnumerable<KeyIdea>>
            {
                Succeeded = true,
                Message = "All KeyIdeas created successfully.",
                Data = createdKeyIdeas // Trả về danh sách các KeyIdea đã tạo
            };
        }



        public async Task<ApiResponse<bool>> DeleteKeyIdea(Guid keyIdeaId)
        {
            try
            {
                var keyIdea = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(keyIdeaId);
                if (keyIdea == null)
                {
                    return new ApiResponse<bool>
                    {
                        Succeeded = false,
                        Message = $"KeyIdea with ID {keyIdeaId} not found.",
                        Errors = new[] { "KeyIdea not found." },
                        Data = false
                    };
                }

                _unitOfWork.KeyIdeaRepository.Delete(keyIdea);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Succeeded = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = ex.Message,
                    Errors = new[] { ex.ToString() },
                    Data = false
                };
            }
        }
        public async Task<ApiResponse<bool>> SoftDeleteKeyIdea(Guid keyIdeaId)
        {
            try
            {
                var keyIdea = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(keyIdeaId);
                if (keyIdea == null)
                {
                    return new ApiResponse<bool>
                    {
                        Succeeded = false,
                        Message = $"KeyIdea with ID {keyIdeaId} not found.",
                        Errors = new[] { "KeyIdea not found." },
                        Data = false
                    };
                }

                _unitOfWork.KeyIdeaRepository.SoftDelete(keyIdea);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Succeeded = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Succeeded = false,
                    Message = ex.Message,
                    Errors = new[] { ex.ToString() },
                    Data = false
                };
            }
        }
        public async Task<ApiResponse<List<KeyIdea>>> GetAllKeyIdeas()
        {
            try
            {
                var keyIdeas = await _unitOfWork.KeyIdeaRepository.GetAllAsync();
                return new ApiResponse<List<KeyIdea>>
                {
                    Succeeded = true,
                    Data = keyIdeas.ToList()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<KeyIdea>>
                {
                    Succeeded = false,
                    Message = ex.Message,
                    Errors = new[] { ex.ToString() },
                    Data = new List<KeyIdea>()
                };
            }
        }
        public async Task<ApiResponse<KeyIdea>> GetKeyIdeaById(Guid keyIdeaId)
        {
            try
            {
                var keyIdea = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(keyIdeaId);
                if (keyIdea == null)
                {
                    return new ApiResponse<KeyIdea>
                    {
                        Succeeded = false,
                        Message = $"KeyIdea with ID {keyIdeaId} not found.",
                        Errors = new[] { "KeyIdea not found." },
                        Data = null
                    };
                }

                return new ApiResponse<KeyIdea>
                {
                    Succeeded = true,
                    Data = keyIdea
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = ex.Message,
                    Errors = new[] { ex.ToString() },
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<KeyIdea>> UpdateKeyIdea(Guid id, UpdateKeyIdeaRequest request, Stream imageStream, string imageContentType)
        {
            if (request == null)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = "Request cannot be null.",
                    Errors = new[] { "Request cannot be null." },
                    Data = null
                };
            }

            try
            {
                // Tìm KeyIdea trong cơ sở dữ liệu
                var keyIdea = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(id);
                if (keyIdea == null)
                {
                    return new ApiResponse<KeyIdea>
                    {
                        Succeeded = false,
                        Message = $"KeyIdea with ID {id} not found.",
                        Errors = new[] { "KeyIdea not found." },
                        Data = null
                    };
                }

                // Xử lý yêu cầu xóa ảnh nếu có
                if (request.RemoveImage && string.IsNullOrEmpty(request.Image))
                {
                    if (!string.IsNullOrEmpty(keyIdea.Image))
                    {
                        Uri oldUri = new Uri(keyIdea.Image);
                        string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                        await _googleCloudService.DeleteFileAsync(oldObjectName); // Xóa ảnh khỏi Google Cloud
                        keyIdea.Image = null; // Đặt trường Image về null trong database
                    }
                }
                else if (imageStream != null && !string.IsNullOrEmpty(imageContentType))
                {
                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(keyIdea.Image))
                    {
                        Uri oldUri = new Uri(keyIdea.Image);
                        string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
                        await _googleCloudService.DeleteFileAsync(oldObjectName);
                    }

                    // Tạo tên file mới cho ảnh và upload
                    string authorFolderName = $"keyidea_image/";
                    string imageFileName = $"{authorFolderName}image_keyidea_{Guid.NewGuid()}.jpg";

                    var imageUrl = await _googleCloudService.UploadImageAsync(imageFileName, imageStream, imageContentType);
                    keyIdea.Image = imageUrl; // Cập nhật URL ảnh mới
                }

                // Lấy version và các KeyIdeas liên quan
                var version = await _unitOfWork.RecapVersionRepository.GetByIdAsync(keyIdea.RecapVersionId);
                if (version == null)
                {
                    return new ApiResponse<KeyIdea>
                    {
                        Succeeded = false,
                        Message = "RecapVersion not found.",
                        Errors = new[] { "RecapVersion not found." },
                        Data = null
                    };
                }

                var keyIdeasInVersion = await _unitOfWork.KeyIdeaRepository.GetAllAsync(k => k.RecapVersionId == version.Id && k.Id != id);

                // Kiểm tra trùng Order
                if (request.Order != 0)
                {
                    if (keyIdeasInVersion.Any(k => k.Order == request.Order))
                    {
                        return new ApiResponse<KeyIdea>
                        {
                            Succeeded = false,
                            Message = "Thứ tự đã có ở Keyidea cùng phiên bản.",
                            Errors = new[] { "Duplicate order in the same RecapVersion." },
                            Data = null
                        };
                    }
                    keyIdea.Order = request.Order;
                }

                // Cập nhật các thuộc tính khác nếu request có giá trị
                if (!string.IsNullOrEmpty(request.Title))
                {
                    keyIdea.Title = request.Title;
                }

                if (string.IsNullOrEmpty(request.Body))
                {
                    keyIdea.Body = null;
                }
                else
                {
                    keyIdea.Body = request.Body;
                }

                // Lưu thay đổi
                _unitOfWork.KeyIdeaRepository.Update(keyIdea);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<KeyIdea>
                {
                    Succeeded = true,
                    Data = keyIdea
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<KeyIdea>
                {
                    Succeeded = false,
                    Message = ex.Message,
                    Errors = new[] { ex.ToString() },
                    Data = null
                };
            }
        }


        //public async Task<ApiResponse<IEnumerable<KeyIdea>>> UpdateMultipleKeyIdeas(IEnumerable<UpdateKeyIdeaRequest> requests, IEnumerable<IFormFile>? imageFiles)
        //{
        //    if (requests == null || !requests.Any())
        //    {
        //        return new ApiResponse<IEnumerable<KeyIdea>>
        //        {
        //            Succeeded = false,
        //            Message = "Request list cannot be null or empty.",
        //            Errors = new[] { "Request list cannot be null or empty." },
        //            Data = null
        //        };
        //    }

        //    var updatedKeyIdeas = new List<KeyIdea>();

        //    // Chuyển đổi danh sách ảnh thành list, mặc định là null nếu không có ảnh
        //    var imageList = imageFiles?.ToList() ?? new List<IFormFile>();

        //    for (int i = 0; i < requests.Count(); i++)
        //    {
        //        var request = requests.ElementAt(i);
        //        var image = imageList.ElementAtOrDefault(i); // Lấy ảnh tương ứng nếu có

        //        // Tìm KeyIdea trong cơ sở dữ liệu
        //        var keyIdea = await _unitOfWork.KeyIdeaRepository.GetByIdAsync(request.Id);
        //        if (keyIdea == null)
        //        {
        //            return new ApiResponse<IEnumerable<KeyIdea>>
        //            {
        //                Succeeded = false,
        //                Message = $"KeyIdea with ID {request.Id} not found.",
        //                Errors = new[] { "KeyIdea not found." },
        //                Data = null
        //            };
        //        }

        //        // Xử lý ảnh nếu có ảnh mới trong request
        //        if (image != null && image.Length > 0)
        //        {
        //            // Xóa ảnh cũ nếu có
        //            if (!string.IsNullOrEmpty(keyIdea.Image))
        //            {
        //                Uri oldUri = new Uri(keyIdea.Image);
        //                string oldObjectName = oldUri.AbsolutePath.TrimStart('/');
        //                await _googleCloudService.DeleteFileAsync(oldObjectName); // Xóa ảnh cũ trên cloud
        //            }

        //            // Tạo tên file mới cho ảnh và upload ảnh mới
        //            string authorFolderName = $"keyidea_image/";
        //            string imageFileName = $"{authorFolderName}image_keyidea_{Guid.NewGuid()}.jpg";
        //            var imageUrl = await _googleCloudService.UploadImageAsync(imageFileName, image.OpenReadStream(), image.ContentType);
        //            keyIdea.Image = imageUrl; // Cập nhật URL ảnh mới
        //        }

        //        // Cập nhật các thuộc tính nếu request có giá trị
        //        if (!string.IsNullOrEmpty(request.Title))
        //        {
        //            keyIdea.Title = request.Title;
        //        }

        //        if (!string.IsNullOrEmpty(request.Body))
        //        {
        //            keyIdea.Body = request.Body;
        //        }

        //        // Nếu `Order` không phải là giá trị mặc định (0), thì cập nhật
        //        if (request.Order != 0)
        //        {
        //            keyIdea.Order = request.Order;
        //        }

        //        // Lưu keyIdea đã được cập nhật
        //        _unitOfWork.KeyIdeaRepository.Update(keyIdea);
        //        updatedKeyIdeas.Add(keyIdea);
        //    }

        //    // Lưu thay đổi cho tất cả KeyIdeas
        //    await _unitOfWork.SaveChangesAsync();

        //    return new ApiResponse<IEnumerable<KeyIdea>>
        //    {
        //        Succeeded = true,
        //        Message = "All KeyIdeas updated successfully.",
        //        Data = updatedKeyIdeas
        //    };
        //}





    }



}

