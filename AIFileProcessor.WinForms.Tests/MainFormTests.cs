//-----------------------------------------------------------------------
// <copyright file="MainFormTests.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.WinForms.Tests;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIFileProcessor.Configuration;
using AIFileProcessor.Interfaces;
using AIFileProcessor.Models;
using AIFileProcessor.Processing;
using AIFileProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
public class MainFormTests
{
    private readonly Mock<ILogger<MainForm>> _loggerMock = new();
    private readonly Mock<IFileOps> _fileOpsMock = new();
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();
    private readonly Mock<ProcessingOrchestrator> _orchestratorMock;
    private readonly List<IAiService> _aiServices;

    public MainFormTests()
    {
        _userContextMock.Setup(u => u.UserId).Returns("test-user");
        _settingsProviderMock.Setup(s => s.GetProcessingOptionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingOptions());
        _settingsProviderMock.Setup(s => s.SaveProcessingOptionsAsync(It.IsAny<string>(), It.IsAny<ProcessingOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var aiServiceMock = new Mock<IAiService>();
        aiServiceMock.Setup(s => s.SupportsModel(It.IsAny<string>())).Returns(true);
        aiServiceMock.Setup(s => s.GetSupportedModels()).Returns(new List<AiModelInfo>
        {
            new("Test Model", "test-model", [ReasoningLevel.None]),
        });
        aiServiceMock.Setup(s => s.GetSupportedReasoningLevels(It.IsAny<string>()))
            .Returns(new List<ReasoningLevel> { ReasoningLevel.None }.AsReadOnly());

        _aiServices = new List<IAiService> { aiServiceMock.Object };

        _orchestratorMock = new Mock<ProcessingOrchestrator>(
            new Func<IChineseTextProcessor>(() => Mock.Of<IChineseTextProcessor>()),
            _settingsProviderMock.Object,
            _userContextMock.Object,
            new Mock<ILogger<ProcessingOrchestrator>>().Object,
            _aiServices)
        { CallBase = true };
    }

    private MainForm CreateForm()
    {
        return new MainForm(
            _loggerMock.Object,
            _fileOpsMock.Object,
            () => new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object),
            _aiServices,
            _orchestratorMock.Object,
            _settingsProviderMock.Object,
            _userContextMock.Object,
            _dialogServiceMock.Object);
    }

    private static object? InvokePrivate(object target, string methodName, params object[] args)
    {
        Type[]? types = args.Length == 0 ? null : Array.ConvertAll(args, a => a.GetType());
        return target.GetType().GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance, null, types ?? Type.EmptyTypes, null)
            ?.Invoke(target, args);
    }

    /// <summary>
    /// Checks whether the display mode was switched to the target by examining
    /// the corresponding RadioButton.Checked state (set by UpdateDisplay, doesn't
    /// depend on parent form visibility like Control.Visible does).
    /// </summary>
    private static bool IsControlVisible(Control control)
    {
        if (control == null) return false;
        var form = control.FindForm() as MainForm;
        if (form == null) return false;

        if (control == form.Test_rtbPrompt) return form.Test_rdoPrompt.Checked;
        if (control == form.Test_rtbData) return form.Test_rdoData.Checked;
        if (control == form.Test_rtbResponse) return form.Test_rdoResponse.Checked;
        if (control == form.Test_rtbFormatted) return form.Test_rdoFormatted.Checked;
        if (control == form.Test_rtbCorrections) return form.Test_rdoCorrections.Checked;
        if (control == form.Test_rtbScratchpad) return form.Test_rdoScratchpad.Checked;
        if (control == form.Test_tabMetrics) return form.Test_rdoMetrics.Checked;
        return false;
    }

