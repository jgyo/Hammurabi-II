using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ham2.Properties;
using Ham2.utility;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Ham2.viewmodel
{
    /// <summary>
    /// !+ This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _applicationName;
        private bool _canBroadcast;
        private bool _canMonitor;
        private bool _canRecord;
        //private string _credentials;
        private ProcessHelper _ffmpegProcess;
        private ProcessHelper _ffplay;
        private H264SettingsViewModel _h264ViewModel;
        private bool _isBroadcasting;
        private bool _isInDebugMode;
        private bool _isPlaying;
        private bool _isRecording;
        private MetadataViewModel _metadataViewModel;
        private RelayCommand<string> _openHyperlinkCommand;
        private RelayCommand _persistCommand;
        private RecorderSettingsViewModel _recorderViewModel;
        private SecurityViewModel _securityViewModel;
        private string _selectedAudioDevice;
        private string _selectedVideoDevice;
        private SettingsViewModel _settingsViewModel;
        private RelayCommand _startBroadcastingCommand;
        private bool _startedProcessInDebugMode;
        private RelayCommand _startPlayingCommand;
        //private RelayCommand _startRecordingCommand;
        private RelayCommand _stopBroadcastingCommand;
        private RelayCommand _stopPlayingCommand;
        private RelayCommand _stopRecordingCommand;
        private string _streamName;
        //private string _username;

        /// <summary>
        /// !+ Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Security.Security.IpAddressAquired += SecurityIpAddressAquired;
            var locator = Application.Current.FindResource("Locator") as ViewModelLocator;
            this._h264ViewModel = locator.H264Settings;
            this._metadataViewModel = locator.Metadata;
            this._recorderViewModel = locator.RecorderSettings;
            this._settingsViewModel = locator.Settings;
            this._securityViewModel = locator.Security;

            this._recorderViewModel.SetMainViewModel(this);

            StreamName = Main.Default.StreamName;
            SelectedVideoDevice = Main.Default.VideoSource;
            SelectedAudioDevice = Main.Default.AudioSource;
            ApplicationName = Main.Default.ApplicationName;

            this._settingsViewModel.DevicesUpdated += SettingsDevicesUpdated;
            this._settingsViewModel.PropertyChanged += ViewModelPropertyChanged;
            this._h264ViewModel.PropertyChanged += ViewModelPropertyChanged;
            this._metadataViewModel.PropertyChanged += ViewModelPropertyChanged;

            CheckBroadcastingAvailability();
        }

        public event EventHandler<DispatchEventArgs> DispatchEvent;
        public event EventHandler FFmpegStarting;
        public event EventHandler FFmpegStopping;

        /// <summary>
        /// Checks the broadcasting availability.
        /// </summary>
        private void CheckBroadcastingAvailability()
        {
            // Check if we have ffmpeg path
            if (this._ffmpegProcess != null || string.IsNullOrEmpty(this._settingsViewModel.FFmpegPath) || !File.Exists(this._settingsViewModel.FFmpegPath))
            {
                CanBroadcast = false;
                return;
            }

            if (BroadcastArgument.Contains("Invalid") || BroadcastArgument.Contains(nameof(Exception)))
            {
                CanBroadcast = false;
                return;
            }

            CanBroadcast = true;
        }

        /// <summary>
        /// ++ Checks the monitor availabllity.
        /// </summary>
        private void CheckMonitorAvailability()
        {
            if (this._ffplay != null || string.IsNullOrEmpty(this._settingsViewModel.FFplayPath) || !File.Exists(this._settingsViewModel.FFplayPath))
            {
                CanMoniter = false;
                return;
            }

            if (IsNotBroadcasting || string.IsNullOrEmpty(MonitorArgument) || MonitorArgument.Contains("Invalid") || MonitorArgument.Contains(nameof(Exception)))
            {
                CanMoniter = false;
                return;
            }

            CanMoniter = true;
        }

        private void FFmpegProcessErrorDataReceived(object sender, ProcessHelper.ErrorDataReceivedEventArgs e)
        {
            if (e.ErrorData != null && !e.ErrorData.StartsWith("frame=", StringComparison.Ordinal))
            {
                Xout.LogInf($"STDERR {DateTime.Now:HH:mm:ss.ffff}: {e.ErrorData}");
            }
        }

        private void FFmpegProcessFFmpegProcessExited(object sender, ProcessHelper.FFmpegProcessExitedEventArgs e)
        {
            OnDispatchEvent(OnFFmpegStopping);
            this._ffmpegProcess.FFmpegProcessExited -= FFmpegProcessFFmpegProcessExited;
            if (this._startedProcessInDebugMode)
            {
                this._ffmpegProcess.OutputDataReceived -= FFmpegProcessOutputDataReceived;
                this._ffmpegProcess.ErrorDataReceived -= FFmpegProcessErrorDataReceived;
            }
            this._ffmpegProcess = null;

            OnDispatchEvent(CheckBroadcastingAvailability);
        }

        private void FFmpegProcessOutputDataReceived(object sender, ProcessHelper.OutputDataReceivedEventArgs e) =>
                            Xout.LogInf($"STDOUT {DateTime.Now:HH:mm:ss.ffff}: {e.OutputData}");

        private void FFplayFFmpegProcessExited(object sender, ProcessHelper.FFmpegProcessExitedEventArgs e)
        {
            this._ffplay.FFmpegProcessExited -= FFplayFFmpegProcessExited;
            this._ffplay = null;

            OnDispatchEvent(() => IsPlaying = false);
            OnDispatchEvent(CheckMonitorAvailability);
        }

        /// <summary>
        /// ++ Called when [broadcast argument updated].
        /// </summary>
        private void OnBroadcastArgumentUpdated()
        {
            RaisePropertyChanged(nameof(BroadcastArgument));
            CheckBroadcastingAvailability();
        }

        private void SecurityIpAddressAquired(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(BroadcastArgument));
            RaisePropertyChanged(nameof(PublicIpAddress));
        }

        /// <summary>
        /// ++ Handles the DevicesUpdated event from the Settings view model.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SettingsDevicesUpdated(object sender, EventArgs e)
        {
            SelectedVideoDevice = Main.Default.VideoSource;
            SelectedAudioDevice = Main.Default.AudioSource;
            CheckBroadcastingAvailability();
        }

        /// <summary>
        /// ++ Handles the PropertyChanged event of the SettingsViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnBroadcastArgumentUpdated();
            CheckMonitorAvailability();
        }

        /// <summary>
        /// Saves the properties.
        /// </summary>
        internal void SaveProperties()
        {
            Main.Default.StreamName = StreamName;
            Main.Default.VideoSource = SelectedVideoDevice;
            Main.Default.AudioSource = SelectedAudioDevice;
            Main.Default.ApplicationName = ApplicationName;

            Main.Default.Save();
        }

        /// <summary>
        /// Called when [dispatch event].
        /// </summary>
        /// <param name="command">The command.</param>
        public void OnDispatchEvent(Action command)
        {
            EventHandler<DispatchEventArgs> t = DispatchEvent;
            t?.Invoke(this, new DispatchEventArgs(command));
        }

        /// <summary>
        /// Called when [f fmpeg starting].
        /// </summary>
        public void OnFFmpegStarting()
        {
            IsBroadcasting = true;

            EventHandler t = FFmpegStarting;
            t?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Called when [f fmpeg stopping].
        /// </summary>
        public void OnFFmpegStopping()
        {
            IsBroadcasting = false;

            EventHandler t = FFmpegStopping;
            t?.Invoke(this, new EventArgs());
        }

        public string ApplicationName
        {
            get => this._applicationName;
            set
            {
                if (this._applicationName == value)
                {
                    return;
                }

                this._applicationName = value;
                RaisePropertyChanged();
                OnBroadcastArgumentUpdated();
                CheckMonitorAvailability();
            }
        }

        /// <summary>
        /// !+ Gets the broadcast argument.
        /// </summary>
        /// <value>
        /// The broadcast argument.
        /// </value>
        public string BroadcastArgument
        {
            get
            {
                if (string.IsNullOrEmpty(this._settingsViewModel.FFmpegArgFmt) ||
                    string.IsNullOrEmpty(this._settingsViewModel.StreamingHost) ||
                    string.IsNullOrEmpty(ApplicationName) || string.IsNullOrEmpty(SelectedAudioDevice) ||
                    string.IsNullOrEmpty(StreamName) || string.IsNullOrEmpty(SelectedVideoDevice))
                {
                    return "Invalid due to one or more missing options.";
                }

                try
                {
                    this._securityViewModel.GenerateHash(ApplicationName, StreamName);

                    return string.Format(this._settingsViewModel.FFmpegArgFmt,
                        SelectedVideoDevice,
                        SelectedAudioDevice,
                        this._settingsViewModel.StreamingHost,
                        ApplicationName,
                        StreamName,
                        this._h264ViewModel.CRF,
                        this._h264ViewModel.Preset,
                        this._h264ViewModel.VideoBitRate,
                        this._h264ViewModel.BufferSize,
                        this._h264ViewModel.PixelFormat,
                        this._h264ViewModel.GOP,
                        this._h264ViewModel.AudioCodec,
                        this._h264ViewModel.AudioBitRate,
                        this._securityViewModel.Username ?? "username",
                        this._metadataViewModel.Artist ?? string.Empty,
                        this._metadataViewModel.Album ?? string.Empty,
                        this._metadataViewModel.Copyright ?? string.Empty,
                        this._metadataViewModel.Description ?? string.Empty,
                        this._metadataViewModel.Comment ?? string.Empty,
                        this._metadataViewModel.Title ?? string.Empty,
                        this._securityViewModel.Expiration,
                        this._securityViewModel.Hash,
                        "-f sdl -pix_fmt yuv420p \"Caution: Input\"");
                }
                catch (Exception e)
                { return $"{e.Message}\r\n{e.Source}\r\n{e.TargetSite}\r\n{e.StackTrace}"; }
            }
        }

        /// <summary>
        /// !+ Gets or sets a value indicating whether this instance can broadcast.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can broadcast; otherwise, <c>false</c>.
        /// </value>
        public bool CanBroadcast
        {
            get => this._canBroadcast;
            set
            {
                if (this._canBroadcast == value)
                {
                    return;
                }

                this._canBroadcast = value;
                RaisePropertyChanged();
                StartBroadcastingCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// !+ Gets or sets a value indicating whether this instance can moniter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can moniter; otherwise, <c>false</c>.
        /// </value>
        public bool CanMoniter
        {
            get => this._canMonitor;
            set
            {
                if (value == this._canMonitor)
                {
                    return;
                }

                this._canMonitor = value;
                RaisePropertyChanged();
                StartPlayingCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can record.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can record; otherwise, <c>false</c>.
        /// </value>
        public bool CanRecord
        {
            private get => this._canRecord;
            set
            {
                if (this._canRecord == value)
                {
                    return;
                }

                this._canRecord = value;
                RaisePropertyChanged();
            }
        }

        public ProcessHelper FFmpegProcess => this._ffmpegProcess;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is broadcasting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is broadcasting; otherwise, <c>false</c>.
        /// </value>
        public bool IsBroadcasting
        {
            get => this._isBroadcasting;
            set
            {
                if (this._isBroadcasting == value)
                {
                    return;
                }

                this._isBroadcasting = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNotBroadcasting));
                //StartRecordingCommand.RaiseCanExecuteChanged();
                CheckMonitorAvailability();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in debug mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in debug mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsInDebugMode
        {
            get => this._isInDebugMode;
            set
            {
                if (this._isInDebugMode == value)
                {
                    return;
                }

                this._isInDebugMode = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is not broadcasting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not broadcasting; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotBroadcasting => !IsBroadcasting;

        /// <summary>
        /// Gets a value indicating whether this instance is not recording.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not recording; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotRecording => !IsRecording;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is playing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying
        {
            get => this._isPlaying;
            set
            {
                if (this._isPlaying == value)
                {
                    return;
                }

                this._isPlaying = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is recording.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is recording; otherwise, <c>false</c>.
        /// </value>
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
                RaisePropertyChanged(nameof(IsNotRecording));
            }
        }

        /// <summary>
        /// ++ Gets the monitor argument.
        /// </summary>
        /// <value>
        /// The monitor argument.
        /// </value>
        public string MonitorArgument
        {
            get
            {
                try
                {
                    return string.Format(this._settingsViewModel.FFplayArgFmt, this._settingsViewModel.StreamingHost, ApplicationName, StreamName);
                }
                catch
                {
                    return "Exception encountered.";
                }
            }
        }

        public RelayCommand<string> OpenHyperlinkCommand => this._openHyperlinkCommand
               ?? (this._openHyperlinkCommand = new RelayCommand<string>(
                        h =>
                        {
                            Process.Start(h);
                        },
                        h =>
                        {
                            return !string.IsNullOrEmpty(h);
                        }
                        ));

        /// <summary>
        /// ++ Gets the persist command.
        /// </summary>
        /// <value>
        /// The persist command.
        /// </value>
        public RelayCommand PersistCommand => this._persistCommand
                    ?? (this._persistCommand = new RelayCommand(
                        () =>
                        {
                            SaveProperties();
                        }));

        public string PublicIpAddress => Security.Security.IpAddress;

        /// <summary>
        /// ++ Gets or sets the selected audio device.
        /// </summary>
        /// <value>
        /// The selected audio device.
        /// </value>
        public string SelectedAudioDevice
        {
            get => this._selectedAudioDevice;
            set
            {
                if (this._selectedAudioDevice == value)
                {
                    return;
                }

                this._selectedAudioDevice = value;
                RaisePropertyChanged();
                OnBroadcastArgumentUpdated();
            }
        }

        /// <summary>
        /// ++ Gets or sets the selected video device.
        /// </summary>
        /// <value>
        /// The selected video device.
        /// </value>
        public string SelectedVideoDevice
        {
            get => this._selectedVideoDevice;
            set
            {
                if (this._selectedVideoDevice == value)
                {
                    return;
                }

                this._selectedVideoDevice = value;
                RaisePropertyChanged();
                OnBroadcastArgumentUpdated();
            }
        }

        /// <summary>
        /// ++ Gets the start broadcasting command.
        /// </summary>
        /// <value>
        /// The start broadcasting command.
        /// </value>
        public RelayCommand StartBroadcastingCommand => this._startBroadcastingCommand
                    ?? (this._startBroadcastingCommand = new RelayCommand(
                        () =>
                        {
                            if (this._ffmpegProcess != null)
                            {
                                return;
                            }

                            SaveProperties();

                            this._ffmpegProcess = new ProcessHelper(this._settingsViewModel.FFmpegPath)
                            {
                                Arguments = BroadcastArgument
                            };

                            this._ffmpegProcess.FFmpegProcessExited += FFmpegProcessFFmpegProcessExited;
                            if (IsInDebugMode)
                            {
                                Xout.FilePath = $@"{this._settingsViewModel.OutputDir}{DateTime.Now:yyyyMMdd HHmmss} {StreamName}.log";
                                this._ffmpegProcess.ErrorDataReceived += FFmpegProcessErrorDataReceived;
                                this._ffmpegProcess.OutputDataReceived += FFmpegProcessOutputDataReceived;
                                this._startedProcessInDebugMode = true;
                            }

                            OnFFmpegStarting();
                            CheckBroadcastingAvailability();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            this._ffmpegProcess.StartAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            // this.CheckMonitorAvailabllity();
                        },
                        () =>
                        {
                            return CanBroadcast;
                        }
                        ));

        /// <summary>
        /// ++ Gets the start playing command.
        /// </summary>
        /// <value>
        /// The start playing command.
        /// </value>
        public RelayCommand StartPlayingCommand => this._startPlayingCommand
                    ?? (this._startPlayingCommand = new RelayCommand(
                        () =>
                        {
                            if (this._ffplay != null)
                            {
                                return;
                            }

                            this._ffplay = new ProcessHelper(this._settingsViewModel.FFplayPath)
                            {
                                Arguments = MonitorArgument
                            };

                            this._ffplay.FFmpegProcessExited += FFplayFFmpegProcessExited;

                            CheckMonitorAvailability();
                            IsPlaying = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            this._ffplay.StartAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        },
                        () =>
                        {
                            return CanMoniter;
                        }
                        ));

        ///// <summary>
        ///// Gets the start recording command.
        ///// </summary>
        ///// <value>
        ///// The start recording command.
        ///// </value>
        //public RelayCommand StartRecordingCommand => this._startRecordingCommand
        //            ?? (this._startRecordingCommand = new RelayCommand(
        //                () =>
        //                {
        //                    // Execute logic
        //                },
        //                () =>
        //                {
        //                    return IsBroadcasting &&
        //                    !this._recorderViewModel.StartRecordingURL.Contains(nameof(Exception));
        //                }));

        /// <summary>
        /// Gets the stop broadcasting command.
        /// </summary>
        /// <value>
        /// The stop broadcasting command.
        /// </value>
        public RelayCommand StopBroadcastingCommand => this._stopBroadcastingCommand
                    ?? (this._stopBroadcastingCommand = new RelayCommand(
                        () =>
                        {
                            if (FFmpegProcess == null)
                            {
                                return;
                            }

                            FFmpegProcess.StandardIn("q");
                        }));

        public RelayCommand StopPlayingCommand => this._stopPlayingCommand
                                            ?? (this._stopPlayingCommand = new RelayCommand(
                        () =>
                        {
                            this._ffplay.KillProcess();
                        }));

        /// <summary>
        /// Gets the stop recording command.
        /// </summary>
        /// <value>
        /// The stop recording command.
        /// </value>
        public RelayCommand StopRecordingCommand => this._stopRecordingCommand
                    ?? (this._stopRecordingCommand = new RelayCommand(
                        () =>
                        {
                            // Execute logic
                        }));

        /// <summary>
        /// Gets or sets the name of the stream.
        /// </summary>
        /// <value>
        /// The name of the stream.
        /// </value>
        public string StreamName
        {
            get => this._streamName;
            set
            {
                if (this._streamName == value)
                {
                    return;
                }

                this._streamName = value;
                RaisePropertyChanged();
                OnBroadcastArgumentUpdated();
                CheckMonitorAvailability();
            }
        }
    }
}