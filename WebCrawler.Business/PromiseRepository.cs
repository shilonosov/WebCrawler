using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;
using WebCrawler.Business.Promises;

namespace WebCrawler.Business
{
    // TODO: move classes to separate files

    public class PromiseRepository<TKey, TValue> : ThreadSafeWrapper
    {
        private IDictionary<TKey, PromisedValue<TValue>> promises;

        public PromiseRepository() : base()
        {
            promises = new Dictionary<TKey, PromisedValue<TValue>>();
        }

        public bool TryAddPromise(TKey key)
        {
            return DoInLock(() =>
            {
                if (promises.ContainsKey(key))
                {
                    return false;
                }
                promises.Add(key, new PromisedValue<TValue>());
                return true;
            });
        }

        public void AddValue(TKey key, TValue value)
        {
            DoInLock(() =>
            {
                var promise = promises[key];
                promise.SetValue(value);
            });
        }

        public bool AddHandler(TKey key, Action<TValue> handlerSelector)
        {
            var disposable = DoInLock(() =>
            {
                if (!promises.ContainsKey(key))
                {
                    return null;
                }
                else
                {
                    var promise = promises[key];
                    return promise.Subscribe(handlerSelector);
                }
            });

            if (object.Equals(disposable, Disposable.Empty))
            {
                var promise = DoInLock(() => promises[key]);
                handlerSelector(promise.Value);
                return true;
            }

            return disposable != null;
        }
    }
}