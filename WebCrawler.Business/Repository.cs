using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace WebCrawler.Business
{
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

    public class ThreadSafeDictionary<TKey, TValue> : ThreadSafeCollection<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {
        public TValue GetValue(TKey key)
        {
            return DoInLock(() => Collection[key]);
        }

        public bool ContainsKey(TKey key)
        {
            return DoInLock(() => Collection.ContainsKey(key));
        }
    }

    public class PromiseRepository<TKey, TValue> : ThreadSafeWrapper
    {
        private ISet<TKey> promises;
        private IDictionary<TKey, IList<Action<TValue>>> handlers;
        private IDictionary<TKey, TValue> values;

        private void InternalAddHandler(TKey key, Action<TValue> handlerSelector)
        {
            if (handlers.ContainsKey(key))
            {
                var list = handlers[key];
                list.Add(handlerSelector);
            }
            else
            {
                var list = new List<Action<TValue>>();
                list.Add(handlerSelector);
                handlers.Add(key, list);
            }
        }

        private async Task NotifySubscribersAsync(TValue value, IList<Action<TValue>> handlersCollection)
        {
            foreach (var handler in handlersCollection)
            {
                await Task.Run(() =>
                {
                    var h = handler;
                    h(value);
                });
            }
        }

        public PromiseRepository()
            : base()
        {
            promises = new HashSet<TKey>();
            handlers = new Dictionary<TKey, IList<Action<TValue>>>();
            values = new Dictionary<TKey, TValue>();
        }

        public bool TryAddPromise(TKey key)
        {
            return DoInLock(() =>
            {
                if (promises.Contains(key) || values.ContainsKey(key))
                {
                    return false;
                }
                promises.Add(key);
                return true;
            });
        }

        public void AddValue(TKey key, TValue value)
        {
            DoInLock(() =>
            {
                promises.Remove(key);
                values.Add(key, value);
                if (handlers.ContainsKey(key))
                {
                    var handlersCollection = handlers[key];
                    NotifySubscribersAsync(value, handlersCollection)
                        .ToObservable()
                        .Subscribe();
                }
            });
        }

        public bool AddHandler(TKey key, Action<TValue> handlerSelector)
        {
            return DoInLock(() =>
            {
                if (promises.Contains(key) || values.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    InternalAddHandler(key, handlerSelector);
                    return true;
                }
            });
        }
    }
}