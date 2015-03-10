using System;
using System.Threading;

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
}