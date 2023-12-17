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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tester
{
    public partial class HTMLEditor : Form
    {
        public static HTMLEditor HTMLinstance;
        public ToolStripMenuItem HTMLSelcelLang;

        string[] HTMLdeclarationSnippets = {
               "<!DOCTYPE html>\r\n    \r<html>\r\n        \r\r<head>\r\n        \r\r</head>\r\n        \r\r<body>\r\n            \r\r\r^\r\n        \r\r</body>\r\n    \r</html>", "<h1>^<h1>", "<h2>^</h2>", "<h3>^</h3>", "<h4>^</h4>", "<h5>^</h5>", "<p>^</p>", "<style>\r\n</style>", "<div>\r\n</giv>"
               };

        public HTMLEditor()
        {
            InitializeComponent();
            HTMLinstance = this;
            HTMLSelcelLang = langToolStripMenuItem;
        }

        private void BuildAutocompleteMenu(AutocompleteMenu popupMenu)
        {
            if (CurrentTB.Language == Language.HTML)
            {
                List<AutocompleteItem> items = new List<AutocompleteItem>();
                foreach (var item in HTMLdeclarationSnippets)
                    items.Add(new DeclarationSnippet(item) { ImageIndex = 2 });

                items.Add(new InsertSpaceSnippet());
                items.Add(new InsertSpaceSnippet(@"^(\w+)([=<>!:]+)(\w+)$"));
                items.Add(new InsertEnterSnippet());

                //set as autocomplete source
                popupMenu.Items.SetAutocompleteItems(items);
                popupMenu.SearchPattern = @"[\w\.:=!<>]";
            }
        }

        private Style sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(50, Color.Gray)));

        void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox tb = (sender as FastColoredTextBox);

            //show invisible chars
            HighlightInvisibleChars(e.ChangedRange);
        }

        Style invisibleCharsStyle = new InvisibleCharsRenderer(Pens.Gray);
        Color currentLineColor = Color.FromArgb(100, 210, 210, 255);
        Color changedLineColor = Color.FromArgb(255, 230, 230, 255);

        private void HighlightInvisibleChars(Range range)
        {
            range.ClearStyle(invisibleCharsStyle);
            if (btInvisibleChars.Checked)
                range.SetStyle(invisibleCharsStyle, @".$|.\r\n|\s");
        }

        DateTime lastNavigatedDateTime = DateTime.Now;

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

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.K | Keys.Control))
            {
                //forced show (MinFragmentLength will be ignored)
                (CurrentTB.Tag as TbInfo).popupMenu.Show(true);
                e.Handled = true;
            }
        }

        FastColoredTextBox CurrentTB
        {
            get
            {
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

        void tb_MouseMove(object sender, MouseEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            var place = tb.PointToPlace(e.Location);
            var r = new Range(tb, place, place);

            string text = r.GetFragment("[a-zA-Z]").Text;
            lbWordUnderMouse.Text = text;
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

        private void CreateTab(string fileName)
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
                tb.Language = Language.HTML;
                tb.AddStyle(sameWordsStyle);//same words style
                var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[new]", tb);
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

        private void langToolStripMenuItem_TextChanged(object sender, EventArgs e)
        {
            if (langToolStripMenuItem.Text == "Lang:HTML")
            {
                string fileName = null;
                try
                {
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
            if (langToolStripMenuItem.Text == "Lang:CSS")
            {
                string fileName = null;
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
                    var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "new.css", tb);
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
            if (langToolStripMenuItem.Text == "Lang:JS")
            {
                string fileName = null;
                try
                {
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
            if (langToolStripMenuItem.Text == "Lang:PHP")
            {
                string fileName = null;
                try
                {
                    var tb = new FastColoredTextBox();
                    tb.Font = new Font("Consolas", 9.75f);
                    tb.ContextMenuStrip = cmMain;
                    tb.Dock = DockStyle.Fill;
                    tb.BorderStyle = BorderStyle.None;
                    //tb.VirtualSpace = true;
                    tb.LeftPadding = 17;
                    tb.Language = Language.JS;
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
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            NewItemHTML newItemHTMLFile = new NewItemHTML();
            newItemHTMLFile.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = true;
            splitContainer1.Panel2Collapsed = false;

            webBrowser1.DocumentText = CurrentTB.Text;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = true;
            splitContainer1.Panel1Collapsed = false;
            webBrowser1.DocumentText = CurrentTB.Text;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel1Collapsed = false;
            splitContainer1.Panel2Collapsed = false;
            webBrowser1.DocumentText = CurrentTB.Text;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = CurrentTB.Text;
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);
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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (tsFiles.SelectedItem != null)
                Save(tsFiles.SelectedItem);
        }

        private void saveasToolStripButton_Click(object sender, EventArgs e)
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

        private void redoStripButton_Click(object sender, EventArgs e)
        {
            if (CurrentTB.UndoEnabled)
                CurrentTB.Undo();
        }

        private void undoStripButton_Click(object sender, EventArgs e)
        {
            if (CurrentTB.RedoEnabled)
                CurrentTB.Redo();
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            CurrentTB.Cut();
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            CurrentTB.Copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        private void btInvisibleChars_Click(object sender, EventArgs e)
        {
            foreach (FATabStripItem tab in tsFiles.Items)
                HighlightInvisibleChars((tab.Controls[0] as FastColoredTextBox).Range);
            if (CurrentTB != null)
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

        private void btShowFoldingLines_Click(object sender, EventArgs e)
        {
            foreach (FATabStripItem tab in tsFiles.Items)
                (tab.Controls[0] as FastColoredTextBox).ShowFoldingLines = btShowFoldingLines.Checked;
            if (CurrentTB != null)
                CurrentTB.Invalidate();
        }

        bool tbFindChanged = false;

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && CurrentTB != null)
            {
                Range r = tbFindChanged ? CurrentTB.Range.Clone() : CurrentTB.Selection.Clone();
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

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowGoToDialog();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            CurrentTB.ShowReplaceDialog();
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            NewItemHTML newItemHTMLFile = new NewItemHTML();
            newItemHTMLFile.Show();
        }

        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);
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

        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            About AboutSyntaxies = new About();
            AboutSyntaxies.Show();
        }

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

        private void tsFiles_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            if ((e.Item.Controls[0] as FastColoredTextBox).IsChanged)
            {
                switch (MessageBox.Show("Do you want save " + e.Item.Title + " ?", " INDEV Syntaxies - Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
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
            this.Hide();
            Editor EditorMode = new Editor();
            EditorMode.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

            HandleFormClosing(a);
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

            HandleFormClosing(a);
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            NewItemHTML newItemHTMLFile = new NewItemHTML();
            newItemHTMLFile.Show();
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            if (ofdMain.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CreateTab(ofdMain.FileName);
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

        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs a = new FormClosingEventArgs(CloseReason.ApplicationExitCall, false);

            HandleFormClosing(a);
        }

        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void tmUpdateInterface_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentTB != null && tsFiles.Items.Count > 0)
                {
                    var tb = CurrentTB;
                    undoStripButton.Enabled = undoToolStripMenuItem.Enabled = tb.UndoEnabled;
                    redoStripButton.Enabled = redoToolStripMenuItem.Enabled = tb.RedoEnabled;
                    toolStripButton1.Enabled = toolStripButton1.Enabled = tb.IsChanged;
                    saveasToolStripButton.Enabled = true;
                    pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = true;
                    cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled =
                    copyToolStripButton.Enabled = copyToolStripMenuItem.Enabled = !tb.Selection.IsEmpty;
                }
                else
                {
                    toolStripButton1.Enabled = toolStripButton1.Enabled = false;
                    saveasToolStripButton.Enabled = false;
                    cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled =
                    copyToolStripButton.Enabled = copyToolStripMenuItem.Enabled = false;
                    pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled = false;
                    undoStripButton.Enabled = undoToolStripMenuItem.Enabled = false;
                    redoStripButton.Enabled = redoToolStripMenuItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = CurrentTB.Text;
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
}
