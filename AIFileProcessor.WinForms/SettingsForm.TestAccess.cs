//-----------------------------------------------------------------------
// <copyright file="SettingsForm.TestAccess.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor
{
    using System.Windows.Forms;

    /// <summary>
    /// Test-only partial class extension that exposes private UI controls
    /// as internal properties for the WinForms test project.
    /// </summary>
    public partial class SettingsForm
    {
        internal ComboBox Test_ddlGeminiModel => ddlGeminiModel;

        internal ComboBox Test_ddlOpenAiModel => ddlOpenAiModel;

        internal ComboBox Test_ddlDeepSeekModel => ddlDeepSeekModel;
    }
}