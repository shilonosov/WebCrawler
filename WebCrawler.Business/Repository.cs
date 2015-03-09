using System;
using System.Collections.Generic;
using System.Threading;
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

    public abstract class ThreadSafeWrapper
    {
        private readonly object synchronizationContext;

        protected ThreadSafeWrapper()
        {
            synchronizationContext = new object();
        }

        protected TResult DoInLock<TResult>(Func<TResult> lockedFunction)
        {
            var lockTaken = false;
            Monitor.Enter(synchronizationContext, ref lockTaken);
            try
            {
                return lockedFunction();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(synchronizationContext);
                }
            }
        }

        protected void DoInLock(Action lockedFunction)
        {
            var lockTaken = false;
            Monitor.Enter(synchronizationContext, ref lockTaken);
            try
            {
                lockedFunction();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(synchronizationContext);
                }
            }
        }
    }

    public class ThreadSafeCollection<T, V> : ThreadSafeWrapper, IRepository<V> where T : ICollection<V>, new()
    {
        protected T Collection { get; private set; }

        public ThreadSafeCollection()
        {
            Collection = new T();
        }

        public bool Add(V entity)
        {
            return DoInLock(() =>
            {
                if (!Collection.Contains(entity))
                {
                    Collection.Add(entity);
                    return true;
                }
                return false;
            });
        }

        public bool Contains(V entity)
        {
            return DoInLock(() => Collection.Contains(entity));
        }

        public V Get(Func<T, V> getSelector)
        {
            return DoInLock(() => getSelector(Collection));
        }
    }

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