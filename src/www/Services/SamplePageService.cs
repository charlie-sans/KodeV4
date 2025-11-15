using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace KodeRunner.Services
{
    [Service]
    internal class SamplePageService
    {
        [RazorRoute("/sample/custom", "Sample/Custom")]
        public object GetSamplePage(HttpContext ctx)
        {
            // Return a simple model object that the Razor page can consume.
            return new Models.SampleViewModel { Message = "Hello from RazorRoute", Id = 777 };
        }

        [RazorRoute("/sample/custom-async", "Sample/Custom")]
        public async Task<object> GetSamplePageAsync(HttpContext ctx)
        {
            await Task.Delay(1); // simulate async
            return new Models.SampleViewModel { Message = "Hello from async RazorRoute", Id = 999 };
        }
    }
}