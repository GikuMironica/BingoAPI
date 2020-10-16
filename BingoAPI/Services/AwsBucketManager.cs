using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BingoAPI.Options;
using Amazon;
using BingoAPI.Domain;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Model;
using BingoAPI.Models;
using BingoAPI.Extensions;

namespace BingoAPI.Services
{
    public class AwsBucketManager : IAwsBucketManager
    {
        private readonly AwsBucketSettings _awsBucketSettings;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IErrorService _errorService;
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 _s3Client;
        public AwsBucketManager(IOptions<AwsBucketSettings> awsBucketSettings, IHttpContextAccessor httpContext,
                                IErrorService errorService)
        {
            this._awsBucketSettings = awsBucketSettings.Value;
            this._httpContext = httpContext;
            this._errorService = errorService;
            _s3Client = new AmazonS3Client(this._awsBucketSettings.aws_access_key_id, this._awsBucketSettings.aws_secret_access_key, BucketRegion);
        }

        public async Task<ImageUploadResult> UploadFileAsync(ImageProcessingResult imageProcessingResult)
        {
            ImageUploadResult imageUploadResult = new ImageUploadResult { ImageNames = new List<string>(), Result = true };
            try
            {
                foreach (var image in imageProcessingResult.ProcessedPictures)
                {                
                    string guid = Guid.NewGuid().ToString();
                    string path = imageProcessingResult.BucketPath;
                    string keyName = $"assets/{path}/{guid}.webp";
                    image.GetBuffer();

                    var request = new PutObjectRequest                        
                        {
                            BucketName = _awsBucketSettings.bucketName,
                            Key = keyName,
                            InputStream = image,
                            ContentType = _awsBucketSettings.contentFormat,
                            CannedACL = S3CannedACL.PublicRead
                        };
                        await _s3Client.PutObjectAsync(request);
                        imageUploadResult.ImageNames.Add(guid);
                }
            }
            catch (Exception e)
            {
                // logg
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Image could not be uploaded to AWS Bucket",
                    Message = e.Message,
                    UserId = _httpContext.HttpContext.GetUserId()
                };
                await _errorService.AddErrorAsync(errorObj);
                imageUploadResult.Result = false;
            }
            return imageUploadResult;
        }

        public async Task<ImageDeleteResult> DeleteFileAsync(List<string>? imagesGuids, string path)
        {
            List<KeyVersion> keyverison = new List<KeyVersion>();
            foreach(var imageGuid in imagesGuids)
            {
                string keyName = $"assets/{path}/{imageGuid}.webp";
                keyverison.Add(new KeyVersion { Key = keyName, VersionId = null });
            }

            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = _awsBucketSettings.bucketName,
                Objects = keyverison
            };

            try
            {
                DeleteObjectsResponse response = await _s3Client.DeleteObjectsAsync(deleteObjectsRequest);
                bool cnt = response.DeletedObjects.Count == imagesGuids.Count;
                return new ImageDeleteResult { Result = cnt };
            }
            catch (DeleteObjectsException e)
            {
                // logg
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Image could not be deleted",
                    Message = e.Message,
                    UserId = _httpContext.HttpContext.GetUserId()
                };
                await _errorService.AddErrorAsync(errorObj);
                return new ImageDeleteResult { Result = false, ErrorMessages = e.Response.DeleteErrors };
            }
        }
    }
}
