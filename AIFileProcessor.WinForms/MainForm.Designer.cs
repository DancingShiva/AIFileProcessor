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
            btnOpenPrompt = new Button();
            btnOpenData = new Button();
            btnOpenResponse = new Button();
            btnProcess = new Button();
            btnSave = new Button();
            btnClose = new Button();
            mnuMenu = new MenuStrip();
            tsmiFile = new ToolStripMenuItem();
            tsmiOpenData = new ToolStripMenuItem();
            tsmiOpen = new ToolStripMenuItem();
            tsmiSave = new ToolStripMenuItem();
            tsmiSettings = new ToolStripMenuItem();
            tsmiExit = new ToolStripMenuItem();
            tlpViewSelect = new TableLayoutPanel();
            rdoPrompt = new RadioButton();
            rdoData = new RadioButton();
            rdoScratchpad = new RadioButton();
            rdoCorrections = new RadioButton();
            rdoResponse = new RadioButton();
            rdoMetrics = new RadioButton();
            rdoFormatted = new RadioButton();
            rtbPrompt = new RichTextBox();
            tlpOptions = new TableLayoutPanel();
            ddlModel = new ComboBox();
            lblModel = new Label();
            lblReasoning = new Label();
            ddlReasoning = new ComboBox();
            lblPrefix = new Label();
            txtPrefix = new TextBox();
            lblStart = new Label();
            nudIdStart = new NumericUpDown();
            lblIdPad = new Label();
            nudIdPad = new NumericUpDown();
            lblLengthPad = new Label();
            nudLengthPad = new NumericUpDown();
            rtbResponse = new RichTextBox();
            rtbData = new RichTextBox();
            rtbScratchpad = new RichTextBox();
            rtbCorrections = new RichTextBox();
            rtbFormatted = new RichTextBox();
            rtbMetrics = new RichTextBox();
            rtbMetricsJson = new RichTextBox();
            tabMetrics = new TabControl();
            tpMetricsTable = new TabPage();
            tpMetricsJson = new TabPage();
            tpMetricsGraph = new TabPage();
            pnlMetricsGraph = new Panel();
            tlpButtons.SuspendLayout();
            mnuMenu.SuspendLayout();
            tlpViewSelect.SuspendLayout();
            tlpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudIdStart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudIdPad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLengthPad).BeginInit();
            tabMetrics.SuspendLayout();
            tpMetricsTable.SuspendLayout();
            tpMetricsJson.SuspendLayout();
            tpMetricsGraph.SuspendLayout();
            SuspendLayout();
            // 
            // tlpButtons
            // 
            tlpButtons.AllowDrop = true;
            tlpButtons.ColumnCount = 6;
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.7F));
            tlpButtons.Controls.Add(btnOpenPrompt, 0, 0);
            tlpButtons.Controls.Add(btnOpenData, 1, 0);
            tlpButtons.Controls.Add(btnOpenResponse, 2, 0);
            tlpButtons.Controls.Add(btnProcess, 3, 0);
            tlpButtons.Controls.Add(btnSave, 4, 0);
            tlpButtons.Controls.Add(btnClose, 5, 0);
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
            // btnOpenPrompt
            // 
            btnOpenPrompt.Dock = DockStyle.Fill;
            btnOpenPrompt.Location = new Point(18, 16);
            btnOpenPrompt.Margin = new Padding(9, 8, 9, 8);
            btnOpenPrompt.Name = "btnOpenPrompt";
            btnOpenPrompt.Size = new Size(208, 50);
            btnOpenPrompt.TabIndex = 8;
            btnOpenPrompt.Text = "Open Prompt";
            btnOpenPrompt.UseVisualStyleBackColor = true;
            btnOpenPrompt.Click += btnOpenPrompt_Click;
            // 
            // btnOpenData
            // 
            btnOpenData.Dock = DockStyle.Fill;
            btnOpenData.Location = new Point(244, 16);
            btnOpenData.Margin = new Padding(9, 8, 9, 8);
            btnOpenData.Name = "btnOpenData";
            btnOpenData.Size = new Size(208, 50);
            btnOpenData.TabIndex = 10;
            btnOpenData.Text = "Open Data";
            btnOpenData.UseVisualStyleBackColor = true;
            btnOpenData.Click += btnOpenData_Click;
            // 
            // btnOpenResponse
            // 
            btnOpenResponse.Dock = DockStyle.Fill;
            btnOpenResponse.Location = new Point(470, 16);
            btnOpenResponse.Margin = new Padding(9, 8, 9, 8);
            btnOpenResponse.Name = "btnOpenResponse";
            btnOpenResponse.Size = new Size(208, 50);
            btnOpenResponse.TabIndex = 15;
            btnOpenResponse.Text = "Open Response";
            btnOpenResponse.UseVisualStyleBackColor = true;
            btnOpenResponse.Click += btnOpenResponse_Click;
            // 
            // btnProcess
            // 
            btnProcess.Dock = DockStyle.Fill;
            btnProcess.Location = new Point(696, 16);
            btnProcess.Margin = new Padding(9, 8, 9, 8);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(208, 50);
            btnProcess.TabIndex = 3;
            btnProcess.Text = "Process";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcessFormat_Click;
            // 
            // btnSave
            // 
            btnSave.Dock = DockStyle.Fill;
            btnSave.Enabled = false;
            btnSave.Location = new Point(922, 16);
            btnSave.Margin = new Padding(9, 8, 9, 8);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(208, 50);
            btnSave.TabIndex = 15;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += saveToolStripMenuItem_Click;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Fill;
            btnClose.Location = new Point(1148, 16);
            btnClose.Margin = new Padding(9, 8, 9, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(209, 50);
            btnClose.TabIndex = 14;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // mnuMenu
            // 
            mnuMenu.ImageScalingSize = new Size(28, 28);
            mnuMenu.Items.AddRange(new ToolStripItem[] { tsmiFile });
            mnuMenu.Location = new Point(0, 0);
            mnuMenu.Name = "mnuMenu";
            mnuMenu.Padding = new Padding(7, 2, 0, 2);
            mnuMenu.Size = new Size(1375, 38);
            mnuMenu.TabIndex = 13;
            mnuMenu.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            tsmiFile.DropDownItems.AddRange(new ToolStripItem[] { tsmiOpenData, tsmiOpen, tsmiSave, tsmiSettings, tsmiExit });
            tsmiFile.Name = "tsmiFile";
            tsmiFile.Size = new Size(62, 34);
            tsmiFile.Text = "&File";
            // 
            // tsmiOpenData
            // 
            tsmiOpenData.Name = "tsmiOpenData";
            tsmiOpenData.Size = new Size(256, 40);
            tsmiOpenData.Text = "Open &Data";
            tsmiOpenData.Click += openDataToolStripMenuItem_Click;
            // 
            // tsmiOpen
            // 
            tsmiOpen.Name = "tsmiOpen";
            tsmiOpen.Size = new Size(256, 40);
            tsmiOpen.Text = "Open &Prompt";
            tsmiOpen.Click += openToolStripMenuItem_Click;
            // 
            // tsmiSave
            // 
            tsmiSave.Enabled = false;
            tsmiSave.Name = "tsmiSave";
            tsmiSave.Size = new Size(256, 40);
            tsmiSave.Text = "&Save";
            tsmiSave.Click += saveToolStripMenuItem_Click;
            // 
            // tsmiSettings
            // 
            tsmiSettings.Name = "tsmiSettings";
            tsmiSettings.Size = new Size(256, 40);
            tsmiSettings.Text = "&Settings";
            tsmiSettings.Click += settingsToolStripMenuItem_Click;
            // 
            // tsmiExit
            // 
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(256, 40);
            tsmiExit.Text = "E&xit";
            tsmiExit.Click += exitToolStripMenuItem_Click;
            // 
            // tlpViewSelect
            // 
            tlpViewSelect.ColumnCount = 7;
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6750031F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.664999F));
            tlpViewSelect.Controls.Add(rdoPrompt, 0, 0);
            tlpViewSelect.Controls.Add(rdoData, 1, 0);
            tlpViewSelect.Controls.Add(rdoScratchpad, 2, 0);
            tlpViewSelect.Controls.Add(rdoCorrections, 3, 0);
            tlpViewSelect.Controls.Add(rdoResponse, 4, 0);
            tlpViewSelect.Controls.Add(rdoMetrics, 5, 0);
            tlpViewSelect.Controls.Add(rdoFormatted, 6, 0);
            tlpViewSelect.Dock = DockStyle.Bottom;
            tlpViewSelect.Location = new Point(0, 942);
            tlpViewSelect.Margin = new Padding(3, 4, 3, 4);
            tlpViewSelect.Name = "tlpViewSelect";
            tlpViewSelect.RowCount = 1;
            tlpViewSelect.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpViewSelect.Size = new Size(1375, 72);
            tlpViewSelect.TabIndex = 13;
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
            rdoPrompt.Size = new Size(178, 56);
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
            rdoData.Location = new Point(205, 8);
            rdoData.Margin = new Padding(9, 8, 9, 8);
            rdoData.Name = "rdoData";
            rdoData.Size = new Size(178, 56);
            rdoData.TabIndex = 12;
            rdoData.Text = "Data";
            rdoData.TextAlign = ContentAlignment.MiddleCenter;
            rdoData.UseVisualStyleBackColor = true;
            rdoData.CheckedChanged += rdoData_CheckedChanged;
            // 
            // rdoScratchpad
            // 
            rdoScratchpad.Appearance = Appearance.Button;
            rdoScratchpad.AutoSize = true;
            rdoScratchpad.Dock = DockStyle.Fill;
            rdoScratchpad.Location = new Point(401, 8);
            rdoScratchpad.Margin = new Padding(9, 8, 9, 8);
            rdoScratchpad.Name = "rdoScratchpad";
            rdoScratchpad.Size = new Size(178, 56);
            rdoScratchpad.TabIndex = 17;
            rdoScratchpad.Text = "Scratchpad";
            rdoScratchpad.TextAlign = ContentAlignment.MiddleCenter;
            rdoScratchpad.UseVisualStyleBackColor = true;
            rdoScratchpad.CheckedChanged += rdoScratchpad_CheckedChanged;
            // 
            // rdoCorrections
            // 
            rdoCorrections.Appearance = Appearance.Button;
            rdoCorrections.AutoSize = true;
            rdoCorrections.Dock = DockStyle.Fill;
            rdoCorrections.Location = new Point(597, 8);
            rdoCorrections.Margin = new Padding(9, 8, 9, 8);
            rdoCorrections.Name = "rdoCorrections";
            rdoCorrections.Size = new Size(178, 56);
            rdoCorrections.TabIndex = 19;
            rdoCorrections.Text = "Corrections";
            rdoCorrections.TextAlign = ContentAlignment.MiddleCenter;
            rdoCorrections.UseVisualStyleBackColor = true;
            rdoCorrections.CheckedChanged += rdoCorrections_CheckedChanged;
            // 
            // rdoResponse
            // 
            rdoResponse.Appearance = Appearance.Button;
            rdoResponse.AutoSize = true;
            rdoResponse.Dock = DockStyle.Fill;
            rdoResponse.Location = new Point(793, 8);
            rdoResponse.Margin = new Padding(9, 8, 9, 8);
            rdoResponse.Name = "rdoResponse";
            rdoResponse.Size = new Size(178, 56);
            rdoResponse.TabIndex = 13;
            rdoResponse.Text = "Response";
            rdoResponse.TextAlign = ContentAlignment.MiddleCenter;
            rdoResponse.UseVisualStyleBackColor = true;
            rdoResponse.CheckedChanged += rdoResponse_CheckedChanged;
            // 
            // rdoMetrics
            // 
            rdoMetrics.Appearance = Appearance.Button;
            rdoMetrics.AutoSize = true;
            rdoMetrics.Dock = DockStyle.Fill;
            rdoMetrics.Location = new Point(989, 8);
            rdoMetrics.Margin = new Padding(9, 8, 9, 8);
            rdoMetrics.Name = "rdoMetrics";
            rdoMetrics.Size = new Size(178, 56);
            rdoMetrics.TabIndex = 19;
            rdoMetrics.Text = "Metrics";
            rdoMetrics.TextAlign = ContentAlignment.MiddleCenter;
            rdoMetrics.UseVisualStyleBackColor = true;
            rdoMetrics.CheckedChanged += rdoMetrics_CheckedChanged;
            // 
            // rdoFormatted
            // 
            rdoFormatted.Appearance = Appearance.Button;
            rdoFormatted.AutoSize = true;
            rdoFormatted.Dock = DockStyle.Fill;
            rdoFormatted.Location = new Point(1185, 8);
            rdoFormatted.Margin = new Padding(9, 8, 9, 8);
            rdoFormatted.Name = "rdoFormatted";
            rdoFormatted.Size = new Size(181, 56);
            rdoFormatted.TabIndex = 18;
            rdoFormatted.Text = "Formatted";
            rdoFormatted.TextAlign = ContentAlignment.MiddleCenter;
            rdoFormatted.UseVisualStyleBackColor = true;
            rdoFormatted.CheckedChanged += rdoFormatted_CheckedChanged;
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
            // tlpOptions
            // 
            tlpOptions.ColumnCount = 12;
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34.6249962F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19.4765625F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18.2265549F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.1384506F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.266719F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpOptions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.266719F));
            tlpOptions.Controls.Add(ddlModel, 1, 0);
            tlpOptions.Controls.Add(lblModel, 0, 0);
            tlpOptions.Controls.Add(lblReasoning, 2, 0);
            tlpOptions.Controls.Add(ddlReasoning, 3, 0);
            tlpOptions.Controls.Add(lblPrefix, 4, 0);
            tlpOptions.Controls.Add(txtPrefix, 5, 0);
            tlpOptions.Controls.Add(lblStart, 6, 0);
            tlpOptions.Controls.Add(nudIdStart, 7, 0);
            tlpOptions.Controls.Add(lblIdPad, 8, 0);
            tlpOptions.Controls.Add(nudIdPad, 9, 0);
            tlpOptions.Controls.Add(lblLengthPad, 10, 0);
            tlpOptions.Controls.Add(nudLengthPad, 11, 0);
            tlpOptions.Dock = DockStyle.Top;
            tlpOptions.Location = new Point(0, 38);
            tlpOptions.Margin = new Padding(3, 4, 3, 4);
            tlpOptions.Name = "tlpOptions";
            tlpOptions.RowCount = 1;
            tlpOptions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpOptions.Size = new Size(1375, 40);
            tlpOptions.TabIndex = 18;
            // 
            // ddlModel
            // 
            ddlModel.Dock = DockStyle.Fill;
            ddlModel.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlModel.FormattingEnabled = true;
            ddlModel.Location = new Point(83, 3);
            ddlModel.Name = "ddlModel";
            ddlModel.Size = new Size(258, 38);
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
            // lblReasoning
            // 
            lblReasoning.AutoSize = true;
            lblReasoning.Dock = DockStyle.Fill;
            lblReasoning.Location = new Point(347, 0);
            lblReasoning.Name = "lblReasoning";
            lblReasoning.Size = new Size(119, 40);
            lblReasoning.TabIndex = 39;
            lblReasoning.Text = "Reasoning:";
            lblReasoning.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ddlReasoning
            // 
            ddlReasoning.Dock = DockStyle.Fill;
            ddlReasoning.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlReasoning.FormattingEnabled = true;
            ddlReasoning.Location = new Point(472, 3);
            ddlReasoning.Name = "ddlReasoning";
            ddlReasoning.Size = new Size(142, 38);
            ddlReasoning.TabIndex = 40;
            // 
            // lblPrefix
            // 
            lblPrefix.AutoSize = true;
            lblPrefix.Dock = DockStyle.Fill;
            lblPrefix.Location = new Point(620, 0);
            lblPrefix.Name = "lblPrefix";
            lblPrefix.Size = new Size(94, 40);
            lblPrefix.TabIndex = 27;
            lblPrefix.Text = "Id &Prefix:";
            lblPrefix.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtPrefix
            // 
            txtPrefix.Dock = DockStyle.Fill;
            txtPrefix.Location = new Point(720, 4);
            txtPrefix.Margin = new Padding(3, 4, 3, 4);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Size = new Size(133, 35);
            txtPrefix.TabIndex = 28;
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Dock = DockStyle.Fill;
            lblStart.Location = new Point(859, 0);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(84, 40);
            lblStart.TabIndex = 29;
            lblStart.Text = "Id &Start:";
            lblStart.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudIdStart
            // 
            nudIdStart.Dock = DockStyle.Fill;
            nudIdStart.Location = new Point(949, 4);
            nudIdStart.Margin = new Padding(3, 4, 3, 4);
            nudIdStart.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudIdStart.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudIdStart.Name = "nudIdStart";
            nudIdStart.Size = new Size(79, 35);
            nudIdStart.TabIndex = 36;
            nudIdStart.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblIdPad
            // 
            lblIdPad.AutoSize = true;
            lblIdPad.Dock = DockStyle.Fill;
            lblIdPad.Location = new Point(1034, 0);
            lblIdPad.Name = "lblIdPad";
            lblIdPad.Size = new Size(79, 40);
            lblIdPad.TabIndex = 31;
            lblIdPad.Text = "Id Pad:";
            lblIdPad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudIdPad
            // 
            nudIdPad.Dock = DockStyle.Fill;
            nudIdPad.Location = new Point(1119, 4);
            nudIdPad.Margin = new Padding(3, 4, 3, 4);
            nudIdPad.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudIdPad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudIdPad.Name = "nudIdPad";
            nudIdPad.Size = new Size(57, 35);
            nudIdPad.TabIndex = 32;
            nudIdPad.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblLengthPad
            // 
            lblLengthPad.AutoSize = true;
            lblLengthPad.Dock = DockStyle.Fill;
            lblLengthPad.Location = new Point(1182, 0);
            lblLengthPad.Name = "lblLengthPad";
            lblLengthPad.Size = new Size(124, 40);
            lblLengthPad.TabIndex = 37;
            lblLengthPad.Text = "Length Pad:";
            lblLengthPad.TextAlign = ContentAlignment.MiddleRight;
            // 
            // nudLengthPad
            // 
            nudLengthPad.Dock = DockStyle.Fill;
            nudLengthPad.Location = new Point(1312, 4);
            nudLengthPad.Margin = new Padding(3, 4, 3, 4);
            nudLengthPad.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudLengthPad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudLengthPad.Name = "nudLengthPad";
            nudLengthPad.Size = new Size(60, 35);
            nudLengthPad.TabIndex = 38;
            nudLengthPad.Value = new decimal(new int[] { 3, 0, 0, 0 });
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
            // rtbMetrics
            // 
            rtbMetrics.Dock = DockStyle.Fill;
            rtbMetrics.Location = new Point(0, 0);
            rtbMetrics.Name = "rtbMetrics";
            rtbMetrics.Size = new Size(1367, 821);
            rtbMetrics.TabIndex = 0;
            rtbMetrics.Text = "";
            // 
            // rtbMetricsJson
            // 
            rtbMetricsJson.Dock = DockStyle.Fill;
            rtbMetricsJson.Font = new Font("Consolas", 9F);
            rtbMetricsJson.Location = new Point(0, 0);
            rtbMetricsJson.Name = "rtbMetricsJson";
            rtbMetricsJson.ReadOnly = true;
            rtbMetricsJson.Size = new Size(1367, 821);
            rtbMetricsJson.TabIndex = 0;
            rtbMetricsJson.Text = "";
            rtbMetricsJson.WordWrap = false;
            // 
            // tabMetrics
            // 
            tabMetrics.Controls.Add(tpMetricsTable);
            tabMetrics.Controls.Add(tpMetricsJson);
            tabMetrics.Controls.Add(tpMetricsGraph);
            tabMetrics.Dock = DockStyle.Fill;
            tabMetrics.Location = new Point(0, 78);
            tabMetrics.Margin = new Padding(3, 4, 3, 4);
            tabMetrics.Name = "tabMetrics";
            tabMetrics.SelectedIndex = 0;
            tabMetrics.Size = new Size(1375, 864);
            tabMetrics.TabIndex = 15;
            tabMetrics.Visible = false;
            // 
            // tpMetricsTable
            // 
            tpMetricsTable.Controls.Add(rtbMetrics);
            tpMetricsTable.Location = new Point(4, 39);
            tpMetricsTable.Name = "tpMetricsTable";
            tpMetricsTable.Size = new Size(1367, 821);
            tpMetricsTable.TabIndex = 0;
            tpMetricsTable.Text = "Table";
            // 
            // tpMetricsJson
            // 
            tpMetricsJson.Controls.Add(rtbMetricsJson);
            tpMetricsJson.Location = new Point(4, 39);
            tpMetricsJson.Name = "tpMetricsJson";
            tpMetricsJson.Size = new Size(1367, 821);
            tpMetricsJson.TabIndex = 1;
            tpMetricsJson.Text = "JSON";
            // 
            // tpMetricsGraph
            // 
            tpMetricsGraph.Controls.Add(pnlMetricsGraph);
            tpMetricsGraph.Location = new Point(4, 39);
            tpMetricsGraph.Name = "tpMetricsGraph";
            tpMetricsGraph.Size = new Size(1367, 821);
            tpMetricsGraph.TabIndex = 2;
            tpMetricsGraph.Text = "Graph";
            // 
            // pnlMetricsGraph
            // 
            pnlMetricsGraph.AutoScroll = true;
            pnlMetricsGraph.BackColor = SystemColors.Window;
            pnlMetricsGraph.Dock = DockStyle.Fill;
            pnlMetricsGraph.Location = new Point(0, 0);
            pnlMetricsGraph.Name = "pnlMetricsGraph";
            pnlMetricsGraph.Size = new Size(1367, 821);
            pnlMetricsGraph.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1375, 1096);
            Controls.Add(rtbFormatted);
            Controls.Add(tabMetrics);
            Controls.Add(rtbCorrections);
            Controls.Add(rtbScratchpad);
            Controls.Add(rtbData);
            Controls.Add(rtbResponse);
            Controls.Add(tlpOptions);
            Controls.Add(rtbPrompt);
            Controls.Add(tlpViewSelect);
            Controls.Add(tlpButtons);
            Controls.Add(mnuMenu);
            MainMenuStrip = mnuMenu;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            tlpButtons.ResumeLayout(false);
            mnuMenu.ResumeLayout(false);
            mnuMenu.PerformLayout();
            tlpViewSelect.ResumeLayout(false);
            tlpViewSelect.PerformLayout();
            tlpOptions.ResumeLayout(false);
            tlpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudIdStart).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudIdPad).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLengthPad).EndInit();
            tabMetrics.ResumeLayout(false);
            tpMetricsTable.ResumeLayout(false);
            tpMetricsJson.ResumeLayout(false);
            tpMetricsGraph.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel  tlpButtons;
        private MenuStrip mnuMenu;
        private ToolStripMenuItem tsmiFile;
        private TableLayoutPanel tlpViewSelect;
        private RadioButton rdoResponse;
        private RadioButton rdoData;
        private Button btnOpenPrompt;
        private Button btnProcess;
        private Button btnSave;
        private ToolStripMenuItem tsmiExit;
        private RichTextBox rtbPrompt;
        private ToolStripMenuItem tsmiOpen;
        private ToolStripMenuItem tsmiSave;
        private Button btnOpenData;
        private ToolStripMenuItem tsmiOpenData;
        private TableLayoutPanel tlpOptions;
        private RadioButton rdoPrompt;
        private RichTextBox rtbResponse;
        private RichTextBox rtbData;
        private RichTextBox rtbScratchpad;
        private RadioButton rdoScratchpad;
        private RichTextBox rtbCorrections;
        private RadioButton rdoCorrections;
        private RadioButton rdoFormatted;
        private RadioButton rdoMetrics;
        private RichTextBox rtbFormatted;
        private RichTextBox rtbMetrics;
        private TabControl tabMetrics;
        private TabPage tpMetricsTable;
        private TabPage tpMetricsJson;
        private TabPage tpMetricsGraph;
        private RichTextBox rtbMetricsJson;
        private Panel pnlMetricsGraph;
        private Button btnClose;
        private Label lblReasoning;
        private ComboBox ddlReasoning;
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
        private Button btnOpenResponse;
        private ToolStripMenuItem tsmiSettings;
    }
}
