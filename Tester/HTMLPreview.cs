using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tester
{
    public partial class HTMLPreview : Form
    {
        public HTMLPreview(string file)
        {
            InitializeComponent();
            webBrowser1.DocumentText = file;
            webBrowser2.DocumentText = file;
            webBrowser3.DocumentText = file;
        }
    }
}
