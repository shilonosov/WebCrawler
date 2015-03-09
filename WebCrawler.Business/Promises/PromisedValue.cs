using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Business.Promises
{
    public class PromisedValue<T>
    {
        private ISubject<T> subject;

        public T Value { get; private set; }

        public bool HasValue { get; private set; }

        public PromisedValue()
        {
            subject = new Subject<T>();
            HasValue = false;
        }

        public void SetValue(T value)
        {
            subject.OnNext(value);
            subject.OnCompleted();

            Value = value;
            HasValue = true;
        }

        // TODO: use IScheduler
        public IDisposable Subscribe(Action<T> action)
        {
            if (HasValue)
            {
                return Disposable.Empty;
            }
            else
            {
                return subject.Subscribe(action);
            }
        }
    }
}
