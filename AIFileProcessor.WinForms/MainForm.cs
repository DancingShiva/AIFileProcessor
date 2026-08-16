//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
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

        /// <summary>
        /// Display processing metrics: per-chunk table followed by JSON summary.
        /// </summary>
        Metrics,
    }

    /// <summary>
    /// MainForm startup. Partial class definition - UI components are defined in MainForm.Designer.cs.
    /// </summary>
    public partial class MainForm : Form
    {
        // Status strings written to the output boxes that do not constitute saved data.
        private static readonly string[] StatusOnlyTexts =
        [
            "Processing in progress.",
            "Processing cancelled.",
            "Processing completed but no sentences were returned.",
            "There were no corrections.",
        ];

        private readonly ILogger<MainForm> _logger;
        private readonly IFileOps _fileOps;
        private readonly Func<SettingsForm> _settingsFormFactory;
        private readonly IEnumerable<IAiService> _aiServices;
        private readonly ProcessingOrchestrator _processingOrchestrator;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IUserContext _userContext;
        private readonly IDialogService _dialogService;

        private readonly ProcessingOptions _options = new();

        private CancellationTokenSource? _processingCts;
        private bool _isProcessing;

        // The metrics of the last completed run, kept for the Gantt chart paint handler.
        private Telemetry.ProcessingMetrics? _currentMetrics;

        // The display name of the model used for the last run, shown in the Gantt title.
        private string? _lastRunModelName;

        // The data file path as of the most recent processing start; used to name the
        // formatted-output save so it reflects the data that actually produced it.
        private string? _processingDataPath;

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
        /// <param name="dialogService">The dialog service for showing message boxes (testable abstraction).</param>
        public MainForm(
            ILogger<MainForm> logger,
            IFileOps fileOps,
            Func<SettingsForm> settingsFormFactory,
            IEnumerable<IAiService> aiServices,
            ProcessingOrchestrator processingOrchestrator,
            ISettingsProvider settingsProvider,
            IUserContext userContext,
            IDialogService dialogService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(fileOps);
            ArgumentNullException.ThrowIfNull(settingsFormFactory);
            ArgumentNullException.ThrowIfNull(aiServices);
            ArgumentNullException.ThrowIfNull(processingOrchestrator);
            ArgumentNullException.ThrowIfNull(settingsProvider);
            ArgumentNullException.ThrowIfNull(userContext);
            ArgumentNullException.ThrowIfNull(dialogService);

            InitializeComponent();

            _logger = logger;
            _fileOps = fileOps;
            _settingsFormFactory = settingsFormFactory;
            _aiServices = aiServices;
            _processingOrchestrator = processingOrchestrator;
            _settingsProvider = settingsProvider;
            _userContext = userContext;
            _dialogService = dialogService;

            FillModelListBox();
            LoadLastProcessingOptions();

            ddlModel.SelectedIndexChanged += (s, e) => OnModelSelectionChanged();
            ddlReasoning.SelectedIndexChanged += (s, e) => OnReasoningSelectionChanged();
            pnlMetricsGraph.Paint += panelMetricsGraph_Paint;
            tabMetrics.SelectedIndexChanged += (s, e) => UpdateSaveState();
            rtbResponse.TextChanged += (s, e) => UpdateSaveState();
            rtbFormatted.TextChanged += (s, e) => UpdateSaveState();
            rtbScratchpad.TextChanged += (s, e) => UpdateSaveState();
            rtbCorrections.TextChanged += (s, e) => UpdateSaveState();
            rtbMetrics.TextChanged += (s, e) => UpdateSaveState();
            rtbMetricsJson.TextChanged += (s, e) => UpdateSaveState();
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

                    // If the saved path contains a filename, pre-populate the dialog so it
                    // defaults to the previously used file for Prompt/Data/Response.
                    string? fileName = Path.GetFileName(savedPath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        openFileDialog.FileName = fileName;
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
                            if (!ConfirmPromptLooksUsable(destination.Text))
                            {
                                _logger.LogInformation("User declined to load the selected prompt file.");
                                return;
                            }

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

                    UpdateInputStatus(source, destination.Text);
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

        /// <summary>
        /// Saves the text from the specified <paramref name="source"/> RichTextBox to a file.
        /// If <paramref name="source"/> is null, the response RichTextBox is used.
        /// The dialog defaults to the last path the user saved that box to (per-user), and
        /// the newly chosen path is persisted as the default for the next save.
        /// </summary>
        /// <param name="defaultFileName">The fallback file name when no previous save path exists.</param>
        /// <param name="source">The RichTextBox to save from, or null to default to the response box.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveTextAsync(string defaultFileName, RichTextBox? source)
        {
            source ??= rtbResponse;

            string? savedPath = GetLastSavePath(source);
            string initialDir = string.Empty;
            string fileName = defaultFileName;

            if (!string.IsNullOrWhiteSpace(savedPath))
            {
                try
                {
                    // Only reuse the directory of the last save; the file name always
                    // comes from the caller's computed default so it reflects the
                    // current data/response rather than a stale earlier file.
                    initialDir = Path.GetDirectoryName(savedPath) ?? string.Empty;
                }
                catch
                {
                    // Ignore invalid persisted path and fall back to the default directory.
                    initialDir = string.Empty;
                }
            }

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*",
                FileName = fileName,
                Title = "Save Response",
                DefaultExt = "txt",
            };

            if (!string.IsNullOrWhiteSpace(initialDir) && Directory.Exists(initialDir))
            {
                saveFileDialog.InitialDirectory = initialDir;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FilterIndex == 2)
                {
                    source.SaveFile(saveFileDialog.FileName, RichTextBoxStreamType.RichText);
                }
                else
                {
                    bool verified = await _fileOps.SaveFileWithVerificationAsync(saveFileDialog.FileName, source.Text);
                    if (!verified)
                    {
                        MessageBox.Show(
                            "The file was saved, but reading it back did not return the expected content. " +
                            "If this file is in a synced folder (e.g., OneDrive), the content may still be syncing and " +
                            "could temporarily appear as an older version. Please re-check the file in a moment.",
                            "Save verification",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Persist the chosen path so this box defaults to it next time (per user).
                SetLastSavePath(source, saveFileDialog.FileName);
                SaveProcessingOptions();
            }
        }

        /// <summary>
        /// True when the box holds text that is not empty and not just a status message.
        /// </summary>
        private static bool HasSavableText(RichTextBox box)
        {
            string text = box.Text.Trim();
            if (text.Length == 0)
            {
                return false;
            }

            foreach (var status in StatusOnlyTexts)
            {
                if (string.Equals(text, status, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Displays all validation errors in a single dialog and focuses the control
        /// for the first one. Used so the user sees every problem at once rather than
        /// re-clicking Process per missing field.
        /// </summary>
        private void ShowErrors(List<string> errors)
        {
            _dialogService.Show(string.Join(Environment.NewLine, errors), "Please fix the following:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Gets the persisted last-save path for the given RichTextBox, or null if none exists.
        /// </summary>
        private string? GetLastSavePath(RichTextBox control)
        {
            if (control == rtbPrompt)
            {
                return _options.PromptPath;
            }
            else if (control == rtbData)
            {
                return _options.DataPath;
            }
            else if (control == rtbScratchpad)
            {
                return _options.ScratchpadSavePath;
            }
            else if (control == rtbCorrections)
            {
                return _options.CorrectionsSavePath;
            }
            else if (control == rtbFormatted)
            {
                return _options.FormattedSavePath;
            }
            else if (control == rtbMetrics)
            {
                return _options.MetricsSavePath;
            }
            else if (control == rtbMetricsJson)
            {
                return _options.MetricsJsonSavePath;
            }
            else if (control == rtbResponse)
            {
                return !string.IsNullOrWhiteSpace(_options.ResponseSavePath) ? _options.ResponseSavePath : _options.ResponsePath;
            }

            return null;
        }

        /// <summary>
        /// Records the path the user just saved the given RichTextBox to.
        /// </summary>
        private void SetLastSavePath(RichTextBox control, string path)
        {
            if (control == rtbPrompt)
            {
                _options.PromptPath = path;
            }
            else if (control == rtbData)
            {
                _options.DataPath = path;
            }
            else if (control == rtbScratchpad)
            {
                _options.ScratchpadSavePath = path;
            }
            else if (control == rtbCorrections)
            {
                _options.CorrectionsSavePath = path;
            }
            else if (control == rtbFormatted)
            {
                _options.FormattedSavePath = path;
            }
            else if (control == rtbMetrics)
            {
                _options.MetricsSavePath = path;
            }
            else if (control == rtbMetricsJson)
            {
                _options.MetricsJsonSavePath = path;
            }
            else if (control == rtbResponse)
            {
                _options.ResponseSavePath = path;
            }
        }

        /// <summary>
        /// Soft-validates a loaded prompt (non-blocking). Returns false only when the user
        /// explicitly declines to proceed after a warning. These are warnings, not hard blocks.
        /// </summary>
        private bool ConfirmPromptLooksUsable(string promptText)
        {
            if (string.IsNullOrWhiteSpace(promptText))
            {
                return _dialogService.Show(
                    "The selected prompt file is empty. The AI call will produce nothing useful. Load it anyway?", "Empty Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            const int MinPromptLength = 50;
            if (promptText.Trim().Length < MinPromptLength)
            {
                if (_dialogService.Show(
                        "This prompt is very short and doesn't look like the processing specification. Load it anyway?", "Very Short Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return false;
                }
            }

            const int MaxPromptLength = 30000;
            if (promptText.Length > MaxPromptLength
                && _dialogService.Show(
                    $"This prompt is very large ({promptText.Length:n0} characters) and may be an oversized/wrong file. Load it anyway?", "Very Large Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reports prompt/data usage stats in the window title so the user can see the size
        /// of the input they loaded (and estimate processing fan-out for Data).
        /// </summary>
        private void UpdateInputStatus(DisplayMode source, string text)
        {
            switch (source)
            {
                case DisplayMode.Prompt:
                    this.Text = $"AI File Processor — Prompt: {(text ?? string.Empty).Length:n0} chars";
                    break;

                case DisplayMode.Data:
                    int lines = string.IsNullOrEmpty(text) ? 0 : text.Split('\n').Length;
                    int chunks = (int)System.Math.Ceiling(lines / 40.0);
                    this.Text = $"AI File Processor — Data: {lines:n0} lines, ~{chunks} chunk{(chunks == 1 ? string.Empty : "s")}";
                    break;
            }
        }

        /// <summary>
        /// Parses the current raw response text and writes the formatted output into
        /// the given RichTextBox. Leaves <paramref name="target"/> untouched when no
        /// parseable rows exist unless <paramref name="showMessageOnEmpty"/> is set,
        /// so processing can chain into this without flickering a message box.
        /// </summary>
        private void FormatResponseInto(RichTextBox target, bool showMessageOnEmpty)
        {
            var rows = ResponseParser.Parse(rtbResponse.Text);
            if (rows.Count == 0)
            {
                if (showMessageOnEmpty)
                {
                    MessageBox.Show("No parseable response rows to format.");
                }

                return;
            }

            target.Text = ResponseFormatter.FormatRows(rows);
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

        // Stage 1 + 2: call the AI API, populate rtbResponse with the raw text, then
        // parse and format it into rtbFormatted.
        private async void btnProcessFormat_Click(object sender, EventArgs e)
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

                // The formatted box is the primary view during processing; status
                // messages are mirrored into both boxes until the real data returns.
                UpdateDisplay(DisplayMode.Formatted);
                rtbResponse.Text = "Processing in progress.";
                rtbFormatted.Text = "Processing in progress.";

                // Re-read the Prompt/Data text windows so edits made directly in the
                // UI are honored; these are otherwise only captured when a file is opened.
                _options.Prompt = rtbPrompt.Text;
                _options.Data = rtbData.Text;
                _processingDataPath = _options.DataPath;

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
                    rtbFormatted.Text = "Processing cancelled.";
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
                    rtbFormatted.Text = "Processing completed but no sentences were returned.";
                    return;
                }

                rtbScratchpad.Text = result.Scratchpad;

                rtbCorrections.Text = result.Corrections.Count == 0
                    ? "There were no corrections."
                    : string.Join(System.Environment.NewLine, result.Corrections);

                rtbResponse.Text = result.RawResponse;

                // Display metrics across three tabs (table, JSON, graph) if present
                if (result.Metrics != null)
                {
                    var m = result.Metrics;

                    // Build a descriptive title that matches the Gantt chart
                    var modelName = _lastRunModelName ?? (ddlModel.SelectedItem as AiModelInfo)?.ModelName ?? _options.ModelId ?? "Unknown model";
                    var title = $"Chunk processing timeline" + (string.IsNullOrEmpty(modelName) ? string.Empty : $" — {modelName}") + $" · {m.ChunkCount} chunk" + (m.ChunkCount == 1 ? string.Empty : "s") + $" · {m.ElapsedMs / 1000.0:n1} s";

                    // Table: human-readable summary plus per-chunk table
                    var table = new System.Text.StringBuilder();
                    table.AppendLine(title);
                    table.AppendLine();
                    table.AppendLine($"Elapsed: {m.ElapsedMs} ms    Chunks: {m.ChunkCount}    Succeeded: {m.Succeeded}    Failed: {m.Failed}");
                    table.AppendLine($"Total Prompt Tokens: {m.TotalPromptTokens?.ToString() ?? "-"}    Total Completion Tokens: {m.TotalCompletionTokens?.ToString() ?? "-"}");
                    table.AppendLine();
                    table.AppendLine("Chunk\tStartLine\tEndLine\tElapsedMs\tTTFBms\tPromptTokens\tCompletionTokens\tRetries");
                    foreach (var c in m.Chunks)
                    {
                        table.AppendLine($"{c.ChunkIndex}\t{c.StartLine}\t{c.EndLine}\t{c.Telemetry.ElapsedMs}\t{c.Telemetry.TimeToFirstByteMs?.ToString() ?? "-"}\t{c.Telemetry.PromptTokens?.ToString() ?? "-"}\t{c.Telemetry.CompletionTokens?.ToString() ?? "-"}\t{c.RetryAttempts}");
                    }

                    rtbMetrics.Text = table.ToString();

                    // Compute line-level success/failure from the input data and chunk results.
                    // LineCount = total source lines (regardless of success or failure).
                    // LinesSuccessful = lines in chunks whose API call returned output.
                    // LinesFailed = lines for which no output was returned by the API.
                    int lineCount = rtbData.TextLength > 0
                        ? rtbData.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length
                        : 0;

                    var chunkLineRanges = m.Chunks
                        .Select(c => new { c.StartLine, c.EndLine, c.Telemetry.Status })
                        .ToList();

                    int linesSuccessful = chunkLineRanges
                        .Where(c => string.Equals(c.Status, "Success", StringComparison.OrdinalIgnoreCase))
                        .Sum(c => c.EndLine - c.StartLine + 1);

                    int linesFailed = Math.Max(0, lineCount - linesSuccessful);

                    // JSON: envelope the metrics with run metadata (title, model, counts, times)
                    var envelope = new
                    {
                        RunInfo = new
                        {
                            Title = title,
                            Model = modelName,
                            ChunkCount = m.ChunkCount,
                            LineCount = lineCount,
                            LinesSuccessful = linesSuccessful,
                            LinesFailed = linesFailed,
                            ElapsedMs = m.ElapsedMs,
                            StartUtc = m.StartUtc,
                            EndUtc = m.EndUtc,
                        },
                        Metrics = m,
                    };

                    rtbMetricsJson.Text = System.Text.Json.JsonSerializer.Serialize(envelope, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                    // Graph: store metrics for the paint handler and trigger repaint
                    _currentMetrics = m;
                    pnlMetricsGraph.Invalidate();
                }

                // Chain formatting: parse the fresh raw response and fill rtbFormatted.
                FormatResponseInto(rtbFormatted, showMessageOnEmpty: false);

                // Remember which model produced this run for the Gantt chart title.
                _lastRunModelName = (ddlModel.SelectedItem as AiModelInfo)?.ModelName;

                UpdateDisplay(DisplayMode.Formatted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in btnProcessFormat_Click: {Message}", ex.Message);
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isProcessing = false;
                btnProcess.Text = "Process";
                btnProcess.Enabled = true;
                UpdateSaveState();
            }
        }

        private async void btnOpenResponse_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadTextAsync(DisplayMode.Response, rtbResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in btnOpenResponse_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panelMetricsGraph_Paint(object? sender, PaintEventArgs e)
        {
            if (_currentMetrics == null || _currentMetrics.Chunks.Count == 0)
            {
                return;
            }

            ChunkTimelineGanttRenderer.Draw(e.Graphics, _currentMetrics, pnlMetricsGraph.ClientRectangle, _lastRunModelName);
        }

        /// <summary>
        /// Renders the current metrics Gantt chart to a PNG chosen by the user.
        /// Defaults to the last location the user saved the graph to (per-user).
        /// </summary>
        private void SaveMetricsGraphPng()
        {
            var metrics = _currentMetrics;
            if (metrics == null)
            {
                return;
            }

            string initialDir = string.Empty;
            string fileName = "Metrics Graph.png";
            if (!string.IsNullOrWhiteSpace(_options.MetricsGraphSavePath))
            {
                try
                {
                    initialDir = Path.GetDirectoryName(_options.MetricsGraphSavePath) ?? string.Empty;
                    string? name = Path.GetFileName(_options.MetricsGraphSavePath);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        fileName = name;
                    }
                }
                catch
                {
                    initialDir = string.Empty;
                }
            }

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "PNG image (*.png)|*.png",
                FileName = fileName,
                Title = "Save Metrics Graph",
                DefaultExt = "png",
            };

            if (!string.IsNullOrWhiteSpace(initialDir) && Directory.Exists(initialDir))
            {
                saveFileDialog.InitialDirectory = initialDir;
            }

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var size = ChunkTimelineGanttRenderer.GetRequiredSize(metrics);
            using var bitmap = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
            using (var g = Graphics.FromImage(bitmap))
            {
                ChunkTimelineGanttRenderer.Draw(g, metrics, new Rectangle(0, 0, bitmap.Width, bitmap.Height), _lastRunModelName);
            }

            bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);

            _options.MetricsGraphSavePath = saveFileDialog.FileName;
            SaveProcessingOptions();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void openDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadTextAsync(DisplayMode.Data, rtbData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in openDataToolStripMenuItem_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Save whichever RichTextBox view is currently visible, using each
                // box's persisted last-save path as the dialog default.
                if (rtbPrompt.Visible)
                {
                    await SaveTextAsync(Path.GetFileName(_options.PromptPath) is { Length: > 0 } p ? p : "Prompt.txt", rtbPrompt);
                }
                else if (rtbData.Visible)
                {
                    await SaveTextAsync(Path.GetFileName(_options.DataPath) is { Length: > 0 } d ? d : "Data.txt", rtbData);
                }
                else if (rtbScratchpad.Visible)
                {
                    await SaveTextAsync("Scratchpad.txt", rtbScratchpad);
                }
                else if (rtbCorrections.Visible)
                {
                    await SaveTextAsync("Corrections.txt", rtbCorrections);
                }
                else if (rtbFormatted.Visible)
                {
                    string formattedDefault = Path.GetFileNameWithoutExtension(_processingDataPath) is { Length: > 0 } dataName
                        ? $"{dataName} Import.txt"
                        : Path.GetFileNameWithoutExtension(_options.DataPath) is { Length: > 0 } fallbackName
                            ? $"{fallbackName} Import.txt"
                            : "Response Import.txt";
                    await SaveTextAsync(formattedDefault, rtbFormatted);
                }
                else if (tabMetrics.Visible)
                {
                    // Save according to the active metrics tab: table text, raw JSON,
                    // or the Gantt graph as PNG.
                    if (tabMetrics.SelectedTab == tpMetricsJson)
                    {
                        await SaveTextAsync("Metrics.json", rtbMetricsJson);
                    }
                    else if (tabMetrics.SelectedTab == tpMetricsGraph)
                    {
                        SaveMetricsGraphPng();
                    }
                    else
                    {
                        await SaveTextAsync("Metrics.txt", rtbMetrics);
                    }
                }
                else
                {
                    await SaveTextAsync("Response.txt", rtbResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in saveToolStripMenuItem_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveMetricsGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentMetrics == null)
                {
                    MessageBox.Show("There are no metrics to save. Process a file first.", "Metrics Graph", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveMetricsGraphPng();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in saveMetricsGraphToolStripMenuItem_Click.");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                // Restore file paths so LoadTextAsync can default to the last-used file
                _options.PromptPath = saved.PromptPath ?? string.Empty;
                _options.DataPath = saved.DataPath ?? string.Empty;
                _options.ResponsePath = saved.ResponsePath ?? string.Empty;

                // Restore per-RichTextBox last-save paths and the graph save path.
                _options.ResponseSavePath = saved.ResponseSavePath ?? string.Empty;
                _options.FormattedSavePath = saved.FormattedSavePath ?? string.Empty;
                _options.ScratchpadSavePath = saved.ScratchpadSavePath ?? string.Empty;
                _options.CorrectionsSavePath = saved.CorrectionsSavePath ?? string.Empty;
                _options.MetricsSavePath = saved.MetricsSavePath ?? string.Empty;
                _options.MetricsJsonSavePath = saved.MetricsJsonSavePath ?? string.Empty;
                _options.MetricsGraphSavePath = saved.MetricsGraphSavePath ?? string.Empty;

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
            // Read raw values from the controls, then validate once with the shared validator.
            string? selectedModelId = null;
            if (ddlModel.SelectedItem is AiModelInfo selectedModel && !string.IsNullOrWhiteSpace(ddlModel.Text))
            {
                selectedModelId = selectedModel.ModelId;
            }

            _options.ModelId = selectedModelId ?? string.Empty;

            if (ddlReasoning.SelectedItem is ReasoningLevel reasoningLevel && !string.IsNullOrEmpty(_options.ModelId))
            {
                _options.ModelReasoningLevels[_options.ModelId] = reasoningLevel;
            }

            _options.IdPrefix = txtPrefix.Text.Trim();
            _options.IdStart = (int)nudIdStart.Value;
            _options.IdPad = (int)nudIdPad.Value;
            _options.LengthPad = (int)nudLengthPad.Value;

            var errors = ProcessingOptionsValidator.Validate(_options, selectedModelId);
            if (errors.Count > 0)
            {
                ShowErrors(errors);

                // Focus the control for the first error so the user knows where to fix.
                if (selectedModelId == null)
                {
                    ddlModel.Focus();
                }
                else if (string.IsNullOrWhiteSpace(_options.IdPrefix))
                {
                    txtPrefix.Focus();
                }
                else if (_options.IdStart <= 0)
                {
                    nudIdStart.Focus();
                }
                else if (_options.IdPad <= 0)
                {
                    nudIdPad.Focus();
                }
                else
                {
                    nudLengthPad.Focus();
                }

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

        private void rdoMetrics_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoMetrics.Checked)
            {
                UpdateDisplay(DisplayMode.Metrics);
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

            // The metrics view is the tab control. rtbMetrics lives inside the
            // Table tab page so its visibility is managed by the tab control.
            tabMetrics.Visible = show == DisplayMode.Metrics;
            tabMetrics.BringToFront();
            rdoMetrics.Checked = show == DisplayMode.Metrics;

            UpdateSaveState();
        }

        /// <summary>
        /// Enables or disables the File ? Save item based on whether the currently
        /// visible output view contains real user/API data (as opposed to being empty
        /// or holding only a status message).
        /// </summary>
        private void UpdateSaveState()
        {
            bool savable = ComputeCurrentViewSavable();
            tsmiSave.Enabled = savable;
            btnSave.Enabled = savable;
        }

        /// <summary>
        /// Determines whether the currently visible view has content worth saving.
        /// For the metrics view the result depends on the active tab.
        /// </summary>
        private bool ComputeCurrentViewSavable()
        {
            if (tabMetrics.Visible)
            {
                if (tabMetrics.SelectedTab == tpMetricsGraph)
                {
                    return _currentMetrics != null && _currentMetrics.Chunks.Count > 0;
                }

                if (tabMetrics.SelectedTab == tpMetricsJson)
                {
                    return HasSavableText(rtbMetricsJson);
                }

                return HasMetricsTableData();
            }

            if (rtbScratchpad.Visible)
            {
                return !string.IsNullOrWhiteSpace(rtbScratchpad.Text);
            }

            if (rtbCorrections.Visible)
            {
                return HasSavableText(rtbCorrections);
            }

            if (rtbResponse.Visible)
            {
                return HasSavableText(rtbResponse);
            }

            if (rtbFormatted.Visible)
            {
                return HasSavableText(rtbFormatted);
            }

            // Prompt and Data are user-supplied inputs, not generated output — saving
            // them through the unified Save item is not supported.
            return false;
        }

        /// <summary>
        /// True when the metrics table contains at least one chunk row beneath the
        /// generated summary/header block.
        /// </summary>
        private bool HasMetricsTableData()
        {
            foreach (var rawLine in rtbMetrics.Text.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0
                    || line.StartsWith("Elapsed:", StringComparison.Ordinal)
                    || line.StartsWith("Total Prompt Tokens:", StringComparison.Ordinal)
                    || line.StartsWith("Chunk\t", StringComparison.Ordinal)
                    || line.StartsWith("Chunk processing timeline", StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}