//-----------------------------------------------------------------------
// <copyright file="MainForm.TestAccess.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System.Windows.Forms;

    /// <summary>
    /// Test-only partial class extension that exposes private UI controls
    /// as internal properties for the WinForms test project. These properties
    /// are only accessible to the test assembly via InternalsVisibleTo.
    /// </summary>
    public partial class MainForm
    {
        internal ComboBox Test_ddlModel => ddlModel;

        internal ComboBox Test_ddlReasoning => ddlReasoning;

        internal Button Test_btnProcess => btnProcess;

        internal Button Test_btnSave => btnSave;

        internal RichTextBox Test_rtbPrompt => rtbPrompt;

        internal RichTextBox Test_rtbData => rtbData;

        internal RichTextBox Test_rtbResponse => rtbResponse;

        internal RichTextBox Test_rtbFormatted => rtbFormatted;

        internal RichTextBox Test_rtbScratchpad => rtbScratchpad;

        internal RichTextBox Test_rtbCorrections => rtbCorrections;

        internal RichTextBox Test_rtbMetrics => rtbMetrics;

        internal RichTextBox Test_rtbMetricsJson => rtbMetricsJson;

        internal TabControl Test_tabMetrics => tabMetrics;

        internal TextBox Test_txtPrefix => txtPrefix;

        internal NumericUpDown Test_nudIdStart => nudIdStart;

        internal NumericUpDown Test_nudIdPad => nudIdPad;

        internal NumericUpDown Test_nudLengthPad => nudLengthPad;

        internal RadioButton Test_rdoPrompt => rdoPrompt;

        internal RadioButton Test_rdoData => rdoData;

        internal RadioButton Test_rdoResponse => rdoResponse;

        internal RadioButton Test_rdoFormatted => rdoFormatted;

        internal RadioButton Test_rdoCorrections => rdoCorrections;

        internal RadioButton Test_rdoScratchpad => rdoScratchpad;

        internal RadioButton Test_rdoMetrics => rdoMetrics;
    }
}