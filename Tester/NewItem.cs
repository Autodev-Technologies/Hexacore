using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tester
{
    public partial class NewItem : Form
    {
        public static NewItem instance;

        public NewItem()
        {
            InitializeComponent();
            instance = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Text File";
            DescripLabel.Text = "a Blank text format";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "C# File";
            DescripLabel.Text = "An empty class definition";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "HTML File";
            DescripLabel.Text = "An HTML Page that can include client-side code.";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "JS File";
            DescripLabel.Text = "A script file containing JavaScript code.";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "JSON File";
            DescripLabel.Text = "a Blank JSON file";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Lua File";
            DescripLabel.Text = "a Blank Lua file";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "PHP File";
            DescripLabel.Text = "a Blank PHP file";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "SQL File";
            DescripLabel.Text = "a Blank SQL file";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Visual Basic File";
            DescripLabel.Text = "An empty class definition";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "XML File";
            DescripLabel.Text = "a Blank XML file";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (HeaderLabel.Text == "Text File")
            {
                Editor.instance.SelcelLang.Text = "Lang:Custom";
            }
            else if (HeaderLabel.Text == "C# File")
            {
                Editor.instance.SelcelLang.Text = "Lang:C#";
            }
            else if (HeaderLabel.Text == "HTML File")
            {
                Editor.instance.SelcelLang.Text = "Lang:HTML";
            }
            else if (HeaderLabel.Text == "JS File")
            {
                Editor.instance.SelcelLang.Text = "Lang:JS";
            }
            else if (HeaderLabel.Text == "JSON File")
            {
                Editor.instance.SelcelLang.Text = "Lang:JSON";
            }
            else if (HeaderLabel.Text == "Lua File")
            {
                Editor.instance.SelcelLang.Text = "Lang:Lua";
            }
            else if (HeaderLabel.Text == "PHP File")
            {
                Editor.instance.SelcelLang.Text = "Lang:PHP";
            }
            else if (HeaderLabel.Text == "SQL File")
            {
                Editor.instance.SelcelLang.Text = "Lang:SQL";
            }
            else if (HeaderLabel.Text == "Visual Basic File")
            {
                Editor.instance.SelcelLang.Text = "Lang:VB";
            }
            else if (HeaderLabel.Text == "XML File")
            {
                Editor.instance.SelcelLang.Text = "Lang:XML";
            }
            else
            {
                Editor.instance.SelcelLang.Text = "N/A";
            }

            HeaderLabel.Text = "N/A";
            DescripLabel.Text = "N/A";

            this.Hide();
        }
    }
}
