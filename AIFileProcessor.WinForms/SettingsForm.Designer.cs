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
            this.tabOllama = new System.Windows.Forms.TabPage();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            // Gemini tab controls
            this.flpGemini = new System.Windows.Forms.FlowLayoutPanel();
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
            this.chkGeminiUseManagedIdentity = new System.Windows.Forms.CheckBox();
            this.txtGeminiCredentialType = new System.Windows.Forms.TextBox();
            this.ddlGeminiReasoning = new System.Windows.Forms.ComboBox();

            // OpenAI tab controls
            this.flpOpenAi = new System.Windows.Forms.FlowLayoutPanel();
            this.ddlOpenAiModel = new System.Windows.Forms.ComboBox();
            this.ddlOpenAiReasoning = new System.Windows.Forms.ComboBox();
            this.nudOpenAiTimeout = new System.Windows.Forms.NumericUpDown();
            this.txtOpenAiOrgId = new System.Windows.Forms.TextBox();
            this.nudOpenAiMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkOpenAiMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudOpenAiTemp = new System.Windows.Forms.NumericUpDown();
            this.chkOpenAiTempNull = new System.Windows.Forms.CheckBox();

            // DeepSeek tab controls
            this.flpDeepSeek = new System.Windows.Forms.FlowLayoutPanel();
            this.ddlDeepSeekModel = new System.Windows.Forms.ComboBox();
            this.ddlDeepSeekReasoning = new System.Windows.Forms.ComboBox();
            this.nudDeepSeekTimeout = new System.Windows.Forms.NumericUpDown();
            this.nudDeepSeekMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkDeepSeekMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudDeepSeekTemp = new System.Windows.Forms.NumericUpDown();
            this.chkDeepSeekTempNull = new System.Windows.Forms.CheckBox();
            this.chkDeepSeekReasoning = new System.Windows.Forms.CheckBox();

            // Ollama tab controls
            this.flpOllama = new System.Windows.Forms.FlowLayoutPanel();
            this.ddlOllamaModel = new System.Windows.Forms.ComboBox();
            this.ddlOllamaReasoning = new System.Windows.Forms.ComboBox();
            this.btnOllamaRefreshModels = new System.Windows.Forms.Button();
            this.nudOllamaTimeout = new System.Windows.Forms.NumericUpDown();
            this.nudOllamaMaxTokens = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaMaxTokensNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaContext = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaContextNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaTemp = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaTempNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaTopP = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaTopPNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaTopK = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaTopKNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaMinP = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaMinPNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaRepeatPenalty = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaRepeatPenaltyNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaRepeatLastN = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaRepeatLastNNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaPresencePenalty = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaPresencePenaltyNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaFrequencyPenalty = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaFrequencyPenaltyNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaSeed = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaSeedNull = new System.Windows.Forms.CheckBox();
            this.txtOllamaStop = new System.Windows.Forms.TextBox();
            this.nudOllamaTypicalP = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaTypicalPNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaTfsZ = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaTfsZNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaMiroStat = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaMiroStatNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaMiroStatTau = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaMiroStatTauNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaMiroStatEta = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaMiroStatEtaNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaNumKeep = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaNumKeepNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaNumBatch = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaNumBatchNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaNumThread = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaNumThreadNull = new System.Windows.Forms.CheckBox();
            this.nudOllamaNumGpu = new System.Windows.Forms.NumericUpDown();
            this.chkOllamaNumGpuNull = new System.Windows.Forms.CheckBox();
            this.chkOllamaNumaNull = new System.Windows.Forms.CheckBox();
            this.chkOllamaNuma = new System.Windows.Forms.CheckBox();
            this.chkOllamaUseMlockNull = new System.Windows.Forms.CheckBox();
            this.chkOllamaUseMlock = new System.Windows.Forms.CheckBox();
            this.chkOllamaUseMmapNull = new System.Windows.Forms.CheckBox();
            this.chkOllamaUseMmap = new System.Windows.Forms.CheckBox();
            this.chkOllamaLowVramNull = new System.Windows.Forms.CheckBox();
            this.chkOllamaLowVram = new System.Windows.Forms.CheckBox();
            this.txtOllamaKeepAlive = new System.Windows.Forms.TextBox();

            // Per-model chunking/concurrency controls
            this.nudGeminiChunkSize = new System.Windows.Forms.NumericUpDown();
            this.nudGeminiMaxConcurrentChunks = new System.Windows.Forms.NumericUpDown();
            this.nudOpenAiChunkSize = new System.Windows.Forms.NumericUpDown();
            this.nudOpenAiMaxConcurrentChunks = new System.Windows.Forms.NumericUpDown();
            this.nudDeepSeekChunkSize = new System.Windows.Forms.NumericUpDown();
            this.nudDeepSeekMaxConcurrentChunks = new System.Windows.Forms.NumericUpDown();
            this.nudOllamaChunkSize = new System.Windows.Forms.NumericUpDown();
            this.nudOllamaMaxConcurrentChunks = new System.Windows.Forms.NumericUpDown();

            this.tabControl.SuspendLayout();
            this.tabGemini.SuspendLayout();
            this.tabOpenAi.SuspendLayout();
            this.tabDeepSeek.SuspendLayout();
            this.tabOllama.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMaxTokens)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaContext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTemp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTopP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTopK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMinP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaRepeatPenalty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaRepeatLastN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaPresencePenalty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaFrequencyPenalty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaSeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTypicalP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTfsZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStatTau)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStatEta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumKeep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumThread)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumGpu)).BeginInit();
            this.SuspendLayout();

            // TabControl
            this.tabControl.Controls.Add(this.tabGemini);
            this.tabControl.Controls.Add(this.tabOpenAi);
            this.tabControl.Controls.Add(this.tabDeepSeek);
            this.tabControl.Controls.Add(this.tabOllama);
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

            // Ollama Tab
            this.tabOllama.Controls.Add(this.flpOllama);
            this.tabOllama.Location = new System.Drawing.Point(4, 24);
            this.tabOllama.Name = "tabOllama";
            this.tabOllama.Padding = new System.Windows.Forms.Padding(10);
            this.tabOllama.Size = new System.Drawing.Size(556, 452);
            this.tabOllama.TabIndex = 3;
            this.tabOllama.Text = "Ollama";
            this.tabOllama.UseVisualStyleBackColor = true;

            this.flpOllama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpOllama.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpOllama.WrapContents = false;
            this.flpOllama.AutoScroll = true;

            // Setup controls helper layout
            BuildLabelledComboBox(this.flpGemini, "Model Name:", this.ddlGeminiModel);
            BuildLabelledComboBox(this.flpGemini, "Reasoning Level:", this.ddlGeminiReasoning);
            BuildLabelledNumeric(this.flpGemini, "Chunk Size (Lines):", this.nudGeminiChunkSize, 1, 1000);
            BuildLabelledNumeric(this.flpGemini, "Max Concurrent Chunks:", this.nudGeminiMaxConcurrentChunks, 1, 32);
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
            BuildCheckbox(this.flpGemini, "Use Managed Identity Authentication", this.chkGeminiUseManagedIdentity);
            BuildLabelledTextBox(this.flpGemini, "Credential Type (Optional):", this.txtGeminiCredentialType, false);

            BuildLabelledComboBox(this.flpOpenAi, "Model Name:", this.ddlOpenAiModel);
            BuildLabelledComboBox(this.flpOpenAi, "Reasoning Level:", this.ddlOpenAiReasoning);
            BuildLabelledNumeric(this.flpOpenAi, "Chunk Size (Lines):", this.nudOpenAiChunkSize, 1, 1000);
            BuildLabelledNumeric(this.flpOpenAi, "Max Concurrent Chunks:", this.nudOpenAiMaxConcurrentChunks, 1, 32);
            BuildLabelledNumeric(this.flpOpenAi, "Timeout (Seconds):", this.nudOpenAiTimeout, 1, 3600);
            BuildLabelledTextBox(this.flpOpenAi, "Organization ID (Optional):", this.txtOpenAiOrgId, false);
            BuildLabelledNumericWithNull(this.flpOpenAi, "Max Tokens:", this.nudOpenAiMaxTokens, this.chkOpenAiMaxTokensNull, 0, 1000000);
            this.chkOpenAiMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkOpenAiMaxTokensNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOpenAi, "Temperature (0.0 - 2.0):", this.nudOpenAiTemp, this.chkOpenAiTempNull, 0.0m, 2.0m);
            this.chkOpenAiTempNull.CheckedChanged += new System.EventHandler(this.ChkOpenAiTempNull_CheckedChanged);

            BuildLabelledComboBox(this.flpDeepSeek, "Model Name:", this.ddlDeepSeekModel);
            BuildLabelledComboBox(this.flpDeepSeek, "Reasoning Level:", this.ddlDeepSeekReasoning);
            BuildLabelledNumeric(this.flpDeepSeek, "Chunk Size (Lines):", this.nudDeepSeekChunkSize, 1, 1000);
            BuildLabelledNumeric(this.flpDeepSeek, "Max Concurrent Chunks:", this.nudDeepSeekMaxConcurrentChunks, 1, 32);
            BuildLabelledNumeric(this.flpDeepSeek, "Timeout (Seconds):", this.nudDeepSeekTimeout, 1, 3600);
            BuildLabelledNumericWithNull(this.flpDeepSeek, "Max Tokens:", this.nudDeepSeekMaxTokens, this.chkDeepSeekMaxTokensNull, 0, 1000000);
            this.chkDeepSeekMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkDeepSeekMaxTokensNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpDeepSeek, "Temperature (0.0 - 2.0):", this.nudDeepSeekTemp, this.chkDeepSeekTempNull, 0.0m, 2.0m);
            this.chkDeepSeekTempNull.CheckedChanged += new System.EventHandler(this.ChkDeepSeekTempNull_CheckedChanged);
            BuildCheckbox(this.flpDeepSeek, "Enable DeepSeek Reasoning Response", this.chkDeepSeekReasoning);

            BuildLabelledComboBox(this.flpOllama, "Model Name:", this.ddlOllamaModel);
            BuildLabelledComboBox(this.flpOllama, "Reasoning Level:", this.ddlOllamaReasoning);
            BuildLabelledButton(this.flpOllama, "Refresh model list from server", this.btnOllamaRefreshModels);
            this.btnOllamaRefreshModels.Click += new System.EventHandler(this.BtnOllamaRefreshModels_Click);
            BuildLabelledNumeric(this.flpOllama, "Chunk Size (Lines):", this.nudOllamaChunkSize, 1, 1000);
            BuildLabelledNumeric(this.flpOllama, "Max Concurrent Chunks:", this.nudOllamaMaxConcurrentChunks, 1, 32);
            BuildLabelledNumeric(this.flpOllama, "Timeout (Seconds):", this.nudOllamaTimeout, 1, 7200);
            BuildLabelledNumericWithNull(this.flpOllama, "Max Tokens (num_predict):", this.nudOllamaMaxTokens, this.chkOllamaMaxTokensNull, 0, 1000000);
            this.chkOllamaMaxTokensNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Context Window (num_ctx):", this.nudOllamaContext, this.chkOllamaContextNull, 0, 1000000);
            this.chkOllamaContextNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Temperature (0.0 - 2.0):", this.nudOllamaTemp, this.chkOllamaTempNull, 0.0m, 2.0m);
            this.chkOllamaTempNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Top P (0.0 - 1.0):", this.nudOllamaTopP, this.chkOllamaTopPNull, 0.0m, 1.0m);
            this.chkOllamaTopPNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Top K:", this.nudOllamaTopK, this.chkOllamaTopKNull, 0, 1000);
            this.chkOllamaTopKNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Min P (0.0 - 1.0):", this.nudOllamaMinP, this.chkOllamaMinPNull, 0.0m, 1.0m);
            this.chkOllamaMinPNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Repeat Penalty (0.0 - 2.0):", this.nudOllamaRepeatPenalty, this.chkOllamaRepeatPenaltyNull, 0.0m, 2.0m);
            this.chkOllamaRepeatPenaltyNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Repeat Last N:", this.nudOllamaRepeatLastN, this.chkOllamaRepeatLastNNull, -1, 100000);
            this.chkOllamaRepeatLastNNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Presence Penalty (-2.0 - 2.0):", this.nudOllamaPresencePenalty, this.chkOllamaPresencePenaltyNull, -2.0m, 2.0m);
            this.chkOllamaPresencePenaltyNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Frequency Penalty (-2.0 - 2.0):", this.nudOllamaFrequencyPenalty, this.chkOllamaFrequencyPenaltyNull, -2.0m, 2.0m);
            this.chkOllamaFrequencyPenaltyNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Seed:", this.nudOllamaSeed, this.chkOllamaSeedNull, 0, 2147483647);
            this.chkOllamaSeedNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledTextBox(this.flpOllama, "Stop Sequences (comma-separated):", this.txtOllamaStop, false);
            BuildLabelledDecimalWithNull(this.flpOllama, "Typical P (0.0 - 1.0):", this.nudOllamaTypicalP, this.chkOllamaTypicalPNull, 0.0m, 1.0m);
            this.chkOllamaTypicalPNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "Tfs Z (0.0 - 2.0):", this.nudOllamaTfsZ, this.chkOllamaTfsZNull, 0.0m, 2.0m);
            this.chkOllamaTfsZNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "MiroStat (0/1/2):", this.nudOllamaMiroStat, this.chkOllamaMiroStatNull, 0, 2);
            this.chkOllamaMiroStatNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "MiroStat Tau (0.0 - 10.0):", this.nudOllamaMiroStatTau, this.chkOllamaMiroStatTauNull, 0.0m, 10.0m);
            this.chkOllamaMiroStatTauNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledDecimalWithNull(this.flpOllama, "MiroStat Eta (0.0 - 1.0):", this.nudOllamaMiroStatEta, this.chkOllamaMiroStatEtaNull, 0.0m, 1.0m);
            this.chkOllamaMiroStatEtaNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Num Keep:", this.nudOllamaNumKeep, this.chkOllamaNumKeepNull, -1, 100000);
            this.chkOllamaNumKeepNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Num Batch:", this.nudOllamaNumBatch, this.chkOllamaNumBatchNull, 1, 8192);
            this.chkOllamaNumBatchNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Num Thread:", this.nudOllamaNumThread, this.chkOllamaNumThreadNull, 0, 256);
            this.chkOllamaNumThreadNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledNumericWithNull(this.flpOllama, "Num GPU Layers:", this.nudOllamaNumGpu, this.chkOllamaNumGpuNull, 0, 10000);
            this.chkOllamaNumGpuNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildNullableCheckbox(this.flpOllama, "NUMA:", this.chkOllamaNuma, this.chkOllamaNumaNull);
            this.chkOllamaNumaNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildNullableCheckbox(this.flpOllama, "Use Mlock:", this.chkOllamaUseMlock, this.chkOllamaUseMlockNull);
            this.chkOllamaUseMlockNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildNullableCheckbox(this.flpOllama, "Use Mmap:", this.chkOllamaUseMmap, this.chkOllamaUseMmapNull);
            this.chkOllamaUseMmapNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildNullableCheckbox(this.flpOllama, "Low Vram:", this.chkOllamaLowVram, this.chkOllamaLowVramNull);
            this.chkOllamaLowVramNull.CheckedChanged += new System.EventHandler(this.ChkOllamaNull_CheckedChanged);
            BuildLabelledTextBox(this.flpOllama, "Keep Alive (e.g., 5m, -1, 0):", this.txtOllamaKeepAlive, false);

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
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

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
            this.tabOllama.ResumeLayout(false);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMaxTokens)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaContext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTemp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTopP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTopK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMinP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaRepeatPenalty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaRepeatLastN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaPresencePenalty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaFrequencyPenalty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaSeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTypicalP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaTfsZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStatTau)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaMiroStatEta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumKeep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumThread)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOllamaNumGpu)).EndInit();
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

        private static void BuildNullableCheckbox(System.Windows.Forms.FlowLayoutPanel panel, string labelText, System.Windows.Forms.CheckBox checkBox, System.Windows.Forms.CheckBox chkNull)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 30) };
            var lbl = new System.Windows.Forms.Label { Text = labelText, Location = new System.Drawing.Point(5, 7), Size = new System.Drawing.Size(120, 15) };
            checkBox.Text = "Enabled";
            checkBox.Location = new System.Drawing.Point(130, 5);
            checkBox.Size = new System.Drawing.Size(80, 20);
            checkBox.AutoCheck = true;
            chkNull.Text = "Not Configured / Inherit Default";
            chkNull.Location = new System.Drawing.Point(220, 5);
            chkNull.Size = new System.Drawing.Size(200, 20);
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(checkBox);
            pnl.Controls.Add(chkNull);
            panel.Controls.Add(pnl);
        }

        private static void BuildLabelledButton(System.Windows.Forms.FlowLayoutPanel panel, string buttonText, System.Windows.Forms.Button button)
        {
            var pnl = new System.Windows.Forms.Panel { Size = new System.Drawing.Size(510, 36) };
            button.Location = new System.Drawing.Point(5, 5);
            button.Size = new System.Drawing.Size(240, 26);
            button.Text = buttonText;
            button.UseVisualStyleBackColor = true;
            pnl.Controls.Add(button);
            panel.Controls.Add(pnl);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGemini;
        private System.Windows.Forms.TabPage tabOpenAi;
        private System.Windows.Forms.TabPage tabDeepSeek;
        private System.Windows.Forms.TabPage tabOllama;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private System.Windows.Forms.FlowLayoutPanel flpGemini;
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
        private System.Windows.Forms.CheckBox chkGeminiUseManagedIdentity;
        private System.Windows.Forms.TextBox txtGeminiCredentialType;
        private System.Windows.Forms.ComboBox ddlGeminiReasoning;

        private System.Windows.Forms.FlowLayoutPanel flpOpenAi;
        private System.Windows.Forms.ComboBox ddlOpenAiModel;
        private System.Windows.Forms.NumericUpDown nudOpenAiTimeout;
        private System.Windows.Forms.TextBox txtOpenAiOrgId;
        private System.Windows.Forms.NumericUpDown nudOpenAiMaxTokens;
        private System.Windows.Forms.CheckBox chkOpenAiMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudOpenAiTemp;
        private System.Windows.Forms.CheckBox chkOpenAiTempNull;
        private System.Windows.Forms.ComboBox ddlOpenAiReasoning;

        private System.Windows.Forms.FlowLayoutPanel flpDeepSeek;
        private System.Windows.Forms.ComboBox ddlDeepSeekModel;
        private System.Windows.Forms.NumericUpDown nudDeepSeekTimeout;
        private System.Windows.Forms.NumericUpDown nudDeepSeekMaxTokens;
        private System.Windows.Forms.CheckBox chkDeepSeekMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudDeepSeekTemp;
        private System.Windows.Forms.CheckBox chkDeepSeekTempNull;
        private System.Windows.Forms.CheckBox chkDeepSeekReasoning;
        private System.Windows.Forms.ComboBox ddlDeepSeekReasoning;

        private System.Windows.Forms.FlowLayoutPanel flpOllama;
        private System.Windows.Forms.ComboBox ddlOllamaModel;
        private System.Windows.Forms.ComboBox ddlOllamaReasoning;
        private System.Windows.Forms.Button btnOllamaRefreshModels;
        private System.Windows.Forms.NumericUpDown nudOllamaTimeout;
        private System.Windows.Forms.NumericUpDown nudOllamaMaxTokens;
        private System.Windows.Forms.CheckBox chkOllamaMaxTokensNull;
        private System.Windows.Forms.NumericUpDown nudOllamaContext;
        private System.Windows.Forms.CheckBox chkOllamaContextNull;
        private System.Windows.Forms.NumericUpDown nudOllamaTemp;
        private System.Windows.Forms.CheckBox chkOllamaTempNull;
        private System.Windows.Forms.NumericUpDown nudOllamaTopP;
        private System.Windows.Forms.CheckBox chkOllamaTopPNull;
        private System.Windows.Forms.NumericUpDown nudOllamaTopK;
        private System.Windows.Forms.CheckBox chkOllamaTopKNull;
        private System.Windows.Forms.NumericUpDown nudOllamaMinP;
        private System.Windows.Forms.CheckBox chkOllamaMinPNull;
        private System.Windows.Forms.NumericUpDown nudOllamaRepeatPenalty;
        private System.Windows.Forms.CheckBox chkOllamaRepeatPenaltyNull;
        private System.Windows.Forms.NumericUpDown nudOllamaRepeatLastN;
        private System.Windows.Forms.CheckBox chkOllamaRepeatLastNNull;
        private System.Windows.Forms.NumericUpDown nudOllamaPresencePenalty;
        private System.Windows.Forms.CheckBox chkOllamaPresencePenaltyNull;
        private System.Windows.Forms.NumericUpDown nudOllamaFrequencyPenalty;
        private System.Windows.Forms.CheckBox chkOllamaFrequencyPenaltyNull;
        private System.Windows.Forms.NumericUpDown nudOllamaSeed;
        private System.Windows.Forms.CheckBox chkOllamaSeedNull;
        private System.Windows.Forms.TextBox txtOllamaStop;
        private System.Windows.Forms.NumericUpDown nudOllamaTypicalP;
        private System.Windows.Forms.CheckBox chkOllamaTypicalPNull;
        private System.Windows.Forms.NumericUpDown nudOllamaTfsZ;
        private System.Windows.Forms.CheckBox chkOllamaTfsZNull;
        private System.Windows.Forms.NumericUpDown nudOllamaMiroStat;
        private System.Windows.Forms.CheckBox chkOllamaMiroStatNull;
        private System.Windows.Forms.NumericUpDown nudOllamaMiroStatTau;
        private System.Windows.Forms.CheckBox chkOllamaMiroStatTauNull;
        private System.Windows.Forms.NumericUpDown nudOllamaMiroStatEta;
        private System.Windows.Forms.CheckBox chkOllamaMiroStatEtaNull;
        private System.Windows.Forms.NumericUpDown nudOllamaNumKeep;
        private System.Windows.Forms.CheckBox chkOllamaNumKeepNull;
        private System.Windows.Forms.NumericUpDown nudOllamaNumBatch;
        private System.Windows.Forms.CheckBox chkOllamaNumBatchNull;
        private System.Windows.Forms.NumericUpDown nudOllamaNumThread;
        private System.Windows.Forms.CheckBox chkOllamaNumThreadNull;
        private System.Windows.Forms.NumericUpDown nudOllamaNumGpu;
        private System.Windows.Forms.CheckBox chkOllamaNumGpuNull;
        private System.Windows.Forms.CheckBox chkOllamaNumaNull;
        private System.Windows.Forms.CheckBox chkOllamaNuma;
        private System.Windows.Forms.CheckBox chkOllamaUseMlockNull;
        private System.Windows.Forms.CheckBox chkOllamaUseMlock;
        private System.Windows.Forms.CheckBox chkOllamaUseMmapNull;
        private System.Windows.Forms.CheckBox chkOllamaUseMmap;
        private System.Windows.Forms.CheckBox chkOllamaLowVramNull;
        private System.Windows.Forms.CheckBox chkOllamaLowVram;
        private System.Windows.Forms.TextBox txtOllamaKeepAlive;

        // Per-model chunking/concurrency controls
        private System.Windows.Forms.NumericUpDown nudGeminiChunkSize;
        private System.Windows.Forms.NumericUpDown nudGeminiMaxConcurrentChunks;
        private System.Windows.Forms.NumericUpDown nudOpenAiChunkSize;
        private System.Windows.Forms.NumericUpDown nudOpenAiMaxConcurrentChunks;
        private System.Windows.Forms.NumericUpDown nudDeepSeekChunkSize;
        private System.Windows.Forms.NumericUpDown nudDeepSeekMaxConcurrentChunks;
        private System.Windows.Forms.NumericUpDown nudOllamaChunkSize;
        private System.Windows.Forms.NumericUpDown nudOllamaMaxConcurrentChunks;
    }
}