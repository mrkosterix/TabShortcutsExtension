//------------------------------------------------------------------------------
// <copyright file="SwitchToTabCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.VisualStudio.VCProjectEngine;

namespace TabShortcutsExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(SwitchToTabCommandPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class SwitchToTabCommandPackage : Package
    {
        /// <summary>
        /// SwitchToTabCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4aea3ee3-ddc5-4833-95db-63544f49096a";

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchToTabCommand"/> class.
        /// </summary>
        public SwitchToTabCommandPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        
        string[] shortcuts = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        UpdateActiveDocumentGroup updateAllTabTitles;
        Dictionary<string, SwitchToTab> switchToTabButtons;
        Dictionary<string, ChangeToShortcut> changeShortcutButtons;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            switchToTabButtons = new Dictionary<string, SwitchToTab>();
            changeShortcutButtons = new Dictionary<string, ChangeToShortcut>();
            SwitchToTabCommand.Initialize(this);
            base.Initialize();

            updateAllTabTitles = new UpdateActiveDocumentGroup(this);

            const int switchToTabButtonStartIndex = 4130;
            const int changeShortcutButtonStartIndex = 4166;

            int buttonIndex = 0;
            foreach (string shortcut in shortcuts)
            {
                switchToTabButtons[shortcut] = new SwitchToTab(this, switchToTabButtonStartIndex + buttonIndex, shortcut);
                changeShortcutButtons[shortcut] = new ChangeToShortcut(this, changeShortcutButtonStartIndex + buttonIndex, shortcut);
                ++buttonIndex;
            }

            EnvDTE.DTE dte = base.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            VCProjectEngineEvents projectItemsEvents = dte.Events.GetObject("VCProjectEngineEventsObject") as VCProjectEngineEvents;
            projectItemsEvents.ItemRenamed += TabShortcutsManager.GetInstance().OnItemRenamed;
        }

        #endregion
    }
}
