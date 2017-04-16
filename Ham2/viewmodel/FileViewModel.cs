using GalaSoft.MvvmLight;
using System;

namespace Ham2.viewmodel
{
    public class FileViewModel : ViewModelBase
    {
        private DateTime _date;
        private string _extension;
        private string _filename;
        private bool _isSelected;
        private long _size;

        public DateTime Date
        {
            get => this._date; set
            {
                if (value == this._date)
                {
                    return;
                }

                this._date = value;
                RaisePropertyChanged();
            }
        }

        public string Extension
        {
            get => this._extension; set
            {
                if (value == this._extension)
                {
                    return;
                }

                this._extension = value;
                RaisePropertyChanged();
            }
        }

        public string Filename
        {
            get => this._filename; set
            {
                if (value == this._filename)
                {
                    return;
                }

                this._filename = value;
                RaisePropertyChanged();
            }
        }

        public string FilenameExt
        {
            get; internal set;
        }

        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (this._isSelected == value)
                {
                    return;
                }

                this._isSelected = value;
                RaisePropertyChanged();
            }
        }

        public long Size
        {
            get => this._size; set
            {
                if (this._size == value)
                {
                    return;
                }

                this._size = value;
                RaisePropertyChanged();
            }
        }
    }
}