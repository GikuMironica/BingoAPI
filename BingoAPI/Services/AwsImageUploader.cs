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
            foreach (var image in imageProcessingResult.ProcessedPictures)
            {
                string guid = Guid.NewGuid().ToString();
                using (var client = new AmazonS3Client(awsBucketSettings.aws_access_key_id, awsBucketSettings.aws_secret_access_key, RegionEndpoint.EUCentral1))
                {
                
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = image,
                            Key = "./assets/images/" + guid + ".webp",
                            BucketName = "bingo-bucket32",
                            CannedACL = S3CannedACL.PublicRead
                        };

                        var fileTransferUtility = new TransferUtility(client);
                        imageUploadResult.ImageNames.Add(guid);
                        await fileTransferUtility.UploadAsync(uploadRequest);                        
                    
                }


            }

            return imageUploadResult;
        }
    }
}
