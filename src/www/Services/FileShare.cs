using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodeRunner
{
    [Service]
    public class FileShare
    {
        public FileShare()
        {
        }
        /// <summary>
        /// Handles an HTTP GET request to retrieve a file from the server.
        /// </summary>
        /// <param name="ctx">The HTTP content context for the current request. Provides access to request data and headers.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Route("/file", "GET")]
        public async Task GetFile(HttpContext ctx)
        {
            var filePath = ctx.Request.Query["path"].ToString();
            if (Config.GetInstance().EnableLogging)
            {
                Console.WriteLine($"[FileShare] GET request for file: {filePath}");
            }
        }
    }
}
