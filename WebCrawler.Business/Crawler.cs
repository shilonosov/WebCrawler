using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using WebCrawler.Business.Models;

namespace WebCrawler.Business
{
    public interface ICrawler
    {
        IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel);
    }

    public class Crawler : ICrawler
    {
        private IList<CrawledPageModel> ParsePage(CrawledPageModel parent)
        {
            return new List<CrawledPageModel>();
        }

        public IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel)
        {
            var rootPage = new CrawledPageModel(startUri, new List<CrawledPageModel>(), 0);
            return Observable.Generate(
                rootPage,
                x => x.Level <= bottomLevel,
                x => ParsePage(x));
        }
    }
}
