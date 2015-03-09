using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Business.Services
{
    public interface IHtmlPageService
    {
        Task<IEnumerable<Uri>> ParseHtmlForLinksAsync(Uri pageUri);
    }

    public class HtmlPageService : IHtmlPageService
    {
        private const string DoubleSlash = "//";

        private IEnumerable<Uri> ComposeUriList(CQ dom, Uri pageUri)
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

        public async Task<IEnumerable<Uri>> ParseHtmlForLinksAsync(Uri pageUri)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var html = await httpClient.GetStringAsync(pageUri);
                    var dom = CQ.CreateDocument(html);
                    var links = dom["a"];
                    return ComposeUriList(links, pageUri);
                }
                catch
                {
                    //TODO: add logging?
                    return new Uri[0];
                }
            }
        }
    }
}