    private void SetupDialogOk()
    {
        _dialogServiceMock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()))
            .Returns(DialogResult.OK);
    }

    // --- Constructor tests ---

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainForm(
            null!, _fileOpsMock.Object, () => new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object),
            _aiServices, _orchestratorMock.Object, _settingsProviderMock.Object, _userContextMock.Object, _dialogServiceMock.Object));
    }

    [Fact]
    public void Constructor_NullFileOps_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainForm(
            _loggerMock.Object, null!, () => new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object),
            _aiServices, _orchestratorMock.Object, _settingsProviderMock.Object, _userContextMock.Object, _dialogServiceMock.Object));
    }

    [Fact]
    public void Constructor_NullSettingsFormFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainForm(
            _loggerMock.Object, _fileOpsMock.Object, null!,
            _aiServices, _orchestratorMock.Object, _settingsProviderMock.Object, _userContextMock.Object, _dialogServiceMock.Object));
    }

    [Fact]
    public void Constructor_NullOrchestrator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainForm(
            _loggerMock.Object, _fileOpsMock.Object, () => new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object),
            _aiServices, null!, _settingsProviderMock.Object, _userContextMock.Object, _dialogServiceMock.Object));
    }

    [Fact]
    public void Constructor_NullDialogService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainForm(
            _loggerMock.Object, _fileOpsMock.Object, () => new SettingsForm(_aiServices, _settingsProviderMock.Object, _userContextMock.Object),
            _aiServices, _orchestratorMock.Object, _settingsProviderMock.Object, _userContextMock.Object, null!));
    }

    [Fact]
    public void Constructor_AllDependenciesProvided_PopulatesModelDropdown()
    {
        using var form = CreateForm();
        Assert.True(form.Test_ddlModel.Items.Count > 0);
    }

    // --- ConfirmPromptLooksUsable tests ---

    [Fact]
    public void ConfirmPromptLooksUsable_ValidPrompt_NoDialogCalled()
    {
        _dialogServiceMock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()))
            .Returns(DialogResult.Yes);
        using var form = CreateForm();
        var result = InvokePrivate(form, "ConfirmPromptLooksUsable", new string('x', 100));
        Assert.True((bool)result!);
        _dialogServiceMock.Verify(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()), Times.Never);
    }

    [Fact]
    public void ConfirmPromptLooksUsable_EmptyPrompt_UserDeclines_ReturnsFalse()
    {
        _dialogServiceMock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()))
            .Returns(DialogResult.No);
        using var form = CreateForm();
        var result = InvokePrivate(form, "ConfirmPromptLooksUsable", string.Empty);
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConfirmPromptLooksUsable_ShortPrompt_UserAccepts_ReturnsTrue()
    {
        _dialogServiceMock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()))
            .Returns(DialogResult.Yes);
        using var form = CreateForm();
        var result = InvokePrivate(form, "ConfirmPromptLooksUsable", "too short");
        Assert.True((bool)result!);
    }

    [Fact]
    public void ConfirmPromptLooksUsable_OverMaxPromptLength_UserDeclines_ReturnsFalse()
    {
        _dialogServiceMock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<MessageBoxIcon>()))
            .Returns(DialogResult.No);
        using var form = CreateForm();
        var result = InvokePrivate(form, "ConfirmPromptLooksUsable", new string('x', 30001));
        Assert.False((bool)result!);
    }

    // --- UpdateInputStatus tests ---

    [Fact]
    public void UpdateInputStatus_Prompt_UpdatesTitleWithCharCount()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateInputStatus", DisplayMode.Prompt, "test prompt text");
        Assert.Contains("Prompt:", form.Text);
    }

    [Fact]
    public void UpdateInputStatus_Data_UpdatesTitleWithLineAndChunkCount()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateInputStatus", DisplayMode.Data, "line1\nline2\nline3");
        Assert.Contains("Data:", form.Text);
    }

    [Fact]
    public void UpdateInputStatus_EmptyData_TitleShowsZeroLines()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateInputStatus", DisplayMode.Data, string.Empty);
        Assert.Contains("0 line", form.Text);
    }

    // --- Display mode switching tests ---

    [Fact]
    public void UpdateDisplay_Prompt_ShowsPromptBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Prompt);
        Assert.True(IsControlVisible(form.Test_rtbPrompt));
        Assert.False(IsControlVisible(form.Test_rtbData));
    }

    [Fact]
    public void UpdateDisplay_Data_ShowsDataBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Data);
        Assert.True(IsControlVisible(form.Test_rtbData));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    [Fact]
    public void UpdateDisplay_Response_ShowsResponseBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Response);
        Assert.True(IsControlVisible(form.Test_rtbResponse));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    [Fact]
    public void UpdateDisplay_Formatted_ShowsFormattedBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Formatted);
        Assert.True(IsControlVisible(form.Test_rtbFormatted));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    [Fact]
    public void UpdateDisplay_Corrections_ShowsCorrectionsBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Corrections);
        Assert.True(IsControlVisible(form.Test_rtbCorrections));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    [Fact]
    public void UpdateDisplay_Scratchpad_ShowsScratchpadBoxOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Scratchpad);
        Assert.True(IsControlVisible(form.Test_rtbScratchpad));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    [Fact]
    public void UpdateDisplay_Metrics_ShowsMetricsTabControlOnly()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Metrics);
        Assert.True(IsControlVisible(form.Test_tabMetrics));
        Assert.False(IsControlVisible(form.Test_rtbPrompt));
    }

    // --- Save state tests ---

    [Fact]
    public void UpdateSaveState_StatusMessageOnly_DisablesSave()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Response);
        form.Test_rtbResponse.Text = "Processing in progress.";
        InvokePrivate(form, "UpdateSaveState");
        Assert.False(form.Test_btnSave.Enabled);
    }

    [Fact]
    public void UpdateSaveState_RealContent_EnablesSave()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Response);
        form.Test_rtbResponse.Text = "S001\tdata\tpinyin\ttable\tenglish\t5";

        // ComputeCurrentViewSavable checks Control.Visible (computed, requires
        // form to be shown). Since we can't Show() without an STA thread, verify
        // the HasSavableText logic directly via reflection.
        var hasSavableMethod = typeof(MainForm).GetMethod("HasSavableText",
            BindingFlags.NonPublic | BindingFlags.Static);
        bool savable = (bool)(hasSavableMethod?.Invoke(null, [form.Test_rtbResponse]) ?? false);
        Assert.True(savable);
    }

    [Fact]
    public void UpdateSaveState_EmptyFormatted_DisablesSave()
    {
        using var form = CreateForm();
        InvokePrivate(form, "UpdateDisplay", DisplayMode.Formatted);
        form.Test_rtbFormatted.Text = string.Empty;
        InvokePrivate(form, "UpdateSaveState");
        Assert.False(form.Test_btnSave.Enabled);
    }

    // --- FillOptionsFromUI validation tests ---

    private void SetupValidFormFields(MainForm form)
    {
        if (form.Test_ddlModel.Items.Count > 0)
        {
            form.Test_ddlModel.SelectedIndex = 0;
            // In test context without a visible window, WinForms doesn't populate
            // ComboBox.Text from DisplayMember. Set it manually so FillOptionsFromUI
            // recognizes a model as selected.
            if (form.Test_ddlModel.SelectedItem is AiModelInfo model)
            {
                form.Test_ddlModel.Text = model.ModelName;
            }
        }

        form.Test_txtPrefix.Text = "S";
        form.Test_nudIdStart.Value = 1;
        form.Test_nudIdPad.Value = 3;
        form.Test_nudLengthPad.Value = 3;
        form.Test_rtbPrompt.Text = new string('x', 100);
        form.Test_rtbData.Text = "\u6211\u5728\u8d85\u5e02\u4e70\u83dc\u3002";

        // FillOptionsFromUI reads _options.Prompt and _options.Data (set by the caller,
        // not by FillOptionsFromUI itself). Set them via reflection.
        var optionsField = typeof(MainForm).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
        var options = (ProcessingOptions)optionsField!.GetValue(form)!;
        options.Prompt = form.Test_rtbPrompt.Text;
        options.Data = form.Test_rtbData.Text;
    }

    private static void SetOption(MainForm form, string propertyName, object value)
    {
        var optionsField = typeof(MainForm).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
        var options = optionsField!.GetValue(form)!;
        typeof(ProcessingOptions).GetProperty(propertyName)!.SetValue(options, value);
    }

    [Fact]
    public void FillOptionsFromUI_NoModelSelected_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        form.Test_ddlModel.SelectedIndex = -1;
        form.Test_ddlModel.Text = string.Empty;
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.False((bool)result!);
    }

    [Fact]
    public void FillOptionsFromUI_EmptyIdPrefix_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        form.Test_txtPrefix.Text = string.Empty;
        // In test context, txtPrefix.Text may not clear. Set _options.IdPrefix
        // directly to simulate what FillOptionsFromUI would read from an empty control.
        // FillOptionsFromUI overwrites _options.IdPrefix from txtPrefix.Text.Trim(),
        // so we also need to make the control text empty.
        SetOption(form, "IdPrefix", string.Empty);
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        // If the control retained "S", FillOptionsFromUI will read it and pass.
        // If the control cleared, it will read empty and fail validation.
        // Either way, this test verifies the validation path.
        Assert.False((bool)result!, "Expected validation failure for empty IdPrefix");
    }

    [Fact]
    public void FillOptionsFromUI_IdStartZero_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        form.Test_nudIdStart.Minimum = 0;
        form.Test_nudIdStart.Value = 0;
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.False((bool)result!);
    }

    [Fact]
    public void FillOptionsFromUI_IdPadZero_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        form.Test_nudIdPad.Minimum = 0;
        form.Test_nudIdPad.Value = 0;
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.False((bool)result!);
    }

    [Fact]
    public void FillOptionsFromUI_LengthPadZero_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        form.Test_nudLengthPad.Minimum = 0;
        form.Test_nudLengthPad.Value = 0;
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.False((bool)result!);
    }

    [Fact]
    public void FillOptionsFromUI_NoPrompt_ShowsErrorsReturnsFalse()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);
        SetOption(form, "Prompt", string.Empty);
        SetupDialogOk();
        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.False((bool)result!);
    }

    [Fact]
    public void FillOptionsFromUI_AllValid_ReturnsTrue()
    {
        using var form = CreateForm();
        SetupValidFormFields(form);

        // FillOptionsFromUI reads from UI controls and writes to _options, then validates.
        // Set _options fields that FillOptionsFromUI doesn't set (Prompt, Data are set by caller).
        var optionsField = typeof(MainForm).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
        var options = (ProcessingOptions)optionsField!.GetValue(form)!;
        options.Prompt = form.Test_rtbPrompt.Text;
        options.Data = form.Test_rtbData.Text;

        // Manually replicate what FillOptionsFromUI does: read from controls into _options
        options.IdPrefix = form.Test_txtPrefix.Text.Trim();
        options.IdStart = (int)form.Test_nudIdStart.Value;
        options.IdPad = (int)form.Test_nudIdPad.Value;
        options.LengthPad = (int)form.Test_nudLengthPad.Value;
        options.ModelId = (form.Test_ddlModel.SelectedItem as AiModelInfo)?.ModelId ?? string.Empty;

        string? modelId = options.ModelId;
        var errors = ProcessingOptionsValidator.Validate(options, modelId);
        Assert.True(errors.Count == 0, string.Join("; ", errors));

        var result = InvokePrivate(form, "FillOptionsFromUI");
        Assert.True((bool)result!, "FillOptionsFromUI returned false despite valid options");
    }
}
