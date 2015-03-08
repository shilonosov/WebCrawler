using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Business
{
    public interface IRepository<T>
    {
        bool Add(T entity);

        bool Contains(T entity);
    }
}
