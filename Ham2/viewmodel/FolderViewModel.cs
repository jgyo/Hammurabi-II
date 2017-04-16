using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Ham2.viewmodel
{
    public class FolderViewModel : ViewModelBase
    {
        private ObservableCollection<FileViewModel> _files = new ObservableCollection<FileViewModel>();
        private readonly FolderTreeViewModel _folderTree;
        private string _fullFolderPath;
        private FileViewModel _selectedFile;

        public FolderViewModel()
        {
            var locator = Application.Current.FindResource("Locator") as ViewModelLocator;
            this._folderTree = locator.FolderTree;
        }

        internal void TraversePath(string fullFilePath)
        {
            var dirName = Path.GetDirectoryName(fullFilePath);
            var fileName = Path.GetFileName(fullFilePath);
            this._folderTree.TraversePath(dirName);
            var f = Files.SingleOrDefault(a => a.Filename + a.Extension == fileName);
            SelectedFile = f;
        }

        public ObservableCollection<FileViewModel> Files
        {
            get => this._files; set
            {
                if (this._files == value)
                {
                    return;
                }

                this._files = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Full path to current file.
        /// </summary>
        public string FullFilePath
        {
            get
            {
                if (this._selectedFile == null)
                {
                    return string.Empty;
                }
                return FullFolderPath + this._selectedFile.Filename + this._selectedFile.Extension;
            }
        }

        /// <summary>
        /// Full path to current folder.
        /// </summary>
        public string FullFolderPath
        {
            get => this._fullFolderPath; set
            {
                if (this._fullFolderPath == value)
                {
                    return;
                }

                this._fullFolderPath = value;
                Files.Clear();
                RaisePropertyChanged();
                if (SelectedFile != null)
                {
                    SelectedFile.IsSelected = false;
                }

                try
                {
                    var files = Directory.EnumerateFiles(this._fullFolderPath);
                    foreach (var file in files)
                    {
                        var fi = new FileInfo(file);
                        var fvm = new FileViewModel()
                        {
                            Filename = fi.Extension.Length == 0 ? fi.Name : fi.Name.Remove(fi.Name.Length - fi.Extension.Length),
                            Extension = fi.Extension,
                            Date = fi.LastWriteTime,
                            Size = fi.Length
                        };

                        Files.Add(fvm);
                    }
                }
                catch { }
            }
        }

        public FileViewModel SelectedFile
        {
            get => this._selectedFile; set
            {
                if (this._selectedFile == value)
                {
                    return;
                }

                if (this._selectedFile != null)
                {
                    this._selectedFile.IsSelected = false;
                }
                this._selectedFile = value;
                if (this._selectedFile != null)
                {
                    this._selectedFile.IsSelected = true;
                }

                RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(FullFilePath));
            }
        }
    }
}