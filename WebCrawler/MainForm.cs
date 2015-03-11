using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;

using WebCrawler.Business;
using WebCrawler.Business.Models;

namespace WebCrawler
{
    public partial class MainForm : Form
    {
        private readonly ICrawler crawler;
        private readonly Control[] uiControls;
        private readonly ConcurrentDictionary<string, TreeNode> treeNodeSafeDictionary;

        private static void SetControlsEnabled(bool state, params Control[] controls)
        {
            foreach(var control in controls)
            {
                control.Enabled = state;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            crawler = Ioc.Ioc.Resolve<ICrawler>();
            uiControls = new Control[] { buttonStart, buttonClear, textBoxUrl, numericUpDownDeepLevel };
            treeNodeSafeDictionary = new ConcurrentDictionary<string, TreeNode>();
        }

        private void ButtonStartClick(object sender, EventArgs e)
        {
            Uri searchUri;
            if (Uri.TryCreate(textBoxUrl.Text, UriKind.Absolute, out searchUri))
            {
                UiLock();
                crawler
                    .Crawl(searchUri, 2)
                    .ObserveOn(new ControlScheduler(this))
                    .Subscribe(CrawlingNext, CrawlingCompleted);
            }
        }

        private void ButtonClearClick(object sender, EventArgs e)
        {
            treeViewResults.Nodes.Clear();
            treeNodeSafeDictionary.Clear();
        }

        private void UiLock()
        {
            SetControlsEnabled(false, uiControls);
        }

        private static string ComposeTreeNodeKey(Uri uri, uint level)
        {
            return string.Format("{0} - {1}", uri.AbsoluteUri, level);
        }

        private void CrawlingNext(CrawledPageModel model)
        {
            var node = new TreeNode
            {
                Text = string.Format("{0} : {1}", model.Level, model.PageUriString)
            };

            var nodeKey = ComposeTreeNodeKey(model.PageUri, model.Level);

            var parentCollection = treeViewResults.Nodes;
            treeNodeSafeDictionary.TryAdd(nodeKey, node);

            if (model.ParentUri != null)
            {
                TreeNode parentNode;
                var parentKey = ComposeTreeNodeKey(model.ParentUri, model.Level - 1);
                if (treeNodeSafeDictionary.TryGetValue(parentKey, out parentNode))
                {
                    {
                        parentCollection = parentNode.Nodes;
                    }
                }
            }

        parentCollection.Add(node);
        }

        private void CrawlingCompleted()
        {
            UiUnlock();
        }

        private void UiUnlock()
        {
            SetControlsEnabled(true, uiControls);
        }
    }
}
