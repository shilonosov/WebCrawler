using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebCrowler
{
    public partial class Form1 : Form
    {
        private ISubject<Uri> searchButtonObservable;
        public Form1()
        {
            InitializeComponent();

            searchButtonObservable = new Subject<Uri>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Uri searchUri;
            if (Uri.TryCreate(textBox1.Text, UriKind.Absolute, out searchUri))
            {
                searchButtonObservable.OnNext(searchUri);
            }
        }
    }
}
