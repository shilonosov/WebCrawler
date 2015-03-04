using System;
using System.Collections.Generic;
using System.Threading;

namespace WebCrawler.Business
{
    public class Repository<T> : IRepository where T : ICollection<string>, new()
    {
        private readonly object synchronizationContext;

        private T Collection { get; set; }

        private TResult DoInLock<TResult>(Func<TResult> lockedFunction)
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

        public Repository()
        {
            synchronizationContext = new object();
            Collection = new T();
        }

        public bool Add(string entity)
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

        public bool Contains(string entity)
        {
            return DoInLock(() => Collection.Contains(entity));
        }
    }
}