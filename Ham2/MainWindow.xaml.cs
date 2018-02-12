using Ham2.utility;
using Ham2.view;
using Ham2.viewmodel;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Ham2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AboutWindow _aboutWindow;
        private H264SettingsViewModel _h264SettingsViewModel;
        private MainViewModel _mainViewModel;
        private MetadataViewModel _metadataViewModel;
        private RecorderSettingsViewModel _recorderViewModel;
        private SettingsViewModel _settingsViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            var locator = Application.Current.FindResource("Locator") as ViewModelLocator;
            this._settingsViewModel = locator.Settings;
            this._mainViewModel = locator.Main;
            this._h264SettingsViewModel = locator.H264Settings;
            this._metadataViewModel = locator.Metadata;
            this._recorderViewModel = locator.RecorderSettings;

            this._mainViewModel.FFmpegStarting += this.MainViewModelFFmpegStarting;
            this._mainViewModel.DispatchEvent += this.MainViewModelDispatchEvent;
            this._recorderViewModel.DispatchEvent += this.MainViewModelDispatchEvent;

            if (App.Args == null)
            {
                return;
            }

            if (App.Args.Contains("-debug"))
            {
                this.DebugCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void AboutMenuItemClicked(object sender, RoutedEventArgs e)
        {
            if (this._aboutWindow == null)
            {
                this._aboutWindow = new AboutWindow() { ShowInTaskbar = true };
                this._aboutWindow.Closed += this.AboutWindowClosed;
                this._aboutWindow.Show();
                return;
            }

            this._aboutWindow.Activate();
        }

        private void AboutWindowClosed(object sender, EventArgs e) => this._aboutWindow = null;

        private void FFmpegArgTBMenuItemClick(object sender, RoutedEventArgs e) => Clipboard.SetText(this.ffmpegArgTextBlock.Text);

        /// <summary>
        /// Handles the ErrorDataReceived event of the Ffmpeg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProcessHelper.ErrorDataReceivedEventArgs"/> instance containing the event data.</param>
        private void FfmpegErrorDataReceived(object sender, ProcessHelper.ErrorDataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.ErrorData))
            {
                return;
            }

            this.Dispatcher.Invoke(() =>
            {
                var data = e.ErrorData;
                var dataTrimmed = data.Trim();

                if (dataTrimmed.StartsWith("Input", StringComparison.CurrentCulture) ||
                dataTrimmed.StartsWith("Stream", StringComparison.CurrentCulture) ||
                dataTrimmed.StartsWith("Output", StringComparison.Ordinal) ||
                dataTrimmed.Contains("Could not run graph") ||
                dataTrimmed.Contains("I/O error"))
                {
                    var textBlock = new TextBlock()
                    {
                        Text = $"{DateTime.Now:HH:mm:ss.fff}: {data}",
                        Margin = new Thickness(4, 0, 4, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    this.StaticData.Children.Add(textBlock);
                    return;
                }

                if (data.StartsWith("frame", StringComparison.Ordinal))
                {
                    var match = Regex.Match(data, "frame=\\s*(?<frames>[^\\s]+)\\sfps=\\s*(?<fps>[^\\s]+)\\sq=\\s*(?<q1>[^\\s]+)\\s.*\\ssize=\\s*(?<size>[^\\s]+)\\stime=\\s*(?<time>[^\\s]+)\\sbitrate=\\s*(?<bitrate>[^\\s]+)\\s(dup=\\s*(?<dup>[^\\s]+)\\s)*(drop=\\s*(?<drop>[^\\s]+)\\s)*speed=\\s*(?<speed>[^\\s]+)");
                    if (!match.Success)
                    {
                        return;
                    }

                    var frameGroup = match.Groups["frames"];
                    var fpsGroup = match.Groups[nameof(this.fps)];
                    var q1Group = match.Groups[nameof(this.q1)];
                    //var q2Group = match.Groups["q2"];
                    var sizeGroup = match.Groups["size"];
                    var timeGroup = match.Groups["time"];
                    var bitrateGroup = match.Groups["bitrate"];
                    var speedGroup = match.Groups["speed"];
                    var dups = match.Groups["dup"];
                    var drops = match.Groups["drop"];

                    this.Frames.Text = frameGroup.Value;
                    this.fps.Text = fpsGroup.Value;
                    this.q1.Text = q1Group.Value;
                    //this.q2.Text = q2Group.Value;
                    this.Size.Text = sizeGroup.Value;
                    this.Time.Text = timeGroup.Value;
                    this.Bitrate.Text = bitrateGroup.Value;
                    this.Speed.Text = speedGroup.Value;
                    this.Dups.Text = dups.Value;
                    this.Drops.Text = drops.Value;
                }
            });
        }

        /// <summary>
        /// Handles the FFmpegProcessExited event of the Ffmpeg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProcessHelper.FFmpegProcessExitedEventArgs"/> instance containing the event data.</param>
        private void FfmpegFFmpegProcessExited(object sender, ProcessHelper.FFmpegProcessExitedEventArgs e) =>
            this.Dispatcher.Invoke(() =>
            {
                this.ExitDataGroup.Visibility = Visibility.Visible;

                this.StartTime.Text = $"{e.StartTime:HH:mm:ss.fff}";
                this.ExitTime.Text = $"{e.EndTime:HH:mm:ss.fff}";
                var elapsed = e.EndTime - e.StartTime;
                if (elapsed < TimeSpan.Parse("00:00:00.5"))
                {
                    this.LiveData.Visibility = Visibility.Collapsed;
                }

                var span = $"{elapsed}";
                this.ElapsedTime.Text = span.Remove(12);
                span = $"{e.TotalProcTime}";
                this.TotalProcTime.Text = span.Length > 12 ? span.Remove(12) : span;
                span = $"{e.UserTime}";
                this.UserTime.Text = span.Length > 12 ? span.Remove(12) : span;
                this.ExitCode.Text = e.ExitCode + string.Empty;
            });

        /// <summary>
        /// Handles the OutputDataReceived event of the Ffmpeg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProcessHelper.OutputDataReceivedEventArgs"/> instance containing the event data.</param>
        private void FfmpegOutputDataReceived(object sender, ProcessHelper.OutputDataReceivedEventArgs e)
        {
        }

        private void MainButtonClicked(object sender, RoutedEventArgs e) => this.Main.Activate();

        private void MainViewModelDispatchEvent(object sender, DispatchEventArgs e) =>
                                                    this.Dispatcher.Invoke(e.Command);

        private void MainViewModelFFmpegStarting(object sender, EventArgs e)
        {
            this.StaticData.Children.Clear();
            this.ExitDataGroup.Visibility = Visibility.Collapsed;
            this.LiveData.Visibility = Visibility.Visible;
            this.StaticGroup.Visibility = Visibility.Visible;

            var ffmpeg = this._mainViewModel.FFmpegProcess;

            ffmpeg.ErrorDataReceived += this.FfmpegErrorDataReceived;
            ffmpeg.OutputDataReceived += this.FfmpegOutputDataReceived;
            ffmpeg.FFmpegProcessExited += this.FfmpegFFmpegProcessExited;
        }

        private void MenuItemClick(object sender, RoutedEventArgs e) => Close();

        private void PropertiesMenuItemClicked(object sender, RoutedEventArgs e)
        {
            var propertiesWindow = new PropertiesWindow()
            {
                Owner = this
            };

            propertiesWindow.ShowDialog();
        }

        private void StatButtonClicked(object sender, RoutedEventArgs e) => this.Stats.Activate();

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (this._mainViewModel.IsBroadcasting)
            {
                var result = MessageBox.Show("A broadcast is running. Closing will stop the broadcast. Do you want to close anyway?", "Stop Broadcasting?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _mainViewModel.SaveProperties();

            base.OnClosing(e);

            if (e.Cancel)
            {
                return;
            }

            if (this._aboutWindow != null)
            {
                this._aboutWindow.Close();
            }

            if (this._mainViewModel.IsRecording)
            {
                this._mainViewModel.StopRecordingCommand.Execute(null);
            }

            if (this._mainViewModel.IsBroadcasting)
            {
                this._mainViewModel.StopBroadcastingCommand.Execute(null);
            }

            if (this._mainViewModel.IsPlaying)
            {
                this._mainViewModel.StopPlayingCommand.Execute(null);
            }
        }
    }
}