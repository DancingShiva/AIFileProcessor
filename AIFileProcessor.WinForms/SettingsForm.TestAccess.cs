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

        internal ComboBox Test_ddlOllamaModel => ddlOllamaModel;

        internal TextBox Test_txtOllamaEndpoint => txtOllamaEndpoint;

        internal TextBox Test_txtOllamaKeepAlive => txtOllamaKeepAlive;

        internal ComboBox Test_ddlOllamaReasoning => ddlOllamaReasoning;

        internal Button Test_btnOllamaRefreshModels => btnOllamaRefreshModels;

        internal NumericUpDown Test_nudOllamaChunkSize => nudOllamaChunkSize;

        internal NumericUpDown Test_nudOllamaMaxConcurrentChunks => nudOllamaMaxConcurrentChunks;

        internal NumericUpDown Test_nudOllamaTemp => nudOllamaTemp;

        internal CheckBox Test_chkOllamaTempNull => chkOllamaTempNull;
    }
}