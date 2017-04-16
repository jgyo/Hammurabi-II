using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Ham2.viewmodel
{
    public class TreeItemViewModel : ViewModelBase
    {
        private readonly ObservableCollection<TreeItemViewModel> _children = new ObservableCollection<TreeItemViewModel>();
        private bool _isExpanded;
        private bool _isSelected;
        private string _name;
        private TreeItemViewModel _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemViewModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        public TreeItemViewModel(string name, TreeItemViewModel parent)
        {
            this._name = name;
            this._parent = parent;
            this._children.CollectionChanged += ChildrenCollectionChanged;
        }

        /// <summary>
        /// Occurs when [item added].
        /// </summary>
        public event EventHandler ItemAdded;

        /// <summary>
        /// Occurs when [item removed].
        /// </summary>
        public event EventHandler ItemRemoved;

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var newItem = item as TreeItemViewModel;
                        if (newItem == null)
                        {
                            continue;
                        }

                        newItem.ItemAdded += NewItemItemAdded;
                        newItem.ItemRemoved += NewItemItemRemoved;
                        OnItemAdded(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var oldItem = item as TreeItemViewModel;
                        if (oldItem == null)
                        {
                            return;
                        }

                        oldItem.ItemAdded -= NewItemItemAdded;
                        oldItem.ItemRemoved -= NewItemItemRemoved;
                        OnItemRemoved(oldItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    break;
            }
        }

        private void NewItemItemAdded(object sender, EventArgs e) => OnItemAdded(sender as TreeItemViewModel);

        private void NewItemItemRemoved(object sender, EventArgs e) => OnItemRemoved(sender as TreeItemViewModel);

        private void OnItemAdded(TreeItemViewModel addedItem)
        {
            var itemAdded = ItemAdded;
            if (itemAdded == null)
            {
                return;
            }

            itemAdded.Invoke(addedItem, EventArgs.Empty);
        }

        private void OnItemRemoved(TreeItemViewModel removedItem)
        {
            var itemRemoved = ItemRemoved;
            if (itemRemoved == null)
            {
                return;
            }
            itemRemoved.Invoke(removedItem, EventArgs.Empty);
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="child">The child.</param>
        internal void RemoveChild(TreeItemViewModel child) => this._children.Remove(child);

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="child">The child.</param>
        public void AddChild(TreeItemViewModel child) => this._children.Add(child);

        /// <summary>
        /// Adds the children.
        /// </summary>
        /// <param name="children">The children.</param>
        public void AddChildren(string[] children)
        {
            foreach (var item in children)
            {
                this._children.Add(new TreeItemViewModel(item, this));
            }
        }

        /// <summary>
        /// Clears the children.
        /// </summary>
        public void ClearChildren() => this._children.Clear();

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public ObservableCollection<TreeItemViewModel> Children => this._children;

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public string FullPath
        {
            get
            {
                if (this._parent == null)
                {
                    return this._name + "\\";
                }

                return this._parent.FullPath + this._name + "\\";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get => this._isExpanded; set
            {
                if (value != this._isExpanded)
                {
                    this._isExpanded = value;
                    RaisePropertyChanged();
                }

                if (this._isExpanded && this._parent != null)
                {
                    this._parent.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get => this._isSelected; set
            {
                if (value == this._isSelected)
                {
                    return;
                }

                this._isSelected = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => this._name;

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public TreeItemViewModel Parent
        {
            get => this._parent; set
            {
                if (value == this._parent)
                {
                    return;
                }

                this._parent = value;
                RaisePropertyChanged();
            }
        }
    }
}