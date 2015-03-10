using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using CsQuery;


namespace WebCrawler.Business.Services
{
    /*
    var r = Observable
        .Range(0, 10)
        .SubscribeOn(ThreadPoolScheduler.Instance)
        .Select(x => DownloadTask(url, x).ToObservable())
        .Merge()
        .ToList()
        .Wait();
    */


    public interface IHtmlPageService
    {
        Task<IList<Uri>> ParseHtmlForLinksAsync(Uri pageUri);
    }

    public class HtmlPageService : ThreadSafeWrapper, IHtmlPageService
    {
        private const string DoubleSlash = "//";

        public async Task<IList<Uri>> ParseHtmlForLinksAsync(Uri pageUri)
        {
            IList<Uri> result;
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                Debug.WriteLine(string.Format("starting {0}", pageUri.AbsoluteUri));

                string html;
                using (var httpClient = new HttpClient())
                {
                    html = await httpClient.GetStringAsync(pageUri.AbsoluteUri);
                }
                Debug.WriteLine("loaded {0} in {1}", pageUri.AbsoluteUri, stopWatch.Elapsed.TotalSeconds);

                var dom = CQ.CreateDocument(html);
                var links = dom["a"];
                result = ComposeUriList(links, pageUri).ToList();

                stopWatch.Stop();
                Debug.WriteLine("loaded+parsed {0} in {1}", pageUri.AbsoluteUri, stopWatch.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                result = new Uri[0];
                //TODO: add logging?
                Debug.WriteLine("{0} was failed due to: {1}", pageUri.AbsoluteUri, e.Message);
            }
            return result;
        }

        private IEnumerable<Uri> ComposeUriList(IEnumerable<IDomObject> dom, Uri pageUri)
        {
            var result = new HashSet<Uri>();
            foreach (var domElement in dom)
            {
                var href = domElement.GetAttribute("href", string.Empty);
                if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                {
                    if (href.StartsWith(DoubleSlash))
                    {
                        href = pageUri.GetLeftPart(UriPartial.Scheme) + href.Substring(DoubleSlash.Length);
                    }
                    else
                    {
                        href = pageUri.GetLeftPart(UriPartial.Authority) + href;
                    }
                }

                Uri uri;
                if (Uri.TryCreate(href, UriKind.Absolute, out uri))
                {
                    if (string.Equals(uri.Scheme, Uri.UriSchemeHttp) || string.Equals(uri.Scheme, Uri.UriSchemeHttps))
                    {
                        result.Add(uri);
                    }
                }
            }
            return result;
        }
    }
}