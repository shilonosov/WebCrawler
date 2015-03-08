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
        private ThreadSafeDictionary<Uri, CrawledPageModel> visitedPages;
        private IHtmlPageService HtmlPageService { get; set; }

        private async Task ParsePage(
            Uri pageUri,
            uint level,
            uint bottomLevel,
            IObserver<CrawledPageModel> observable,
            BooleanDisposable booleanDisposable)
        {
            var crawledPage = new CrawledPageModel(pageUri, null, level);

            if (visitedPages.ContainsKey(pageUri))
            {
                var visitedPage = visitedPages.GetValue(pageUri);
                crawledPage.CopyDescendents(visitedPage);
                observable.OnNext(crawledPage);
            }

            var childLinks = await HtmlPageService.ParseHtmlForLinksAsync(pageUri);
            visitedPages.Add(new KeyValuePair<Uri, CrawledPageModel>(pageUri, crawledPage));

            if (booleanDisposable.IsDisposed)
            {
                return;
            }

            observable.OnNext(crawledPage);

            if (level > bottomLevel)
            {
                return;
            }

            foreach (var uri in childLinks)
            {
                if (booleanDisposable.IsDisposed)
                {
                    return;
                }
                await ParsePage(uri, level + 1, bottomLevel, observable, booleanDisposable);
            }
        }

        public Crawler(IHtmlPageService htmlPageService)
        {
            visitedPages = new ThreadSafeDictionary<Uri, CrawledPageModel>();
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
