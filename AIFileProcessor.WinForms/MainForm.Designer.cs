using System.Drawing;
using System.Windows.Forms;

namespace AIFileProcessor
{
    partial class MainForm
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                _processingCts?.Dispose();
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
            tlpButtons = new TableLayoutPanel();
            btnFormatResponse = new Button();
            btnOpenPrompt = new Button();
            btnOpenData = new Button();
            btnProcess = new Button();
            btnSaveResponse = new Button();
            btnClose = new Button();
            btnSaveFormatted = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openDataToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            tlpViewSelect = new TableLayoutPanel();
            rdoCorrections = new RadioButton();
            rdoScratchpad = new RadioButton();
            rdoPrompt = new RadioButton();
            rdoData = new RadioButton();
            rdoFormatted = new RadioButton();
            rdoResponse = new RadioButton();
            rtbPrompt = new RichTextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            ddlModel = new ComboBox();
            lblModel = new Label();
            lblPrefix = new Label();
            txtPrefix = new TextBox();
            lblStart = new Label();
            nudIdStart = new NumericUpDown();
            lblLengthPad = new Label();
            lblIdPad = new Label();
            nudLengthPad = new NumericUpDown();
            nudIdPad = new NumericUpDown();
            rtbResponse = new RichTextBox();
            rtbData = new RichTextBox();
            rtbScratchpad = new RichTextBox();
            rtbCorrections = new RichTextBox();
            rtbFormatted = new RichTextBox();
            tlpButtons.SuspendLayout();
            menuStrip1.SuspendLayout();
            tlpViewSelect.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudIdStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLengthPad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudIdPad).BeginInit();
            SuspendLayout();
            // 
            // tlpButtons
            // 
            tlpButtons.AllowDrop = true;
            tlpButtons.ColumnCount = 7;
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.32F));
            tlpButtons.Controls.Add(btnFormatResponse, 4, 0);
            tlpButtons.Controls.Add(btnOpenPrompt, 0, 0);
            tlpButtons.Controls.Add(btnOpenData, 1, 0);
            tlpButtons.Controls.Add(btnProcess, 2, 0);
            tlpButtons.Controls.Add(btnSaveResponse, 3, 0);
            tlpButtons.Controls.Add(btnClose, 6, 0);
            tlpButtons.Controls.Add(btnSaveFormatted, 5, 0);
            tlpButtons.Dock = DockStyle.Bottom;
            tlpButtons.Location = new Point(0, 1014);
            tlpButtons.Margin = new Padding(3, 4, 3, 4);
            tlpButtons.Name = "tlpButtons";
            tlpButtons.Padding = new Padding(9, 8, 9, 8);
            tlpButtons.RowCount = 1;
            tlpButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpButtons.Size = new Size(1375, 82);
            tlpButtons.TabIndex = 7;
            // 
            // btnFormatResponse
            // 
            btnFormatResponse.Dock = DockStyle.Fill;
            btnFormatResponse.Location = new Point(790, 16);
            btnFormatResponse.Margin = new Padding(9, 8, 9, 8);
            btnFormatResponse.Name = "btnFormatResponse";
            btnFormatResponse.Size = new Size(175, 50);
            btnFormatResponse.TabIndex = 16;
            btnFormatResponse.Text = "Format";
            btnFormatResponse.UseVisualStyleBackColor = true;
            btnFormatResponse.Click += btnFormatResponse_Click;
            // 
            // btnOpenPrompt
            // 
            btnOpenPrompt.Dock = DockStyle.Fill;
            btnOpenPrompt.Location = new Point(18, 16);
            btnOpenPrompt.Margin = new Padding(9, 8, 9, 8);
            btnOpenPrompt.Name = "btnOpenPrompt";
            btnOpenPrompt.Size = new Size(175, 50);
            btnOpenPrompt.TabIndex = 8;
            btnOpenPrompt.Text = "Open Prompt";
            btnOpenPrompt.UseVisualStyleBackColor = true;
            btnOpenPrompt.Click += btnOpenPrompt_Click;
            // 
            // btnOpenData
            // 
            btnOpenData.Dock = DockStyle.Fill;
            btnOpenData.Location = new Point(211, 16);
            btnOpenData.Margin = new Padding(9, 8, 9, 8);
            btnOpenData.Name = "btnOpenData";
            btnOpenData.Size = new Size(175, 50);
            btnOpenData.TabIndex = 10;
            btnOpenData.Text = "Open Data";
            btnOpenData.UseVisualStyleBackColor = true;
            btnOpenData.Click += btnOpenData_Click;
            // 
            // btnProcess
            // 
            btnProcess.Dock = DockStyle.Fill;
            btnProcess.Location = new Point(404, 16);
            btnProcess.Margin = new Padding(9, 8, 9, 8);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(175, 50);
            btnProcess.TabIndex = 3;
            btnProcess.Text = "Process";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // btnSaveResponse
            // 
            btnSaveResponse.Dock = DockStyle.Fill;
            btnSaveResponse.Location = new Point(597, 16);
            btnSaveResponse.Margin = new Padding(9, 8, 9, 8);
            btnSaveResponse.Name = "btnSaveResponse";
            btnSaveResponse.Size = new Size(175, 50);
            btnSaveResponse.TabIndex = 11;
            btnSaveResponse.Text = "Save Response";
            btnSaveResponse.UseVisualStyleBackColor = true;
            btnSaveResponse.Click += btnSaveResponse_Click;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Fill;
            btnClose.Location = new Point(1176, 16);
            btnClose.Margin = new Padding(9, 8, 9, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(181, 50);
            btnClose.TabIndex = 14;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSaveFormatted
            // 
            btnSaveFormatted.Dock = DockStyle.Fill;
            btnSaveFormatted.Location = new Point(983, 16);
            btnSaveFormatted.Margin = new Padding(9, 8, 9, 8);
            btnSaveFormatted.Name = "btnSaveFormatted";
            btnSaveFormatted.Size = new Size(175, 50);
            btnSaveFormatted.TabIndex = 15;
            btnSaveFormatted.Text = "Save Formatted";
            btnSaveFormatted.UseVisualStyleBackColor = true;
            btnSaveFormatted.Click += btnSaveFormatted_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(28, 28);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new Size(1375, 38);
            menuStrip1.TabIndex = 13;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openDataToolStripMenuItem, openToolStripMenuItem, settingsToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(62, 34);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openDataToolStripMenuItem
            // 
            openDataToolStripMenuItem.Name = "openDataToolStripMenuItem";
            openDataToolStripMenuItem.Size = new Size(256, 40);
            openDataToolStripMenuItem.Text = "Open &Data";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(256, 40);
            openToolStripMenuItem.Text = "Open &Prompt";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(256, 40);
            settingsToolStripMenuItem.Text = "&Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(256, 40);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // tlpViewSelect
            // 
            tlpViewSelect.ColumnCount = 6;
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6750031F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.Controls.Add(rdoCorrections, 3, 0);
            tlpViewSelect.Controls.Add(rdoScratchpad, 2, 0);
            tlpViewSelect.Controls.Add(rdoPrompt, 0, 0);
            tlpViewSelect.Controls.Add(rdoData, 1, 0);
            tlpViewSelect.Controls.Add(rdoFormatted, 5, 0);
            tlpViewSelect.Controls.Add(rdoResponse, 4, 0);
            tlpViewSelect.Dock = DockStyle.Bottom;
            tlpViewSelect.Location = new Point(0, 942);
            tlpViewSelect.Margin = new Padding(3, 4, 3, 4);
            tlpViewSelect.Name = "tlpViewSelect";
            tlpViewSelect.RowCount = 1;
            tlpViewSelect.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpViewSelect.Size = new Size(1375, 72);
            tlpViewSelect.TabIndex = 13;
            // 
            // rdoCorrections
            // 
            rdoCorrections.Appearance = Appearance.Button;
            rdoCorrections.AutoSize = true;
            rdoCorrections.Dock = DockStyle.Fill;
            rdoCorrections.Location = new Point(696, 8);
            rdoCorrections.Margin = new Padding(9, 8, 9, 8);
            rdoCorrections.Name = "rdoCorrections";
            rdoCorrections.Size = new Size(211, 56);
            rdoCorrections.TabIndex = 19;
            rdoCorrections.Text = "Corrections";
            rdoCorrections.TextAlign = ContentAlignment.MiddleCenter;
            rdoCorrections.UseVisualStyleBackColor = true;
            rdoCorrections.CheckedChanged += rdoCorrections_CheckedChanged;
            // 
            // rdoScratchpad
            // 
            rdoScratchpad.Appearance = Appearance.Button;
            rdoScratchpad.AutoSize = true;
            rdoScratchpad.Dock = DockStyle.Fill;
            rdoScratchpad.Location = new Point(467, 8);
            rdoScratchpad.Margin = new Padding(9, 8, 9, 8);
            rdoScratchpad.Name = "rdoScratchpad";
            rdoScratchpad.Size = new Size(211, 56);
            rdoScratchpad.TabIndex = 17;
            rdoScratchpad.Text = "Scratchpad";
            rdoScratchpad.TextAlign = ContentAlignment.MiddleCenter;
            rdoScratchpad.UseVisualStyleBackColor = true;
            rdoScratchpad.CheckedChanged += rdoScratchpad_CheckedChanged;
            // 
            // rdoPrompt
            // 
            rdoPrompt.Appearance = Appearance.Button;
            rdoPrompt.AutoSize = true;
            rdoPrompt.Checked = true;
            rdoPrompt.Dock = DockStyle.Fill;
            rdoPrompt.Location = new Point(9, 8);
            rdoPrompt.Margin = new Padding(9, 8, 9, 8);
            rdoPrompt.Name = "rdoPrompt";
            rdoPrompt.Size = new Size(211, 56);
            rdoPrompt.TabIndex = 15;
            rdoPrompt.TabStop = true;
            rdoPrompt.Text = "Prompt";
            rdoPrompt.TextAlign = ContentAlignment.MiddleCenter;
            rdoPrompt.UseVisualStyleBackColor = true;
            rdoPrompt.CheckedChanged += rdoPrompt_CheckedChanged;
            // 
            // rdoData
            // 
            rdoData.Appearance = Appearance.Button;
            rdoData.AutoSize = true;
            rdoData.Dock = DockStyle.Fill;
            rdoData.Location = new Point(238, 8);
            rdoData.Margin = new Padding(9, 8, 9, 8);
            rdoData.Name = "rdoData";
            rdoData.Size = new Size(211, 56);
            rdoData.TabIndex = 12;
            rdoData.Text = "Data";
            rdoData.TextAlign = ContentAlignment.MiddleCenter;
            rdoData.UseVisualStyleBackColor = true;
            rdoData.CheckedChanged += rdoData_CheckedChanged;
            // 
            // rdoFormatted
            // 
            rdoFormatted.Appearance = Appearance.Button;
            rdoFormatted.AutoSize = true;
            rdoFormatted.Dock = DockStyle.Fill;
            rdoFormatted.Location = new Point(1154, 8);
            rdoFormatted.Margin = new Padding(9, 8, 9, 8);
            rdoFormatted.Name = "rdoFormatted";
            rdoFormatted.Size = new Size(212, 56);
            rdoFormatted.TabIndex = 18;
            rdoFormatted.Text = "Formatted";
            rdoFormatted.TextAlign = ContentAlignment.MiddleCenter;
            rdoFormatted.UseVisualStyleBackColor = true;
            rdoFormatted.CheckedChanged += rdoFormatted_CheckedChanged;
            // 
            // rdoResponse
            // 
            rdoResponse.Appearance = Appearance.Button;
            rdoResponse.AutoSize = true;
            rdoResponse.Dock = DockStyle.Fill;
            rdoResponse.Location = new Point(925, 8);
            rdoResponse.Margin = new Padding(9, 8, 9, 8);
            rdoResponse.Name = "rdoResponse";
            rdoResponse.Size = new Size(211, 56);
            rdoResponse.TabIndex = 13;
            rdoResponse.Text = "Response";
            rdoResponse.TextAlign = ContentAlignment.MiddleCenter;
            rdoResponse.UseVisualStyleBackColor = true;
            rdoResponse.CheckedChanged += rdoResponse_CheckedChanged;
            // 
            // rtbPrompt
            // 
            rtbPrompt.DetectUrls = false;
            rtbPrompt.Dock = DockStyle.Fill;
            rtbPrompt.Font = new Font("Segoe UI", 9F);
            rtbPrompt.Location = new Point(0, 38);
            rtbPrompt.Margin = new Padding(3, 4, 3, 4);
            rtbPrompt.Name = "rtbPrompt";
            rtbPrompt.ReadOnly = true;
            rtbPrompt.Size = new Size(1375, 904);
            rtbPrompt.TabIndex = 14;
            rtbPrompt.Text = "";
            rtbPrompt.WordWrap = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 10;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.Controls.Add(ddlModel, 1, 0);
            tableLayoutPanel1.Controls.Add(lblModel, 0, 0);
            tableLayoutPanel1.Controls.Add(lblPrefix, 2, 0);
            tableLayoutPanel1.Controls.Add(txtPrefix, 3, 0);
            tableLayoutPanel1.Controls.Add(lblStart, 4, 0);
            tableLayoutPanel1.Controls.Add(nudIdStart, 5, 0);
            tableLayoutPanel1.Controls.Add(lblLengthPad, 8, 0);
            tableLayoutPanel1.Controls.Add(lblIdPad, 6, 0);
            tableLayoutPanel1.Controls.Add(nudLengthPad, 9, 0);
            tableLayoutPanel1.Controls.Add(nudIdPad, 7, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 38);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1375, 40);
            tableLayoutPanel1.TabIndex = 18;
            // 
            // ddlModel
            // 
            ddlModel.Dock = DockStyle.Fill;
            ddlModel.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlModel.FormattingEnabled = true;
            ddlModel.Location = new Point(83, 3);
            ddlModel.Name = "ddlModel";
            ddlModel.Size = new Size(348, 38);
            ddlModel.TabIndex = 35;
            // 
            // lblModel
            // 
            lblModel.AutoSize = true;
            lblModel.Dock = DockStyle.Fill;
            lblModel.Location = new Point(3, 0);
            lblModel.Name = "lblModel";
            lblModel.Size = new Size(74, 40);
            lblModel.TabIndex = 34;
            lblModel.Text = "Model:";
            lblModel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblPrefix
            // 
            lblPrefix.AutoSize = true;
            lblPrefix.Dock = DockStyle.Fill;
            lblPrefix.Location = new Point(437, 0);
            lblPrefix.Name = "lblPrefix";
            lblPrefix.Size = new Size(94, 40);
            lblPrefix.TabIndex = 27;
            lblPrefix.Text = "Id &Prefix:";
            lblPrefix.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPrefix
            // 
            txtPrefix.Dock = DockStyle.Fill;
            txtPrefix.Location = new Point(537, 4);
            txtPrefix.Margin = new Padding(3, 4, 3, 4);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Size = new Size(126, 35);
            txtPrefix.TabIndex = 28;
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Dock = DockStyle.Fill;
            lblStart.Location = new Point(669, 0);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(84, 40);
            lblStart.TabIndex = 29;
            lblStart.Text = "Id &Start:";
            lblStart.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudIdStart
            // 
            nudIdStart.Dock = DockStyle.Fill;
            nudIdStart.Location = new Point(759, 4);
            nudIdStart.Margin = new Padding(3, 4, 3, 4);
            nudIdStart.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudIdStart.Name = "nudIdStart";
            nudIdStart.Size = new Size(126, 35);
            nudIdStart.TabIndex = 36;
            nudIdStart.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblLengthPad
            // 
            lblLengthPad.AutoSize = true;
            lblLengthPad.Dock = DockStyle.Fill;
            lblLengthPad.Location = new Point(1113, 0);
            lblLengthPad.Name = "lblLengthPad";
            lblLengthPad.Size = new Size(124, 40);
            lblLengthPad.TabIndex = 37;
            lblLengthPad.Text = "Length Pad:";
            lblLengthPad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblIdPad
            // 
            lblIdPad.AutoSize = true;
            lblIdPad.Dock = DockStyle.Fill;
            lblIdPad.Location = new Point(891, 0);
            lblIdPad.Name = "lblIdPad";
            lblIdPad.Size = new Size(84, 40);
            lblIdPad.TabIndex = 31;
            lblIdPad.Text = "Id Pad:";
            lblIdPad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudLengthPad
            // 
            nudLengthPad.Dock = DockStyle.Fill;
            nudLengthPad.Location = new Point(1243, 4);
            nudLengthPad.Margin = new Padding(3, 4, 3, 4);
            nudLengthPad.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudLengthPad.Name = "nudLengthPad";
            nudLengthPad.Size = new Size(129, 35);
            nudLengthPad.TabIndex = 38;
            nudLengthPad.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // nudIdPad
            // 
            nudIdPad.Dock = DockStyle.Fill;
            nudIdPad.Location = new Point(981, 4);
            nudIdPad.Margin = new Padding(3, 4, 3, 4);
            nudIdPad.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudIdPad.Name = "nudIdPad";
            nudIdPad.Size = new Size(126, 35);
            nudIdPad.TabIndex = 32;
            nudIdPad.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // rtbResponse
            // 
            rtbResponse.DetectUrls = false;
            rtbResponse.Dock = DockStyle.Fill;
            rtbResponse.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rtbResponse.Location = new Point(0, 78);
            rtbResponse.Margin = new Padding(3, 4, 3, 4);
            rtbResponse.Name = "rtbResponse";
            rtbResponse.ReadOnly = true;
            rtbResponse.Size = new Size(1375, 864);
            rtbResponse.TabIndex = 21;
            rtbResponse.Text = "";
            rtbResponse.Visible = false;
            rtbResponse.WordWrap = false;
            // 
            // rtbData
            // 
            rtbData.DetectUrls = false;
            rtbData.Dock = DockStyle.Fill;
            rtbData.Font = new Font("Segoe UI", 9F);
            rtbData.Location = new Point(0, 78);
            rtbData.Margin = new Padding(3, 4, 3, 4);
            rtbData.Name = "rtbData";
            rtbData.ReadOnly = true;
            rtbData.Size = new Size(1375, 864);
            rtbData.TabIndex = 22;
            rtbData.Text = "";
            rtbData.Visible = false;
            rtbData.WordWrap = false;
            // 
            // rtbScratchpad
            // 
            rtbScratchpad.DetectUrls = false;
            rtbScratchpad.Dock = DockStyle.Fill;
            rtbScratchpad.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rtbScratchpad.Location = new Point(0, 78);
            rtbScratchpad.Margin = new Padding(3, 4, 3, 4);
            rtbScratchpad.Name = "rtbScratchpad";
            rtbScratchpad.ReadOnly = true;
            rtbScratchpad.Size = new Size(1375, 864);
            rtbScratchpad.TabIndex = 24;
            rtbScratchpad.Text = "";
            rtbScratchpad.Visible = false;
            rtbScratchpad.WordWrap = false;
            // 
            // rtbCorrections
            // 
            rtbCorrections.DetectUrls = false;
            rtbCorrections.Dock = DockStyle.Fill;
            rtbCorrections.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            rtbCorrections.Location = new Point(0, 78);
            rtbCorrections.Margin = new Padding(3, 4, 3, 4);
            rtbCorrections.Name = "rtbCorrections";
            rtbCorrections.ReadOnly = true;
            rtbCorrections.Size = new Size(1375, 864);
            rtbCorrections.TabIndex = 25;
            rtbCorrections.Text = "";
            rtbCorrections.Visible = false;
            rtbCorrections.WordWrap = false;
            // 
            // rtbFormatted
            // 
            rtbFormatted.Dock = DockStyle.Fill;
            rtbFormatted.Location = new Point(0, 78);
            rtbFormatted.Margin = new Padding(3, 4, 3, 4);
            rtbFormatted.Name = "rtbFormatted";
            rtbFormatted.Size = new Size(1375, 864);
            rtbFormatted.TabIndex = 26;
            rtbFormatted.Text = "";
            rtbFormatted.WordWrap = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1375, 1096);
            Controls.Add(rtbFormatted);
            Controls.Add(rtbCorrections);
            Controls.Add(rtbScratchpad);
            Controls.Add(rtbData);
            Controls.Add(rtbResponse);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(rtbPrompt);
            Controls.Add(tlpViewSelect);
            Controls.Add(tlpButtons);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            tlpButtons.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tlpViewSelect.ResumeLayout(false);
            tlpViewSelect.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudIdStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLengthPad).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudIdPad).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel  tlpButtons;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private TableLayoutPanel tlpViewSelect;
        private RadioButton rdoResponse;
        private RadioButton rdoData;
        private Button btnOpenPrompt;
        private Button btnProcess;
        private ToolStripMenuItem exitToolStripMenuItem;
        private RichTextBox rtbPrompt;
        private ToolStripMenuItem openToolStripMenuItem;
        private Button btnOpenData;
        private ToolStripMenuItem openDataToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private RadioButton rdoPrompt;
        private RichTextBox rtbResponse;
        private RichTextBox rtbData;
        private Button btnSaveResponse;
        private RichTextBox rtbScratchpad;
        private RadioButton rdoScratchpad;
        private RichTextBox rtbCorrections;
        private RadioButton rdoCorrections;
        private RadioButton rdoFormatted;
        private RichTextBox rtbFormatted;
        private Button btnFormatResponse;
        private Button btnClose;
        private Button btnSaveFormatted;
        private Label lblModel;
        private Label lblPrefix;
        private TextBox txtPrefix;
        private Label lblIdPad;
        private Label lblStart;
        private NumericUpDown nudIdPad;
        private ComboBox ddlModel;
        private Label lblLengthPad;
        private NumericUpDown nudLengthPad;
        private NumericUpDown nudIdStart;
        private ToolStripMenuItem settingsToolStripMenuItem;
    }
}
