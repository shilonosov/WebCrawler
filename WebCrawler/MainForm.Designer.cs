namespace WebCrawler
{
    partial class MainForm
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
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.treeViewResults = new System.Windows.Forms.TreeView();
            this.numericUpDownDeepLevel = new System.Windows.Forms.NumericUpDown();
            this.buttonClear = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeepLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.AcceptsReturn = true;
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.Location = new System.Drawing.Point(12, 12);
            this.textBoxUrl.Margin = new System.Windows.Forms.Padding(3, 3, 29, 3);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(429, 22);
            this.textBoxUrl.TabIndex = 0;
            this.textBoxUrl.Text = "http://www.timera.com";
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(537, 12);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(82, 23);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Crawl";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // treeViewResults
            // 
            this.treeViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewResults.Location = new System.Drawing.Point(12, 40);
            this.treeViewResults.Name = "treeViewResults";
            this.treeViewResults.Size = new System.Drawing.Size(695, 507);
            this.treeViewResults.TabIndex = 3;
            // 
            // numericUpDown1
            // 
            this.numericUpDownDeepLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownDeepLevel.Location = new System.Drawing.Point(449, 12);
            this.numericUpDownDeepLevel.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numericUpDownDeepLevel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDeepLevel.Name = "numericUpDownDeepLevel";
            this.numericUpDownDeepLevel.Size = new System.Drawing.Size(82, 22);
            this.numericUpDownDeepLevel.TabIndex = 1;
            this.numericUpDownDeepLevel.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(625, 12);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(82, 23);
            this.buttonClear.TabIndex = 4;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.ButtonClearClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 559);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.numericUpDownDeepLevel);
            this.Controls.Add(this.treeViewResults);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.textBoxUrl);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "MainForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeepLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TreeView treeViewResults;
        private System.Windows.Forms.NumericUpDown numericUpDownDeepLevel;
        private System.Windows.Forms.Button buttonClear;
    }
}

