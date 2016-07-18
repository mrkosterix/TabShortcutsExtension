using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.VCProjectEngine;

namespace TabShortcutsExtension
{
    class TabShortcutsManager
    {
        TabShortcutsManager()
        {
            viewToShortcutDictionary = new Dictionary<string, int>();
            previousViewTitles = new Dictionary<string, string>();
            isShortcutAvailable = new bool[shortcutNames.Length];
            shortcutViews = new string[shortcutNames.Length];
            for (int i = 0; i < isShortcutAvailable.Length; ++i)
            {
                isShortcutAvailable[i] = true;
                shortcutViews[i] = null;
            }
            viewToHandleDictionary = new Dictionary<View, string>();
        }

        static public TabShortcutsManager GetInstance()
        {
            if (instance == null)
                instance = new TabShortcutsManager();
            return instance;
        }

        private string GetHandle(View view)
        {
            string name = view.Name;
            var startIndex = name.IndexOf('|', name.IndexOf('|') + 1) + 1;
            var finishIndex = name.IndexOf('|', startIndex + 1);
            return name.Substring(startIndex, finishIndex - startIndex);
        }

        private View GetView(string handle)
        {
            foreach (View view in activeDocumentGroup.VisibleChildren)
                if (handle.CompareTo(GetHandle(view)) == 0)
                    return view;

            foreach (View view in activeDocumentGroup.PinnedViews)
                if (handle.CompareTo(GetHandle(view)) == 0)
                    return view;

            return null;
        }

        private void SetTitleWithShortcut(View view, int shortcutIndex)
        {
            WindowFrameTitle title = (view.Title as WindowFrameTitle);
            previousViewTitles[GetHandle(view)] = title.Title;
            title.Title = "[" + shortcutNames[shortcutIndex] + "]" + title.Title;
        }

        private void SetTitleWithoutShortcut(View view)
        {
            WindowFrameTitle title = (view.Title as WindowFrameTitle);
            title.Title = previousViewTitles[GetHandle(view)];
        }

