//-----------------------------------------------------------------------
// <copyright file="SettingsForm.Designer.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace AIFileProcessor
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGemini = new System.Windows.Forms.TabPage();
            this.tabOpenAi = new System.Windows.Forms.TabPage();
            this.tabDeepSeek = new System.Windows.Forms.TabPage();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            // Gemini tab controls
            this.flpGemini = new System.Windows.Forms.FlowLayoutPanel();
            this.txtGeminiEndpoint = new System.Windows.Forms.TextBox();
            this.ddlGeminiModel = new System.Windows.Forms.ComboBox();
            this.nudGeminiTimeout = new System.Windows.Forms.NumericUpDown();
            this.txtGeminiProjectId = new System.Windows.Forms.TextBox();
            this.txtGeminiLocation = new System.Windows.Forms.TextBox();
            this.nudGeminiMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkGeminiMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudGeminiTemp = new System.Windows.Forms.NumericUpDown();
            this.chkGeminiTempNull = new System.Windows.Forms.CheckBox();
            this.nudGeminiTopP = new System.Windows.Forms.NumericUpDown();
            this.chkGeminiTopPNull = new System.Windows.Forms.CheckBox();
            this.chkGeminiStreaming = new System.Windows.Forms.CheckBox();
            this.nudGeminiMaxRetries = new System.Windows.Forms.NumericUpDown();
            this.nudGeminiBaseDelay = new System.Windows.Forms.NumericUpDown();
            this.nudGeminiMaxDelay = new System.Windows.Forms.NumericUpDown();
            this.chkGeminiEnableLogging = new System.Windows.Forms.CheckBox();
            this.nudGeminiRateLimit = new System.Windows.Forms.NumericUpDown();
            this.chkGeminiRateLimitNull = new System.Windows.Forms.CheckBox();
            this.txtGeminiProxyUrl = new System.Windows.Forms.TextBox();
            this.chkGeminiUseManagedIdentity = new System.Windows.Forms.CheckBox();
            this.txtGeminiCredentialType = new System.Windows.Forms.TextBox();

            // OpenAI tab controls
            this.flpOpenAi = new System.Windows.Forms.FlowLayoutPanel();
            this.txtOpenAiEndpoint = new System.Windows.Forms.TextBox();
            this.ddlOpenAiModel = new System.Windows.Forms.ComboBox();
            this.nudOpenAiTimeout = new System.Windows.Forms.NumericUpDown();
            this.txtOpenAiOrgId = new System.Windows.Forms.TextBox();
            this.nudOpenAiMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkOpenAiMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudOpenAiTemp = new System.Windows.Forms.NumericUpDown();
            this.chkOpenAiTempNull = new System.Windows.Forms.CheckBox();

            // DeepSeek tab controls
            this.flpDeepSeek = new System.Windows.Forms.FlowLayoutPanel();
            this.ddlDeepSeekModel = new System.Windows.Forms.ComboBox();
            this.txtDeepSeekEndpoint = new System.Windows.Forms.TextBox();
            this.nudDeepSeekTimeout = new System.Windows.Forms.NumericUpDown();
            this.nudDeepSeekMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkDeepSeekMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudDeepSeekTemp = new System.Windows.Forms.NumericUpDown();
            this.chkDeepSeekTempNull = new System.Windows.Forms.CheckBox();
            this.chkDeepSeekReasoning = new System.Windows.Forms.CheckBox();

            this.tabControl.SuspendLayout();
            this.tabGemini.SuspendLayout();
            this.tabOpenAi.SuspendLayout();
            this.tabDeepSeek.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxTokens)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTemp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTopP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxRetries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiBaseDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiRateLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiMaxTokens)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiTemp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekMaxTokens)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekTemp)).BeginInit();
            this.SuspendLayout();

            // TabControl
            this.tabControl.Controls.Add(this.tabGemini);
            this.tabControl.Controls.Add(this.tabOpenAi);
            this.tabControl.Controls.Add(this.tabDeepSeek);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(10, 10);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(564, 480);
            this.tabControl.TabIndex = 0;

            // Gemini Tab
            this.tabGemini.Controls.Add(this.flpGemini);
            this.tabGemini.Location = new System.Drawing.Point(4, 24);
            this.tabGemini.Name = "tabGemini";
            this.tabGemini.Padding = new System.Windows.Forms.Padding(10);
            this.tabGemini.Size = new System.Drawing.Size(556, 452);
            this.tabGemini.TabIndex = 0;
            this.tabGemini.Text = "Gemini";
            this.tabGemini.UseVisualStyleBackColor = true;

            this.flpGemini.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpGemini.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpGemini.WrapContents = false;
            this.flpGemini.AutoScroll = true;

            // OpenAI Tab
            this.tabOpenAi.Controls.Add(this.flpOpenAi);
            this.tabOpenAi.Location = new System.Drawing.Point(4, 24);
            this.tabOpenAi.Name = "tabOpenAi";
            this.tabOpenAi.Padding = new System.Windows.Forms.Padding(10);
            this.tabOpenAi.Size = new System.Drawing.Size(556, 452);
            this.tabOpenAi.TabIndex = 1;
            this.tabOpenAi.Text = "OpenAI";
            this.tabOpenAi.UseVisualStyleBackColor = true;

            this.flpOpenAi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOpenAi.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOpenAi.WrapContents = false;
            this.flpOpenAi.AutoScroll = true;

            // DeepSeek Tab
            this.tabDeepSeek.Controls.Add(this.flpDeepSeek);
            this.tabDeepSeek.Location = new System.Drawing.Point(4, 24);
            this.tabDeepSeek.Name = "tabDeepSeek";
            this.tabDeepSeek.Padding = new System.Windows.Forms.Padding(10);
            this.tabDeepSeek.Size = new System.Drawing.Size(556, 452);
            this.tabDeepSeek.TabIndex = 2;
            this.tabDeepSeek.Text = "DeepSeek";
            this.tabDeepSeek.UseVisualStyleBackColor = true;

            this.flpDeepSeek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDeepSeek.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpDeepSeek.WrapContents = false;
            this.flpDeepSeek.AutoScroll = true;

            // Setup controls helper layout
            BuildLabelledComboBox(this.flpGemini, "Model Name:", this.ddlGeminiModel);
            BuildLabelledTextBox(this.flpGemini, "Endpoint:", this.txtGeminiEndpoint, false);
            BuildLabelledNumeric(this.flpGemini, "Timeout (Seconds):", this.nudGeminiTimeout, 1, 3600);
            BuildLabelledTextBox(this.flpGemini, "Project ID (Optional):", this.txtGeminiProjectId, false);
            BuildLabelledTextBox(this.flpGemini, "Location (Optional):", this.txtGeminiLocation, false);
            BuildLabelledNumericWithNull(this.flpGemini, "Max Tokens:", this.nudGeminiMaxTokens, this.chkGeminiMaxTokensNull, 0, 1000000);
            this.chkGeminiMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkGeminiMaxTokensNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpGemini, "Temperature (0.0 - 2.0):", this.nudGeminiTemp, this.chkGeminiTempNull, 0.0m, 2.0m);
            this.chkGeminiTempNull.CheckedChanged += new System.EventHandler(this.ChkGeminiTempNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpGemini, "Top P (0.0 - 1.0):", this.nudGeminiTopP, this.chkGeminiTopPNull, 0.0m, 1.0m);
            this.chkGeminiTopPNull.CheckedChanged += new System.EventHandler(this.ChkGeminiTopPNull_CheckedChanged);
            BuildCheckbox(this.flpGemini, "Enable Streaming", this.chkGeminiStreaming);
            BuildLabelledNumeric(this.flpGemini, "Max Retries:", this.nudGeminiMaxRetries, 0, 10);
            BuildLabelledNumeric(this.flpGemini, "Base Delay (Seconds):", this.nudGeminiBaseDelay, 1, 3600);
            BuildLabelledNumeric(this.flpGemini, "Max Delay (Seconds):", this.nudGeminiMaxDelay, 1, 3600);
            BuildCheckbox(this.flpGemini, "Enable Network Logging", this.chkGeminiEnableLogging);
            BuildLabelledNumericWithNull(this.flpGemini, "Rate Limit Per Minute:", this.nudGeminiRateLimit, this.chkGeminiRateLimitNull, 0, 100000);
            this.chkGeminiRateLimitNull.CheckedChanged += new System.EventHandler(this.ChkGeminiRateLimitNull_CheckedChanged);
            BuildLabelledTextBox(this.flpGemini, "Proxy URL (Optional):", this.txtGeminiProxyUrl, false);
            BuildCheckbox(this.flpGemini, "Use Managed Identity Authentication", this.chkGeminiUseManagedIdentity);
            BuildLabelledTextBox(this.flpGemini, "Credential Type (Optional):", this.txtGeminiCredentialType, false);

            BuildLabelledComboBox(this.flpOpenAi, "Model Name:", this.ddlOpenAiModel);
            BuildLabelledTextBox(this.flpOpenAi, "Endpoint:", this.txtOpenAiEndpoint, false);
            BuildLabelledNumeric(this.flpOpenAi, "Timeout (Seconds):", this.nudOpenAiTimeout, 1, 3600);
            BuildLabelledTextBox(this.flpOpenAi, "Organization ID (Optional):", this.txtOpenAiOrgId, false);
            BuildLabelledNumericWithNull(this.flpOpenAi, "Max Tokens:", this.nudOpenAiMaxTokens, this.chkOpenAiMaxTokensNull, 0, 1000000);
            this.chkOpenAiMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkOpenAiMaxTokensNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOpenAi, "Temperature (0.0 - 2.0):", this.nudOpenAiTemp, this.chkOpenAiTempNull, 0.0m, 2.0m);
            this.chkOpenAiTempNull.CheckedChanged += new System.EventHandler(this.ChkOpenAiTempNull_CheckedChanged);

            BuildLabelledComboBox(this.flpDeepSeek, "Model Name:", this.ddlDeepSeekModel);
            BuildLabelledTextBox(this.flpDeepSeek, "Endpoint:", this.txtDeepSeekEndpoint, false);
            BuildLabelledNumeric(this.flpDeepSeek, "Timeout (Seconds):", this.nudDeepSeekTimeout, 1, 3600);
            BuildLabelledNumericWithNull(this.flpDeepSeek, "Max Tokens:", this.nudDeepSeekMaxTokens, this.chkDeepSeekMaxTokensNull, 0, 1000000);
            this.chkDeepSeekMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkDeepSeekMaxTokensNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpDeepSeek, "Temperature (0.0 - 2.0):", this.nudDeepSeekTemp, this.chkDeepSeekTempNull, 0.0m, 2.0m);
            this.chkDeepSeekTempNull.CheckedChanged += new System.EventHandler(this.ChkDeepSeekTempNull_CheckedChanged);
            BuildCheckbox(this.flpDeepSeek, "Enable DeepSeek Reasoning Response", this.chkDeepSeekReasoning);

            // Panel buttons
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(10, 490);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(564, 40);
            this.pnlButtons.TabIndex = 1;

            // btnSave
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(398, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

            // btnCancel
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(479, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // SettingsForm
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 540);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "API Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);

            this.tabControl.ResumeLayout(false);
            this.tabGemini.ResumeLayout(false);
            this.tabOpenAi.ResumeLayout(false);
            this.tabDeepSeek.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxTokens)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTemp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiTopP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxRetries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiBaseDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiMaxDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeminiRateLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiMaxTokens)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOpenAiTemp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekMaxTokens)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDeepSeekTemp)).EndInit();
            this.ResumeLayout(false);
        }

        private static void BuildLabelledTextBox(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.TextBox textBox, bool isPassword)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 45) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 5), Size = new System.Drawing.Size(180, 15), TextAlign = System.Drawing.ContentAlignment.BottomLeft };
            textBox.Location = new System.Drawing.Point(5, 22);
            textBox.Size = new System.Drawing.Size(500, 23);
            if (isPassword)
            {
                textBox.UseSystemPasswordChar = true;
            }
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(textBox);
            panel.Controls.Add(pnl);
        }

        private static void BuildLabelledComboBox(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.ComboBox comboBox)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 45) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 5), Size = new System.Drawing.Size(180, 15), TextAlign = System.Drawing.ContentAlignment.BottomLeft };
            comboBox.Location = new System.Drawing.Point(5, 22);
            comboBox.Size = new System.Drawing.Size(500, 23);
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(comboBox);
            panel.Controls.Add(pnl);
        }

        private static void BuildLabelledNumeric(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.NumericUpDown numericUpDown, decimal min, decimal max)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 45) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 5), Size = new System.Drawing.Size(180, 15), TextAlign = System.Drawing.ContentAlignment.BottomLeft };
            numericUpDown.Location = new System.Drawing.Point(5, 22);
            numericUpDown.Size = new System.Drawing.Size(150, 23);
            numericUpDown.Minimum = min;
            numericUpDown.Maximum = max;
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(numericUpDown);
            panel.Controls.Add(pnl);
        }

        private static void BuildLabelledNumericWithNull(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.NumericUpDown numericUpDown, System.Windows.Forms.CheckBox chkNull, decimal min, decimal max)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 45) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 5), Size = new System.Drawing.Size(180, 15), TextAlign = System.Drawing.ContentAlignment.BottomLeft };
            numericUpDown.Location = new System.Drawing.Point(5, 22);
            numericUpDown.Size = new System.Drawing.Size(150, 23);
            numericUpDown.Minimum = min;
            numericUpDown.Maximum = max;
            chkNull.Text = "Not Configured / Inherit Default";
            chkNull.Location = new System.Drawing.Point(170, 22);
            chkNull.Size = new System.Drawing.Size(200, 23);
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(numericUpDown);
            pnl.Controls.Add(chkNull);
            panel.Controls.Add(pnl);
        }

        private static void BuildLabelledDecimalWithNull(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.NumericUpDown numericUpDown, System.Windows.Forms.CheckBox chkNull, decimal min, decimal max)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 45) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 5), Size = new System.Drawing.Size(180, 15), TextAlign = System.Drawing.ContentAlignment.BottomLeft };
            numericUpDown.Location = new System.Drawing.Point(5, 22);
            numericUpDown.Size = new System.Drawing.Size(150, 23);
            numericUpDown.Minimum = min;
            numericUpDown.Maximum = max;
            numericUpDown.DecimalPlaces = 2;
            numericUpDown.Increment = 0.1m;
            chkNull.Text = "Not Configured / Inherit Default";
            chkNull.Location = new System.Drawing.Point(170, 22);
            chkNull.Size = new System.Drawing.Size(200, 23);
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(numericUpDown);
            pnl.Controls.Add(chkNull);
            panel.Controls.Add(pnl);
        }

        private static void BuildCheckbox(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.CheckBox checkBox)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 30) };
            checkBox.Text = labelText;
            checkBox.Location = new System.Drawing.Point(5, 5);
            checkBox.Size = new System.Drawing.Size(400, 20);
            pnl.Controls.Add(checkBox);
            panel.Controls.Add(pnl);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGemini;
        private System.Windows.Forms.TabPage tabOpenAi;
        private System.Windows.Forms.TabPage tabDeepSeek;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private System.Windows.Forms.FlowLayoutPanel flpGemini;
        private System.Windows.Forms.TextBox txtGeminiEndpoint;
        private System.Windows.Forms.ComboBox ddlGeminiModel;
        private System.Windows.Forms.NumericUpDown nudGeminiTimeout;
        private System.Windows.Forms.TextBox txtGeminiProjectId;
        private System.Windows.Forms.TextBox txtGeminiLocation;
        private System.Windows.Forms.NumericUpDown nudGeminiMaxTokens;
        private System.Windows.Forms.CheckBox chkGeminiMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudGeminiTemp;
        private System.Windows.Forms.CheckBox chkGeminiTempNull;
        private System.Windows.Forms.NumericUpDown nudGeminiTopP;
        private System.Windows.Forms.CheckBox chkGeminiTopPNull;
        private System.Windows.Forms.CheckBox chkGeminiStreaming;
        private System.Windows.Forms.NumericUpDown nudGeminiMaxRetries;
        private System.Windows.Forms.NumericUpDown nudGeminiBaseDelay;
        private System.Windows.Forms.NumericUpDown nudGeminiMaxDelay;
        private System.Windows.Forms.CheckBox chkGeminiEnableLogging;
        private System.Windows.Forms.NumericUpDown nudGeminiRateLimit;
        private System.Windows.Forms.CheckBox chkGeminiRateLimitNull;
        private System.Windows.Forms.TextBox txtGeminiProxyUrl;
        private System.Windows.Forms.CheckBox chkGeminiUseManagedIdentity;
        private System.Windows.Forms.TextBox txtGeminiCredentialType;

        private System.Windows.Forms.FlowLayoutPanel flpOpenAi;
        private System.Windows.Forms.TextBox txtOpenAiEndpoint;
        private System.Windows.Forms.ComboBox ddlOpenAiModel;
        private System.Windows.Forms.NumericUpDown nudOpenAiTimeout;
        private System.Windows.Forms.TextBox txtOpenAiOrgId;
        private System.Windows.Forms.NumericUpDown nudOpenAiMaxTokens;
        private System.Windows.Forms.CheckBox chkOpenAiMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudOpenAiTemp;
        private System.Windows.Forms.CheckBox chkOpenAiTempNull;

        private System.Windows.Forms.FlowLayoutPanel flpDeepSeek;
        private System.Windows.Forms.ComboBox ddlDeepSeekModel;
        private System.Windows.Forms.TextBox txtDeepSeekEndpoint;
        private System.Windows.Forms.NumericUpDown nudDeepSeekTimeout;
        private System.Windows.Forms.NumericUpDown nudDeepSeekMaxTokens;
        private System.Windows.Forms.CheckBox chkDeepSeekMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudDeepSeekTemp;
        private System.Windows.Forms.CheckBox chkDeepSeekTempNull;
        private System.Windows.Forms.CheckBox chkDeepSeekReasoning;
    }
}