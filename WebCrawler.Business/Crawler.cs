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
using System.Reactive.Disposables;
using System.Collections.Concurrent;
using WebCrawler.Business.Services;

namespace WebCrawler.Business
{
    public interface ICrawler
    {
        IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel);
    }

    public class Crawler : ICrawler
    {
        private PromiseRepository<Uri, CrawledPageModel> visitedPromises;
        private IHtmlPageService HtmlPageService { get; set; }

        private async Task ParsePage(
            Uri pageUri,
            uint level,
            uint bottomLevel,
            IObserver<CrawledPageModel> observable,
            BooleanDisposable booleanDisposable)
        {
            if (booleanDisposable.IsDisposed)
            {
                return;
            }

            if (visitedPromises.TryAddPromise(pageUri))
            {
                var childLinks = await HtmlPageService.ParseHtmlForLinksAsync(pageUri);
                var crawledPage = new CrawledPageModel(pageUri, childLinks, level);
                visitedPromises.AddValue(pageUri, crawledPage);
                observable.OnNext(crawledPage);

                if (level >= bottomLevel)
                {
                    return;
                }

                var tasks = new List<Task>();
                foreach (var uri in childLinks)
                {
                    if (booleanDisposable.IsDisposed)
                    {
                        return;
                    }
                    //var task = Task.Factory.StartNew(() => ParsePage(uri, level + 1, bottomLevel, observable, booleanDisposable));
                    //tasks.Add(task);
                    await ParsePage(uri, level + 1, bottomLevel, observable, booleanDisposable);
                }

                //Task.WaitAll(tasks.ToArray());
                if (level == 0)
                {
                    observable.OnCompleted();
                }
            }
            else
            {
                visitedPromises.AddHandler(pageUri, x =>
                {
                    var crawledPage = new CrawledPageModel(pageUri, x, level);
                    observable.OnNext(crawledPage);
                });
            }
        }

        public Crawler(IHtmlPageService htmlPageService)
        {
            visitedPromises = new PromiseRepository<Uri, CrawledPageModel>();
            HtmlPageService = htmlPageService;
        }

        public IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel)
        {
            return Observable.Create<CrawledPageModel>(o =>
            {
                var disposable = new BooleanDisposable();
                ParsePage(startUri, 0, bottomLevel, o, disposable)
                    .ToObservable()
                    .Subscribe();
                return disposable;
            });
        }
    }
}
