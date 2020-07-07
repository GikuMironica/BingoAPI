using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Options;
using Amazon;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using Amazon.S3.Model;
using Npgsql;
using BingoAPI.Models;
using BingoAPI.Extensions;

namespace BingoAPI.Services
{
    public class AwsBucketManager : IAwsBucketManager
    {
        private readonly AwsBucketSettings awsBucketSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContext;
        private readonly IErrorService errorService;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        public AwsBucketManager(IOptions<AwsBucketSettings> awsBucketSettings, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContext,
                                IErrorService errorService)
        {
            this.awsBucketSettings = awsBucketSettings.Value;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContext = httpContext;
            this.errorService = errorService;
            s3Client = new AmazonS3Client(this.awsBucketSettings.aws_access_key_id, this.awsBucketSettings.aws_secret_access_key, bucketRegion);
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
                    byte[] memString = image.GetBuffer();
                    var length = memString.Length;

                        var request = new Amazon.S3.Model.PutObjectRequest                        
                        {
                            BucketName = awsBucketSettings.bucketName,
                            Key = keyName,
                            InputStream = image,
                            ContentType = awsBucketSettings.contentFormat,
                            CannedACL = S3CannedACL.PublicRead
                        };
                        var rez = await s3Client.PutObjectAsync(request);
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
                    UserId = httpContext.HttpContext.GetUserId()
                };
                await errorService.AddErrorAsync(errorObj);
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
                BucketName = awsBucketSettings.bucketName,
                Objects = keyverison
            };

            try
            {
                DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(deleteObjectsRequest);
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
                    UserId = httpContext.HttpContext.GetUserId()
                };
                await errorService.AddErrorAsync(errorObj);
                return new ImageDeleteResult { Result = false, ErrorMessages = e.Response.DeleteErrors };
            }
        }
    }
}
