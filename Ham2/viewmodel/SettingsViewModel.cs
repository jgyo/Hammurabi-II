using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ham2.Properties;
using Ham2.utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Ham2.viewmodel
{
    public class SettingsViewModel : ViewModelBase, ISettings
    {
        private ObservableCollection<string> _audioDevices = new ObservableCollection<string>();
        private string _ffmpegArgFormat;
        private string _ffmpegPath;
        private string _ffplayArgFmt;
        private string _ffplayPath;
        private string _ffprobePath;
        private RelayCommand<System.Windows.Controls.TextBox> _getFilePathCommand;
        private RelayCommand<System.Windows.Controls.TextBox> _getFolderCommand;
        private string _inputDir;
        private string _outputDir;
        private RelayCommand _persistCommand;
        private RelayCommand<System.Windows.Controls.TextBox> _resetArgFormatCommand;
        private readonly Settings _settings;
        private string _streamingHost;
        readonly private ObservableCollection<string> _videoDevices = new ObservableCollection<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel()
        {
            this._settings = Settings.Default;
            ResetModel();
        }

        /// <summary>
        /// Occurs when [devices updated].
        /// </summary>
        public event EventHandler DevicesUpdated;

        /// <summary>
        /// Occurs when [model updated].
        /// </summary>
        public event EventHandler ModelUpdated;

        private string GetFilePath(Window window, string text)
        {
            if (window == null)
            {
                throw new ArgumentNullException();
            }

            var picker = new OpenFileDialog()
            {
                CheckFileExists = true,
                FileName = Path.GetFileName(text),

                InitialDirectory = string.IsNullOrEmpty(text) ? "c:\\" : Path.GetDirectoryName(text)
            };

            picker.ShowDialog();
            return picker.FileName;
        }

        private string GetFolder(Window window, string oldValue)
        {
            //Window window = GetWindow(tb);
            if (window == null)
            {
                throw new ArgumentNullException();
            }

            var picker = new FolderBrowserDialog()
            {
                Description = "Pick a folder",
                SelectedPath = oldValue
            };

            picker.ShowDialog();
            return picker.SelectedPath;
        }

        private Window GetWindow(DependencyObject child)
        {
            var parent = LogicalTreeHelper.GetParent(child);
            while (parent != null && !(parent is Window))
            {
                child = parent;
                parent = LogicalTreeHelper.GetParent(child);
            }

            return parent as Window;
        }

        private void InitFFpaths(string path)
        {
            path = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(this.FFmpegPath) && File.Exists($"{path}\\{"ffmpeg.exe"}"))
            {
                this.FFmpegPath = $"{path}\\{"ffmpeg.exe"}";
            }
            if (string.IsNullOrEmpty(this.FFplayPath) && File.Exists($"{path}\\{"ffplay.exe"}"))
            {
                this.FFplayPath = $"{path}\\{"ffplay.exe"}";
            }
            if (string.IsNullOrEmpty(this.FFProbePath) && File.Exists($"{path}\\{"ffprobe.exe"}"))
            {
                this.FFProbePath = $"{path}\\{"ffprobe.exe"}";
            }
        }

        /// <summary>
        /// Updates the devices.
        /// </summary>
        private void UpdateDevices()
        {
            try
            {
                if (string.IsNullOrEmpty(this.FFmpegPath) || !File.Exists(this.FFmpegPath))
                {
                    return;
                }
            }
            catch { return; }

            var ffmpeg = new ProcessHelper(this.FFmpegPath)
            {
                Arguments = "-hide_banner -list_devices 1 -f dshow -i dummy"
            };

            ffmpeg.Start();
            var lines = ffmpeg.StandardError;

            var gettingVid = false;
            var gettingAud = false;
            AudioDevices.Clear();
            VideoDevices.Clear();

            foreach (var item in lines)
            {
                if (item.Contains("DirectShow video devices"))
                {
                    gettingVid = true;
                    gettingAud = false;
                    continue;
                }
                else if (item.Contains("DirectShow audio devices"))
                {
                    gettingAud = true;
                    gettingVid = false;
                    continue;
                }
                else if (string.IsNullOrEmpty(item) || item.Contains("Alternative name") || item.Contains("exit requested"))
                {
                    continue;
                }

                var quoteIndex = item.IndexOf("\"", StringComparison.Ordinal);
                if (quoteIndex < 0)
                {
                    continue;
                }

                var device = item.Remove(0, quoteIndex + 1);
                quoteIndex = device.IndexOf("\"", StringComparison.Ordinal);
                if (quoteIndex < 0)
                {
                    throw new ArgumentNullException(item);
                }

                device = device.Remove(quoteIndex);

                if (gettingAud)
                {
                    AudioDevices.Add(device);
                }
                if (gettingVid)
                {
                    VideoDevices.Add(device);
                }
            }

            OnDevicesUpdated();
        }

        /// <summary>
        /// Called when [devices updated].
        /// </summary>
        public void OnDevicesUpdated()
        {
            EventHandler t = DevicesUpdated;
            t?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Called when [model updated].
        /// </summary>
        public void OnModelUpdated()
        {
            EventHandler t = ModelUpdated;
            t?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Updates the model.
        /// </summary>
        public void ResetModel()
        {
            if (string.IsNullOrEmpty(this._settings.inputDir))
            {
                if (string.IsNullOrEmpty(this._settings.outputDir))
                {
                    this._settings.outputDir = this._settings.inputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    this._settings.inputDir = this._settings.outputDir;
                }
            }

            if (string.IsNullOrEmpty(this._settings.outputDir))
            {
                this._settings.outputDir = this._settings.inputDir;
            }

            FFmpegPath = this._settings.ffmpegPath;
            FFplayPath = this._settings.ffplayPath;
            FFProbePath = this._settings.ffprobePath;
            InputDir = this._settings.inputDir;
            OutputDir = this._settings.outputDir;
            FFmpegArgFmt = this._settings.ffmpegArgFmt;
            StreamingHost = this._settings.streamingHost;
            FFplayArgFmt = this._settings.ffplayArgFmt;

            UpdateDevices();
            OnModelUpdated();
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        public void UpdateSettings()
        {
            this._settings.ffmpegPath = FFmpegPath;
            this._settings.ffplayPath = FFplayPath;
            this._settings.ffprobePath = FFProbePath;
            this._settings.inputDir = InputDir;
            this._settings.outputDir = OutputDir;
            this._settings.ffmpegArgFmt = FFmpegArgFmt;
            this._settings.streamingHost = StreamingHost;
            this._settings.ffplayArgFmt = FFplayArgFmt;

            this._settings.Save();

            UpdateDevices();
        }

        public void UpdateTextBox(System.Windows.Controls.TextBox tb, string value) => tb.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, value);

        /// <summary>
        /// Gets the audio devices.
        /// </summary>
        /// <value>
        /// The audio devices.
        /// </value>
        public ObservableCollection<string> AudioDevices => this._audioDevices;

        /// <summary>
        /// Gets or sets the ffmpeg argument FMT.
        /// </summary>
        /// <value>
        /// The ffmpeg argument FMT.
        /// </value>
        public string FFmpegArgFmt
        {
            get => this._ffmpegArgFormat; set
            {
                if (value == this._ffmpegArgFormat)
                {
                    return;
                }

                this._ffmpegArgFormat = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ffmpeg path.
        /// </summary>
        /// <value>
        /// The ffmpeg path.
        /// </value>
        public string FFmpegPath
        {
            get => this._ffmpegPath; set
            {
                if (value == this._ffmpegPath)
                {
                    return;
                }

                this._ffmpegPath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ffplay argument FMT.
        /// </summary>
        /// <value>
        /// The f fplay argument FMT.
        /// </value>
        public string FFplayArgFmt
        {
            get => this._ffplayArgFmt; set
            {
                if (value == this._ffplayArgFmt)
                {
                    return;
                }

                this._ffplayArgFmt = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ffplay path.
        /// </summary>
        /// <value>
        /// The f fplay path.
        /// </value>
        public string FFplayPath
        {
            get => this._ffplayPath; set
            {
                if (value == this._ffplayPath)
                {
                    return;
                }

                this._ffplayPath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ff probe path.
        /// </summary>
        /// <value>
        /// The ff probe path.
        /// </value>
        public string FFProbePath
        {
            get => this._ffprobePath; set
            {
                if (value == this._ffprobePath)
                {
                    return;
                }

                this._ffprobePath = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<System.Windows.Controls.TextBox> GetFilePathCommand => this._getFilePathCommand
                    ?? (this._getFilePathCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        tb =>
                        {
                            if (tb == null)
                            {
                                return;
                            }

                            var window = GetWindow(tb);
                            if (window == null)
                            {
                                return;
                            }

                            UpdateTextBox(tb, GetFilePath(window, tb.Text));
                            InitFFpaths(tb.Text);
                        }));

        public RelayCommand<System.Windows.Controls.TextBox> GetFolderCommand => this._getFolderCommand
                    ?? (this._getFolderCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        tb =>
                        {
                            if (tb == null)
                            {
                                return;
                            }

                            var window = GetWindow(tb);
                            if (window == null)
                            {
                                return;
                            }

                            UpdateTextBox(tb, GetFolder(window, tb.Text));
                        }));

        /// <summary>
        /// Gets or sets the input dir.
        /// </summary>
        /// <value>
        /// The input dir.
        /// </value>
        public string InputDir
        {
            get => this._inputDir; set
            {
                if (value == this._inputDir)
                {
                    return;
                }

                this._inputDir = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the output dir.
        /// </summary>
        /// <value>
        /// The output dir.
        /// </value>
        public string OutputDir
        {
            get => this._outputDir; set
            {
                if (value == this._outputDir)
                {
                    return;
                }

                this._outputDir = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand PersistCommand => this._persistCommand ??
            (this._persistCommand = new RelayCommand(UpdateSettings));

        public RelayCommand<System.Windows.Controls.TextBox> ResetArgFormatCommand => this._resetArgFormatCommand
                    ?? (this._resetArgFormatCommand = new RelayCommand<System.Windows.Controls.TextBox>(
                        tb =>
                        {
                            if (tb == null)
                            {
                                return;
                            }

                            var s = new Settings();
                            s.Reset();

                            UpdateTextBox(tb, s.ffmpegArgFmt);
                        }));

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public string StreamingHost
        {
            get => this._streamingHost; set
            {
                if (value == this._streamingHost)
                {
                    return;
                }

                this._streamingHost = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the video devices.
        /// </summary>
        /// <value>
        /// The video devices.
        /// </value>
        public ObservableCollection<string> VideoDevices => this._videoDevices;
    }
}