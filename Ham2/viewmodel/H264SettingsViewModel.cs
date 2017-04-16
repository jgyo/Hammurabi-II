using GalaSoft.MvvmLight;
using Ham2.Properties;
using System;
using System.Text.RegularExpressions;

namespace Ham2.viewmodel
{
    public class H264SettingsViewModel : ViewModelBase, ISettings
    {
        private string _audioBitRate;
        private string _audioCodec;
        private string _bufferSize;
        private int _crf;
        private int _gop;
        readonly private H264 _h264Settings;
        private string _pixelFormat;
        private string _preset;
        private string _videoBitRate;

        public H264SettingsViewModel()
        {
            this._h264Settings = H264.Default;
            ResetModel();
        }

        public void ResetModel()
        {
            CRF = this._h264Settings.crf;
            Preset = this._h264Settings.preset;
            VideoBitRate = this._h264Settings.videoBitRate;
            BufferSize = this._h264Settings.buffersize;
            GOP = this._h264Settings.gop;
            PixelFormat = this._h264Settings.pixelFormat;
            AudioBitRate = this._h264Settings.audioBitRate;
            AudioCodec = this._h264Settings.audioCodec;
        }

        public void UpdateSettings()
        {
            this._h264Settings.crf = CRF;
            this._h264Settings.preset = Preset;
            this._h264Settings.videoBitRate = VideoBitRate;
            this._h264Settings.buffersize = BufferSize;
            this._h264Settings.gop = GOP;
            this._h264Settings.audioCodec = AudioCodec;
            this._h264Settings.pixelFormat = PixelFormat;
            this._h264Settings.audioBitRate = AudioBitRate;
            this._h264Settings.Save();
        }

        public string AudioBitRate
        {
            get => this._audioBitRate; set
            {
                if (value == this._audioBitRate)
                {
                    return;
                }

                this._audioBitRate = value;
                RaisePropertyChanged();
                if (Regex.IsMatch(value, "[0-9.]{1,5}[km]", RegexOptions.IgnoreCase))
                {
                    return;
                }

                throw new ArgumentException("Invalid format.");
            }
        }

        public string AudioCodec
        {
            get => this._audioCodec; set
            {
                if (this._audioCodec == value)
                {
                    return;
                }

                this._audioCodec = value;
                RaisePropertyChanged();
            }
        }

        public string BufferSize
        {
            get => this._bufferSize; set
            {
                if (this._bufferSize == value)
                {
                    return;
                }

                this._bufferSize = value;
                RaisePropertyChanged();
                if (Regex.IsMatch(value, "[0-9.]{1,5}[km]", RegexOptions.IgnoreCase))
                {
                    return;
                }

                throw new ArgumentException("Invalid format.");
            }
        }

        public int CRF
        {
            get => this._crf; set
            {
                if (this._crf == value)
                {
                    return;
                }

                this._crf = value;
                RaisePropertyChanged();
            }
        }

        public int GOP
        {
            get => this._gop; set
            {
                if (this._gop == value)
                {
                    return;
                }

                this._gop = value;
                RaisePropertyChanged();
            }
        }

        public string PixelFormat
        {
            get => this._pixelFormat; set
            {
                if (this._pixelFormat == value)
                {
                    return;
                }

                this._pixelFormat = value;
                RaisePropertyChanged();
            }
        }

        public string Preset
        {
            get => this._preset; set
            {
                if (this._preset == value)
                {
                    return;
                }

                this._preset = value;
                RaisePropertyChanged();
            }
        }

        public string VideoBitRate
        {
            get => this._videoBitRate;
            set
            {
                if (this._videoBitRate == value)
                {
                    return;
                }

                this._videoBitRate = value;
                RaisePropertyChanged();
                if (Regex.IsMatch(value, "[0-9.]{1,5}[km]", RegexOptions.IgnoreCase))
                {
                    return;
                }

                throw new ArgumentException("Invalid format.");
            }
        }
    }
}