using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing.Drawing2D;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Windows.Forms.Design;

namespace Tester
{
    public partial class Editor : Form
    {
        // Csharp
        string[] keywords = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", "add", "alias", "ascending", "descending", "dynamic", "from", "get", "global", "group", "into", "join", "let", "orderby", "partial", "remove", "select", "set", "value", "var", "where", "yield" };
        string[] methods = { "Equals()", "GetHashCode()", "GetType()", "ToString()" };
        string[] snippets = { "if(^)\n{\n;\n}", "if(^)\n{\n;\n}\nelse\n{\n;\n}", "for(^;;)\n{\n;\n}", "while(^)\n{\n;\n}", "do\n{\n^;\n}while();", "switch(^)\n{\ncase : break;\n}" };
        string[] declarationSnippets = { 
               "public class ^\n{\n}", "private class ^\n{\n}", "internal class ^\n{\n}",
               "public struct ^\n{\n;\n}", "private struct ^\n{\n;\n}", "internal struct ^\n{\n;\n}",
               "public void ^()\n{\n;\n}", "private void ^()\n{\n;\n}", "internal void ^()\n{\n;\n}", "protected void ^()\n{\n;\n}",
               "public ^{ get; set; }", "private ^{ get; set; }", "internal ^{ get; set; }", "protected ^{ get; set; }"
               };

        // Lua
        string[] Luakeywords = { "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if", "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while" };
        string[] Luamethods = { "assert", "collectgarbage", "dofile", "error", "gefenv", "getmetatable", "ipairs", "load", "loadfile", "loadstring", "module", "next", "pairs", "pcall", "print", "rawequal", "rawget", "require", "select", "setfenv", "setmetatable", "tonumber", "tostring", "type", "unpack", "xpcall" };
        string[] LuadeclarationSnippets = {
               "function ^ ()\n\r\rend", "if ^ then", "elseif ^ then", "print(^)", "for ^ ==  do\n\r\rend", "while not ^ do\n\r\r\rend"
               };

        Style invisibleCharsStyle = new InvisibleCharsRenderer(Pens.Gray);
        Color currentLineColor = Color.FromArgb(100, 210, 210, 255);
        Color changedLineColor = Color.FromArgb(255, 230, 230, 255);


        public static Editor instance;
        public ToolStripMenuItem SelcelLang;
        public ToolStripMenuItem ProjLangua;
        public ToolStripLabel DirectoryPath;
        public Editor()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.ResizeRedraw, true);

            instance = this;
            SelcelLang = langToolStripMenuItem;
            ProjLangua = projLangToolStripMenuItem;
            DirectoryPath = toolStripLabel2;

