namespace ai_chat_helper_net8
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgv_Files = new DataGridView();
            lbl_filepaths = new Label();
            bt_convertToMarkdown = new Button();
            chk_mergeAll_intoSingleFile = new CheckBox();
            tbx_outputDir = new TextBox();
            label1 = new Label();
            bt_clear = new Button();
            tbx_CssSelector_for_Root = new TextBox();
            label2 = new Label();
            groupBox1 = new GroupBox();
            bt_removeSelectedRows = new Button();
            tbx_includeFilter = new TextBox();
            label4 = new Label();
            bt_filterExample = new Button();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dgv_Files).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // dgv_Files
            // 
            dgv_Files.AllowUserToAddRows = false;
            dgv_Files.AllowUserToDeleteRows = false;
            dgv_Files.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv_Files.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_Files.Location = new Point(288, 75);
            dgv_Files.Name = "dgv_Files";
            dgv_Files.ReadOnly = true;
            dgv_Files.RowHeadersWidth = 51;
            dgv_Files.Size = new Size(714, 669);
            dgv_Files.TabIndex = 1;
            // 
            // lbl_filepaths
            // 
            lbl_filepaths.BackColor = Color.MidnightBlue;
            lbl_filepaths.BorderStyle = BorderStyle.FixedSingle;
            lbl_filepaths.FlatStyle = FlatStyle.Flat;
            lbl_filepaths.Font = new Font("Segoe UI Semibold", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_filepaths.ForeColor = Color.White;
            lbl_filepaths.Location = new Point(7, 9);
            lbl_filepaths.Name = "lbl_filepaths";
            lbl_filepaths.Size = new Size(256, 146);
            lbl_filepaths.TabIndex = 2;
            lbl_filepaths.Text = "Drag Files, Folders Here:   <--";
            lbl_filepaths.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bt_convertToMarkdown
            // 
            bt_convertToMarkdown.FlatStyle = FlatStyle.System;
            bt_convertToMarkdown.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            bt_convertToMarkdown.Location = new Point(31, 135);
            bt_convertToMarkdown.Name = "bt_convertToMarkdown";
            bt_convertToMarkdown.Size = new Size(190, 63);
            bt_convertToMarkdown.TabIndex = 3;
            bt_convertToMarkdown.Text = "Convert Html To Markdown";
            bt_convertToMarkdown.UseVisualStyleBackColor = false;
            bt_convertToMarkdown.Click += bt_convertToMarkdown_Click;
            // 
            // chk_mergeAll_intoSingleFile
            // 
            chk_mergeAll_intoSingleFile.AutoSize = true;
            chk_mergeAll_intoSingleFile.Location = new Point(15, 86);
            chk_mergeAll_intoSingleFile.Name = "chk_mergeAll_intoSingleFile";
            chk_mergeAll_intoSingleFile.Size = new Size(190, 24);
            chk_mergeAll_intoSingleFile.TabIndex = 4;
            chk_mergeAll_intoSingleFile.Text = "mergeAll into SingleFile";
            chk_mergeAll_intoSingleFile.UseVisualStyleBackColor = true;
            // 
            // tbx_outputDir
            // 
            tbx_outputDir.Location = new Point(13, 38);
            tbx_outputDir.Name = "tbx_outputDir";
            tbx_outputDir.Size = new Size(247, 27);
            tbx_outputDir.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 15);
            label1.Name = "label1";
            label1.Size = new Size(82, 20);
            label1.TabIndex = 6;
            label1.Text = "Output Dir:";
            // 
            // bt_clear
            // 
            bt_clear.Location = new Point(317, 12);
            bt_clear.Name = "bt_clear";
            bt_clear.Size = new Size(126, 57);
            bt_clear.TabIndex = 7;
            bt_clear.Text = "Clear";
            bt_clear.UseVisualStyleBackColor = true;
            bt_clear.Click += bt_clear_Click;
            // 
            // tbx_CssSelector_for_Root
            // 
            tbx_CssSelector_for_Root.Location = new Point(13, 48);
            tbx_CssSelector_for_Root.Name = "tbx_CssSelector_for_Root";
            tbx_CssSelector_for_Root.Size = new Size(196, 27);
            tbx_CssSelector_for_Root.TabIndex = 8;
            tbx_CssSelector_for_Root.Text = " .page ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 25);
            label2.Name = "label2";
            label2.Size = new Size(152, 20);
            label2.TabIndex = 9;
            label2.Text = "Css Selector For Root:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tbx_CssSelector_for_Root);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(7, 620);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(269, 105);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "HTML:";
            // 
            // bt_removeSelectedRows
            // 
            bt_removeSelectedRows.Location = new Point(476, 12);
            bt_removeSelectedRows.Name = "bt_removeSelectedRows";
            bt_removeSelectedRows.Size = new Size(126, 57);
            bt_removeSelectedRows.TabIndex = 11;
            bt_removeSelectedRows.Text = "Remove Selected_Rows";
            bt_removeSelectedRows.UseVisualStyleBackColor = true;
            bt_removeSelectedRows.Click += bt_removeSelectedRows_Click;
            // 
            // tbx_includeFilter
            // 
            tbx_includeFilter.Location = new Point(6, 56);
            tbx_includeFilter.Multiline = true;
            tbx_includeFilter.Name = "tbx_includeFilter";
            tbx_includeFilter.ScrollBars = ScrollBars.Both;
            tbx_includeFilter.Size = new Size(256, 121);
            tbx_includeFilter.TabIndex = 12;
            tbx_includeFilter.TextChanged += tbx_includeFilter_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(5, 33);
            label4.Name = "label4";
            label4.Size = new Size(125, 20);
            label4.TabIndex = 14;
            label4.Text = "ie:\" hi ; ini , docs\" ";
            // 
            // bt_filterExample
            // 
            bt_filterExample.Location = new Point(173, 23);
            bt_filterExample.Name = "bt_filterExample";
            bt_filterExample.Size = new Size(89, 30);
            bt_filterExample.TabIndex = 15;
            bt_filterExample.Text = "Example";
            bt_filterExample.UseVisualStyleBackColor = true;
            bt_filterExample.Click += bt_filterExample_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(bt_convertToMarkdown);
            groupBox2.Controls.Add(chk_mergeAll_intoSingleFile);
            groupBox2.Controls.Add(tbx_outputDir);
            groupBox2.Location = new Point(7, 179);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(269, 224);
            groupBox2.TabIndex = 16;
            groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(tbx_includeFilter);
            groupBox3.Controls.Add(bt_filterExample);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(7, 419);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(269, 183);
            groupBox3.TabIndex = 17;
            groupBox3.TabStop = false;
            groupBox3.Text = "FileName Include Filter";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1014, 756);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(bt_removeSelectedRows);
            Controls.Add(groupBox1);
            Controls.Add(bt_clear);
            Controls.Add(lbl_filepaths);
            Controls.Add(dgv_Files);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgv_Files).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private DataGridView dgv_Files;
        private Label lbl_filepaths;
        private Button bt_convertToMarkdown;
        private CheckBox chk_mergeAll_intoSingleFile;
        private TextBox tbx_outputDir;
        private Label label1;
        private Button bt_clear;
        private TextBox tbx_CssSelector_for_Root;
        private Label label2;
        private GroupBox groupBox1;
        private Button bt_removeSelectedRows;
        private TextBox tbx_includeFilter;
        private Label label4;
        private Button bt_filterExample;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
    }
}
