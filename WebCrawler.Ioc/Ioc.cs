using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;

using WebCrawler.Business;
using WebCrawler.Business.Services;

namespace WebCrawler.Ioc
{
    public static class Ioc
    {
        private static readonly IContainer container;

        static Ioc()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Crawler>().AsImplementedInterfaces().SingleInstance();
            containerBuilder.RegisterType<HtmlPageService>().AsImplementedInterfaces().SingleInstance();

            container = containerBuilder.Build();
        }

        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }
    }
}
