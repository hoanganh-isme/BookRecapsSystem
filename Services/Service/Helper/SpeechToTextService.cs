using Google.Cloud.Speech.V1;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Services.Service.Helper
{
    public class SpeechToTextService
    {
        private readonly SpeechClient _speechClient;

        // Đảm bảo GoogleCloudService được truyền vào để thiết lập thông tin xác thực
        public SpeechToTextService(GoogleCloudService googleCloudService)
        {
            _speechClient = SpeechClient.Create();
        }

        public async Task<string> GenerateWebVttAsync(string audioFilePath, string outputVttPath, string originalText)
        {
            var recognitionConfig = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Mp3, // Định dạng MP3
                SampleRateHertz = 16000, // Đặt tần số lấy mẫu phù hợp với MP3
                LanguageCode = "vi-VN", // Ngôn ngữ tiếng Việt
                EnableAutomaticPunctuation = true, // Bật dấu câu tự động
                EnableWordTimeOffsets = true // Bật tính năng lưu thời gian của từng từ
            };

            var audio = RecognitionAudio.FromFile(audioFilePath);
            var response = await _speechClient.RecognizeAsync(recognitionConfig, audio);

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT");

            var words = response.Results
                .SelectMany(result => result.Alternatives)
                .SelectMany(alternative => alternative.Words)
                .ToList();

            if (words.Count > 0)
            {
                // Số lượng chữ tối đa trong mỗi đoạn
                const int maxWordsPerSegment = 8;

                // Tách văn bản gốc thành các phần nhỏ theo số lượng từ tối đa
                var originalSegments = SplitTextIntoSegments(originalText, maxWordsPerSegment);

                int currentIndex = 0;

                foreach (var segment in originalSegments)
                {
                    // Tìm các từ tương ứng với đoạn văn bản gốc
                    var segmentWords = words.Skip(currentIndex).Take(segment.Split(' ').Length).ToList();
                    if (segmentWords.Count == 0) break;

                    var firstWord = segmentWords.First();
                    var lastWord = segmentWords.Last();

                    // Nếu thời gian kết thúc của đoạn trước >= thời gian bắt đầu của đoạn hiện tại, thêm một khoảng buffer
                    var startTime = firstWord.StartTime.ToTimeSpan();
                    var endTime = lastWord.EndTime.ToTimeSpan();

                    // Đảm bảo thời gian kết thúc luôn lớn hơn thời gian bắt đầu
                    if (endTime <= startTime)
                    {
                        endTime = startTime.Add(TimeSpan.FromMilliseconds(500)); // Thêm 500ms để đảm bảo không bị trùng
                    }

                    // Định dạng thời gian theo chuẩn "hh:mm:ss.fff"
                    var startTimeFormatted = $"{startTime.Hours:D2}:{startTime.Minutes:D2}:{startTime.Seconds:D2}.{startTime.Milliseconds:D3}";
                    var endTimeFormatted = $"{endTime.Hours:D2}:{endTime.Minutes:D2}:{endTime.Seconds:D2}.{endTime.Milliseconds:D3}";

                    // Thêm khoảng trắng giữa các đoạn transcript
                    sb.AppendLine();
                    sb.AppendLine($"{startTimeFormatted} --> {endTimeFormatted}");
                    sb.AppendLine(segment);

                    currentIndex += segmentWords.Count;
                }
            }

            // Ghi nội dung vào file WebVTT
            await File.WriteAllTextAsync(outputVttPath, sb.ToString());
            return outputVttPath;
        }

        private IEnumerable<string> SplitTextIntoSegments(string text, int maxWordsPerSegment)
        {
            var words = text.Split(' ');
            for (int i = 0; i < words.Length; i += maxWordsPerSegment)
            {
                yield return string.Join(" ", words.Skip(i).Take(maxWordsPerSegment));
            }
        }

        // Hàm để đọc nội dung file .vtt
        public async Task<string> ReadVttFileAsync(string vttFilePath)
        {
            if (!File.Exists(vttFilePath))
            {
                Console.WriteLine("File does not exist.");
                return string.Empty;
            }

            try
            {
                // Đọc nội dung của file .vtt
                string recap = await File.ReadAllTextAsync(vttFilePath);
                Console.WriteLine("File recap:");
                Console.WriteLine(recap);
                return recap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the file: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
