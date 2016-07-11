//------------------------------------------------------------------------------
// <copyright file="Command1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TabShortcutsExtension
{
    internal sealed class SwitchToTab
    {
        public int CommandId;

        public static readonly Guid CommandSet = new Guid("619bed49-70fd-470e-82e1-840544513d8d");

        private readonly Package package;

        private readonly string Shortcut;

        public SwitchToTab(Package package, int commandId, string shortcut)
        {
            CommandId = commandId;
            Shortcut = shortcut;
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

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            TabShortcutsManager.GetInstance().SwitchToTab(Shortcut);
        }
    }


    internal sealed class ChangeToShortcut
    {
        public int CommandId;

        public static readonly Guid CommandSet = new Guid("619bed49-70fd-470e-82e1-840544513d8d");

        private readonly Package package;

        private readonly string Shortcut;

        public ChangeToShortcut(Package package, int commandId, string shortcut)
        {
            CommandId = commandId;
            Shortcut = shortcut;
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

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            TabShortcutsManager.GetInstance().ChangeActiveToShortcut(Shortcut);
        }
    }

    internal sealed class UpdateAllTabTitles
    {
        public const int CommandId = 4129;

        public static readonly Guid CommandSet = new Guid("619bed49-70fd-470e-82e1-840544513d8d");

        private readonly Package package;

        public UpdateAllTabTitles(Package package)
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

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            TabShortcutsManager.GetInstance().UpdateAllTabTitles();
        }
    }
}
