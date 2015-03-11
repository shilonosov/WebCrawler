using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Business.Models
{
    public class CrawledPageModel
    {
        public Uri ParentUri { get; private set; }

        public Uri PageUri { get; private set; }

        public IList<Uri> Descendents { get; private set; }

        public uint Level { get; private set; }

        public string PageUriString
        {
            get { return PageUri.OriginalString; }
        }

        public CrawledPageModel(Uri pageUri, Uri parentUri, uint level, IEnumerable<Uri> descendents)
        {
            PageUri = pageUri;
            ParentUri = parentUri;
            Level = level;
            Descendents = new List<Uri>(descendents);
        }

        public CrawledPageModel(Uri pageUri, Uri parentUri, CrawledPageModel crawledPage, uint level) : this(pageUri, parentUri, level, crawledPage.Descendents)
        {
        }
    }
}
