﻿//------------------------------------------------------------------------------
// <copyright file="SwitchToTabCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.PlatformUI.Shell;
using Microsoft.VisualStudio.Platform.WindowManagement;

namespace TabShortcutsExtension
{

    [Export(typeof(IVsTextViewCreationListener))]
    [Microsoft.VisualStudio.Utilities.ContentType("text")]
    [Microsoft.VisualStudio.Text.Editor.TextViewRole(Microsoft.VisualStudio.Text.Editor.PredefinedTextViewRoles.Editable)]
    public class TextViewEventListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;

        internal DocumentGroup documentGroup = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var activeView = ViewManager.Instance.ActiveView;
            if (activeView == null)
                return;
            var group = activeView.Parent as DocumentGroup;
            if (group != null && documentGroup == null)
            {
                documentGroup = group;

                documentGroup.VisibleChildren.CollectionChanged += TabShortcutsManager.GetInstance().OnDocumentGroupChanged;

                List<View> activeViews = new List<View>();
                var children = documentGroup.VisibleChildren.OfType<View>();
                foreach (var child in children)
                {
                    activeViews.Add(child);
                }
                TabShortcutsManager.GetInstance().UpdateActiveViews(activeViews);
            }
        }
    }


    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SwitchToTabCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("619bed49-70fd-470e-82e1-840544513d8d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchToTabCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SwitchToTabCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SwitchToTabCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new SwitchToTabCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "SwitchToTabCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}