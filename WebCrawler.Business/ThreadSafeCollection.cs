using System;
using System.Collections.Generic;

namespace WebCrawler.Business
{
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
}