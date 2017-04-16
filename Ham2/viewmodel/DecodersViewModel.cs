using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ham2.viewmodel
{
    public class DecodersCollection : ViewModelBase
    {
        private bool _isSetup;

        public DecodersCollection() => Decoders = new ObservableCollection<DecodersViewModel>();

        public void Setup(List<string> standardOut)
        {
            if (this._isSetup)
            {
                return;
            }

            this._isSetup = true;
            bool foundStart = false;
            foreach (var item in standardOut)
            {
                if (!foundStart)
                {
                    foundStart = item == " ------";
                    continue;
                }

                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                var itemParts = item.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                var decoder = new DecodersViewModel()
                {
                    ShortName = itemParts[1],
                    LongName = itemParts[2],
                    DecoderType = itemParts[0][0] == 'V' ? "Video" : (itemParts[0][0] == 'A' ? "Audio" : (itemParts[0][0] == 'S' ? "Subtitles" : "Unknown")),
                    FLM = itemParts[0][1] == 'F',
                    SLM = itemParts[0][2] == 'S',
                    Experimental = itemParts[0][3] == 'X',
                    DHB = itemParts[0][4] == 'B',
                    DRM1 = itemParts[0][5] == 'D'
                };

                Decoders.Add(decoder);
            }
        }

        public ObservableCollection<DecodersViewModel> Decoders
        {
            get; private set;
        }
    }

    public class DecodersViewModel : ViewModelBase
    {
        private string _decoderType;
        private bool _dhb;
        private bool _drm1;
        private bool _experimental;
        private bool _flm;
        private string _longName;
        private string _shortName;
        private bool _slm;

        public string DecoderType
        {
            get => this._decoderType; set
            {
                if (this._decoderType == value)
                {
                    return;
                }

                this._decoderType = value;
                RaisePropertyChanged();
            }
        }

        public bool DHB
        {
            get => this._dhb;
            set
            {
                if (this._dhb == value)
                {
                    return;
                }

                this._dhb = value;
                RaisePropertyChanged();
            }
        }

        public bool DRM1
        {
            get => this._drm1;
            set
            {
                if (this._drm1 == value)
                {
                    return;
                }

                this._drm1 = value;
                RaisePropertyChanged();
            }
        }

        public bool Experimental
        {
            get => this._experimental;
            set
            {
                if (this._experimental == value)
                {
                    return;
                }

                this._experimental = value;
                RaisePropertyChanged();
            }
        }

        public bool FLM
        {
            get => this._flm;
            set
            {
                if (this._flm == value)
                {
                    return;
                }

                this._flm = value;
                RaisePropertyChanged();
            }
        }

        public string LongName
        {
            get => this._longName;
            set
            {
                if (this._longName == value)
                {
                    return;
                }

                this._longName = value;
                RaisePropertyChanged();
            }
        }

        public string ShortName
        {
            get => this._shortName;
            set
            {
                if (this._shortName == value)
                {
                    return;
                }

                this._shortName = value;
                RaisePropertyChanged();
            }
        }

        public bool SLM
        {
            get => this._slm;
            set
            {
                if (this._slm == value)
                {
                    return;
                }

                this._slm = value;
                RaisePropertyChanged();
            }
        }
    }
}