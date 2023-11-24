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
    public partial class MixedProject : Form
    {
        public static MixedProject instance;

        public MixedProject()
        {
            InitializeComponent();
            instance = this;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "CSharp")
            {
                Editor.instance.ProjLangua.Text = "ProjLang:C#";
            }
            if (comboBox1.Text == "Lua")
            {
                Editor.instance.ProjLangua.Text = "ProjLang:Lua";
            }
            if (comboBox1.Text == "Web page")
            {
                Editor.instance.ProjLangua.Text = "ProjLang:WebPage";
            }
            if (comboBox1.Text == "PHP")
            {
                Editor.instance.ProjLangua.Text = "ProjLang:PHP";
            }
            if (comboBox1.Text == "Visual Basic")
            {
                Editor.instance.ProjLangua.Text = "ProjLang:VB";
            }

            this.Hide();
        }

        private void MixedProject_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}
