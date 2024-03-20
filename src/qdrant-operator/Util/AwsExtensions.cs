using Amazon.Runtime;
using System.Net;

namespace QdrantOperator.Util
{
    /// <summary>
    /// Provides extension methods for AWS-related functionality.
    /// </summary>
    public static class AwsExtensions
    {
        /// <summary>
        /// Ensures that the Amazon web service response is successful.
        /// Throws an exception if the response status code indicates failure.
        /// </summary>
        /// <param name="response">The Amazon web service response.</param>
        public static void EnsureSuccess(this AmazonWebServiceResponse response)
        {
            if (!response.HttpStatusCode.IsSuccessStatusCode())
            {
                throw new System.Net.Http.HttpRequestException($"Request failed with status code {response.HttpStatusCode}");
            }
        }

        /// <summary>
        /// Checks if the HTTP status code is a success status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>True if the status code is a success status code; otherwise, false.</returns>
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            int statusCodeNum = (int)statusCode;
            return statusCodeNum >= 200 && statusCodeNum <= 299;
        }
    }
}
