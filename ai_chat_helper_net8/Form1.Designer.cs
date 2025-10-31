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
			checkBox2 = new CheckBox();
			bt_removeSelectedRows = new Button();
			tbx_includeFilter = new TextBox();
			label4 = new Label();
			groupBox2 = new GroupBox();
			chk_SplitBy_Type = new CheckBox();
			groupBox3 = new GroupBox();
			bt_ClearFilterTbx = new Button();
			label3 = new Label();
			bt_RemoveFilterHitRows = new Button();
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
			dgv_Files.Location = new Point(288, 115);
			dgv_Files.Name = "dgv_Files";
			dgv_Files.ReadOnly = true;
			dgv_Files.RowHeadersWidth = 51;
			dgv_Files.Size = new Size(756, 479);
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
			lbl_filepaths.Size = new Size(269, 146);
			lbl_filepaths.TabIndex = 2;
			lbl_filepaths.Text = "Drag Files, Folders Here:   <--";
			lbl_filepaths.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// bt_convertToMarkdown
			// 
			bt_convertToMarkdown.FlatStyle = FlatStyle.System;
			bt_convertToMarkdown.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			bt_convertToMarkdown.Location = new Point(31, 154);
			bt_convertToMarkdown.Name = "bt_convertToMarkdown";
			bt_convertToMarkdown.Size = new Size(190, 63);
			bt_convertToMarkdown.TabIndex = 3;
			bt_convertToMarkdown.Text = "Convert / Process";
			bt_convertToMarkdown.UseVisualStyleBackColor = false;
			bt_convertToMarkdown.Click += bt_convertToMarkdown_Click;
			// 
			// chk_mergeAll_intoSingleFile
			// 
			chk_mergeAll_intoSingleFile.AutoSize = true;
			chk_mergeAll_intoSingleFile.Location = new Point(6, 78);
			chk_mergeAll_intoSingleFile.Name = "chk_mergeAll_intoSingleFile";
			chk_mergeAll_intoSingleFile.Size = new Size(190, 24);
			chk_mergeAll_intoSingleFile.TabIndex = 4;
			chk_mergeAll_intoSingleFile.Text = "mergeAll into SingleFile";
			chk_mergeAll_intoSingleFile.UseVisualStyleBackColor = true;
			chk_mergeAll_intoSingleFile.CheckedChanged += chk_mergeAll_intoSingleFile_CheckedChanged;
			// 
			// tbx_outputDir
			// 
			tbx_outputDir.Location = new Point(6, 35);
			tbx_outputDir.Name = "tbx_outputDir";
			tbx_outputDir.Size = new Size(256, 27);
			tbx_outputDir.TabIndex = 5;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(10, 12);
			label1.Name = "label1";
			label1.Size = new Size(82, 20);
			label1.TabIndex = 6;
			label1.Text = "Output Dir:";
			// 
			// bt_clear
			// 
			bt_clear.Location = new Point(884, 39);
			bt_clear.Name = "bt_clear";
			bt_clear.Size = new Size(123, 57);
			bt_clear.TabIndex = 7;
			bt_clear.Text = "Clear";
			bt_clear.UseVisualStyleBackColor = true;
			bt_clear.Click += bt_clear_Click;
			// 
			// tbx_CssSelector_for_Root
			// 
			tbx_CssSelector_for_Root.Location = new Point(6, 84);
			tbx_CssSelector_for_Root.Name = "tbx_CssSelector_for_Root";
			tbx_CssSelector_for_Root.Size = new Size(196, 27);
			tbx_CssSelector_for_Root.TabIndex = 8;
			tbx_CssSelector_for_Root.Text = " .page ";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(10, 61);
			label2.Name = "label2";
			label2.Size = new Size(152, 20);
			label2.TabIndex = 9;
			label2.Text = "Css Selector For Root:";
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(checkBox2);
			groupBox1.Controls.Add(tbx_CssSelector_for_Root);
			groupBox1.Controls.Add(label2);
			groupBox1.Location = new Point(7, 405);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(269, 120);
			groupBox1.TabIndex = 10;
			groupBox1.TabStop = false;
			groupBox1.Text = "HTML:";
			// 
			// checkBox2
			// 
			checkBox2.AutoSize = true;
			checkBox2.Checked = true;
			checkBox2.CheckState = CheckState.Checked;
			checkBox2.Enabled = false;
			checkBox2.Location = new Point(13, 25);
			checkBox2.Name = "checkBox2";
			checkBox2.Size = new Size(163, 24);
			checkBox2.TabIndex = 10;
			checkBox2.Text = "Convert html to .md";
			checkBox2.UseVisualStyleBackColor = true;
			// 
			// bt_removeSelectedRows
			// 
			bt_removeSelectedRows.Location = new Point(755, 40);
			bt_removeSelectedRows.Name = "bt_removeSelectedRows";
			bt_removeSelectedRows.Size = new Size(123, 57);
			bt_removeSelectedRows.TabIndex = 11;
			bt_removeSelectedRows.Text = "Remove Selected_Rows";
			bt_removeSelectedRows.UseVisualStyleBackColor = true;
			bt_removeSelectedRows.Click += bt_removeSelectedRows_Click;
			// 
			// tbx_includeFilter
			// 
			tbx_includeFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			tbx_includeFilter.Location = new Point(0, 36);
			tbx_includeFilter.Name = "tbx_includeFilter";
			tbx_includeFilter.PlaceholderText = "hi ; .md , .txt";
			tbx_includeFilter.Size = new Size(197, 27);
			tbx_includeFilter.TabIndex = 12;
			tbx_includeFilter.TextChanged += tbx_includeFilter_TextChanged;
			// 
			// label4
			// 
			label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			label4.AutoSize = true;
			label4.Location = new Point(108, 13);
			label4.Name = "label4";
			label4.Size = new Size(119, 20);
			label4.TabIndex = 14;
			label4.Text = "ie: \"hi ; .md , .txt\"";
			label4.Visible = false;
			// 
			// groupBox2
			// 
			groupBox2.Controls.Add(tbx_outputDir);
			groupBox2.Controls.Add(chk_SplitBy_Type);
			groupBox2.Controls.Add(label1);
			groupBox2.Controls.Add(bt_convertToMarkdown);
			groupBox2.Controls.Add(chk_mergeAll_intoSingleFile);
			groupBox2.Location = new Point(7, 168);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new Size(269, 226);
			groupBox2.TabIndex = 16;
			groupBox2.TabStop = false;
			// 
			// chk_SplitBy_Type
			// 
			chk_SplitBy_Type.AutoSize = true;
			chk_SplitBy_Type.Location = new Point(31, 108);
			chk_SplitBy_Type.Name = "chk_SplitBy_Type";
			chk_SplitBy_Type.Size = new Size(143, 24);
			chk_SplitBy_Type.TabIndex = 7;
			chk_SplitBy_Type.Text = "Split by File Type";
			chk_SplitBy_Type.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			groupBox3.Controls.Add(tbx_includeFilter);
			groupBox3.Controls.Add(bt_ClearFilterTbx);
			groupBox3.Controls.Add(label3);
			groupBox3.Controls.Add(label4);
			groupBox3.Location = new Point(341, 33);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new Size(230, 64);
			groupBox3.TabIndex = 17;
			groupBox3.TabStop = false;
			// 
			// bt_ClearFilterTbx
			// 
			bt_ClearFilterTbx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			bt_ClearFilterTbx.Location = new Point(197, 35);
			bt_ClearFilterTbx.Name = "bt_ClearFilterTbx";
			bt_ClearFilterTbx.Size = new Size(33, 28);
			bt_ClearFilterTbx.TabIndex = 18;
			bt_ClearFilterTbx.Text = "X";
			bt_ClearFilterTbx.UseVisualStyleBackColor = true;
			bt_ClearFilterTbx.Click += bt_ClearFilterTbx_Click;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(4, 13);
			label3.Name = "label3";
			label3.Size = new Size(45, 20);
			label3.TabIndex = 18;
			label3.Text = "Filter:";
			// 
			// bt_RemoveFilterHitRows
			// 
			bt_RemoveFilterHitRows.Location = new Point(599, 40);
			bt_RemoveFilterHitRows.Name = "bt_RemoveFilterHitRows";
			bt_RemoveFilterHitRows.Size = new Size(123, 57);
			bt_RemoveFilterHitRows.TabIndex = 18;
			bt_RemoveFilterHitRows.Text = "Remove Filter_Hit_Rows";
			bt_RemoveFilterHitRows.UseVisualStyleBackColor = true;
			bt_RemoveFilterHitRows.Click += bt_RemoveFilterHitRows_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(120F, 120F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(1056, 606);
			Controls.Add(bt_RemoveFilterHitRows);
			Controls.Add(dgv_Files);
			Controls.Add(groupBox3);
			Controls.Add(groupBox2);
			Controls.Add(bt_removeSelectedRows);
			Controls.Add(groupBox1);
			Controls.Add(bt_clear);
			Controls.Add(lbl_filepaths);
			MinimumSize = new Size(661, 453);
			Name = "Form1";
			Text = "AI CHAT HELPER ( Merge Files  / Convert .html to .md ) ";
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
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private CheckBox chk_SplitBy_Type;
        private CheckBox checkBox2;
        private Label label3;
        private Button bt_ClearFilterTbx;
		private Button bt_RemoveFilterHitRows;
	}
}
