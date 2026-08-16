//-----------------------------------------------------------------------
// <copyright file="MessageBoxDialogService.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Services
{
    using System.Windows.Forms;
    using AIFileProcessor.Interfaces;

    /// <summary>
    /// Production implementation of <see cref="IDialogService"/> that delegates
    /// to <see cref="MessageBox"/>.
    /// </summary>
    public class MessageBoxDialogService : IDialogService
    {
        /// <inheritdoc/>
        public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
    }
}