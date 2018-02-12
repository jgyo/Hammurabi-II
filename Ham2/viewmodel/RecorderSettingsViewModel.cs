using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ham2.Properties;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;

namespace Ham2.viewmodel
{
    public class RecorderSettingsViewModel : ViewModelBase, ISettings
    {
        private bool _isRecording;
        private MainViewModel _mainViewModel;
        private string _name;
        private string _path;
        private RelayCommand _persistCommand;
        private SecurityViewModel _securityViewModel;
        private SettingsViewModel _settingsViewModel;
        private RelayCommand _startRecordingCommand;
        private RelayCommand _stopRecordingCommand;

        /// <summary>
        /// Implements the RecorderSettingsViewModel.
        /// </summary>
        public RecorderSettingsViewModel()
        {
            var locator = Application.Current.FindResource("Locator") as ViewModelLocator;
            this._settingsViewModel = locator.Settings;
            this._securityViewModel = locator.Security;

            ResetModel();

            this._settingsViewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        public event EventHandler<DispatchEventArgs> DispatchEvent;

        private void OnUrlPropertiesChanged()
        {
            RaisePropertyChanged(nameof(StartRecordingURL));
            RaisePropertyChanged(nameof(StopRecordingURL));
            StartRecordingCommand.RaiseCanExecuteChanged();
            StopRecordingCommand.RaiseCanExecuteChanged();
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) =>
            OnDispatchEvent(() =>
           {
               switch (e.PropertyName)
               {
                   case "StreamingHost":
                       OnUrlPropertiesChanged();
                       break;

                   case "ApplicationName":
                       OnUrlPropertiesChanged();
                       break;

                   case "StreamName":
                       OnUrlPropertiesChanged();
                       break;

                   case "Username":
                       OnUrlPropertiesChanged();
                       break;

                   case "Credentials":
                       OnUrlPropertiesChanged();
                       break;

                   case "IsBroadcasting":
                       StartRecordingCommand.RaiseCanExecuteChanged();
                       StopRecordingCommand.RaiseCanExecuteChanged();
                       if (this._mainViewModel.IsBroadcasting)
                       {
                           break;
                       }

                       IsRecording = false;
                       break;
               }
           });

        public void OnDispatchEvent(Action action)
        {
            EventHandler<DispatchEventArgs> t = DispatchEvent;
            t?.Invoke(this, new DispatchEventArgs(action));
        }

        public void ResetModel()
        {
            Name = Settings.Default.RecName;
            Path = Settings.Default.RecPath;
        }

        public void SetMainViewModel(MainViewModel main)
        {
            this._mainViewModel = main;
            this._mainViewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        public void UpdateSettings()
        {
            Settings.Default.RecName = Name;
            Settings.Default.RecPath = Path;
            Settings.Default.Save();
        }

        public bool IsRecording
        {
            get => this._isRecording;
            set
            {
                if (this._isRecording == value)
                {
                    return;
                }

                this._isRecording = value;
                RaisePropertyChanged();
                StartRecordingCommand.RaiseCanExecuteChanged();
                StopRecordingCommand.RaiseCanExecuteChanged();
            }
        }

        public string Name
        {
            get => this._name;
            set
            {
                if (this._name == value)
                {
                    return;
                }

                this._name = value;
                RaisePropertyChanged();
                OnUrlPropertiesChanged();
            }
        }

        public string Path
        {
            get => this._path;
            set
            {
                if (this._path == value)
                {
                    return;
                }

                this._path = value;
                RaisePropertyChanged();
                OnUrlPropertiesChanged();
            }
        }

        public RelayCommand PersistCommand => this._persistCommand
                    ?? (this._persistCommand = new RelayCommand(UpdateSettings));

        public RelayCommand StartRecordingCommand => this._startRecordingCommand
                    ?? (this._startRecordingCommand = new RelayCommand(
                        async () =>
                        {
                            var client = new HttpClient();
                            var response = await client.GetAsync(StartRecordingURL);
                            if (!response.IsSuccessStatusCode)
                            {
                                var s = await response.Content.ReadAsStringAsync();
                                System.Windows.Forms.MessageBox.Show(s, response.ReasonPhrase);
                                return;
                            }

                            IsRecording = true;
                        },
                        () =>
                        {
                            return !IsRecording &&
                            !StartRecordingURL.Contains(nameof(Exception)) &&
                            !this._mainViewModel.IsNotBroadcasting;
                        }
                        ));

        public string StartRecordingURL
        {
            get
            {
                try
                {
                    return string.Format(Settings.Default.ControlArgFmt,
                    "start",
                    this._settingsViewModel.StreamingHost,
                    Path,
                    this._mainViewModel.ApplicationName,
                    this._mainViewModel.StreamName,
                    Name,
                    this._securityViewModel.Username,
                    this._securityViewModel.SharedSecret);
                }
                catch { return "Exception encountered"; }
            }
        }

        public RelayCommand StopRecordingCommand => this._stopRecordingCommand
                            ?? (this._stopRecordingCommand = new RelayCommand(
                        async () =>
                        {
                            var client = new HttpClient();
                            var response = await client.GetAsync(StopRecordingURL);
                            if (!response.IsSuccessStatusCode)
                            {
                                var s = await response.Content.ReadAsStringAsync();
                                System.Windows.Forms.MessageBox.Show(s, response.ReasonPhrase);
                                return;
                            }

                            IsRecording = false;
                        },
                        () =>
                        {
                            return IsRecording &&
                            !StopRecordingURL.Contains(nameof(Exception)) &&
                            !this._mainViewModel.IsNotBroadcasting;
                        }
                        ));

        public string StopRecordingURL
        {
            get
            {
                try
                {
                    return string.Format(Settings.Default.ControlArgFmt,
                    "stop",
                    this._settingsViewModel.StreamingHost,
                    Path,
                    this._mainViewModel.ApplicationName,
                    this._mainViewModel.StreamName,
                    Name,
                    this._securityViewModel.Username,
                    this._securityViewModel.SharedSecret);
                }
                catch { return "Exception encountered"; }
            }
        }
    }
}