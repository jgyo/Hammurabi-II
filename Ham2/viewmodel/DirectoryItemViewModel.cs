using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Ham2.viewmodel
{
    public class DirectoryItemViewModel : TreeItemViewModel
    {
        public DirectoryItemViewModel(string name, TreeItemViewModel parent) : base(name, parent) =>
            this.PropertyChanged += DirectoryItemViewModelPropertyChanged;

        private void DirectoryItemViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != this)
            {
                throw new InvalidOperationException("Received property changed event from another object.");
            }

            if (e.PropertyName == nameof(IsExpanded) && this.IsExpanded)
            {
                try
                {
                    var directories = Directory.GetDirectories(this.FullPath);
                    for (var i = 0; i < directories.Length; i++)
                    {
                        var name = directories[i] = Path.GetFileName(directories[i]);
                        if (!this.Children.Any(a => a.Name == name))
                        {
                            AddChild(new DirectoryItemViewModel(name, this));
                        }
                    }

                    for (int i = this.Children.Count - 1; i >= 0; i--)
                    {
                        var child = this.Children[i];
                        if (!directories.Any(a => a == child.Name))
                        {
                            base.RemoveChild(child);
                        }
                    }
                }
                catch { }
            }
        }
    }
}