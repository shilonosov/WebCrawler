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
            //Scheduler = new TaskPoolScheduler(new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1)));
            Scheduler = CurrentThreadScheduler.Instance;
        }

        private IObservable<IObservable<Unit>> ParsePage(
            Uri pageUri,
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
                var childLinks = HtmlPageService.ParseHtmlForLinksAsync(pageUri);

                var crawledPage = new CrawledPageModel(pageUri, childLinks, level);
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
                        .ToObservable()
                        .ObserveOn(Scheduler)
                        .Select(
                            x =>
                            {
                                if (booleanDisposable.IsDisposed)
                                {
                                    return Observable.Empty<IObservable<Unit>>();
                                }
                                var r = Observable.Start(
                                    () => ParsePage(x, level + 1, bottomLevel, observable, booleanDisposable),
                                    Scheduler).Merge();
                                return r;
                            })
                            .Merge();
                }
                else
                {
                    Interlocked.Decrement(ref counter);
                    result = Observable.Empty<IObservable<Unit>>();
                }

                if (level == 0)
                {
                    result
                        .Wait().Subscribe(_ =>
                        {
                            Debug.WriteLine("level 0 completed");
                        });
                    observable.OnCompleted();
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
                    observable.OnNext(new CrawledPageModel(pageUri, x, level));
                    Debug.WriteLine(string.Format("{0} promise resolved", x.PageUri.AbsoluteUri));
                });
            }

            Interlocked.Decrement(ref counter);
            return Observable.Empty<IObservable<Unit>>();
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
                Observable.Start(() => ParsePage(startUri, 0, bottomLevel, o, disposable));
                return disposable;
            });
        }
    }
}
