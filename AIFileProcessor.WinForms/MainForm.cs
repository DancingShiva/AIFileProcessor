//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using AIFileProcessor.Interfaces;
    using AIFileProcessor.Models;
    using AIFileProcessor.Processing;
    using AIFileProcessor.Response;
    using AIFileProcessor.Services;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This enum defines the different display modes for the main form, allowing the user to switch between viewing the prompt, data, scratchpad, response, corrections, and formatted output.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// The original prompt text loaded by the user.
        /// </summary>
        Prompt,

        /// <summary>
        /// The input data lines loaded by the user.
        /// </summary>
        Data,

        /// <summary>
        /// The intermediate scratchpad output from the Gemini pipeline.
        /// </summary>
        Scratchpad,

        /// <summary>
        /// The raw response output from the Gemini pipeline (tab-delimited).
        /// </summary>
        Response,

        /// <summary>
        /// The corrected output from the user (if any corrections were made).
        /// </summary>
        Corrections,

        /// <summary>
        /// The final formatted output after processing the raw response.
        /// </summary>
        Formatted,
    }

    /// <summary>
    /// MainForm startup. Partial class definition - UI components are defined in MainForm.Designer.cs.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private readonly IFileOps _fileOps;
        private readonly Func<SettingsForm> _settingsFormFactory;
        private readonly IEnumerable<IAiService> _aiServices;
        private readonly ProcessingOrchestrator _processingOrchestrator;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;

        private readonly ProcessingOptions _options = new();

        private CancellationTokenSource? _processingCts;
        private bool _isProcessing;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class with constructor-injected dependencies.
        /// </summary>
        /// <param name="logger">The logger for diagnostic messages.</param>
        /// <param name="fileOps">The file operations service.</param>
        /// <param name="settingsFormFactory">Factory delegate to create transient <see cref="SettingsForm"/> instances.</param>
        /// <param name="aiServices">The collection of registered AI services used to populate the model dropdown.</param>
        /// <param name="processingOrchestrator">The processing orchestrator that executes the Chinese text processing pipeline.</param>
        /// <param name="settingsProvider">The settings provider for persisting per-user processing options.</param>
        /// <param name="userContext">The user context for resolving the current user's settings.</param>
        public MainForm(
            ILogger<MainForm> logger,
            IFileOps fileOps,
            Func<SettingsForm> settingsFormFactory,
            IEnumerable<IAiService> aiServices,
            ProcessingOrchestrator processingOrchestrator,
            ISettingsProvider settingsProvider,
            IUserContext userContext)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(fileOps);
            ArgumentNullException.ThrowIfNull(settingsFormFactory);
            ArgumentNullException.ThrowIfNull(aiServices);
            ArgumentNullException.ThrowIfNull(processingOrchestrator);
            ArgumentNullException.ThrowIfNull(settingsProvider);
            ArgumentNullException.ThrowIfNull(userContext);

            InitializeComponent();

            _logger = logger;
            _fileOps = fileOps;
            _settingsFormFactory = settingsFormFactory;
            _aiServices = aiServices;
            _processingOrchestrator = processingOrchestrator;
            _settingsProvider = settingsProvider;
            _userContext = userContext;

            FillModelListBox();
            LoadLastProcessingOptions();

            ddlModel.SelectedIndexChanged += (s, e) => OnModelSelectionChanged();
            ddlReasoning.SelectedIndexChanged += (s, e) => OnReasoningSelectionChanged();
        }

        /// <summary>
        /// Saves the text from the specified <paramref name="source"/> RichTextBox to a file.
        /// If <paramref name="source"/> is null, the response RichTextBox is used.
        /// </summary>
        /// <param name="defaultFileName">The default file name for the save dialog.</param>
        /// <param name="source">The RichTextBox to save from, or null to default to the response box.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveTextAsync(string defaultFileName, RichTextBox? source)
        {
            source ??= rtbResponse;

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*",
                FileName = defaultFileName,
                Title = "Save Response",
                DefaultExt = "txt",
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FilterIndex == 2)
                {
                    source.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.RichText);
                }
                else
                {
                    await _fileOps.SaveFileAsync(saveFileDialog.FileName, source.Text);
                }
            }
        }

        public async Task LoadTextAsync(DisplayMode source, RichTextBox destination)
        {
            // Modern using declaration - resource disposed at the end of the method scope
            using OpenFileDialog openFileDialog = new() { Filter = "Text Files|*.txt|All Files|*.*" };

            _logger.LogInformation("Opening file dialog with filter: {Filter}", openFileDialog.Filter);

            openFileDialog.Title = "Select a File";

            // Use the last-saved path as the initial directory for the file dialog
            string? savedPath = source switch
            {
                DisplayMode.Prompt => _options.PromptPath,
                DisplayMode.Data => _options.DataPath,
                DisplayMode.Response => _options.ResponsePath,
                _ => null,
            };

            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                try
                {
                    string? dir = Path.GetDirectoryName(savedPath);
                    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    {
                        openFileDialog.InitialDirectory = dir;
                    }
                }
                catch
                {
                    // Ignore invalid path — fall back to default directory
                }
            }

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _logger.LogInformation("Opening file name: {FileName}", openFileDialog.FileName);

                    // Asynchronous read - frees the UI message loop while reading
                    destination.Text = await _fileOps.LoadFileAsync(openFileDialog.FileName);

                    switch (source)
                    {
                        case DisplayMode.Data:
                            _options.Data = destination.Text;
                            _options.DataPath = openFileDialog.FileName;
                            break;
                        case DisplayMode.Prompt:
                            _options.Prompt = destination.Text;
                            _options.PromptPath = openFileDialog.FileName;
                            break;
                        case DisplayMode.Response:
                            _options.Response = destination.Text;
                            _options.ResponsePath = openFileDialog.FileName;
                            break;
                    }

                    // Persist the updated paths so they're available as defaults next session
                    SaveProcessingOptions();

                    UpdateDisplay(source);
                    _logger.LogInformation("Successfully read: {FileName}", openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while loading file: {Message}", ex.Message);
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnOpenPrompt_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadTextAsync(DisplayMode.Prompt, rtbPrompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in btnOpenPrompt_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnOpenData_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadTextAsync(DisplayMode.Data, rtbData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in btnOpenData_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Stage 1: call AI API and prepare raw text for rtbResponse
        private async void btnProcess_Click(object sender, EventArgs e)
        {
            if (_isProcessing)
            {
                try
                {
                    btnProcess.Enabled = false;
                    btnProcess.Text = "Cancelling...";
                    _processingCts?.Cancel();
                    _logger.LogInformation("Cancel requested by user.");
                }
                catch (ObjectDisposedException)
                {
                    // Safe to ignore if already disposed on completion
                }

                return;
            }

            try
            {
                _isProcessing = true;
                btnProcess.Text = "Cancel";

                UpdateDisplay(DisplayMode.Response);
                rtbResponse.Text = "Processing in progress.";

                if (!FillOptionsFromUI())
                {
                    return;
                }

                SaveProcessingOptions();

                _processingCts?.Dispose();
                _processingCts = new CancellationTokenSource();
                var ct = _processingCts.Token;

                ProcessingResult result;
                try
                {
                    result = await _processingOrchestrator.ProcessAsync(_options, ct);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Processing was cancelled by the user.");
                    rtbResponse.Text = "Processing cancelled.";
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("ProcessingOrchestrator.ProcessAsync failed: {exception}.", ex.ToString());
                    MessageBox.Show($"An error occurred during processing: {ex.Message}", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    _processingCts?.Dispose();
                    _processingCts = null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Successfully called ProcessAsync.");
                }

                if (result.ProcessedSentenceRows.Count == 0)
                {
                    _logger.LogWarning("ProcessAsync completed with no sentence rows.");
                    rtbResponse.Text = "Processing completed but no sentences were returned.";
                    return;
                }

                rtbScratchpad.Text = result.Scratchpad;

                rtbCorrections.Text = result.Corrections == null || result.Corrections.Count == 0
                    ? "There were no corrections."
                    : string.Join(System.Environment.NewLine, result.Corrections);

                rtbResponse.Text = result.RawResponse;

                UpdateDisplay(DisplayMode.Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in btnProcess_Click: {Message}", ex.Message);
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnProcess.Text = "Process";
                btnProcess.Enabled = true;
            }
        }

        // Stage 2: parse rtbResponse, format breakdowns, and populate rtbFormatted
        private async void btnFormatResponse_Click(object sender, EventArgs e)
        {
            try
            {
                string textToParse = rtbResponse.Text;
                btnFormatResponse.Enabled = false;

                var (parsedRows, formattedText) = await Task.Run(() =>
                {
                    var rows = ResponseParser.Parse(textToParse);
                    var formatted = rows.Count > 0 ? ResponseFormatter.FormatRows(rows) : string.Empty;
                    return (rows, formatted);
                });

                if (parsedRows.Count == 0)
                {
                    MessageBox.Show("No parseable response rows to format.");
                    return;
                }

                rtbFormatted.Text = formattedText;

                UpdateDisplay(DisplayMode.Formatted);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Formatting failed: {ex.Message}");
            }
            finally
            {
                btnFormatResponse.Enabled = true;
            }
        }

        private async void btnSaveResponse_Click(object sender, EventArgs e)
        {
            try
            {
                if (_options.DataPath.Length > 0)
                {
                    await SaveTextAsync(string.Format("{0}{1}", _options.DataPath.Replace(".txt", string.Empty), " Response.txt"), rtbResponse);
                }
                else
                {
                    await SaveTextAsync("Response.txt", rtbResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in btnSaveResponse_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSaveFormatted_Click(object sender, EventArgs e)
        {
            try
            {
                if (_options.DataPath.Length > 0)
                {
                    await SaveTextAsync(string.Format("{0}{1}", _options.DataPath.Replace(".txt", string.Empty), " Import.txt"), rtbFormatted);
                }
                else
                {
                    await SaveTextAsync("Response Import.txt", rtbFormatted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in btnSaveFormatted_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (rtbPrompt.Visible)
                {
                    await LoadTextAsync(DisplayMode.Prompt, rtbPrompt);
                }
                else if (rtbData.Visible)
                {
                    await LoadTextAsync(DisplayMode.Data, rtbData);
                }
                else if (rtbResponse.Visible)
                {
                    await LoadTextAsync(DisplayMode.Response, rtbResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in openToolStripMenuItem_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var settingsForm = _settingsFormFactory();
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Settings may have changed the per-model reasoning levels;
                    // reload them and refresh the reasoning dropdown for the current model.
                    ReloadModelReasoningLevels();
                    OnModelSelectionChanged();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open Settings Form.");
                MessageBox.Show($"Failed to open settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FillModelListBox()
        {
            ddlModel.DisplayMember = nameof(AiModelInfo.ModelName);
            ddlModel.ValueMember = nameof(AiModelInfo.ModelId);

            foreach (var service in _aiServices)
            {
                foreach (var model in service.GetSupportedModels())
                {
                    ddlModel.Items.Add(model);
                }
            }
        }

        private void OnModelSelectionChanged()
        {
            if (ddlModel.SelectedItem is not AiModelInfo selectedModel)
            {
                ddlReasoning.Items.Clear();
                return;
            }

            // Find the service that supports this model and get its reasoning levels
            var reasoningLevels = Array.Empty<ReasoningLevel>();
            foreach (var service in _aiServices)
            {
                if (service.SupportsModel(selectedModel.ModelId))
                {
                    reasoningLevels = service.GetSupportedReasoningLevels(selectedModel.ModelId).ToArray();
                    break;
                }
            }

            ddlReasoning.Items.Clear();
            foreach (var level in reasoningLevels)
            {
                ddlReasoning.Items.Add(level);
            }

            // Restore the user's last-selected reasoning level for this model
            if (_options.ModelReasoningLevels.TryGetValue(selectedModel.ModelId, out var savedLevel))
            {
                for (int i = 0; i < ddlReasoning.Items.Count; i++)
                {
                    if (ddlReasoning.Items[i] is ReasoningLevel level && level == savedLevel)
                    {
                        ddlReasoning.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (ddlReasoning.Items.Count > 0)
            {
                // Default to the first (lowest) reasoning level
                ddlReasoning.SelectedIndex = 0;
            }
        }

        private void OnReasoningSelectionChanged()
        {
            if (ddlModel.SelectedItem is AiModelInfo selectedModel
                && ddlReasoning.SelectedItem is ReasoningLevel level)
            {
                _options.ModelReasoningLevels[selectedModel.ModelId] = level;
                SaveProcessingOptions();
            }
        }

        private void ReloadModelReasoningLevels()
        {
            try
            {
                var saved = _settingsProvider.GetProcessingOptionsAsync(_userContext.UserId).GetAwaiter().GetResult();
                if (saved.ModelReasoningLevels != null)
                {
                    _options.ModelReasoningLevels = saved.ModelReasoningLevels;
                }

                if (saved.ModelMaxTokens != null)
                {
                    _options.ModelMaxTokens = saved.ModelMaxTokens;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reload reasoning levels after settings change.");
            }
        }

        private void LoadLastProcessingOptions()
        {
            try
            {
                var saved = _settingsProvider.GetProcessingOptionsAsync(_userContext.UserId).GetAwaiter().GetResult();

                // Copy the saved per-model reasoning levels into the live options
                if (saved.ModelReasoningLevels != null)
                {
                    _options.ModelReasoningLevels = saved.ModelReasoningLevels;
                }

                if (saved.ModelMaxTokens != null)
                {
                    _options.ModelMaxTokens = saved.ModelMaxTokens;
                }

                if (!string.IsNullOrWhiteSpace(saved.ModelId))
                {
                    for (int i = 0; i < ddlModel.Items.Count; i++)
                    {
                        if (ddlModel.Items[i] is AiModelInfo model && model.ModelId == saved.ModelId)
                        {
                            ddlModel.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // OnModelSelectionChanged (fired by SelectedIndexChanged) handles
                // populating ddlReasoning and restoring the saved level from
                // _options.ModelReasoningLevels. But if the model was set
                // programmatically and the event already fired, ensure the
                // reasoning dropdown is populated by calling it explicitly.
                if (ddlReasoning.Items.Count == 0 && ddlModel.SelectedItem is AiModelInfo)
                {
                    OnModelSelectionChanged();
                }

                if (!string.IsNullOrWhiteSpace(saved.IdPrefix))
                {
                    txtPrefix.Text = saved.IdPrefix;
                }

                nudIdStart.Value = Math.Clamp(saved.IdStart, nudIdStart.Minimum, nudIdStart.Maximum);
                nudIdPad.Value = Math.Clamp(saved.IdPad, nudIdPad.Minimum, nudIdPad.Maximum);
                nudLengthPad.Value = Math.Clamp(saved.LengthPad, nudLengthPad.Minimum, nudLengthPad.Maximum);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load last processing options; using defaults.");
            }
        }

        private void SaveProcessingOptions()
        {
            try
            {
                _settingsProvider.SaveProcessingOptionsAsync(_userContext.UserId, _options).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save processing options.");
            }
        }

        private bool FillOptionsFromUI()
        {
            if (ddlModel.SelectedItem != null && !string.IsNullOrWhiteSpace(ddlModel.Text))
            {
                _options.ModelId = ((AiModelInfo)ddlModel.SelectedItem).ModelId;
            }
            else
            {
                MessageBox.Show("Please select a model before processing.");
                ddlModel.Focus();
                return false;
            }

            if (ddlReasoning.SelectedItem is ReasoningLevel reasoningLevel)
            {
                _options.ModelReasoningLevels[_options.ModelId] = reasoningLevel;
            }

            if (!string.IsNullOrWhiteSpace(txtPrefix.Text))
            {
                _options.IdPrefix = txtPrefix.Text.Trim();
            }
            else
            {
                MessageBox.Show("Please enter a prefix before processing.");
                txtPrefix.Focus();
                return false;
            }

            if (int.TryParse(nudIdStart.Text, out int tempInt))
            {
                _options.IdStart = tempInt;
            }
            else
            {
                MessageBox.Show("The starting index is not a numeric value.");
                nudIdStart.Focus();
                return false;
            }

            if (int.TryParse(nudIdPad.Text, out tempInt))
            {
                _options.IdPad = tempInt;
            }
            else
            {
                MessageBox.Show("The value for the Id padding is not an integer.");
                nudIdPad.Focus();
                return false;
            }

            if (int.TryParse(nudLengthPad.Text, out tempInt))
            {
                _options.LengthPad = tempInt;
            }
            else
            {
                MessageBox.Show("The value for the length padding is not an integer.");
                nudLengthPad.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.Prompt))
            {
                MessageBox.Show("Please load a prompt file before processing.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.Data))
            {
                MessageBox.Show("Please load a data file before processing.");
                return false;
            }

            return true;
        }

        private void rdoCorrections_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoCorrections.Checked)
            {
                UpdateDisplay(DisplayMode.Corrections);
            }
        }

        private void rdoData_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoData.Checked)
            {
                UpdateDisplay(DisplayMode.Data);
            }
        }

        private void rdoFormatted_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFormatted.Checked)
            {
                UpdateDisplay(DisplayMode.Formatted);
            }
        }

        private void rdoPrompt_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoPrompt.Checked)
            {
                UpdateDisplay(DisplayMode.Prompt);
            }
        }

        private void rdoResponse_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoResponse.Checked)
            {
                UpdateDisplay(DisplayMode.Response);
            }
        }

        private void rdoScratchpad_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoScratchpad.Checked)
            {
                UpdateDisplay(DisplayMode.Scratchpad);
            }
        }

        private void UpdateDisplay(DisplayMode show)
        {
            rtbPrompt.Visible = show == DisplayMode.Prompt;
            rdoPrompt.Checked = show == DisplayMode.Prompt;

            rtbData.Visible = show == DisplayMode.Data;
            rdoData.Checked = show == DisplayMode.Data;

            rtbScratchpad.Visible = show == DisplayMode.Scratchpad;
            rdoScratchpad.Checked = show == DisplayMode.Scratchpad;

            rtbCorrections.Visible = show == DisplayMode.Corrections;
            rdoCorrections.Checked = show == DisplayMode.Corrections;

            rtbResponse.Visible = show == DisplayMode.Response;
            rdoResponse.Checked = show == DisplayMode.Response;

            rtbFormatted.Visible = show == DisplayMode.Formatted;
            rdoFormatted.Checked = show == DisplayMode.Formatted;
        }
    }
}