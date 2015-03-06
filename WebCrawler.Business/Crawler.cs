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

using Autofac.Util;

using WebCrawler.Business.Models;

using Disposable = System.Reactive.Disposables.Disposable;

namespace WebCrawler.Business
{
    public interface ICrawler
    {
        IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel);
    }

    public class Crawler : ICrawler
    {
        private async Task ParsePage(CrawledPageModel parent, IObserver<CrawledPageModel> subject)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(4000);
                subject.OnNext(null);
            });
        }

        public IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel)
        {
            var rootPage = new CrawledPageModel(startUri, new List<CrawledPageModel>(), 0);
            return Observable.Create<CrawledPageModel>(o =>
            {
                ParsePage(rootPage, o);
                return Disposable.Empty;
            });
        }
    }
}
