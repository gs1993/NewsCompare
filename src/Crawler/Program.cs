using Abot2;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AbotX2.Crawler;
using AbotX2.Parallel;
using AbotX2.Poco;
using Crawler.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Demo starting up!");

            //await DemoSimpleCrawler();
            await DemoSinglePageRequest();

            Console.ReadKey();
        }

        private static async Task DemoSimpleCrawler()
        {
            var config = new CrawlConfiguration
            {
                MaxPagesToCrawl = 10, //Only crawl 10 pages
                MinCrawlDelayPerDomainMilliSeconds = 3000 //Wait this many millisecs between requests
            };
            var crawler = new PoliteWebCrawler(config);

            crawler.PageCrawlCompleted += PageCrawlCompleted;//Several events available...

            var crawlResult = await crawler.CrawlAsync(new Uri("https://www.onet.pl"));
        }

        private static async Task DemoSinglePageRequest()
        {
            var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            var crawledPage = await pageRequester.MakeRequestAsync(new Uri("https://www.onet.pl"));

            var articleTitles = crawledPage.AngleSharpHtmlDocument.All
                .Where(x => x.LocalName == "span" && x.ClassName == "title" && !string.IsNullOrWhiteSpace(x.TextContent))
                .Select(x => x.TextContent)
                .ToList();

            Log.Logger.Information("{result}", new
            {
                url = crawledPage.Uri,
                status = Convert.ToInt32(crawledPage.HttpResponseMessage.StatusCode),
                rawResponse = crawledPage.AngleSharpHtmlDocument.QuerySelectorAll("article")
            });
        }

        private static void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var httpStatus = e.CrawledPage.HttpResponseMessage.StatusCode;
            var rawPageText = e.CrawledPage.Content.Text;
        }





        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
