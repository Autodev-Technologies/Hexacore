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
            DescripLabel.Text = "In computing, formatted text, styled text, or rich text, as opposed to plain text, is digital text which has styling information beyond the minimum of semantic elements: colours, styles, sizes, and special features in HTML. a Blank text format";
            fastColoredTextBox1.Text = "Write whatever you want!";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "C# File";
            DescripLabel.Text = "C# is a general-purpose high-level programming language supporting multiple paradigms. C# encompasses static typing, strong typing, lexically scoped, imperative, declarative, functional, generic, object-oriented, and component-oriented programming disciplines. An empty class definition";
            fastColoredTextBox1.Text = "using System;\r\n\r\nnamespace HelloWorld\r\n{\r\n  class Program\r\n  {\r\n    static void Main(string[] args)\r\n    {\r\n      Console.WriteLine(\"Hello World!\");    \r\n    }\r\n  }\r\n}";
            fastColoredTextBox1.Language = Language.CSharp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "HTML File";
            DescripLabel.Text = "The HyperText Markup Language or HTML is the standard markup language for documents designed to be displayed in a web browser. An HTML Page that can include client-side code.";
            fastColoredTextBox1.Text = "<!DOCTYPE html>\r\n<html>\r\n<body>\r\n\r\n<h1>My First Heading</h1>\r\n<p>My first paragraph.</p>\r\n\r\n</body>\r\n</html>";
            fastColoredTextBox1.Language = Language.HTML;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "JS File";
            DescripLabel.Text = "JavaScript, often abbreviated as JS, is a programming language that is one of the core technologies of the World Wide Web, alongside HTML and CSS. A script file containing JavaScript code.";
            fastColoredTextBox1.Text = "<!DOCTYPE html>\r\n<html>\r\n<body>\r\n\r\n<h2>My First JavaScript</h2>\r\n\r\n<button type=\"button\"\r\nonclick=\"document.getElementById('demo').innerHTML = Date()\">\r\nClick me to display Date and Time.</button>\r\n\r\n<p id=\"demo\"></p>\r\n\r\n</body>\r\n</html> \r\n";
            fastColoredTextBox1.Language = Language.JS;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "JSON File";
            DescripLabel.Text = "JSON stands for JavaScript Object Notation, JSON is a text format for storing and transporting data, JSON is self-describing and easy to understand. a Blank JSON file";
            fastColoredTextBox1.Text = "{\"employees\":[\r\n  { \"firstName\":\"John\", \"lastName\":\"Doe\" },\r\n  { \"firstName\":\"Anna\", \"lastName\":\"Smith\" },\r\n  { \"firstName\":\"Peter\", \"lastName\":\"Jones\" }\r\n]}";
            fastColoredTextBox1.Language = Language.JSON;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Lua File";
            DescripLabel.Text = "Lua is a lightweight, high-level, multi-paradigm programming language designed primarily for embedded use in applications. a Blank Lua file";
            fastColoredTextBox1.Text = "    -- defines a factorial function\r\n    function fact (n)\r\n      if n == 0 then\r\n        return 1\r\n      else\r\n        return n * fact(n-1)\r\n      end\r\n    end\r\n    \r\n    print(\"enter a number:\")\r\n    a = io.read(\"*number\")        -- read a number\r\n    print(fact(a))";
            fastColoredTextBox1.Language = Language.Lua;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "PHP File";
            DescripLabel.Text = "PHP is a general-purpose scripting language geared towards web development. It was originally created by Danish-Canadian programmer Rasmus Lerdorf in 1993 and released in 1995. a Blank PHP file";
            fastColoredTextBox1.Text = "<!DOCTYPE html>\r\n<html>\r\n<body>\r\n\r\n<h1>My first PHP page</h1>\r\n\r\n<?php\r\necho \"Hello World!\";\r\n?>\r\n\r\n</body>\r\n</html>";
            fastColoredTextBox1.Language = Language.PHP;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "SQL File";
            DescripLabel.Text = "Structured Query Language is a domain-specific language used in programming and designed for managing data held in a relational database management system, or for stream processing in a relational data stream management system. a Blank SQL file";
            fastColoredTextBox1.Text = "No Example";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Visual Basic File";
            DescripLabel.Text = "Visual Basic, originally called Visual Basic .NET, is a multi-paradigm, object-oriented programming language, implemented on .NET, Mono, and the .NET Framework. An empty class definition";
            fastColoredTextBox1.Text = "imports System\r\n\r\nModule Program\r\n           Sub Main(args As String())\r\n                   Console.WriteLine(\"Hello World\");\r\n           End Sub\r\nEnd Module";
            fastColoredTextBox1.Language = Language.VB;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "XML File";
            DescripLabel.Text = "Extensible Markup Language is a markup language and file format for storing, transmitting, and reconstructing arbitrary data. a Blank XML file";
            fastColoredTextBox1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<note>\r\n  <to>Tove</to>\r\n  <from>Jani</from>\r\n  <heading>Reminder</heading>\r\n  <body>Don't forget me this weekend!</body>\r\n</note>";
            fastColoredTextBox1.Language = Language.XML;
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
            else if (HeaderLabel.Text == "(.cpp) C++ File")
            {
                Editor.instance.SelcelLang.Text = "Lang:C++CPP";
            }
            else if (HeaderLabel.Text == "(.h) C++ File")
            {
                Editor.instance.SelcelLang.Text = "Lang:C++H";
            }
            else if (HeaderLabel.Text == "C File")
            {
                Editor.instance.SelcelLang.Text = "Lang:C";
            }
            else if (HeaderLabel.Text == "Java File")
            {
                Editor.instance.SelcelLang.Text = "Lang:Java";
            }
            else if (HeaderLabel.Text == "Python File")
            {
                Editor.instance.SelcelLang.Text = "Lang:Python";
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

        private void button13_Click(object sender, EventArgs e)
        {
            button2.Visible = true;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = true;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = true;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = true;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = true;
            button4.Visible = true;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = true;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button25_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = true;
            button21.Visible = true;
            button27.Visible = false;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
            button27.Visible = true;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = true;
            button20.Visible = false;
            button21.Visible = false;
            button27.Visible = false;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = true;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button26_Click(object sender, EventArgs e)
        {
            button2.Visible = false;
            button1.Visible = true;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button20.Visible = false;
            button21.Visible = false;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Python File";
            DescripLabel.Text = "Python is a high-level, general-purpose programming language. Its design philosophy emphasizes code readability with the use of significant indentation.";
            fastColoredTextBox1.Text = "message = \"Hello, world!\"\r\nprint(message)";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "Java File";
            DescripLabel.Text = "Java is a set of computer software and specifications that provides a software platform for developing application software and deploying it in a cross-platform computing environment.";
            fastColoredTextBox1.Text = "public class HelloWorld {\r\n    public static void main(String[] args) {\r\n        System.out.println(\"Hello, world!\");\r\n    }\r\n}";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "(.cpp) C++ File";
            DescripLabel.Text = "C++ is a high-level, general-purpose programming language created by Danish computer scientist Bjarne Stroustrup. filename extension that may apply to: Files containing C Preprocessor directives. Files containing C++ code.";
            fastColoredTextBox1.Text = "#include <iostream>\r\n\r\nint main() {\r\n  std::cout << \"Hello, world!\" << std::endl;\r\n  return 0;\r\n}";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "(.h) C++ File";
            DescripLabel.Text = "h is the extension of the header file. Unlike C, C++ header files may or may not have . h extension. It must enclose the name of the header file in the angle brackets, and #include is the preprocessor directive.";
            fastColoredTextBox1.Text = "#ifndef MY_HEADER_H\r\n#define MY_HEADER_H\r\n\r\nstruct Point {\r\n  int x;\r\n  int y;\r\n};\r\n\r\ndouble distance(Point p1, Point p2);\r\n\r\n#define MAX_SIZE 100\r\n\r\n#endif";
            fastColoredTextBox1.Language = Language.Custom;
        }

        private void button27_Click(object sender, EventArgs e)
        {
            HeaderLabel.Text = "C File";
            DescripLabel.Text = "C is a general-purpose computer programming language. It was created in the 1970s by Dennis Ritchie, and remains very widely used and influential. By design, C's features cleanly reflect the capabilities of the targeted CPUs.";
            fastColoredTextBox1.Text = "#include <stdio.h>\r\n\r\nint main() {\r\n  printf(\"Hello, world!\\n\");\r\n  return 0;\r\n}";
            fastColoredTextBox1.Language = Language.Custom;
        }
    }
}
