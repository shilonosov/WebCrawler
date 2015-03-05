using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCrawler.Business;
using WebCrawler.Business.Models;
using WebCrawler.Ioc;

namespace WebCrowler
{
    public partial class Form1 : Form
    {
        private ISubject<Uri> searchButtonObservable;
        private ICrawler crawler;
        private Control[] uiControls;

        private void SetControlsEnabled(bool state, params Control[] controls)
        {
            foreach(var control in controls)
            {
                control.Enabled = state;
            }
        }

        public Form1()
        {
            InitializeComponent();

            crawler = Ioc.Resolve<ICrawler>();
            searchButtonObservable = new Subject<Uri>();
            uiControls = new Control[] { button1, textBox1 };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Uri searchUri;
            if (Uri.TryCreate(textBox1.Text, UriKind.Absolute, out searchUri))
            {
                UiLock();
                crawler
                    .Crawl(searchUri, 2, new ControlScheduler(this))
                    .Subscribe(x => CrawlingCompeted(x));
            }
        }

        private void UiLock()
        {
            SetControlsEnabled(false, uiControls);
        }

        private void CrawlingCompeted(CrawledPageModel model)
        {
            MessageBox.Show("completed!");
            UiUnlock();
        }

        private void UiUnlock()
        {
            SetControlsEnabled(true, uiControls);
        }
    }
}
