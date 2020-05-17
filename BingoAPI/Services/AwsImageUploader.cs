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

namespace BingoAPI.Services
{
    public class AwsImageUploader : IAwsImageUploader
    {
        private readonly AwsBucketSettings awsBucketSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
        private static IAmazonS3 s3Client;
        private static readonly string bucketName = "bingo-bucket32";
        public AwsImageUploader(IOptions<AwsBucketSettings> awsBucketSettings, IWebHostEnvironment webHostEnvironment)
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
                            BucketName = bucketName,
                            Key = keyName,
                            InputStream = image,
                            ContentType = "image/webp",
                            CannedACL = S3CannedACL.PublicRead
                        };
                        var rez = await s3Client.PutObjectAsync(request);
                                  
                }
            }
            catch (Exception e)
            {
                imageUploadResult.Result = false;
            }
            return imageUploadResult;
        }
    }
}
