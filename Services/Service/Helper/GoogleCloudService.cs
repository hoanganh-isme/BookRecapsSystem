using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using System;

namespace Services.Service.Helper
{
    public class GoogleCloudService
    {
        private readonly IConfiguration _configuration;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GoogleCloudService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Lấy giá trị từ biến môi trường
            //var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            //if (string.IsNullOrEmpty(credentialsPath))
            //{
            //    throw new InvalidOperationException("Google Cloud credentials path is not configured.");
            //}
            SetGoogleCredentials();

            // Tạo StorageClient
            _storageClient = StorageClient.Create();
            _bucketName = _configuration["GoogleCloud:BucketName"];
        }


        private void SetGoogleCredentials()
        {
            // Lấy đường dẫn từ appsettings.json
            string credentialsPath = _configuration["GoogleCloud:CredentialsPath"];
            if (string.IsNullOrEmpty(credentialsPath))
            {
                throw new Exception("Google Cloud credentials path is not configured.");
            }

            // Thiết lập biến môi trường GOOGLE_APPLICATION_CREDENTIALS
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        }
        public async Task<string> UploadFileAsync(string objectName, Stream fileStream)
        {
            try
            {
                // Upload file lên Google Cloud Storage
                await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);

                // Trả về public URL sau khi upload
                string publicUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
                return publicUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(string objectName, Stream fileStream, string contentType)
        {
            try
            {
                // Upload file lên Google Cloud Storage
                await _storageClient.UploadObjectAsync(_bucketName, objectName, contentType, fileStream);

                // Trả về public URL sau khi upload
                string publicUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
                return publicUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string imageObjectName)
        {
            try
            {
                // Kiểm tra xem tên đối tượng có bắt đầu bằng tên bucket không
                if (imageObjectName.StartsWith($"{_bucketName}/"))
                {
                    // Cắt bỏ tên bucket từ đường dẫn
                    imageObjectName = imageObjectName.Substring(_bucketName.Length + 1); // +1 để bỏ dấu '/'
                }

                await _storageClient.DeleteObjectAsync(_bucketName, imageObjectName);
                Console.WriteLine($"Deleted image {imageObjectName} from {_bucketName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                throw;
            }
        }
        public string GetBucketNameFromUri(string gcsUri)
        {
            var uri = new Uri(gcsUri);
            return uri.Host; // Lấy bucket name từ URI
        }

        public string GetObjectNameFromUri(string gcsUri)
        {
            var uri = new Uri(gcsUri);
            return uri.AbsolutePath.TrimStart('/'); // Lấy object name từ URI
        }
        public StorageClient GetStorageClient()
        {
            return _storageClient;
        }


    }
}
