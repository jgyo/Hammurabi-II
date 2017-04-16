using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ham2.Properties;
using System;

namespace Ham2.viewmodel
{
    public class MetadataViewModel : ViewModelBase, ISettings
    {
        private string _album;
        private string _artist;
        private string _comment;
        private string _copyright;
        private string _description;
        private readonly Metadata _metadataSettings;
        private RelayCommand _persistCommand;
        private string _title;

        public MetadataViewModel()
        {
            this._metadataSettings = Metadata.Default;

            ResetModel();
        }

        public void ResetModel()
        {
            Album = this._metadataSettings.Album;
            Artist = this._metadataSettings.Artist;
            Comment = this._metadataSettings.Comment;
            Copyright = this._metadataSettings.Copyright;
            Description = this._metadataSettings.Description;
            Title = this._metadataSettings.Title;
        }

        public void UpdateSettings()
        {
            this._metadataSettings.Album = Album;
            this._metadataSettings.Artist = Artist;
            this._metadataSettings.Comment = Comment;
            this._metadataSettings.Copyright = Copyright;
            this._metadataSettings.Description = Description;
            this._metadataSettings.Title = Title;
            this._metadataSettings.Save();
        }

        public string Album
        {
            get => this._album; set
            {
                if (this._album == value)
                {
                    return;
                }

                this._album = value;
                RaisePropertyChanged();
            }
        }

        public string Artist
        {
            get => this._artist; set
            {
                if (this._artist == value)
                {
                    return;
                }

                this._artist = value;
                RaisePropertyChanged();
            }
        }

        public string Comment
        {
            get => this._comment; set
            {
                if (this._comment == value)
                {
                    return;
                }

                this._comment = value;
                RaisePropertyChanged();
            }
        }

        public string Copyright
        {
            get => this._copyright; set
            {
                if (this._copyright == value)
                {
                    return;
                }

                this._copyright = value;
                RaisePropertyChanged();
            }
        }

        public string Description
        {
            get => this._description; set
            {
                if (this._description == value)
                {
                    return;
                }

                this._description = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand PersistCommand => this._persistCommand ??
            (this._persistCommand = new RelayCommand(UpdateSettings));

        public string Title
        {
            get => this._title; set
            {
                if (this._title == value)
                {
                    return;
                }

                this._title = value;
                RaisePropertyChanged();
            }
        }
    }
}