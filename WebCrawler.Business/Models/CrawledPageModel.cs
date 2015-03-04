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

        public IList<CrawledPageModel> Descendents { get; private set; }

        public uint Level { get; private set; }

        public string PageUriString
        {
            get { return PageUri.OriginalString; }
        }

        public int DescendentsCount
        {
            get { return Descendents.Count; }
        }

        public CrawledPageModel(Uri pageUri, IList<CrawledPageModel> descendents, uint level)
        {
            PageUri = pageUri;
            Descendents = descendents;
            Level = level;
        }
    }
}
