namespace Tester
{
    partial class HTMLPreview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HTMLPreview));
            this.faTabStrip1 = new FarsiLibrary.Win.FATabStrip();
            this.DesktopMod = new FarsiLibrary.Win.FATabStripItem();
            this.MoblieMod = new FarsiLibrary.Win.FATabStripItem();
            this.TabletMod = new FarsiLibrary.Win.FATabStripItem();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.webBrowser2 = new System.Windows.Forms.WebBrowser();
            this.webBrowser3 = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.faTabStrip1)).BeginInit();
            this.faTabStrip1.SuspendLayout();
            this.DesktopMod.SuspendLayout();
            this.MoblieMod.SuspendLayout();
            this.TabletMod.SuspendLayout();
            this.SuspendLayout();
            // 
            // faTabStrip1
            // 
            this.faTabStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.faTabStrip1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.faTabStrip1.Items.AddRange(new FarsiLibrary.Win.FATabStripItem[] {
            this.DesktopMod,
            this.MoblieMod,
            this.TabletMod});
            this.faTabStrip1.Location = new System.Drawing.Point(0, 0);
            this.faTabStrip1.Name = "faTabStrip1";
            this.faTabStrip1.SelectedItem = this.DesktopMod;
            this.faTabStrip1.Size = new System.Drawing.Size(890, 592);
            this.faTabStrip1.TabIndex = 0;
            this.faTabStrip1.Text = "faTabStrip1";
            // 
            // DesktopMod
            // 
            this.DesktopMod.CanClose = false;
            this.DesktopMod.Controls.Add(this.webBrowser1);
            this.DesktopMod.IsDrawn = true;
            this.DesktopMod.Name = "DesktopMod";
            this.DesktopMod.Selected = true;
            this.DesktopMod.Size = new System.Drawing.Size(888, 571);
            this.DesktopMod.TabIndex = 0;
            this.DesktopMod.Title = "Desktop";
            // 
            // MoblieMod
            // 
            this.MoblieMod.CanClose = false;
            this.MoblieMod.Controls.Add(this.webBrowser2);
            this.MoblieMod.IsDrawn = true;
            this.MoblieMod.Name = "MoblieMod";
            this.MoblieMod.Size = new System.Drawing.Size(888, 571);
            this.MoblieMod.TabIndex = 1;
            this.MoblieMod.Title = "Moblie";
            // 
            // TabletMod
            // 
            this.TabletMod.CanClose = false;
            this.TabletMod.Controls.Add(this.webBrowser3);
            this.TabletMod.IsDrawn = true;
            this.TabletMod.Name = "TabletMod";
            this.TabletMod.Size = new System.Drawing.Size(888, 571);
            this.TabletMod.TabIndex = 2;
            this.TabletMod.Title = "Tablet";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(888, 571);
            this.webBrowser1.TabIndex = 0;
            // 
            // webBrowser2
            // 
            this.webBrowser2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser2.Location = new System.Drawing.Point(284, 0);
            this.webBrowser2.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser2.Name = "webBrowser2";
            this.webBrowser2.Size = new System.Drawing.Size(277, 571);
            this.webBrowser2.TabIndex = 0;
            // 
            // webBrowser3
            // 
            this.webBrowser3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.webBrowser3.Location = new System.Drawing.Point(189, 2);
            this.webBrowser3.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser3.Name = "webBrowser3";
            this.webBrowser3.Size = new System.Drawing.Size(494, 568);
            this.webBrowser3.TabIndex = 0;
            // 
            // HTMLPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 592);
            this.Controls.Add(this.faTabStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HTMLPreview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "INDEV Syntaxies - HTML Preview";
            ((System.ComponentModel.ISupportInitialize)(this.faTabStrip1)).EndInit();
            this.faTabStrip1.ResumeLayout(false);
            this.DesktopMod.ResumeLayout(false);
            this.MoblieMod.ResumeLayout(false);
            this.TabletMod.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FarsiLibrary.Win.FATabStrip faTabStrip1;
        private FarsiLibrary.Win.FATabStripItem DesktopMod;
        private FarsiLibrary.Win.FATabStripItem MoblieMod;
        private FarsiLibrary.Win.FATabStripItem TabletMod;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.WebBrowser webBrowser2;
        private System.Windows.Forms.WebBrowser webBrowser3;
    }
}