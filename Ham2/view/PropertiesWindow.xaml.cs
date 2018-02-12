using Ham2.Security;
using Ham2.viewmodel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ham2.view
{
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        private bool _cancelled = true;
        private SettingsViewModel _genSettings;
        private H264SettingsViewModel _h264Settins;
        private MetadataViewModel _metadataSettings;
        private RecorderSettingsViewModel _recorderSettings;
        private SecurityViewModel _secureSettings;

        public PropertiesWindow()
        {
            InitializeComponent();

            var loc = App.Current.FindResource("Locator") as ViewModelLocator;
            this._genSettings = loc.Settings;
            this._h264Settins = loc.H264Settings;
            this._recorderSettings = loc.RecorderSettings;
            this._metadataSettings = loc.Metadata;
            this._secureSettings = loc.Security;

            this.SetSecureData();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this._cancelled = true;
            Close();
        }

        private void OkayButtonClicked(object sender, RoutedEventArgs e)
        {
            this._cancelled = false;
            UpdateSettings();
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.pwb1.Password == this.pwb2.Password && this.pwb1.Password.Length > 6)
            {
                this.pwb1.Background = new SolidColorBrush(Colors.White);
                this.pwb2.Background = new SolidColorBrush(Colors.White);

                var model = this._secureSettings;
                if (model == null)
                {
                    return;
                }

                    model.SecureData = new SecureData(this.pwb1.Password);
            }
            else
            {
                this.pwb1.Background = new SolidColorBrush(Colors.MistyRose);
                this.pwb2.Background = new SolidColorBrush(Colors.MistyRose);
            }
        }

        private void PlaceHolderCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var tag = e.Parameter as string;
            var tb = e.Source as TextBox;
            if (tb == null || tag == null)
            {
                return;
            }

            tb.SelectedText = tag;
        }

        private void RestoreModels()
        {
            this._genSettings.ResetModel();
            this._h264Settins.ResetModel();
            this._recorderSettings.ResetModel();
            this._metadataSettings.ResetModel();
            this._secureSettings.ResetModel();
        }

        private void SetSecureData()
        {
            var model = this._secureSettings;
            if (model == null || model.SecureData == null)
            {
                this.pwb1.Password = string.Empty;
                this.pwb2.Password = string.Empty;
                return;
            }

            this.pwb1.Password = model.SecureData.GetUnsecuredText();
            this.pwb2.Password = model.SecureData.GetUnsecuredText();
        }

        private void UpdateSettings()
        {
            this._genSettings.UpdateSettings();
            this._h264Settins.UpdateSettings();
            this._recorderSettings.UpdateSettings();
            this._metadataSettings.UpdateSettings();
            this._secureSettings.UpdateSettings();            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (this._cancelled)
            {
                RestoreModels();
            }
        }
    }
}
