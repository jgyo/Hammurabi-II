using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Ham2.viewmodel
{
    public class FolderTreeViewModel : ViewModelBase
    {
        private string _selectedPath;
        private DirectoryItemViewModel _selectedTreeItem;
        private readonly ObservableCollection<TreeItemViewModel> _treeRoot = new ObservableCollection<TreeItemViewModel>();

        public FolderTreeViewModel()
        {
            var drives = Directory.GetLogicalDrives();
            foreach (var item in drives)
            {
                var treeItem = new DirectoryItemViewModel(item.Remove(item.Length - 1), null);
                treeItem.PropertyChanged += TreeItemPropertyChanged;
                treeItem.ItemAdded += TreeItemItemAdded;
                treeItem.ItemRemoved -= TreeItemItemRemoved;
                this._treeRoot.Add(treeItem);
            }
        }

        private void TreeItemItemAdded(object sender, EventArgs e)
        {
            var item = sender as TreeItemViewModel;
            if (item == null)
            {
                return;
            }

            item.PropertyChanged += TreeItemPropertyChanged;
            item.ItemAdded += TreeItemItemAdded;
            item.ItemRemoved += TreeItemItemRemoved;
        }

        private void TreeItemItemRemoved(object sender, EventArgs e)
        {
            var item = sender as TreeItemViewModel;
            if (item == null)
            {
                return;
            }

            item.PropertyChanged -= TreeItemPropertyChanged;
            item.ItemAdded -= TreeItemItemAdded;
            item.ItemRemoved -= TreeItemItemRemoved;
        }

        private void TreeItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected")
            {
                return;
            }

            var treeItem = sender as DirectoryItemViewModel;
            if (this._selectedTreeItem != null && this._selectedTreeItem != treeItem)
            {
                this._selectedTreeItem.IsSelected = false;
            }

            this._selectedTreeItem = treeItem;
            if (treeItem == null)
            {
                SelectedPath = string.Empty;
                return;
            }

            SelectedPath = treeItem.FullPath ?? string.Empty;
        }

        public void TraversePath(string path)
        {
            if (path == null)
            {
                return;
            }

            var pathParts = path.Split('\\');
            var nodes = TreeRoot;

            TreeItemViewModel selectedNode = null;
            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                var node = nodes.SingleOrDefault(p => p.Name.ToLower() == part.ToLower());
                if (node == null)
                {
                    break;
                }

                node.IsExpanded = true;
                node.IsSelected = true;
                selectedNode = node;
                nodes = node.Children;
            }
        }

        public string SelectedPath
        {
            get => this._selectedPath;
            set
            {
                if (this._selectedPath == value)
                {
                    return;
                }

                this._selectedPath = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<TreeItemViewModel> TreeRoot => this._treeRoot;
    }
}