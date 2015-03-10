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
    public interface IHtmlPageService
    {
        IList<Uri> ParseHtmlForLinksAsync(Uri pageUri);
    }

    public class HtmlPageService : ThreadSafeWrapper, IHtmlPageService
    {
        private const string DoubleSlash = "//";

        public IList<Uri> ParseHtmlForLinksAsync(Uri pageUri)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    Debug.WriteLine(string.Format("starting {0}", pageUri.AbsoluteUri));

                    var html = DoInLock(() =>  httpClient.GetStringAsync(pageUri.AbsoluteUri).ToObservable().Wait());
                    Debug.WriteLine("loaded {0} in {1}", pageUri.AbsoluteUri, stopWatch.Elapsed.TotalSeconds);

                    var dom = CQ.CreateDocument(html);
                    var links = dom["a"];
                    var result = ComposeUriList(links, pageUri).ToList();
                    
                    stopWatch.Stop();
                    Debug.WriteLine("loaded+parsed {0} in {1}", pageUri.AbsoluteUri, stopWatch.Elapsed.TotalSeconds);

                    return result;
                }
            }
            catch(Exception e)
            {
                //TODO: add logging?
                Debug.WriteLine("{0} was failed due to: {1}", pageUri.AbsoluteUri, e.Message);
            }
            return new Uri[0].ToList();
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