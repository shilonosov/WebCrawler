using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using WebCrawler.Business.Extensions.Parallel;
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
        private readonly PromiseRepository<Uri, CrawledPageModel> visitedPromises;
        private IHtmlPageService HtmlPageService { get; set; }

        private static IScheduler Scheduler { get; set; }
        private static int counter = 0;

        static Crawler()
        {
            var taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
            Scheduler = new TaskPoolScheduler(taskFactory);
        }

        private async Task<IObservable<IObservable<Unit>>> ParsePage(
            Uri pageUri,
            Uri parentUri,
            uint level,
            uint bottomLevel,
            IObserver<CrawledPageModel> observable,
            ICancelable booleanDisposable)
        {
            Interlocked.Increment(ref counter);
            Debug.WriteLine("{0} | {1}", counter, pageUri.AbsoluteUri);

            if (booleanDisposable.IsDisposed)
            {
                Interlocked.Decrement(ref counter);
                return Observable.Empty<IObservable<Unit>>();
            }

            if (visitedPromises.TryAddPromise(pageUri))
            {
                var childLinks = await HtmlPageService.ParseHtmlForLinksAsync(pageUri);

                var crawledPage = new CrawledPageModel(pageUri, parentUri, level, childLinks);
                visitedPromises.AddValue(pageUri, crawledPage);
                observable.OnNext(crawledPage);

                if (level >= bottomLevel)
                {
                    Interlocked.Decrement(ref counter);
                    return Observable.Empty<IObservable<Unit>>();
                }

                IObservable<IObservable<Unit>> result;
                if (childLinks.Any())
                {
                    result = childLinks
                        .ToObservable(Scheduler)
                        .Select(x => ComposeRecursiveCrawlObservable(pageUri, level, bottomLevel, observable, booleanDisposable, x))
                        .Merge();
                }
                else
                {
                    //Interlocked.Decrement(ref counter);
                    result = Observable.Empty<IObservable<Unit>>();
                }

                if (level == 0)
                {
                    result.Subscribe(_ => { }, () =>
                    {
                        Debug.WriteLine("level 0 completed");
                        observable.OnCompleted();
                    });
                }
                else
                {
                    Interlocked.Decrement(ref counter);
                    return result;
                }
            }
            else
            {
                Debug.WriteLine(string.Format("{0} is promised, skipped", pageUri.AbsoluteUri));
                visitedPromises.AddHandler(pageUri, x =>
                {
                    observable.OnNext(new CrawledPageModel(pageUri, parentUri, x, level));
                    Debug.WriteLine(string.Format("{0} promise resolved", x.PageUri.AbsoluteUri));
                });
            }

            Interlocked.Decrement(ref counter);
            return Observable.Empty<IObservable<Unit>>();
        }

        private IObservable<IObservable<Unit>> ComposeRecursiveCrawlObservable(Uri parentUri, uint level, uint bottomLevel, IObserver<CrawledPageModel> observable, ICancelable booleanDisposable, Uri x)
        {
            if (booleanDisposable.IsDisposed)
            {
                return Observable.Empty<IObservable<Unit>>();
            }

            return Observable
                .Start(
                    () => ParsePage(x, parentUri, level + 1, bottomLevel, observable, booleanDisposable).ToObservable().Merge(),
                    Scheduler)
                .Merge();
        }

        public Crawler(IHtmlPageService htmlPageService)
        {
            visitedPromises = new PromiseRepository<Uri, CrawledPageModel>();
            HtmlPageService = htmlPageService;
        }

        public IObservable<CrawledPageModel> Crawl(Uri startUri, uint bottomLevel)
        {
            visitedPromises.Clear();
            return Observable.Create<CrawledPageModel>(o =>
            {
                var disposable = new BooleanDisposable();
                Observable.Start(() => ParsePage(startUri, null, 0, bottomLevel, o, disposable), Scheduler);
                return disposable;
            });
        }
    }
}
