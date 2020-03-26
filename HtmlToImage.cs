using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Text;

namespace HtmlToImageDockerCsharp
{
   class PostHtmlToImageDTO
    {
        public string HtmlBase64 { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public static class HtmlToImage
    {
        [FunctionName("HtmlToImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "htmlToImage")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonConvert.DeserializeObject<PostHtmlToImageDTO>(requestBody);

//#if true == DEBUG
//            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
//#endif

            ViewPortOptions defaultViewport = null;
            if (dto.Width.HasValue && dto.Height.HasValue)
                defaultViewport = new ViewPortOptions
                {
                    Width = dto.Width.Value,
                    Height = dto.Height.Value
                };

            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" },
                DefaultViewport = defaultViewport
            }))
            using (var page = await browser.NewPageAsync())
            {
                var buff = Convert.FromBase64String(dto.HtmlBase64);
                var text = UTF8Encoding.UTF8.GetString(buff);

                await page.SetContentAsync(text);
                var image = await page.ScreenshotDataAsync();

                return new FileContentResult(image, "image/png");
            }
        }
    }
}
