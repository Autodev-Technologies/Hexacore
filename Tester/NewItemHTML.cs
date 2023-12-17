using FastColoredTextBoxNS;
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
    public partial class NewItemHTML : Form
    {
        public NewItemHTML()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HeaderLabel2.Text = "HTML File";
            DescripLabel2.Text = "The HyperText Markup Language or HTML is the standard markup language for documents designed to be displayed in a web browser. An HTML Page that can include client-side code.";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HeaderLabel2.Text = "HTM File";
            DescripLabel2.Text = "The HyperText Markup Language or HTML is the standard markup language for documents designed to be displayed in a web browser. An HTML Page that can include client-side code.";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            HeaderLabel2.Text = "JS File";
            DescripLabel2.Text = "JavaScript, often abbreviated as JS, is a programming language that is one of the core technologies of the World Wide Web, alongside HTML and CSS. A script file containing JavaScript code.";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HeaderLabel2.Text = "PHP File";
            DescripLabel2.Text = "PHP is a general-purpose scripting language geared towards web development. It was originally created by Danish-Canadian programmer Rasmus Lerdorf in 1993 and released in 1995. a Blank PHP file";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (HeaderLabel2.Text == "HTML File")
            {
                HTMLEditor.HTMLinstance.HTMLSelcelLang.Text = "Lang:HTML";
                this.Hide();
            }
            else if (HeaderLabel2.Text == "HTM File")
            {
                HTMLEditor.HTMLinstance.HTMLSelcelLang.Text = "Lang:HTM";
                this.Hide();
            }
            else if (HeaderLabel2.Text == "JS File")
            {
                HTMLEditor.HTMLinstance.HTMLSelcelLang.Text = "Lang:JS";
                this.Hide();
            }
            else if (HeaderLabel2.Text == "PHP File")
            {
                HTMLEditor.HTMLinstance.HTMLSelcelLang.Text = "Lang:PHP";
                this.Hide();
            }
            else if (HeaderLabel2.Text == "CSS File")
            {
                HTMLEditor.HTMLinstance.HTMLSelcelLang.Text = "Lang:CSS";
                this.Hide();
            }
            else
            {
                this.Hide();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            HeaderLabel2.Text = "CSS File";
            DescripLabel2.Text = "Cascading Style Sheets is a style sheet language used for describing the presentation of a document written in a markup language such as HTML or XML. CSS is a cornerstone technology of the World Wide Web, alongside HTML and JavaScript. a Blank CSS file";
        }
    }
}
