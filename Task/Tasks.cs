using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Task
{
    public static class Tasks
    {
        /// <summary>
        /// Returns the content of required uri's.
        /// Method has to use the synchronous way and can be used to compare the
        ///  performace of sync/async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris)
        {
            List<string> content = new List<string>();
            using (WebClient client = new WebClient())
            {
                foreach (var uri in uris)
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    content.Add(client.DownloadString(uri));
                    Debug.WriteLine(uri + " " + timer.Elapsed);
                }
            }
            return content;
        }

        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace 
        /// of sync \ async approaches. 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams 
        /// that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            HttpClient client = new HttpClient();

            var urisList = uris.ToList();
            var result = new string[urisList.Count];
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = maxConcurrentStreams };
            Parallel.For(0, urisList.Count, i =>
            {
                result[i] = client.GetStringAsync(urisList[i]).Result;
                Debug.WriteLine($"Number {i}: {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread}");

            });
            return result;
        }


        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            using (WebClient client = new WebClient())
            {
                var resourceData = await client.DownloadDataTaskAsync(resource);

                return await GetMD5Async(resourceData);
            }
        }

        private static Task<string> GetMD5Async(byte[] data)
        {
            Task<string> getHash = System.Threading.Tasks.Task.Run(() =>
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(data);
                    StringBuilder sBuilder = new StringBuilder();
                    foreach (byte b in hash)
                    {
                        sBuilder.Append(b.ToString("x2"));
                    }
                    return sBuilder.ToString();
                }
            });

            return getHash;
        }
    }
}
