using System.Windows;
using System.Windows.Controls;

namespace Ham2.commands
{
    public class TreeViewItemBehavior : DependencyObject
    {
        public static readonly DependencyProperty BringIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached("BringIntoViewWhenSelected", typeof(bool),
                typeof(TreeViewItemBehavior),
                new UIPropertyMetadata(false, OnBringIntoViewWhenSelectedChanged));

        private static void OnBringIntoViewWhenSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as TreeViewItem;
            if (item == null)
            {
                return;
            }

            if (e.NewValue is bool == false)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                item.BringIntoView();
            }
        }

        public static bool GetBringIntoViewWhenSelected(TreeViewItem treeViewItem) =>
            (bool)treeViewItem.GetValue(BringIntoViewWhenSelectedProperty);

        public static void SetBringIntoViewWhenSelected(TreeViewItem treeViewItem, bool value) =>
            treeViewItem.SetValue(BringIntoViewWhenSelectedProperty, value);
    }
}