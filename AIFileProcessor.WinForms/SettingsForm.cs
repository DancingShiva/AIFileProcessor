//-----------------------------------------------------------------------
// <copyright file="SettingsForm.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.Windows.Forms;
    using AIFileProcessor.Configuration;
    using AIFileProcessor.Services;

    /// <summary>
    /// Serves as the configuration settings dialog, allowing users to modify and securely persist API settings for Gemini, OpenAI, and DeepSeek.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly GeminiSettingsManager _geminiManager;
        private readonly OpenAiSettingsManager _openAiManager;
        private readonly DeepSeekSettingsManager _deepSeekManager;

        public SettingsForm(
            GeminiSettingsManager geminiManager,
            OpenAiSettingsManager openAiManager,
            DeepSeekSettingsManager deepSeekManager)
        {
            InitializeComponent();

            _geminiManager = geminiManager ?? throw new ArgumentNullException(nameof(geminiManager));
            _openAiManager = openAiManager ?? throw new ArgumentNullException(nameof(openAiManager));
            _deepSeekManager = deepSeekManager ?? throw new ArgumentNullException(nameof(deepSeekManager));
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load Gemini Settings
                var gemini = _geminiManager.ReadSettings();
                txtGeminiApiKey.Text = gemini.ApiKey;
                txtGeminiEndpoint.Text = gemini.Endpoint ?? string.Empty;
                txtGeminiModel.Text = gemini.ModelName ?? string.Empty;
                nudGeminiTimeout.Value = gemini.TimeoutSeconds;
                txtGeminiProjectId.Text = gemini.ProjectId ?? string.Empty;
                txtGeminiLocation.Text = gemini.Location ?? string.Empty;
                nudGeminiMaxTokens.Value = gemini.MaxTokens ?? 0;
                chkGeminiMaxTokensNull.Checked = !gemini.MaxTokens.HasValue;
                nudGeminiTemp.Value = (decimal)(gemini.Temperature ?? 0.0);
                chkGeminiTempNull.Checked = !gemini.Temperature.HasValue;
                nudGeminiTopP.Value = (decimal)(gemini.TopP ?? 0.0);
                chkGeminiTopPNull.Checked = !gemini.TopP.HasValue;
                chkGeminiStreaming.Checked = gemini.StreamingEnabled;
                nudGeminiMaxRetries.Value = gemini.MaxRetries;
                nudGeminiBaseDelay.Value = gemini.BaseDelay;
                nudGeminiMaxDelay.Value = gemini.MaxDelay;
                chkGeminiEnableLogging.Checked = gemini.EnableLogging;
                nudGeminiRateLimit.Value = gemini.RateLimitPerMinute ?? 0;
                chkGeminiRateLimitNull.Checked = !gemini.RateLimitPerMinute.HasValue;
                txtGeminiProxyUrl.Text = gemini.ProxyUrl ?? string.Empty;
                chkGeminiUseManagedIdentity.Checked = gemini.UseManagedIdentity;
                txtGeminiCredentialType.Text = gemini.CredentialType ?? string.Empty;

                // Load OpenAI Settings
                var openAi = _openAiManager.ReadSettings();
                txtOpenAiApiKey.Text = openAi.ApiKey;
                txtOpenAiEndpoint.Text = openAi.Endpoint ?? string.Empty;
                txtOpenAiModel.Text = openAi.ModelName ?? string.Empty;
                nudOpenAiTimeout.Value = openAi.TimeoutSeconds;
                txtOpenAiOrgId.Text = openAi.OrganizationId ?? string.Empty;
                nudOpenAiMaxTokens.Value = openAi.MaxTokens ?? 0;
                chkOpenAiMaxTokensNull.Checked = !openAi.MaxTokens.HasValue;
                nudOpenAiTemp.Value = (decimal)(openAi.Temperature ?? 0.0);
                chkOpenAiTempNull.Checked = !openAi.Temperature.HasValue;

                // Load DeepSeek Settings
                var deepSeek = _deepSeekManager.ReadSettings();
                txtDeepSeekApiKey.Text = deepSeek.ApiKey;
                txtDeepSeekEndpoint.Text = deepSeek.Endpoint ?? string.Empty;
                nudDeepSeekTimeout.Value = deepSeek.TimeoutSeconds;
                nudDeepSeekMaxTokens.Value = deepSeek.MaxTokens ?? 0;
                chkDeepSeekMaxTokensNull.Checked = !deepSeek.MaxTokens.HasValue;
                nudDeepSeekTemp.Value = (decimal)(deepSeek.Temperature ?? 0.0);
                chkDeepSeekTempNull.Checked = !deepSeek.Temperature.HasValue;
                chkDeepSeekReasoning.Checked = deepSeek.EnableReasoning;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Save Gemini Settings
                var gemini = new GeminiSettings
                {
                    ApiKey = txtGeminiApiKey.Text,
                    Endpoint = string.IsNullOrWhiteSpace(txtGeminiEndpoint.Text) ? null : txtGeminiEndpoint.Text,
                    ModelName = string.IsNullOrWhiteSpace(txtGeminiModel.Text) ? null : txtGeminiModel.Text,
                    TimeoutSeconds = (int)nudGeminiTimeout.Value,
                    ProjectId = string.IsNullOrWhiteSpace(txtGeminiProjectId.Text) ? null : txtGeminiProjectId.Text,
                    Location = string.IsNullOrWhiteSpace(txtGeminiLocation.Text) ? null : txtGeminiLocation.Text,
                    MaxTokens = chkGeminiMaxTokensNull.Checked ? null : (int)nudGeminiMaxTokens.Value,
                    Temperature = chkGeminiTempNull.Checked ? null : (double)nudGeminiTemp.Value,
                    TopP = chkGeminiTopPNull.Checked ? null : (double)nudGeminiTopP.Value,
                    StreamingEnabled = chkGeminiStreaming.Checked,
                    MaxRetries = (int)nudGeminiMaxRetries.Value,
                    BaseDelay = (int)nudGeminiBaseDelay.Value,
                    MaxDelay = (int)nudGeminiMaxDelay.Value,
                    EnableLogging = chkGeminiEnableLogging.Checked,
                    RateLimitPerMinute = chkGeminiRateLimitNull.Checked ? null : (int)nudGeminiRateLimit.Value,
                    ProxyUrl = string.IsNullOrWhiteSpace(txtGeminiProxyUrl.Text) ? null : txtGeminiProxyUrl.Text,
                    UseManagedIdentity = chkGeminiUseManagedIdentity.Checked,
                    CredentialType = string.IsNullOrWhiteSpace(txtGeminiCredentialType.Text) ? null : txtGeminiCredentialType.Text
                };
                _geminiManager.SaveSettings(gemini);

                // Save OpenAI Settings
                var openAi = new OpenAiSettings
                {
                    ApiKey = txtOpenAiApiKey.Text,
                    Endpoint = txtOpenAiEndpoint.Text,
                    ModelName = txtOpenAiModel.Text,
                    TimeoutSeconds = (int)nudOpenAiTimeout.Value,
                    OrganizationId = string.IsNullOrWhiteSpace(txtOpenAiOrgId.Text) ? null : txtOpenAiOrgId.Text,
                    MaxTokens = chkOpenAiMaxTokensNull.Checked ? null : (int)nudOpenAiMaxTokens.Value,
                    Temperature = chkOpenAiTempNull.Checked ? null : (double)nudOpenAiTemp.Value
                };
                _openAiManager.SaveSettings(openAi);

                // Save DeepSeek Settings
                var deepSeek = new DeepSeekSettings
                {
                    ApiKey = txtDeepSeekApiKey.Text,
                    Endpoint = txtDeepSeekEndpoint.Text,
                    TimeoutSeconds = (int)nudDeepSeekTimeout.Value,
                    MaxTokens = chkDeepSeekMaxTokensNull.Checked ? null : (int)nudDeepSeekMaxTokens.Value,
                    Temperature = chkDeepSeekTempNull.Checked ? null : (double)nudDeepSeekTemp.Value,
                    EnableReasoning = chkDeepSeekReasoning.Checked
                };
                _deepSeekManager.SaveSettings(deepSeek);

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration settings: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ChkGeminiMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiMaxTokens.Enabled = !chkGeminiMaxTokensNull.Checked;
        }

        private void ChkGeminiTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiTemp.Enabled = !chkGeminiTempNull.Checked;
        }

        private void ChkGeminiTopPNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiTopP.Enabled = !chkGeminiTopPNull.Checked;
        }

        private void ChkGeminiRateLimitNull_CheckedChanged(object sender, EventArgs e)
        {
            nudGeminiRateLimit.Enabled = !chkGeminiRateLimitNull.Checked;
        }

        private void ChkOpenAiMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudOpenAiMaxTokens.Enabled = !chkOpenAiMaxTokensNull.Checked;
        }

        private void ChkOpenAiTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudOpenAiTemp.Enabled = !chkOpenAiTempNull.Checked;
        }

        private void ChkDeepSeekMaxTokensNull_CheckedChanged(object sender, EventArgs e)
        {
            nudDeepSeekMaxTokens.Enabled = !chkDeepSeekMaxTokensNull.Checked;
        }

        private void ChkDeepSeekTempNull_CheckedChanged(object sender, EventArgs e)
        {
            nudDeepSeekTemp.Enabled = !chkDeepSeekTempNull.Checked;
        }
    }
}