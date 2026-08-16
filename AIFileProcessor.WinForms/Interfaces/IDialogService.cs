//-----------------------------------------------------------------------
// <copyright file="IDialogService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Interfaces
{
    using System.Windows.Forms;

    /// <summary>
    /// Abstraction over <see cref="MessageBox"/> to enable unit testing of
    /// dialog-dependent logic in <see cref="MainForm"/> and
    /// <see cref="SettingsForm"/>.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a message box with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="caption">The dialog caption.</param>
        /// <param name="buttons">The buttons to display.</param>
        /// <param name="icon">The icon to display.</param>
        /// <returns>The dialog result.</returns>
        DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}