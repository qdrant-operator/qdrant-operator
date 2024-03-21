using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;

using Neon.Diagnostics;

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
                awsAccessKeyId: Environment.GetEnvironmentVariable("S3_ACCESS_KEY"),
                awsSecretAccessKey: Environment.GetEnvironmentVariable("S3_SECRET_ACCESS_KEY"),
                new AmazonS3Config()
                {
                    Timeout = TimeSpan.FromHours(1),
                    RetryMode = Amazon.Runtime.RequestRetryMode.Standard,
                    MaxErrorRetry = 3,
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("S3_BUCKET_REGION"))
                });

            var snapshotId     = Environment.GetEnvironmentVariable("SNAPSHOT_ID");
            var snapshotName   = Environment.GetEnvironmentVariable("SNAPSHOT_NAME");
            var nodeId         = Environment.GetEnvironmentVariable("QDRANT_NODE_ID");
            var collectionName = Environment.GetEnvironmentVariable("COLLECTION_NAME");

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
                        BucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME"),
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