        public void SetDocumentGroup(DocumentGroup group)
        {
            if ((group == null && activeDocumentGroup == null) || group.Equals(activeDocumentGroup))
                return;
            if (activeDocumentGroup != null)
            {
                activeDocumentGroup.PinnedViews.CollectionChanged -= OnPinnedViewsChanged;
                activeDocumentGroup.VisibleChildren.CollectionChanged -= OnVisibleChildrenChanged;
            }

            activeDocumentGroup = group;
            if (activeDocumentGroup != null)
            {
                activeDocumentGroup.PinnedViews.CollectionChanged += OnPinnedViewsChanged;
                activeDocumentGroup.VisibleChildren.CollectionChanged += OnVisibleChildrenChanged;

                activeDocumentGroup.IsVisibleChanged += OnDocumentGroupVisibilityChanged;

                UpdateAllTabTitles();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnVisibleChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (View view in e.NewItems)
                    RegisterTabView(view);
            if (e.OldItems != null)
                foreach (View view in e.OldItems)
                    if (!view.IsVisible && !view.IsPinned)
                        UnregisterTabView(view);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnPinnedViewsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (View view in e.NewItems)
                    RegisterTabView(view);
            if (e.OldItems != null)
                foreach (View view in e.OldItems)
                    if (!view.IsVisible)
                        UnregisterTabView(view);
        }

        private void RegisterTabView(View view)
        {
            string viewHandle = GetHandle(view);
            if (viewToShortcutDictionary.ContainsKey(viewHandle))
                return;

            int availableShortcutIndex = 0;
            while (!isShortcutAvailable[availableShortcutIndex]) ++availableShortcutIndex;
            viewToShortcutDictionary[viewHandle] = availableShortcutIndex;
            isShortcutAvailable[availableShortcutIndex] = false;
            shortcutViews[availableShortcutIndex] = viewHandle;

            SetTitleWithShortcut(view, availableShortcutIndex);

            viewToHandleDictionary[view] = GetHandle(view);
        }

        private void UnregisterTab(string viewHandle)
        {
            int shortcutIndex = viewToShortcutDictionary[viewHandle];
            isShortcutAvailable[shortcutIndex] = true;
            shortcutViews[shortcutIndex] = null;

            viewToShortcutDictionary.Remove(viewHandle);
            previousViewTitles.Remove(viewHandle);
        }

        private void UnregisterTabView(View view)
        {
            string viewHandle = GetHandle(view);
            if (!viewToShortcutDictionary.ContainsKey(viewHandle))
                return;

            SetTitleWithoutShortcut(view);
            UnregisterTab(viewHandle);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SwitchToTab(string shortcut)
        {
            int shortcutIndex = 0;
            while (shortcutNames[shortcutIndex] != shortcut) ++shortcutIndex;

            if (isShortcutAvailable[shortcutIndex] || activeDocumentGroup == null)
                return;

            GetView(shortcutViews[shortcutIndex]).ShowInFront();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ChangeActiveToShortcut(string shortcut)
        {
            View activeView = ViewManager.Instance.ActiveView;
            if (activeView == null)
                return;

            string activeViewHandle = GetHandle(activeView);

            if (!viewToShortcutDictionary.ContainsKey(activeViewHandle))
                return;

            int activeViewShortcutIndex = viewToShortcutDictionary[activeViewHandle];

            int shortcutIndex = 0;
            while (shortcutNames[shortcutIndex] != shortcut) ++shortcutIndex;

            if (!isShortcutAvailable[shortcutIndex])
            {
                string oldViewHandle = shortcutViews[shortcutIndex];
                isShortcutAvailable[activeViewShortcutIndex] = false;
                shortcutViews[activeViewShortcutIndex] = oldViewHandle;
                viewToShortcutDictionary[oldViewHandle] = activeViewShortcutIndex;

                View oldView = GetView(oldViewHandle);
                SetTitleWithoutShortcut(oldView);
                SetTitleWithShortcut(oldView, activeViewShortcutIndex);
            }
            else
            {
                isShortcutAvailable[activeViewShortcutIndex] = true;
                shortcutViews[activeViewShortcutIndex] = null;
            }

            isShortcutAvailable[shortcutIndex] = false;
            shortcutViews[shortcutIndex] = activeViewHandle;
            viewToShortcutDictionary[activeViewHandle] = shortcutIndex;

            SetTitleWithoutShortcut(activeView);
            SetTitleWithShortcut(activeView, shortcutIndex);
        }

        private void FixTabHandle(View view)
        {
            if (!viewToHandleDictionary.ContainsKey(view))
                return;

            string oldViewHandle = viewToHandleDictionary[view];
            string newViewHandle = GetHandle(view);
            if (!newViewHandle.Equals(oldViewHandle))
            {
                viewToHandleDictionary[view] = newViewHandle;

                int shortcutIndex = viewToShortcutDictionary[oldViewHandle];
                viewToShortcutDictionary[newViewHandle] = viewToShortcutDictionary[oldViewHandle];
                shortcutViews[shortcutIndex] = newViewHandle;

                WindowFrameTitle title = (view.Title as WindowFrameTitle);
                previousViewTitles[newViewHandle] = title.AnnotatedTitle;

                SetTitleWithoutShortcut(view);
                SetTitleWithShortcut(view, shortcutIndex);

                viewToShortcutDictionary.Remove(oldViewHandle);
                previousViewTitles.Remove(oldViewHandle);
            }
        }

        public void FixAllTabHandles()
        {
            foreach (View view in activeDocumentGroup.VisibleChildren)
                FixTabHandle(view);

            foreach (View view in activeDocumentGroup.PinnedViews)
                FixTabHandle(view);
        }

        private void UpdateTabTitle(View view)
        {
            if (view.Title == null)
                return;

            RegisterTabView(view);

            string viewHandle = GetHandle(view);

            WindowFrameTitle title = (view.Title as WindowFrameTitle);
            previousViewTitles[viewHandle] = title.AnnotatedTitle;

            SetTitleWithoutShortcut(view);
            SetTitleWithShortcut(view, viewToShortcutDictionary[viewHandle]);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateAllTabTitles()
        {
            foreach (View view in activeDocumentGroup.VisibleChildren)
                UpdateTabTitle(view);

            foreach (View view in activeDocumentGroup.PinnedViews)
                UpdateTabTitle(view);
        }

        public void UpdateActiveDocumentGroup()
        {
            var activeView = ViewManager.Instance.ActiveView;
            if (activeView == null)
                return;
            var group = activeView.Parent as DocumentGroup;
            if (group != null)
            {
                SetDocumentGroup(group);
            }
        }

        public void ForceUpdateActiveDocumentGroup()
        {
            var activeView = ViewManager.Instance.ActiveView;
            if (activeView == null)
                return;
            var group = activeView.Parent as DocumentGroup;
            if (group != null)
            {
                SetDocumentGroup(group);
                FixAllTabHandles();
                FreeShortcutsForNonVisibleDocuments();
            }
        }

        public void OnDocumentGroupVisibilityChanged(object sender, EventArgs e)
        {
            DocumentGroup group = sender as DocumentGroup;
            if (group.IsVisible)
                SetDocumentGroup(group);
        }

        private void FreeShortcutsForNonVisibleDocuments()
        {
            List<string> handlesToUnregister = new List<string>();
            foreach (var viewHandle in viewToShortcutDictionary.Keys)
            {
                bool isVisible = false;
                foreach (View view in activeDocumentGroup.VisibleChildren)
                {
                    if (GetHandle(view).Equals(viewHandle))
                    {
                        isVisible = true;
                        break;
                    }
                }
                if (!isVisible)
                {
                    foreach (View view in activeDocumentGroup.PinnedViews)
                    {
                        if (GetHandle(view).Equals(viewHandle))
                        {
                            isVisible = true;
                            break;
                        }
                    }
                }
                if (!isVisible)
                {
                    handlesToUnregister.Add(viewHandle);
                }
            }
            foreach (var viewHandle in handlesToUnregister)
                UnregisterTab(viewHandle);
        }

        private void RenameTab(string oldViewHandle, string newViewHandle)
        {
            if (!viewToShortcutDictionary.ContainsKey(oldViewHandle))
                return;

            if (!newViewHandle.Equals(oldViewHandle))
            {
                int shortcutIndex = viewToShortcutDictionary[oldViewHandle];
                viewToShortcutDictionary[newViewHandle] = viewToShortcutDictionary[oldViewHandle];
                shortcutViews[shortcutIndex] = newViewHandle;

                View view = null;
                foreach (var iter in viewToHandleDictionary)
                {
                    if (iter.Value.Equals(oldViewHandle))
                    {
                        view = iter.Key;
                        break;
                    }
                }

                if (view != null)
                {
                    previousViewTitles[newViewHandle] = newViewHandle.Substring(newViewHandle.LastIndexOf('\\') + 1);

                    WindowFrameTitle title = (view.Title as WindowFrameTitle);
                    title.Title = "[" + shortcutNames[shortcutIndex] + "]" + previousViewTitles[newViewHandle];

                    viewToHandleDictionary[view] = newViewHandle;
                }

                viewToShortcutDictionary.Remove(oldViewHandle);
                previousViewTitles.Remove(oldViewHandle);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnItemRenamed(object Item, object ItemParent, string OldName)
        {
            string newViewHandle = (string)Item.GetType().GetProperty("FullPath").GetValue(Item, null);
            string oldViewHandle = newViewHandle.Substring(0, newViewHandle.LastIndexOf('\\') + 1) + OldName;

            RenameTab(oldViewHandle, newViewHandle);
        }


        private string[] shortcutNames = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private bool[] isShortcutAvailable;
        private string[] shortcutViews;
        private Dictionary<string, int> viewToShortcutDictionary;
        private Dictionary<string, string> previousViewTitles;
        private Dictionary<View, string> viewToHandleDictionary;

        private DocumentGroup activeDocumentGroup = null;

        private static TabShortcutsManager instance = null;
    }
}
