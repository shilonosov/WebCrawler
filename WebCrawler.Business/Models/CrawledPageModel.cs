using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Business.Models
{
    public class CrawledPageModel
    {
        public Uri PageUri { get; private set; }

        public IList<Uri> Descendents { get; private set; }

        public uint Level { get; private set; }

        public string PageUriString
        {
            get { return PageUri.OriginalString; }
        }

        public CrawledPageModel(Uri pageUri, IEnumerable<Uri> descendents, uint level)
        {
            PageUri = pageUri;
            Descendents = new List<Uri>(descendents);
            Level = level;
        }

        public CrawledPageModel(Uri pageUri, CrawledPageModel crawledPage, uint level)
        {
            PageUri = pageUri;
            Descendents = new List<Uri>(crawledPage.Descendents);
            Level = level;
        }
    }
}
