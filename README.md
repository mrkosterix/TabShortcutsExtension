# TabShortcutsExtension

Tab Shortucts Extension is an extension for Visual Studio 2015. Main purpose of the extension - navigation between opened
documents using shortcuts. To every tab is automatically assigned one of 36 buttons (26 letters and 10 digits). Each tab has
a shortcut before filename in square brackets (for example, "[A]ClassA.h"). Extension contains commands, to navigate betweed
documents, with names like "View.SwitchToTabA", where last character is a name of shortcut. When command "View.SwitchToTabA"
invokes extension finds tab with shortcut "A" and sets focus to it.

When document becomes invisible shortcut assigned to it frees and can be used for any other opened document. It is possible to change
shortcut of any tab to any shortcut manually using commands like View.ChangeTabShortcutToA, however when this document will becomes
invisible shortcut will be unassigned anyway.

Extension could be used effectively with VsVim extension.
