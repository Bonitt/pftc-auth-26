using Google.Cloud.Storage.V1;
using pftc_auth.Interfaces;

namespace pftc_auth.Services
{
    public class BucketStorageService : IBucketStorageService
    {
        private readonly ILogger<BucketStorageService> _logger;
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public BucketStorageService(ILogger<BucketStorageService> logger, IConfiguration config)
        {
            _logger = logger;
            _bucketName = config.GetValue<string>("Storage:Google:BucketName");
            _storageClient = StorageClient.Create();
        }

        public Task DeleteFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileNameForStorage)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }

            try
            {
                if (string.IsNullOrWhiteSpace(fileNameForStorage)) {
                    fileNameForStorage = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                }

                string contentType = file.ContentType;

                using (var memoryStream = new MemoryStream()) {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    //upload the file to google cloud storage
                    UploadObjectOptions options = new UploadObjectOptions();
                    var storageObject = await _storageClient.UploadObjectAsync(_bucketName, fileNameForStorage, contentType, memoryStream, options);
                    _logger.LogInformation($"Uploaded file {fileNameForStorage} to bucket: {_bucketName}");
                    return $"https://storage.googleapis.com/{_bucketName}/{fileNameForStorage}";
                }
            }
            catch (Google.GoogleApiException gae)
            {
                _logger.LogError(gae, $"Google API error while uploading file {fileNameForStorage} to bucket {_bucketName}");
                throw new ApplicationException($"Google API error while uploading file {fileNameForStorage} to bucket {_bucketName}", gae);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected error while uploading file {fileNameForStorage} to bucket {_bucketName}");
                throw new ApplicationException($"Unexpected error while uploading file {e.Message}", e);
            }
        }
    }
}
