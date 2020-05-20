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

namespace BingoAPI.Services
{
    public class AwsBucketManager : IAwsBucketManager
    {
        private readonly AwsBucketSettings awsBucketSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        public AwsBucketManager(IOptions<AwsBucketSettings> awsBucketSettings, IWebHostEnvironment webHostEnvironment)
        {
            this.awsBucketSettings = awsBucketSettings.Value;
            this.webHostEnvironment = webHostEnvironment;
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
                    string keyName = $"assets/images/{guid}.webp";
                    image.Position = 0;
                    byte[] memString = image.GetBuffer();                                   

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
                imageUploadResult.ErrrorMessage = e.Message;
                imageUploadResult.Result = false;
            }
            return imageUploadResult;
        }

        public async Task<ImageDeleteResult> DeleteFileAsync(List<string>? imagesGuids)
        {
            List<KeyVersion> keyverison = new List<KeyVersion>();
            foreach(var imageGuid in imagesGuids)
            {
                keyverison.Add(new KeyVersion { Key = imageGuid, VersionId = null });
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
                return new ImageDeleteResult { Result = false, ErrorMessages = e.Response.DeleteErrors };
            }
        }
    }
}
