using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using Google.LongRunning;
using Microsoft.Extensions.Configuration;
using Services.Service.Helper;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public class TextToSpeechService
{

    private readonly IConfiguration _configuration;
    private readonly TextToSpeechClient _client;
    private readonly TextToSpeechLongAudioSynthesizeClient _clientlong;
    private readonly GoogleCloudService _googleCloudService;
    private readonly string _bucketName;

    // Đảm bảo GoogleCloudService được truyền vào để thiết lập thông tin xác thực
    public TextToSpeechService(GoogleCloudService googleCloudService, IConfiguration configuration)
    {
        _configuration = configuration;
        _googleCloudService = googleCloudService;
        _client = TextToSpeechClient.Create();
        _clientlong = TextToSpeechLongAudioSynthesizeClient.Create();
        _bucketName = _configuration["GoogleCloud:BucketName"];

    }

    // Phương thức chính để generate audio
    public async Task<MemoryStream> GenerateAudioAsync(string text, double speakingRate = 1.0, string? customVoice = null)
    {
        // Kiểm tra độ dài của text
        if (text.Length > 5000)
        {
            // Nếu text dài hơn 5000 ký tự, sử dụng Long API
            return await GenerateLongAudioAsync(text, speakingRate, customVoice);
        }
        else
        {
            // Text ngắn, xử lý với Short API
            return await GenerateShortAudioAsync(text, speakingRate, customVoice);
        }
    }

    // Xử lý đoạn text ngắn với Short API
    private async Task<MemoryStream> GenerateShortAudioAsync(string text, double speakingRate = 1.0, string? customVoice = null)
    {
        var ssml = $@"
<speak>
    {text}
</speak>";

        var input = new SynthesisInput
        {
            Ssml = ssml
        };

        var voiceSelection = new VoiceSelectionParams
        {
            LanguageCode = "vi-VN", // Chọn ngôn ngữ tiếng Việt
            SsmlGender = customVoice != null ? (SsmlVoiceGender)Enum.Parse(typeof(SsmlVoiceGender), customVoice) : SsmlVoiceGender.Female
        };

        var audioConfig = new AudioConfig
        {
            AudioEncoding = AudioEncoding.Linear16, // Sử dụng Linear16 để output thành file .wav
            SpeakingRate = speakingRate
        };

        var response = await _client.SynthesizeSpeechAsync(input, voiceSelection, audioConfig);

        // Tạo stream từ AudioContent
        var memoryStream = new MemoryStream(response.AudioContent.ToByteArray());
        memoryStream.Position = 0; // Đặt vị trí về đầu stream

        return memoryStream;
    }

    // Xử lý đoạn text dài với Long API
    private async Task<MemoryStream> GenerateLongAudioAsync(string text, double speakingRate = 1.0, string? customVoice = null)
    {
        string folderName = "recap_audio/"; // Folder muốn lưu
        string newAudioFileName = $"{folderName}audio_{Guid.NewGuid()}.wav"; // Tên file mới
        string outputGcsUri = $"gs://{_bucketName}/{newAudioFileName}";
        var input = new SynthesisInput
        {
            Text = text
        };

        var voiceSelection = new VoiceSelectionParams
        {
            LanguageCode = "vi-VN", // Ngôn ngữ tiếng Việt
            Name = "vi-VN-Wavenet-A"
        };

        var audioConfig = new AudioConfig
        {
            AudioEncoding = AudioEncoding.Linear16, // Định dạng file .wav
            SpeakingRate = speakingRate
        };
        var parent = "projects/619402755996/locations/global";
        var longAudioRequest = new SynthesizeLongAudioRequest
        {
            Parent = parent,
            Input = input,
            Voice = voiceSelection,
            AudioConfig = audioConfig,
            OutputGcsUri = outputGcsUri
        };

        // Thực hiện synthesize long audio với Google Cloud Speech API (chắc chắn đã được tích hợp API đúng cách)
        var operation = await _clientlong.SynthesizeLongAudioAsync(longAudioRequest); // Chắc chắn đã sử dụng đúng API.

        // Chờ hoàn thành và nhận kết quả
        var completedOperation = await operation.PollUntilCompletedAsync();

        // Kiểm tra nếu operation hoàn thành và có lỗi
        if (completedOperation.IsFaulted)
        {
            throw new Exception("Audio synthesis failed.");
        }

        // Nếu đã hoàn thành, tải xuống file từ Google Cloud Storage
        return await DownloadAudioFromGcs(outputGcsUri);
    }

    // Hàm để tải file âm thanh từ Google Cloud Storage (GCS)
    private async Task<MemoryStream> DownloadAudioFromGcs(string gcsUri)
    {
        // Lấy thông tin bucket và object name từ GCS URI
        var bucketName = _googleCloudService.GetBucketNameFromUri(gcsUri);
        var objectName = _googleCloudService.GetObjectNameFromUri(gcsUri);

        // Sử dụng GoogleCloudService để tải file
        var storageClient = _googleCloudService.GetStorageClient(); // Sử dụng phương thức GetStorageClient
        var memoryStream = new MemoryStream();
        await storageClient.DownloadObjectAsync(bucketName, objectName, memoryStream);

        memoryStream.Position = 0; // Đặt vị trí về đầu stream
        return memoryStream;
    }


}
