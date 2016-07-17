using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.CompilerServices;

namespace TabShortcutsExtension
{
    class TabShortcutsManager
    {
        TabShortcutsManager()
        {
            viewToShortcutDictionary = new Dictionary<View, int>();
            previousViewTitles = new Dictionary<View, string>();
            isShortcutAvailable = new bool[shortcutNames.Length];
            shortcutViews = new View[shortcutNames.Length];
            for (int i = 0; i < isShortcutAvailable.Length; ++i)
            {
                isShortcutAvailable[i] = true;
                shortcutViews[i] = null;
            }
        }

        static public TabShortcutsManager GetInstance()
        {
            if (instance == null)
                instance = new TabShortcutsManager();
            return instance;
        }

        private void SetTitleWithShortcut(View view, int shortcutIndex)
        {
            WindowFrameTitle title = (view.Title as WindowFrameTitle);
            previousViewTitles[view] = title.Title;
            title.Title = "[" + shortcutNames[shortcutIndex] + "]" + title.Title;
        }

        private void SetTitleWithoutShortcut(View view)
        {
            WindowFrameTitle title = (view.Title as WindowFrameTitle);
            title.Title = previousViewTitles[view];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnVisibleChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (View view in e.NewItems)
                    RegisterTabView(view);
            if (e.OldItems != null)
                foreach (View view in e.OldItems)
                    if (!view.IsPinned)
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateActiveViews(List<View> activeViews)
        {
            List<View> viewsToUnregister = new List<View>();
            List<View> viewsToRegister = new List<View>();

            foreach (View view in activeViews)
                if (!viewToShortcutDictionary.ContainsKey(view))
                    viewsToRegister.Add(view);

            foreach (View view in viewToShortcutDictionary.Keys)
                if (!activeViews.Contains(view))
                    viewsToUnregister.Add(view);

            foreach (View view in viewsToUnregister)
                UnregisterTabView(view);

            foreach (View view in viewsToRegister)
                RegisterTabView(view);
        }

        private void RegisterTabView(View view)
        {
            if (viewToShortcutDictionary.ContainsKey(view))
                return;

            int availableShortcutIndex = 0;
            while (!isShortcutAvailable[availableShortcutIndex]) ++availableShortcutIndex;
            viewToShortcutDictionary[view] = availableShortcutIndex;
            isShortcutAvailable[availableShortcutIndex] = false;
            shortcutViews[availableShortcutIndex] = view;

            SetTitleWithShortcut(view, availableShortcutIndex);
        }

        private void UnregisterTabView(View view)
        {
            if (!viewToShortcutDictionary.ContainsKey(view))
                return;

            int shortcutIndex = viewToShortcutDictionary[view];
            isShortcutAvailable[shortcutIndex] = true;
            shortcutViews[shortcutIndex] = null;

            SetTitleWithoutShortcut(view);

            viewToShortcutDictionary.Remove(view);
            previousViewTitles.Remove(view);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SwitchToTab(string shortcut)
        {
            int shortcutIndex = 0;
            while (shortcutNames[shortcutIndex] != shortcut) ++shortcutIndex;

            if (isShortcutAvailable[shortcutIndex])
                return;

            shortcutViews[shortcutIndex].ShowInFront();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ChangeActiveToShortcut(string shortcut)
        {
            View activeView = ViewManager.Instance.ActiveView;
            if (activeView == null)
                return;

            if (!viewToShortcutDictionary.ContainsKey(activeView))
                return;

            int activeViewShortcutIndex = viewToShortcutDictionary[activeView];

            int shortcutIndex = 0;
            while (shortcutNames[shortcutIndex] != shortcut) ++shortcutIndex;

            if (!isShortcutAvailable[shortcutIndex])
            {
                View oldView = shortcutViews[shortcutIndex];
                isShortcutAvailable[activeViewShortcutIndex] = false;
                shortcutViews[activeViewShortcutIndex] = oldView;
                viewToShortcutDictionary[oldView] = activeViewShortcutIndex;
                SetTitleWithoutShortcut(oldView);
                SetTitleWithShortcut(oldView, activeViewShortcutIndex);
            }

            isShortcutAvailable[shortcutIndex] = false;
            shortcutViews[shortcutIndex] = activeView;
            viewToShortcutDictionary[activeView] = shortcutIndex;
            SetTitleWithoutShortcut(activeView);
            SetTitleWithShortcut(activeView, shortcutIndex);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateAllTabTitles()
        {
            foreach (View view in viewToShortcutDictionary.Keys)
            {
                WindowFrameTitle title = (view.Title as WindowFrameTitle);
                previousViewTitles[view] = title.AnnotatedTitle;

                SetTitleWithoutShortcut(view);
                SetTitleWithShortcut(view, viewToShortcutDictionary[view]);
            }
        }

        private string[] shortcutNames = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private bool[] isShortcutAvailable;
        private View[] shortcutViews;
        private Dictionary<View, int> viewToShortcutDictionary;
        private Dictionary<View, string> previousViewTitles;

        private static TabShortcutsManager instance = null;
    }
}
