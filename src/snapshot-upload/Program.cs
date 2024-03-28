using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;

using Neon.Diagnostics;

using QdrantOperator;



namespace SnapshotUpload
{

    /// <summary>
    /// Represents the entry point of the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main method of the program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(o =>
            {
                o.AddConsole();
            });

            var logger = loggerFactory.CreateLogger("qdrant-upload-job");

            var s3 = new AmazonS3Client(
                awsAccessKeyId: Environment.GetEnvironmentVariable(Constants.S3AccessKey),
                awsSecretAccessKey: Environment.GetEnvironmentVariable(Constants.S3SecretAccessKey),
                new AmazonS3Config()
                {
                    Timeout = TimeSpan.FromHours(1),
                    RetryMode = Amazon.Runtime.RequestRetryMode.Standard,
                    MaxErrorRetry = 3,
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable(Constants.S3BucketRegion))
                });

            var snapshotId     = Environment.GetEnvironmentVariable(Constants.QdrantSnapshotId);
            var snapshotName   = Environment.GetEnvironmentVariable(Constants.QdrantSnapshotName);
            var nodeId         = Environment.GetEnvironmentVariable(Constants.QdrantNodeId);
            var collectionName = Environment.GetEnvironmentVariable(Constants.QdrantCollectionName);

            logger?.LogInformationEx(() => $"Uploading data");

            var path = $"/qdrant/snapshots/{collectionName}";

            var files = new List<string>
            {
                snapshotId,
                snapshotId + ".checksum"
            };

            foreach (string fileName in files)
            {
                logger?.LogInformationEx(() => $"Uploading file: {fileName}");

                var key = Path.Combine(snapshotId, nodeId, fileName);
                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = Environment.GetEnvironmentVariable(Constants.S3BucketName),
                        Key = key,
                        InputStream = stream,

                    };

                    var putResponse = await s3.PutObjectAsync(putRequest);

                    if (putResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        logger?.LogErrorEx(() => $"Error uploading {fileName} to S3: {putResponse.HttpStatusCode}");
                        throw new Exception($"Error uploading {fileName} to S3: {putResponse.HttpStatusCode}");
                    }
                }
            }
        }
    }
}