            //init menu images
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
            copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripButton.Image")));
            cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripButton.Image")));
            pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripButton.Image")));
        }

        int grip = 20;
        int caption = 45;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point p = new Point(m.LParam.ToInt32());
                p = this.PointToClient(p);
                if (p.Y <= caption && p.Y >= grip)
                {
                    m.Result = (IntPtr)2;
                    return;
                }
                if (p.X >= this.ClientSize.Width - grip && p.Y >= this.ClientSize.Height - grip)
                {
                    m.Result = (IntPtr)16;
                    return;
                }
                if (p.X <= grip && p.Y >= this.ClientSize.Height - grip)
                {
                    m.Result = (IntPtr)16;
                }
                if (p.X <= grip)
                {
                    m.Result = (IntPtr)10;
                }
                if (p.X >= ClientSize.Width - grip)
                {
                    m.Result = (IntPtr)11;
                    return;
                }
                if (p.Y <= grip)
                {
                    m.Result = (IntPtr)12;
                    return;
                }
                if (p.Y >= this.ClientSize.Height - grip)
                {
                    m.Result = (IntPtr)15;
                    return;
                }
            }
            base.WndProc(ref m);
        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewItem NewItemPopup = new NewItem();
            NewItemPopup.Show();

            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private Style sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(50, Color.Gray)));

        private void CreateTab(string fileName)
        {
            try
            {
                treeView5.Visible = true;
                var tb = new FastColoredTextBox();
                tb.Font = new Font("Consolas", 9.75f);
                tb.ContextMenuStrip = cmMain;
                tb.Dock = DockStyle.Fill;
                tb.BorderStyle = BorderStyle.None;
                //tb.VirtualSpace = true;
                tb.LeftPadding = 17;
                tb.Language = Language.CSharp;
                tb.AddStyle(sameWordsStyle);//same words style
                var tab = new FATabStripItem(fileName!=null?Path.GetFileName(fileName):"[new]", tb);
                tab.Tag = fileName;
                if (fileName != null)
                    tb.OpenFile(fileName);
                tb.Tag = new TbInfo();
                tsFiles.AddTab(tab);
                tsFiles.SelectedItem = tab;
                tb.Focus();
                tb.DelayedTextChangedInterval = 1000;
                tb.DelayedEventsInterval = 500;
                tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                tb.ChangedLineColor = changedLineColor;
                if(btHighlightCurrentLine.Checked)
                    tb.CurrentLineColor = currentLineColor;
                tb.ShowFoldingLines = btShowFoldingLines.Checked;
                tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                //create autocomplete popup menu
                AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                popupMenu.Items.ImageList = ilAutocomplete;
                popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                BuildAutocompleteMenu(popupMenu);
                (tb.Tag as TbInfo).popupMenu = popupMenu;

                var dm = new DocumentMap();
                dm.Dock = DockStyle.Right;
                dm.Size = new Size(82, 340);
                dm.Target = tb;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

        void popupMenu_Opening(object sender, CancelEventArgs e)
        {
            //---block autocomplete menu for comments
            //get index of green style (used for comments)
            var iGreenStyle = CurrentTB.GetStyleIndex(CurrentTB.SyntaxHighlighter.GreenStyle);
            if (iGreenStyle >= 0)
                if (CurrentTB.Selection.Start.iChar > 0)
                {
                    //current char (before caret)
                    var c = CurrentTB[CurrentTB.Selection.Start.iLine][CurrentTB.Selection.Start.iChar - 1];
                    //green Style
                    var greenStyleIndex = Range.ToStyleIndex(iGreenStyle);
                    //if char contains green style then block popup menu
                    if ((c.style & greenStyleIndex) != 0)
                        e.Cancel = true;
                }
        }

        private void BuildAutocompleteMenu(AutocompleteMenu popupMenu)
        {
            if (CurrentTB.Language == Language.CSharp)
            {
                List<AutocompleteItem> items = new List<AutocompleteItem>();

                foreach (var item in snippets)
                    items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
                foreach (var item in declarationSnippets)
                    items.Add(new DeclarationSnippet(item) { ImageIndex = 0 });
                foreach (var item in methods)
                    items.Add(new MethodAutocompleteItem(item) { ImageIndex = 2 });
                foreach (var item in keywords)
                    items.Add(new AutocompleteItem(item));

                items.Add(new InsertSpaceSnippet());
                items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!:]+)(\w+)$"));
                items.Add(new InsertEnterSnippet());

                //set as autocomplete source
                popupMenu.Items.SetAutocompleteItems(items);
                popupMenu.SearchPattern = @"[\w\.:=!<>]";
            }
            if (CurrentTB.Language == Language.Lua)
            {
                List<AutocompleteItem> items = new List<AutocompleteItem>();

                foreach (var item in Luamethods)
                    items.Add(new MethodAutocompleteItem(item) { ImageIndex = 2 });
                foreach (var item in Luakeywords)
                    items.Add(new AutocompleteItem(item));
                foreach (var item in LuadeclarationSnippets)
                    items.Add(new DeclarationSnippet(item) { ImageIndex = 1 });

                items.Add(new InsertSpaceSnippet());
                items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!:]+)(\w+)$"));
                items.Add(new InsertEnterSnippet());

                //set as autocomplete source
                popupMenu.Items.SetAutocompleteItems(items);
                popupMenu.SearchPattern = @"[\w\.:=!<>]";
            }
        }

        void tb_MouseMove(object sender, MouseEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            var place = tb.PointToPlace(e.Location);
            var r = new Range(tb, place, place);

            string text = r.GetFragment("[a-zA-Z]").Text;
            lbWordUnderMouse.Text = text;
        }

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.OemMinus)
            {
                NavigateBackward();
                e.Handled = true;
            }

            if (e.Modifiers == (Keys.Control|Keys.Shift) && e.KeyCode == Keys.OemMinus)
            {
                NavigateForward();
                e.Handled = true;
            }

            if (e.KeyData == (Keys.K | Keys.Control))
            {
                //forced show (MinFragmentLength will be ignored)
                (CurrentTB.Tag as TbInfo).popupMenu.Show(true);
                e.Handled = true;
            }
        }

        void tb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            //remember last visit time
            if (tb.Selection.IsEmpty && tb.Selection.Start.iLine < tb.LinesCount)
            {
                if (lastNavigatedDateTime != tb[tb.Selection.Start.iLine].LastVisit)
                {
                    tb[tb.Selection.Start.iLine].LastVisit = DateTime.Now;
                    lastNavigatedDateTime = tb[tb.Selection.Start.iLine].LastVisit;
                }
            }

            //highlight same words
            tb.VisibleRange.ClearStyle(sameWordsStyle);
            if (!tb.Selection.IsEmpty)
                return;//user selected diapason
            //get fragment around caret
            var fragment = tb.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            Range[] ranges = tb.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();

            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(sameWordsStyle);
        }

        void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox tb = (sender as FastColoredTextBox);
            //rebuild object explorer
            string text = (sender as FastColoredTextBox).Text;
            ThreadPool.QueueUserWorkItem(
                (o)=>ReBuildObjectExplorer(text)
            );

            //show invisible chars
            HighlightInvisibleChars(e.ChangedRange);
        }

        private void HighlightInvisibleChars(Range range)
        {
            range.ClearStyle(invisibleCharsStyle);
            if (btInvisibleChars.Checked)
                range.SetStyle(invisibleCharsStyle, @".$|.\r\n|\s");
        }

        List<ExplorerItem> explorerList = new List<ExplorerItem>();

        private void ReBuildObjectExplorer(string text)
        {
            try
            {
                List<ExplorerItem> list = new List<ExplorerItem>();
                int lastClassIndex = -1;
                //find classes, methods and properties
                Regex regex = new Regex(@"^(?<range>[\w\s]+\b(class|struct|enum|interface)\s+[\w<>,\s]+)|^\s*(public|private|internal|protected)[^\n]+(\n?\s*{|;)?", RegexOptions.Multiline);
                foreach (Match r in regex.Matches(text))
                    try
                    {
                        string s = r.Value;
                        int i = s.IndexOfAny(new char[] { '=', '{', ';' });
                        if (i >= 0)
                            s = s.Substring(0, i);
                        s = s.Trim();

                        var item = new ExplorerItem() { title = s, position = r.Index };
                        if (Regex.IsMatch(item.title, @"\b(class|struct|enum|interface)\b"))
                        {
                            item.title = item.title.Substring(item.title.LastIndexOf(' ')).Trim();
                            item.type = ExplorerItemType.Class;
                            list.Sort(lastClassIndex + 1, list.Count - (lastClassIndex + 1), new ExplorerItemComparer());
                            lastClassIndex = list.Count;
                        }
                        else
                            if (item.title.Contains(" event "))
                            {
                                int ii = item.title.LastIndexOf(' ');
                                item.title = item.title.Substring(ii).Trim();
                                item.type = ExplorerItemType.Event;
                            }
                            else
                                if (item.title.Contains("("))
                                {
                                    var parts = item.title.Split('(');
                                    item.title = parts[0].Substring(parts[0].LastIndexOf(' ')).Trim() + "(" + parts[1];
                                    item.type = ExplorerItemType.Method;
                                }
                                else
                                    if (item.title.EndsWith("]"))
                                    {
                                        var parts = item.title.Split('[');
                                        if (parts.Length < 2) continue;
                                        item.title = parts[0].Substring(parts[0].LastIndexOf(' ')).Trim() + "[" + parts[1];
                                        item.type = ExplorerItemType.Method;
                                    }
                                    else
                                    {
                                        int ii = item.title.LastIndexOf(' ');
                                        item.title = item.title.Substring(ii).Trim();
                                        item.type = ExplorerItemType.Property;
                                    }
                        list.Add(item);
                    }
                    catch { ;}

                list.Sort(lastClassIndex + 1, list.Count - (lastClassIndex + 1), new ExplorerItemComparer());

                BeginInvoke(
                    new Action(() =>
                        {
                            explorerList = list;
                            dgvObjectExplorer.RowCount = explorerList.Count;
                            dgvObjectExplorer.Invalidate();
                        })
                );
            }
            catch { ;}
        }

        enum ExplorerItemType
        {
            Class, Method, Property, Event
        }

        class ExplorerItem
        {
            public ExplorerItemType type;
            public string title;
            public int position;
        }

        class ExplorerItemComparer : IComparer<ExplorerItem>
        {
            public int Compare(ExplorerItem x, ExplorerItem y)
            {
                return x.title.CompareTo(y.title);
            }
        }

        private void tsFiles_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            if ((e.Item.Controls[0] as FastColoredTextBox).IsChanged)
            {
                switch(MessageBox.Show("Do you want save " + e.Item.Title + " ?", " INDEV Syntaxies - Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        if (!Save(e.Item))
                            e.Cancel = true;
                        break;
                    case DialogResult.Cancel:
                         e.Cancel = true;
                        break;
                }
            }
        }

        private bool Save(FATabStripItem tab)
        {
            var tb = (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null)
            {
                if (sfdMain.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                tab.Title = Path.GetFileName(sfdMain.FileName);
                tab.Tag = sfdMain.FileName;
            }

            try
            {
                File.WriteAllText(tab.Tag as string, tb.Text);
                tb.IsChanged = false;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return Save(tab);
                else
                    return false;
            }

            tb.Invalidate();

            return true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
            {
                string oldFile = tsFiles.SelectedItem.Tag as string;
                tsFiles.SelectedItem.Tag = null;
                if (!Save(tsFiles.SelectedItem))
                    if (oldFile != null)
                    {
                        tsFiles.SelectedItem.Tag = oldFile;
                        tsFiles.SelectedItem.Title = Path.GetFileName(oldFile);
                    }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
            {
                string oldFile = tsFiles.SelectedItem.Tag as string;
                tsFiles.SelectedItem.Tag = null;
                if (!Save(tsFiles.SelectedItem))
                if(oldFile!=null)
                {
                    tsFiles.SelectedItem.Tag = oldFile;
                    tsFiles.SelectedItem.Title = Path.GetFileName(oldFile);
                }
            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);

            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        FastColoredTextBox CurrentTB
        {
            get {
                if (tsFiles.SelectedItem == null)
                    return null;
                return (tsFiles.SelectedItem.Controls[0] as FastColoredTextBox);
            }

            set
            {
                tsFiles.SelectedItem = (value.Parent as FATabStripItem);
                value.Focus();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.Selection.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        private void tmUpdateInterface_Tick(object sender, EventArgs e)
        {
            try
            {
                if(CurrentTB != null && tsFiles.Items.Count>0)
                {
                    var tb = CurrentTB;
                    undoStripButton.Enabled = undoToolStripMenuItem.Enabled = tb.UndoEnabled;
                    redoStripButton.Enabled = redoToolStripMenuItem.Enabled = tb.RedoEnabled;
                    toolStripButton1.Enabled = toolStripButton1.Enabled = tb.IsChanged;
                    saveasToolStripButton.Enabled = true;
                    pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = true;
                    cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled =
                    copyToolStripButton.Enabled = copyToolStripMenuItem.Enabled = !tb.Selection.IsEmpty;
                    printToolStripButton.Enabled = true;
                }
                else
                {
                    toolStripButton1.Enabled = toolStripButton1.Enabled = false;
                    saveasToolStripButton.Enabled = false;
                    cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled =
                    copyToolStripButton.Enabled = copyToolStripMenuItem.Enabled = false;
                    pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = false;
                    printToolStripButton.Enabled = false;
                    undoStripButton.Enabled = undoToolStripMenuItem.Enabled = false;
                    redoStripButton.Enabled = redoToolStripMenuItem.Enabled = false;
                    dgvObjectExplorer.RowCount = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            if(CurrentTB!=null)
            {
                var settings = new PrintDialogSettings();
                settings.Title = tsFiles.SelectedItem.Title;
                settings.Header = "&b&w&b";
                settings.Footer = "&b&p";
                CurrentTB.Print(settings);
            }
        }

        bool tbFindChanged = false;

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && CurrentTB != null)
            {
                Range r = tbFindChanged?CurrentTB.Range.Clone():CurrentTB.Selection.Clone();
                tbFindChanged = false;
                r.End = new Place(CurrentTB[CurrentTB.LinesCount - 1].Count, CurrentTB.LinesCount - 1);
                var pattern = Regex.Escape(tbFind.Text);
                foreach (var found in r.GetRanges(pattern))
                {
                    found.Inverse();
                    CurrentTB.Selection = found;
                    CurrentTB.DoSelectionVisible();
                    return;
                }
                MessageBox.Show("seems like there's no more coincidences.");
            }
            else
                tbFindChanged = true;
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowFindDialog();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        private void PowerfulCSharpEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
            foreach (FATabStripItem tab in  tsFiles.Items)
                list.Add(tab);
            foreach (var tab in list)
            {
                TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
                tsFiles_TabStripItemClosing(args);
                if (args.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                tsFiles.RemoveTab(tab);
            }
            Application.Exit();
        }

        private void dgvObjectExplorer_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (CurrentTB != null)
            {
                var item = explorerList[e.RowIndex];
                CurrentTB.GoEnd();
                CurrentTB.SelectionStart = item.position;
                CurrentTB.DoSelectionVisible();
                CurrentTB.Focus();
            }
        }

        private void dgvObjectExplorer_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            try
            {
                ExplorerItem item = explorerList[e.RowIndex];
                if (e.ColumnIndex == 1)
                    e.Value = item.title;
                else
                    switch (item.type)
                    {
                        case ExplorerItemType.Class:
                            e.Value = global::Tester.Properties.Resources.class_libraries;
                            return;
                        case ExplorerItemType.Method:
                            e.Value = global::Tester.Properties.Resources.box;
                            return;
                        case ExplorerItemType.Event:
                            e.Value = global::Tester.Properties.Resources.lightning;
                            return;
                        case ExplorerItemType.Property:
                            e.Value = global::Tester.Properties.Resources.property;
                            return;
                    }
            }
            catch{;}
        }

        private void tsFiles_TabStripItemSelectionChanged(TabStripItemChangedEventArgs e)
        {
            if (CurrentTB != null)
            {
                CurrentTB.Focus();
                string text = CurrentTB.Text;
                ThreadPool.QueueUserWorkItem(
                    (o) => ReBuildObjectExplorer(text)
                );
            }
        }

        private void backStripButton_Click(object sender, EventArgs e)
        {
            NavigateBackward();
        }

        private void forwardStripButton_Click(object sender, EventArgs e)
        {
            NavigateForward();
        }

        DateTime lastNavigatedDateTime = DateTime.Now;

        private bool NavigateBackward()
        {
            DateTime max = new DateTime();
            int iLine = -1;
            FastColoredTextBox tb = null;
            for (int iTab = 0; iTab < tsFiles.Items.Count; iTab++)
            {
                var t = (tsFiles.Items[iTab].Controls[0] as FastColoredTextBox);
                for (int i = 0; i < t.LinesCount; i++)
                    if (t[i].LastVisit < lastNavigatedDateTime && t[i].LastVisit > max)
                    {
                        max = t[i].LastVisit;
                        iLine = i;
                        tb = t;
                    }
            }
            if (iLine >= 0)
            {
                tsFiles.SelectedItem = (tb.Parent as FATabStripItem);
                tb.Navigate(iLine);
                lastNavigatedDateTime = tb[iLine].LastVisit;
                Console.WriteLine("Backward: " + lastNavigatedDateTime);
                tb.Focus();
                tb.Invalidate();
                return true;
            }
            else
                return false;
        }

        private bool NavigateForward()
        {
            DateTime min = DateTime.Now;
            int iLine = -1;
            FastColoredTextBox tb = null;
            for (int iTab = 0; iTab < tsFiles.Items.Count; iTab++)
            {
                var t = (tsFiles.Items[iTab].Controls[0] as FastColoredTextBox);
                for (int i = 0; i < t.LinesCount; i++)
                    if (t[i].LastVisit > lastNavigatedDateTime && t[i].LastVisit < min)
                    {
                        min = t[i].LastVisit;
                        iLine = i;
                        tb = t;
                    }
            }
            if (iLine >= 0)
            {
                tsFiles.SelectedItem = (tb.Parent as FATabStripItem);
                tb.Navigate(iLine);
                lastNavigatedDateTime = tb[iLine].LastVisit;
                Console.WriteLine("Forward: " + lastNavigatedDateTime);
                tb.Focus();
                tb.Invalidate();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// This item appears when any part of snippet text is typed
        /// </summary>
        class DeclarationSnippet : SnippetAutocompleteItem
        {
            public DeclarationSnippet(string snippet)
                : base(snippet)
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                var pattern = Regex.Escape(fragmentText);
                if (Regex.IsMatch(Text, "\\b" + pattern, RegexOptions.IgnoreCase))
                    return CompareResult.Visible;
                return CompareResult.Hidden;
            }
        }

        /// <summary>
        /// Divides numbers and words: "123AND456" -> "123 AND 456"
        /// Or "i=2" -> "i = 2"
        /// </summary>
        class InsertSpaceSnippet : AutocompleteItem
        {
            string pattern;

            public InsertSpaceSnippet(string pattern)
                : base("")
            {
                this.pattern = pattern;
            }

            public InsertSpaceSnippet()
                : this(@"^(\d+)([a-zA-Z_]+)(\d*)$")
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                if (Regex.IsMatch(fragmentText, pattern))
                {
                    Text = InsertSpaces(fragmentText);
                    if (Text != fragmentText)
                        return CompareResult.Visible;
                }
                return CompareResult.Hidden;
            }

            public string InsertSpaces(string fragment)
            {
                var m = Regex.Match(fragment, pattern);
                if (m == null)
                    return fragment;
                if (m.Groups[1].Value == "" && m.Groups[3].Value == "")
                    return fragment;
                return (m.Groups[1].Value + " " + m.Groups[2].Value + " " + m.Groups[3].Value).Trim();
            }

            public override string ToolTipTitle
            {
                get
                {
                    return Text;
                }
            }
        }

        /// <summary>
        /// Inerts line break after '}'
        /// </summary>
        class InsertEnterSnippet : AutocompleteItem
        {
            Place enterPlace = Place.Empty;

            public InsertEnterSnippet()
                : base("[Line break]")
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                var r = Parent.Fragment.Clone();
                while (r.Start.iChar > 0)
                {
                    if (r.CharBeforeStart == '}')
                    {
                        enterPlace = r.Start;
                        return CompareResult.Visible;
                    }

                    r.GoLeftThroughFolded();
                }

                return CompareResult.Hidden;
            }

            public override string GetTextForReplace()
            {
                //extend range
                Range r = Parent.Fragment;
                Place end = r.End;
                r.Start = enterPlace;
                r.End = r.End;
                //insert line break
                return Environment.NewLine + r.Text;
            }

            public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
            {
                base.OnSelected(popupMenu, e);
                if (Parent.Fragment.tb.AutoIndent)
                    Parent.Fragment.tb.DoAutoIndent();
            }

            public override string ToolTipTitle
            {
                get
                {
                    return "Insert line break after '}'";
                }
            }
        }

        private void autoIndentSelectedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.DoAutoIndent();
        }

        private void btInvisibleChars_Click(object sender, EventArgs e)
        {
            foreach (FATabStripItem tab in tsFiles.Items)
                HighlightInvisibleChars((tab.Controls[0] as FastColoredTextBox).Range);
            if (CurrentTB!=null)
                CurrentTB.Invalidate();
        }

        private void btHighlightCurrentLine_Click(object sender, EventArgs e)
        {
            foreach (FATabStripItem tab in tsFiles.Items)
            {
                if (btHighlightCurrentLine.Checked)
                    (tab.Controls[0] as FastColoredTextBox).CurrentLineColor = currentLineColor;
                else
                    (tab.Controls[0] as FastColoredTextBox).CurrentLineColor = Color.Transparent;
            }
            if (CurrentTB != null)
                CurrentTB.Invalidate();
        }

        private void commentSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.InsertLinePrefix("//");
        }

        private void uncommentSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.RemoveLinePrefix("//");
        }

        private void cloneLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //expand selection
            CurrentTB.Selection.Expand();
            //get text of selected lines
            string text = Environment.NewLine + CurrentTB.Selection.Text;
            //move caret to end of selected lines
            CurrentTB.Selection.Start = CurrentTB.Selection.End;
            //insert text
            CurrentTB.InsertText(text);
        }

        private void cloneLinesAndCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start autoUndo block
            CurrentTB.BeginAutoUndo();
            //expand selection
            CurrentTB.Selection.Expand();
            //get text of selected lines
            string text = Environment.NewLine + CurrentTB.Selection.Text;
            //comment lines
            CurrentTB.InsertLinePrefix("//");
            //move caret to end of selected lines
            CurrentTB.Selection.Start = CurrentTB.Selection.End;
            //insert text
            CurrentTB.InsertText(text);
            //end of autoUndo block
            CurrentTB.EndAutoUndo();
        }

        private void bookmarkPlusButton_Click(object sender, EventArgs e)
        {
            if(CurrentTB == null) 
                return;
            CurrentTB.BookmarkLine(CurrentTB.Selection.Start.iLine);
        }

        private void bookmarkMinusButton_Click(object sender, EventArgs e)
        {
            if (CurrentTB == null)
                return;
            CurrentTB.UnbookmarkLine(CurrentTB.Selection.Start.iLine);
        }

        private void gotoButton_DropDownOpening(object sender, EventArgs e)
        {
            gotoButton.DropDownItems.Clear();
            foreach (Control tab in tsFiles.Items)
            {
                FastColoredTextBox tb = tab.Controls[0] as FastColoredTextBox;
                foreach (var bookmark in tb.Bookmarks)
                {
                    var item = gotoButton.DropDownItems.Add(bookmark.Name + " [" + Path.GetFileNameWithoutExtension(tab.Tag as String) + "]");
                    item.Tag = bookmark;
                    item.Click += (o, a) => {
                        var b = (Bookmark)(o as ToolStripItem).Tag;
                        try
                        {
                            CurrentTB = b.TB;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                        b.DoVisible();
                    };
                }
            }
        }

        private void btShowFoldingLines_Click(object sender, EventArgs e)
        {
            foreach (FATabStripItem tab in tsFiles.Items)
                (tab.Controls[0] as FastColoredTextBox).ShowFoldingLines = btShowFoldingLines.Checked;
            if (CurrentTB != null)
                CurrentTB.Invalidate();
        }

        private void Zoom_click(object sender, EventArgs e)
        {
            if (CurrentTB != null)
                CurrentTB.Zoom = int.Parse((sender as ToolStripItem).Tag.ToString());
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentTB.Language == FastColoredTextBoxNS.Language.HTML) //if language is html
                {
                    HTMLPreview h = new HTMLPreview(CurrentTB.Text);
                    h.Show();
                }
                else if (CurrentTB.Language == FastColoredTextBoxNS.Language.CSharp) //if language is c#
                {
                    toolStripStatusLabel1.Text = "Info: " + "Starting to compile the program";

                    SaveFileDialog sf = new SaveFileDialog();
                    sf.Filter = "Executable File|*.exe";
                    string OutPath = "There is no Output Path to create the program";
                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        OutPath = sf.FileName;
                    }
                    //compile code:
                    //create c# code compiler
                    CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                    //create new parameters for compilation and add references(libs) to compiled app
                    CompilerParameters parameters = new CompilerParameters(new string[] { "System.dll" });
                    //is compiled code will be executable?(.exe)
                    parameters.GenerateExecutable = true;
                    //output path
                    parameters.OutputAssembly = OutPath;
                    //code sources to compile
                    string[] sources = { CurrentTB.Text };
                    //results of compilation
                    CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, sources);

                    // Get the current time
                    string currentTime = DateTime.Now.ToString();


                    toolStripProgressBar1.Enabled = true;

                    toolStripStatusLabel1.Text = "Info: " + "Loading";

                    //if has errors
                    if (results.Errors.Count > 0)
                    {
                        string errsText = "";
                        foreach (CompilerError CompErr in results.Errors)
                        {
                            errsText = "Code Error: (" + CompErr.ErrorNumber +
                                        "), Line " + CompErr.Line +
                                        ", Column " + CompErr.Column +
                                        ": " + CompErr.ErrorText + "" +
                                        Environment.NewLine;
                        }
                        //show error message
                        MessageBox.Show(errsText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        foreach (CompilerError CompErr in results.Errors)
                        {
                            string ErrorText = "Error:" + errsText.ToString();
                            ;
                            Log(DateTime.Now, ErrorText, "\r\n", errorStyle);
                            //richTextBox1.ForeColor = Color.Red;
                            //richTextBox1.Text += currentTime + ": Error: " + errsText.ToString() + "\n";
                        }

                        toolStripStatusLabel1.ForeColor = Color.Red;
                        toolStripStatusLabel1.Text = currentTime + ": Error: " + errsText.ToString();

                    }
                    else
                    {
                        //run compiled app
                        System.Diagnostics.Process.Start(OutPath);

                        FCTBConsole.Text = "";

                        Log(DateTime.Now, " Info: Successful Compilation", "\r\n", infoStyle);

                        toolStripStatusLabel1.ForeColor = Color.Green;
                        toolStripStatusLabel1.Text = currentTime + ": Info: " + "Successful compilation";
                    }
                }
                else
                {
                    MessageBox.Show("INDEV Syntaxies: Cannot open this file!");
                }
            }
            catch (Exception)
            {
                // Handle the exception here
                MessageBox.Show("Error: There is no tab open to compile code.", "Compiler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem);
        }

        private void aboutINDEVSyntaxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About AboutSyntaxies = new About();
            AboutSyntaxies.Show();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowGoToDialog();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            Cursor.Current = Cursors.WaitCursor;

            treeView1.Nodes.Clear();
            foreach (var item in Directory.GetDirectories(folderBrowserDialog1.SelectedPath))
            {
                DirectoryInfo di = new DirectoryInfo(item);
                var node = treeView1.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 1, selectedImageIndex: 1);
                node.Tag = di;
            }

            foreach (var item in Directory.GetFiles(folderBrowserDialog1.SelectedPath))
            {
                FileInfo di = new FileInfo(item);
                var node = treeView1.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 0, selectedImageIndex: 0);
                node.Tag = di;
            }
            Cursor.Current = Cursors.Default;
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                // Return
            }
            else if (e.Node.Tag.GetType() == typeof(DirectoryInfo))
            {
                // open Folder
                e.Node.Nodes.Clear();
                foreach (var item in Directory.GetDirectories(((DirectoryInfo)e.Node.Tag).FullName))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 1, selectedImageIndex: 1);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                {
                    FileInfo fileInfo = new FileInfo(item);
                    var node = e.Node.Nodes.Add(key: fileInfo.Name, text: fileInfo.Name, imageIndex: 0, selectedImageIndex: 0);
                    node.Tag = fileInfo;
                }
                e.Node.Expand();
            }
            else
            {
                string fileName = (((FileInfo)e.Node.Tag).FullName);

                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;

                    if (e.Node.Name.EndsWith(".html"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".htm"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".xhtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".shtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".cs"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.CSharp;
                    }
                    else if (e.Node.Name.EndsWith(".php"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".phps"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".phtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".lua"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.Lua;
                    }
                    else if (e.Node.Name.EndsWith(".luac"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.Lua;
                    }
                    else if (e.Node.Name.EndsWith(".xml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".xsd"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".xsl"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".vb"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.VB;
                    }
                    else if (e.Node.Name.EndsWith(".json"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JSON;
                    }
                    else if (e.Node.Name.EndsWith(".js"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JS;
                    }
                    else if (e.Node.Name.EndsWith(".jsx"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JS;
                    }
                    else if (e.Node.Name.EndsWith(".sql"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.SQL;
                    }
                    else
                    {
                        tb.Language = Language.Custom;
                    }

                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();

                    // Open File
                    tb.Text = File.ReadAllText(((FileInfo)e.Node.Tag).FullName);

                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                }
            }
        }

        private void aboutINDEVSyntaxiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About AboutSyntaxies = new About();
            AboutSyntaxies.Show();
        }

        private void textFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = "[new]";

            try
            {
                var tb = new FastColoredTextBox();
                tb.Font = new Font("Consolas", 9.75f);
                tb.ContextMenuStrip = cmMain;
                tb.Dock = DockStyle.Fill;
                tb.BorderStyle = BorderStyle.None;
                //tb.VirtualSpace = true;
                tb.LeftPadding = 17;

                tb.Language = Language.Custom;

                tb.AddStyle(sameWordsStyle);//same words style
                var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                tab.Tag = fileName;
                if (fileName != null)
                    tb.OpenFile(fileName);
                tb.Tag = new TbInfo();
                tsFiles.AddTab(tab);
                tsFiles.SelectedItem = tab;
                tb.Focus();

                tb.DelayedTextChangedInterval = 1000;
                tb.DelayedEventsInterval = 500;
                tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                tb.ChangedLineColor = changedLineColor;
                if (btHighlightCurrentLine.Checked)
                    tb.CurrentLineColor = currentLineColor;
                tb.ShowFoldingLines = btShowFoldingLines.Checked;
                tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                //create autocomplete popup menu
                AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                popupMenu.Items.ImageList = ilAutocomplete;
                popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                BuildAutocompleteMenu(popupMenu);
                (tb.Tag as TbInfo).popupMenu = popupMenu;

                var dm = new DocumentMap();
                dm.Dock = DockStyle.Right;
                dm.Size = new Size(82, 340);
                dm.Target = tb;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    CreateTab(fileName);
            }
        }

        private void newToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            NewItem NewItemPopup = new NewItem();
            NewItemPopup.Show();
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void saveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem);
        }

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);

            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB != null)
            {
                var settings = new PrintDialogSettings();
                settings.Title = tsFiles.SelectedItem.Title;
                settings.Header = "&b&w&b";
                settings.Footer = "&b&p";
                CurrentTB.Print(settings);
            }
        }

        private void saveAsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
            {
                string oldFile = tsFiles.SelectedItem.Tag as string;
                tsFiles.SelectedItem.Tag = null;
                if (!Save(tsFiles.SelectedItem))
                    if (oldFile != null)
                    {
                        tsFiles.SelectedItem.Tag = oldFile;
                        tsFiles.SelectedItem.Title = Path.GetFileName(oldFile);
                    }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowGoToDialog();
        }

        private void replaceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        private void undoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void redoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentTB.Copy();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentTB.Cut();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB.Language == FastColoredTextBoxNS.Language.HTML) //if language is html
            {
                HTMLPreview h = new HTMLPreview(CurrentTB.Text);
                h.Show();
            }
            else if (CurrentTB.Language == FastColoredTextBoxNS.Language.CSharp) //if language is c#
            {
                toolStripStatusLabel1.Text = "Info: " + "Starting to compile the program";

                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "Executable File|*.exe";
                string OutPath = "There is no Output Path to create the program";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    OutPath = sf.FileName;
                }
                //compile code:
                //create c# code compiler
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                //create new parameters for compilation and add references(libs) to compiled app
                CompilerParameters parameters = new CompilerParameters(new string[] { "System.dll" });
                //is compiled code will be executable?(.exe)
                parameters.GenerateExecutable = true;
                //output path
                parameters.OutputAssembly = OutPath;
                //code sources to compile
                string[] sources = { CurrentTB.Text };
                //results of compilation
                CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, sources);

                // Get the current time
                string currentTime = DateTime.Now.ToString();

                timer1.Enabled = true;
                toolStripProgressBar1.Enabled = true;

                toolStripStatusLabel1.Text = "Info: " + "Loading";

                //if has errors
                if (results.Errors.Count > 0)
                {
                    string errsText = "";
                    foreach (CompilerError CompErr in results.Errors)
                    {
                        errsText = "Code Error: (" + CompErr.ErrorNumber +
                                    "), Line " + CompErr.Line +
                                    ", Column " + CompErr.Column +
                                    ": " + CompErr.ErrorText + "" +
                                    Environment.NewLine;
                    }
                    //show error message
                    MessageBox.Show(errsText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    foreach (CompilerError CompErr in results.Errors)
                    {
                        string ErrorText = "Error:" + errsText.ToString();
                        
                        Log(DateTime.Now, ErrorText, "\r\n", errorStyle);

                        //richTextBox1.ForeColor = Color.Red;
                        //richTextBox1.Text += currentTime + ": Error: " + errsText.ToString() + "\n";
                    }

                    toolStripStatusLabel1.ForeColor = Color.Red;
                    toolStripStatusLabel1.Text = currentTime + ": Error: " + errsText.ToString();

                }
                else
                {
                    //run compiled app
                    System.Diagnostics.Process.Start(OutPath);

                    FCTBConsole.Text = "";

                    Log(DateTime.Now, " Info: Successful Compilation", "\r\n", infoStyle);

                    toolStripStatusLabel1.ForeColor = Color.Green;
                    toolStripStatusLabel1.Text = currentTime + ": Info: " + "Successful compilation";
                }
            }
            else
            {
                MessageBox.Show("INDEV Syntaxies: Cannot open this file!");
            }
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WindowState.ToString() == "Normal")
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (WindowState.ToString() == "Normal")
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
        }

        private void fullscreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
            TopMost = true;
            fullscreenToolStripMenuItem1.Visible = false;
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            string fileName = "[new]";
            if (langToolStripMenuItem.Text == "Lang:Custom")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Java")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.java", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C++Cpp")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.cpp", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C++H")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.h", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.c", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Python")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.py", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C#")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.CSharp;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.cs", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:HTML")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.HTML;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.html", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:JS")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.JS;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.js", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:JSON")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.JSON;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.json", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Lua")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Lua;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.lua", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:PHP")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.PHP;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.php", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:SQL")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.SQL;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.sql", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:VB")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.VB;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.vb", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:XML")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.XML;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.xml", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else
            {
                langToolStripMenuItem.Text = "N/A";
            }
        }

        private void Editor_QueryAccessibilityHelp(object sender, QueryAccessibilityHelpEventArgs e)
        {

        }

        private void langToolStripMenuItem_TextChanged(object sender, EventArgs e)
        {
            string fileName = null;
            if (langToolStripMenuItem.Text == "Lang:Custom")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Java")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.java", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C++CPP")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.cpp", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C++H")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.h", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.c", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Python")
            {
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Custom;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.py", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:C#")
            {
                try
                {
                    treeView5.Visible = true;

                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.CSharp;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.cs", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:HTML")
            {
                try
                {

                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.HTML;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.html", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:JS")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.JS;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.js", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:JSON")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.JSON;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.json", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:Lua")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.Lua;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.lua", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:PHP")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.PHP;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.php", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:SQL")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.SQL;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.sql", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:VB")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.VB;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.vb", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else if (langToolStripMenuItem.Text == "Lang:XML")
            {
                try
                {
                    treeView5.Visible = true;
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.XML;
                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.xml", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();
                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                    langToolStripMenuItem.Text = "Lang";
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                    langToolStripMenuItem.Text = "Lang";
                }
            }
            else
            {
                langToolStripMenuItem.Text = "N/A";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (splitContainer4.Panel1Collapsed == true)
            {
                splitContainer4.Panel1Collapsed = false;
                splitContainer4.Panel2Collapsed = true;
            }
            else
            {
                splitContainer4.Panel1Collapsed = true;
                splitContainer4.Panel2Collapsed = false;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Panel1Collapsed == true)
            {
                splitContainer2.Panel1Collapsed = false;
                splitContainer2.Panel2Collapsed = true;
            }
            else
            {
                splitContainer2.Panel1Collapsed = true;
                splitContainer2.Panel2Collapsed = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Panel2Collapsed == true)
            {
                splitContainer2.Panel2Collapsed = false;
                splitContainer2.Panel1Collapsed = true;
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
                splitContainer2.Panel1Collapsed = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Visible == true)
            {
                button4.ImageIndex = 1;
                splitter1.Visible = false;
                splitContainer2.Visible = false;
            }
            else
            {
                button4.ImageIndex = 0;
                splitter1.Visible = true;
                splitContainer2.Visible = true;
            }
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
        }

        private void button3_MouseHover(object sender, EventArgs e)
        {
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
            Cursor.Current = Cursors.WaitCursor;

            MixedProject ProjLanguage = new MixedProject();
            ProjLanguage.Show();

            treeView3.Nodes.Clear();

            if (projLangToolStripMenuItem.Text == "ProjLang:C#")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 0;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C# Project";
                    treeView4.Nodes[0].ImageIndex = 0;
                    treeView4.Nodes[0].SelectedImageIndex = 0;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cs"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 5, selectedImageIndex: 5);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:C++")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 14;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C++ Project";
                    treeView4.Nodes[0].ImageIndex = 14;
                    treeView4.Nodes[0].SelectedImageIndex = 14;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cpp"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 16, selectedImageIndex: 16);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".h"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 18, selectedImageIndex: 18);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:C")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 17;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C Project";
                    treeView4.Nodes[0].ImageIndex = 17;
                    treeView4.Nodes[0].SelectedImageIndex = 17;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".c"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 15, selectedImageIndex: 15);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Java")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 20;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Java Project";
                    treeView4.Nodes[0].ImageIndex = 20;
                    treeView4.Nodes[0].SelectedImageIndex = 20;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".java"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 19, selectedImageIndex: 19);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Python")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 22;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Python Project";
                    treeView4.Nodes[0].ImageIndex = 22;
                    treeView4.Nodes[0].SelectedImageIndex = 22;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".py"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 21, selectedImageIndex: 21);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Lua")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }
                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 3;
                    treeView4.Nodes[0].SelectedImageIndex = 3;
                    treeView4.Nodes[0].ImageIndex = 3;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Lua Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".lua"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".luac"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:WebPage")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 2;
                    treeView4.Nodes[0].SelectedImageIndex = 2;
                    treeView4.Nodes[0].ImageIndex = 2;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Web Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".html"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".htm"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".xhtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".shtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".js"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 11, selectedImageIndex: 11);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".css"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 8, selectedImageIndex: 8);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:PHP")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 4;
                    treeView4.Nodes[0].SelectedImageIndex = 4;
                    treeView4.Nodes[0].ImageIndex = 4;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "PHP Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".php"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phps"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:VB")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 1;
                    treeView4.Nodes[0].SelectedImageIndex = 1;
                    treeView4.Nodes[0].ImageIndex = 1;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Visual Basic Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".vb"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 6, selectedImageIndex: 6);
                        node.Tag = di;
                    }
                }
            }
            else
            {

            }

            Cursor.Current = Cursors.Default;
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (splitContainer4.Panel2Collapsed == true)
            {
                splitContainer4.Panel1Collapsed = false;
                splitContainer4.Panel2Collapsed = true;
            }
            else
            {
                splitContainer4.Panel2Collapsed = true;
                splitContainer4.Panel1Collapsed = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (splitContainer4.Visible == true)
            {
                button6.ImageIndex = 0;
                splitter1.Visible = false;
                splitContainer4.Visible = false;
                splitContainer1.Panel1Collapsed = true;
            }
            else
            {
                button6.ImageIndex = 1;
                splitter1.Visible = true;
                splitContainer4.Visible = true;
                splitContainer1.Panel1Collapsed = false;
            }
        }

        private void treeView3_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                // Return
            }
            else if (e.Node.Tag.GetType() == typeof(DirectoryInfo))
            {
                // open Folder
                e.Node.Nodes.Clear();
                foreach (var item in Directory.GetDirectories(((DirectoryInfo)e.Node.Tag).FullName))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                if (projLangToolStripMenuItem.Text == "ProjLang:C#")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].ImageIndex = 0;
                        treeView4.Nodes[0].SelectedImageIndex = 0;

                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".cs"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 5, selectedImageIndex: 5);
                            node.Tag = di;
                        }
                    }
                }

                if (projLangToolStripMenuItem.Text == "ProjLang:C++")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].ImageIndex = 0;
                        treeView4.Nodes[0].SelectedImageIndex = 0;

                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".cpp"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 16, selectedImageIndex: 16);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".h"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 18, selectedImageIndex: 18);
                            node.Tag = di;
                        }
                    }
                }
                if (projLangToolStripMenuItem.Text == "ProjLang:C")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].ImageIndex = 0;
                        treeView4.Nodes[0].SelectedImageIndex = 0;

                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".c"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 15, selectedImageIndex: 15);
                            node.Tag = di;
                        }
                    }
                }
                if (projLangToolStripMenuItem.Text == "ProjLang:Java")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].ImageIndex = 0;
                        treeView4.Nodes[0].SelectedImageIndex = 0;

                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".java"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 19, selectedImageIndex: 19);
                            node.Tag = di;
                        }
                    }
                }
                if (projLangToolStripMenuItem.Text == "ProjLang:Python")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].ImageIndex = 0;
                        treeView4.Nodes[0].SelectedImageIndex = 0;

                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".py"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 21, selectedImageIndex: 21);
                            node.Tag = di;
                        }
                    }
                }

                else if (projLangToolStripMenuItem.Text == "ProjLang:Lua")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].SelectedImageIndex = 3;
                        treeView4.Nodes[0].ImageIndex = 3;
                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".lua"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".luac"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                            node.Tag = di;
                        }
                    }
                }
                else if (projLangToolStripMenuItem.Text == "ProjLang:WebPage")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].SelectedImageIndex = 2;
                        treeView4.Nodes[0].ImageIndex = 2;
                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".html"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".htm"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".xhtml"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".shtml"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".js"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 11, selectedImageIndex: 11);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".css"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 8, selectedImageIndex: 8);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".php"))
                        {
                            var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                            node.Tag = di;
                        }
                    }
                }
                else if (projLangToolStripMenuItem.Text == "ProjLang:PHP")
                {
                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].SelectedImageIndex = 4;
                        treeView4.Nodes[0].ImageIndex = 4;
                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".php"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".phps"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                            node.Tag = di;
                        }
                        if (di.Name.EndsWith(".phtml"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                            node.Tag = di;
                        }
                    }
                }
                else if (projLangToolStripMenuItem.Text == "ProjLang:VB")
                {


                    foreach (var item in Directory.GetFiles(((DirectoryInfo)e.Node.Tag).FullName))
                    {
                        treeView4.Nodes[0].SelectedImageIndex = 1;
                        treeView4.Nodes[0].ImageIndex = 1;
                        FileInfo di = new FileInfo(item);

                        if (di.Name.EndsWith(".vb"))
                        {
                            var node = e.Node.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 6, selectedImageIndex: 6);
                            node.Tag = di;
                        }
                    }
                }
                else
                {

                }
                e.Node.Expand();
            }
            else
            {
                string fileName = (((FileInfo)e.Node.Tag).FullName);

                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;

                    if (e.Node.Name.EndsWith(".html"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".htm"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".xhtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".shtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.HTML;
                    }
                    else if (e.Node.Name.EndsWith(".cs"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.CSharp;
                    }
                    else if (e.Node.Name.EndsWith(".php"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".phps"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".phtml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.PHP;
                    }
                    else if (e.Node.Name.EndsWith(".lua"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.Lua;
                    }
                    else if (e.Node.Name.EndsWith(".luac"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.Lua;
                    }
                    else if (e.Node.Name.EndsWith(".xml"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".xsd"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".xsl"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.XML;
                    }
                    else if (e.Node.Name.EndsWith(".vb"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.VB;
                    }
                    else if (e.Node.Name.EndsWith(".json"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JSON;
                    }
                    else if (e.Node.Name.EndsWith(".js"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JS;
                    }
                    else if (e.Node.Name.EndsWith(".jsx"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.JS;
                    }
                    else if (e.Node.Name.EndsWith(".sql"))
                    {
                        treeView5.Visible = true;
                        tb.Language = Language.SQL;
                    }
                    else
                    {
                        tb.Language = Language.Custom;
                    }

                    tb.AddStyle(sameWordsStyle);//same words style
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
                    label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
                    tab.Tag = fileName;
                    if (fileName != null)
                        tb.OpenFile(fileName);
                    tb.Tag = new TbInfo();
                    tsFiles.AddTab(tab);
                    tsFiles.SelectedItem = tab;
                    tb.Focus();

                    // Open File
                    tb.Text = File.ReadAllText(((FileInfo)e.Node.Tag).FullName);

                    tb.DelayedTextChangedInterval = 1000;
                    tb.DelayedEventsInterval = 500;
                    tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                    tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                    tb.ChangedLineColor = changedLineColor;
                    if (btHighlightCurrentLine.Checked)
                        tb.CurrentLineColor = currentLineColor;
                    tb.ShowFoldingLines = btShowFoldingLines.Checked;
                    tb.HighlightingRangeType = HighlightingRangeType.VisibleRange;
                    //create autocomplete popup menu
                    AutocompleteMenu popupMenu = new AutocompleteMenu(tb);
                    popupMenu.Items.ImageList = ilAutocomplete;
                    popupMenu.Opening += new EventHandler<CancelEventArgs>(popupMenu_Opening);
                    BuildAutocompleteMenu(popupMenu);
                    (tb.Tag as TbInfo).popupMenu = popupMenu;

                    var dm = new DocumentMap();
                    dm.Dock = DockStyle.Right;
                    dm.Size = new Size(82, 340);
                    dm.Target = tb;
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                        CreateTab(fileName);
                }
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            MixedProject ProjLanguage = new MixedProject();
            ProjLanguage.Show();
        }

        private void projLangToolStripMenuItem_TextChanged(object sender, EventArgs e)
        { 
            treeView3.Nodes.Clear();

            if (projLangToolStripMenuItem.Text == "ProjLang:C#")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 0;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C# Project";
                    treeView4.Nodes[0].ImageIndex = 0;
                    treeView4.Nodes[0].SelectedImageIndex = 0;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cs"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 5, selectedImageIndex: 5);
                        node.Tag = di;
                    }
                }
            }

            else if (projLangToolStripMenuItem.Text == "ProjLang:C++")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 14;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C++ Project";
                    treeView4.Nodes[0].ImageIndex = 14;
                    treeView4.Nodes[0].SelectedImageIndex = 14;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cpp"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 16, selectedImageIndex: 16);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".h"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 18, selectedImageIndex: 18);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:C")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 17;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C Project";
                    treeView4.Nodes[0].ImageIndex = 17;
                    treeView4.Nodes[0].SelectedImageIndex = 17;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".c"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 15, selectedImageIndex: 15);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Java")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 20;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Java Project";
                    treeView4.Nodes[0].ImageIndex = 20;
                    treeView4.Nodes[0].SelectedImageIndex = 20;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".java"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 19, selectedImageIndex: 19);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Python")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 22;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Python Project";
                    treeView4.Nodes[0].ImageIndex = 22;
                    treeView4.Nodes[0].SelectedImageIndex = 22;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".py"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 21, selectedImageIndex: 21);
                        node.Tag = di;
                    }
                }
            }

            else if (projLangToolStripMenuItem.Text == "ProjLang:Lua")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 3;
                    treeView4.Nodes[0].SelectedImageIndex = 3;
                    treeView4.Nodes[0].ImageIndex = 3;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Lua Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".lua"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".luac"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:WebPage")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 2;
                    treeView4.Nodes[0].SelectedImageIndex = 2;
                    treeView4.Nodes[0].ImageIndex = 2;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Web Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".html"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".htm"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".xhtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".shtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".js"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 11, selectedImageIndex: 11);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".css"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 8, selectedImageIndex: 8);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".php"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:PHP")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 4;
                    treeView4.Nodes[0].SelectedImageIndex = 4;
                    treeView4.Nodes[0].ImageIndex = 4;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "PHP Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".php"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phps"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:VB")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 1;
                    treeView4.Nodes[0].SelectedImageIndex = 1;
                    treeView4.Nodes[0].ImageIndex = 1;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Visual Basic Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".vb"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 6, selectedImageIndex: 6);
                        node.Tag = di;
                    }
                }
            }
            else
            {

            }
        }

        TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        TextStyle warningStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);

        private void Log(DateTime now, string text, string text2, Style style)
        {
            //some stuffs for best performance
            FCTBConsole.BeginUpdate();
            FCTBConsole.Selection.BeginUpdate();
            //remember user selection
            var userSelection = FCTBConsole.Selection.Clone();
            //add text with predefined style
            FCTBConsole.TextSource.CurrentTB = FCTBConsole;
            FCTBConsole.AppendText(text, style);
            //restore user selection
            if (!userSelection.IsEmpty || userSelection.Start.iLine < FCTBConsole.LinesCount - 2)
            {
                FCTBConsole.Selection.Start = userSelection.Start;
                FCTBConsole.Selection.End = userSelection.End;
            }
            else
                FCTBConsole.GoEnd();//scroll to end of the text
            //
            FCTBConsole.Selection.EndUpdate();
            FCTBConsole.EndUpdate();
        }

        private void LogError(DateTime now, string text, string text2,  Style style)
        {
            //some stuffs for best performance
            FCTBConsole.BeginUpdate();
            FCTBConsole.Selection.BeginUpdate();
            //remember user selection
            var userSelection = FCTBConsole.Selection.Clone();
            //add text with predefined style
            FCTBConsole.TextSource.CurrentTB = FCTBConsole;
            FCTBConsole.AppendText(text, style);
            //restore user selection
            if (!userSelection.IsEmpty || userSelection.Start.iLine < FCTBConsole.LinesCount - 2)
            {
                FCTBConsole.Selection.Start = userSelection.Start;
                FCTBConsole.Selection.End = userSelection.End;
            }
            else
                FCTBConsole.GoEnd();//scroll to end of the text
            //
            FCTBConsole.Selection.EndUpdate();
            FCTBConsole.EndUpdate();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            FCTBConsole.Text = "";
        }

        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                
            }
        }

        private void HandleFormClosing(FormClosingEventArgs a)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
            foreach (FATabStripItem tab in tsFiles.Items)
                list.Add(tab);
            foreach (var tab in list)
            {
                TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
                tsFiles_TabStripItemClosing(args);
                if (args.Cancel)
                {
                    a.Cancel = true;
                    return;
                }
                tsFiles.RemoveTab(tab);
            }
            Application.Exit();
        }

        private void EnteringHTMLEditor(FormClosingEventArgs a)
        {
            List<FATabStripItem> list = new List<FATabStripItem>();
            foreach (FATabStripItem tab in tsFiles.Items)
                list.Add(tab);
            foreach (var tab in list)
            {
                TabStripItemClosingEventArgs args = new TabStripItemClosingEventArgs(tab);
                tsFiles_TabStripItemClosing(args);
                if (args.Cancel)
                {
                    a.Cancel = true;
                    return;
                }
                tsFiles.RemoveTab(tab);
            }
            HTMLEditor HTMLEdi = new HTMLEditor();
            HTMLEdi.Show();
            this.Hide();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

            HandleFormClosing(a);
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
        
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            NewItem NewItemPopup = new NewItem();
            NewItemPopup.Show();
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem);
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
            {
                string oldFile = tsFiles.SelectedItem.Tag as string;
                tsFiles.SelectedItem.Tag = null;
                if (!Save(tsFiles.SelectedItem))
                    if (oldFile != null)
                    {
                        tsFiles.SelectedItem.Tag = oldFile;
                        tsFiles.SelectedItem.Title = Path.GetFileName(oldFile);
                    }
            }
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {

            if (CurrentTB != null)
            {
                var settings = new PrintDialogSettings();
                settings.Title = tsFiles.SelectedItem.Title;
                settings.Header = "&b&w&b";
                settings.Footer = "&b&p";
                CurrentTB.Print(settings);
            }
        }

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);
            HandleFormClosing(a);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            NewItem NewItemPopup = new NewItem();
            NewItemPopup.Show();
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);

            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
            Cursor.Current = Cursors.WaitCursor;

            MixedProject ProjLanguage = new MixedProject();
            ProjLanguage.Show();

            treeView3.Nodes.Clear();
             
            //

            if (projLangToolStripMenuItem.Text == "ProjLang:C#")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 0;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C# Project";
                    treeView4.Nodes[0].ImageIndex = 0;
                    treeView4.Nodes[0].SelectedImageIndex = 0;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cs"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 5, selectedImageIndex: 5);
                        node.Tag = di;
                    }
                }
            }

            else if (projLangToolStripMenuItem.Text == "ProjLang:C++")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 14;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C++ Project";
                    treeView4.Nodes[0].ImageIndex = 14;
                    treeView4.Nodes[0].SelectedImageIndex = 14;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".cpp"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 16, selectedImageIndex: 16);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".h"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 18, selectedImageIndex: 18);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:C")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 17;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "C Project";
                    treeView4.Nodes[0].ImageIndex = 17;
                    treeView4.Nodes[0].SelectedImageIndex = 17;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".c"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 15, selectedImageIndex: 15);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Java")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 20;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Java Project";
                    treeView4.Nodes[0].ImageIndex = 20;
                    treeView4.Nodes[0].SelectedImageIndex = 20;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".java"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 19, selectedImageIndex: 19);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:Python")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }



                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView4.ImageIndex = 22;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Python Project";
                    treeView4.Nodes[0].ImageIndex = 22;
                    treeView4.Nodes[0].SelectedImageIndex = 22;

                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".py"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 21, selectedImageIndex: 21);
                        node.Tag = di;
                    }
                }
            }

            else if (projLangToolStripMenuItem.Text == "ProjLang:Lua")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }
                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 3;
                    treeView4.Nodes[0].SelectedImageIndex = 3;
                    treeView4.Nodes[0].ImageIndex = 3;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Lua Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".lua"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".luac"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 9, selectedImageIndex: 9);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:WebPage")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 2;
                    treeView4.Nodes[0].SelectedImageIndex = 2;
                    treeView4.Nodes[0].ImageIndex = 2;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Web Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".html"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".htm"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".xhtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".shtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 7, selectedImageIndex: 7);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".js"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 11, selectedImageIndex: 11);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".css"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 8, selectedImageIndex: 8);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:PHP")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 4;
                    treeView4.Nodes[0].SelectedImageIndex = 4;
                    treeView4.Nodes[0].ImageIndex = 4;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "PHP Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".php"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phps"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                    if (di.Name.EndsWith(".phtml"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 10, selectedImageIndex: 10);
                        node.Tag = di;
                    }
                }
            }
            else if (projLangToolStripMenuItem.Text == "ProjLang:VB")
            {
                foreach (var item in Directory.GetDirectories(folderBrowserDialog2.SelectedPath))
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 12, selectedImageIndex: 12);
                    node.Tag = di;
                }

                foreach (var item in Directory.GetFiles(folderBrowserDialog2.SelectedPath))
                {
                    treeView3.Nodes.Clear();
                    treeView4.ImageIndex = 1;
                    treeView4.Nodes[0].SelectedImageIndex = 1;
                    treeView4.Nodes[0].ImageIndex = 1;
                    treeView4.Visible = true;
                    treeView4.Nodes[0].Text = "Visual Basic Project";
                    FileInfo di = new FileInfo(item);

                    if (di.Name.EndsWith(".vb"))
                    {
                        var node = treeView3.Nodes.Add(key: di.Name, text: di.Name, imageIndex: 6, selectedImageIndex: 6);
                        node.Tag = di;
                    }
                }
            }
            else
            {

            }

            Cursor.Current = Cursors.Default;
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            NewItem NewItemPopup = new NewItem();
            NewItemPopup.Show();
            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);

            splitContainer5.Panel1Collapsed = true;
            splitContainer5.Panel2Collapsed = false;
        }

        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem);
        }

        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
            {
                string oldFile = tsFiles.SelectedItem.Tag as string;
                tsFiles.SelectedItem.Tag = null;
                if (!Save(tsFiles.SelectedItem))
                    if (oldFile != null)
                    {
                        tsFiles.SelectedItem.Tag = oldFile;
                        tsFiles.SelectedItem.Title = Path.GetFileName(oldFile);
                    }
            }
        }

        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            if (CurrentTB != null)
            {
                var settings = new PrintDialogSettings();
                settings.Title = tsFiles.SelectedItem.Title;
                settings.Header = "&b&w&b";
                settings.Footer = "&b&p";
                CurrentTB.Print(settings);
            }
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);
            HandleFormClosing(a);
        }

        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowGoToDialog();
        }

        private void toolStripMenuItem28_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        private void toolStripMenuItem29_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            CurrentTB.Copy();
        }

        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            CurrentTB.Cut();
        }

        private void toolStripMenuItem33_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        private void toolStripMenuItem37_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentTB.Language == FastColoredTextBoxNS.Language.HTML) //if language is html
                {
                    HTMLPreview h = new HTMLPreview(CurrentTB.Text);
                    h.Show();
                }
                else if (CurrentTB.Language == FastColoredTextBoxNS.Language.CSharp) //if language is c#
                {
                    toolStripStatusLabel1.Text = "Info: " + "Starting to compile the program";

                    SaveFileDialog sf = new SaveFileDialog();
                    sf.Filter = "Executable File|*.exe";
                    string OutPath = "There is no Output Path to create the program";
                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        OutPath = sf.FileName;
                    }
                    //compile code:
                    //create c# code compiler
                    CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                    //create new parameters for compilation and add references(libs) to compiled app
                    CompilerParameters parameters = new CompilerParameters(new string[] { "System.dll" });
                    //is compiled code will be executable?(.exe)
                    parameters.GenerateExecutable = true;
                    //output path
                    parameters.OutputAssembly = OutPath;
                    //code sources to compile
                    string[] sources = { CurrentTB.Text };
                    //results of compilation
                    CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, sources);

                    // Get the current time
                    string currentTime = DateTime.Now.ToString();


                    toolStripProgressBar1.Enabled = true;

                    toolStripStatusLabel1.Text = "Info: " + "Loading";

                    //if has errors
                    if (results.Errors.Count > 0)
                    {
                        string errsText = "";
                        foreach (CompilerError CompErr in results.Errors)
                        {
                            errsText = "Code Error: (" + CompErr.ErrorNumber +
                                        "), Line " + CompErr.Line +
                                        ", Column " + CompErr.Column +
                                        ": " + CompErr.ErrorText + "" +
                                        Environment.NewLine;
                        }
                        //show error message
                        MessageBox.Show(errsText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        foreach (CompilerError CompErr in results.Errors)
                        {
                            string ErrorText = "Error:" + errsText.ToString();
                            ;
                            Log(DateTime.Now, ErrorText, "\r\n", errorStyle);
                            //richTextBox1.ForeColor = Color.Red;
                            //richTextBox1.Text += currentTime + ": Error: " + errsText.ToString() + "\n";
                        }

                        toolStripStatusLabel1.ForeColor = Color.Red;
                        toolStripStatusLabel1.Text = currentTime + ": Error: " + errsText.ToString();

                    }
                    else
                    {
                        //run compiled app
                        System.Diagnostics.Process.Start(OutPath);

                        FCTBConsole.Text = "";

                        Log(DateTime.Now, " Info: Successful Compilation", "\r\n", infoStyle);

                        toolStripStatusLabel1.ForeColor = Color.Green;
                        toolStripStatusLabel1.Text = currentTime + ": Info: " + "Successful compilation";
                    }
                }
                else
                {
                    MessageBox.Show("INDEV Syntaxies: Cannot open this file!");
                }
            }
            catch (Exception)
            {
                // Handle the exception here
                MessageBox.Show("Error: There is no tab open to compile code.", "Compiler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            About AboutSyntaxies = new About();
            AboutSyntaxies.Show();
        }

        private void label2_TextChanged(object sender, EventArgs e)
        {
            //label2.Text = "INDEV Syntaxies 2024 - " + tab.Title;
        }

        private void hTMLEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

            EnteringHTMLEditor(a);
        }

        private void tbFind_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }

    public class InvisibleCharsRenderer : Style
    {
        Pen pen;

        public InvisibleCharsRenderer(Pen pen)
        {
            this.pen = pen;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            var tb = range.tb;
            using(Brush brush = new SolidBrush(pen.Color))
            foreach (var place in range)
            {
                switch (tb[place].c)
                {
                    case ' ':
                        var point = tb.PlaceToPoint(place);
                        point.Offset(tb.CharWidth / 2, tb.CharHeight / 2);
                        gr.DrawLine(pen, point.X, point.Y, point.X + 1, point.Y);
                        break;
                }

                if (tb[place.iLine].Count - 1 == place.iChar)
                {
                    var point = tb.PlaceToPoint(place);
                    point.Offset(tb.CharWidth, 0);
                    gr.DrawString("¶", tb.Font, brush, point);
                }
            }
        }
    }

    public class TbInfo
    {
        public AutocompleteMenu popupMenu;
    }
}
