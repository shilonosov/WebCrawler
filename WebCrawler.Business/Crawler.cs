using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Business.Models;

namespace WebCrawler.Business
{
    public interface ICrawler
    {
        IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel, IScheduler scheduler);
    }

    public class Crawler : ICrawler
    {
        private async Task<CrawledPageModel> ParsePage(CrawledPageModel parent, ISubject<CrawledPageModel> subject)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(4000);
                subject.OnNext(null);
            });
            return new CrawledPageModel(null, null, 0);
        }

        public IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel, IScheduler scheduler)
        {
            var rootPage = new CrawledPageModel(startUri, new List<CrawledPageModel>(), 0);
            var subject = new Subject<CrawledPageModel>();
            ParsePage(rootPage, subject)
                .ToObservable()
                .Publish()
                .RefCount()
                .ObserveOn(scheduler);
            return subject;
        }
    }
}
