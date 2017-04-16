/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Ham2"
                           x:Key="Locator" />
  </Application.Resources>

  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

/// <summary>
/// Hammurabi View Models
/// </summary>
namespace Ham2.viewmodel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<FolderTreeViewModel>();
            SimpleIoc.Default.Register<FolderViewModel>();
            SimpleIoc.Default.Register<MetadataViewModel>();
            SimpleIoc.Default.Register<H264SettingsViewModel>();
            SimpleIoc.Default.Register<RecorderSettingsViewModel>();
            SimpleIoc.Default.Register<SecurityViewModel>();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }


        /// <summary>
        /// Gets the FolderViewModel.
        /// </summary>
        /// <value>
        /// The FolderViewModel.
        /// </value>
        public FolderViewModel Folder => ServiceLocator.Current.GetInstance<FolderViewModel>();

        /// <summary>
        /// Gets the folder tree.
        /// </summary>
        /// <value>
        /// The folder tree.
        /// </value>
        public FolderTreeViewModel FolderTree => ServiceLocator.Current.GetInstance<FolderTreeViewModel>();

        /// <summary>
        /// Gets the H264SettingsViewModel.
        /// </summary>
        /// <value>
        /// The H264SettingsViewModel.
        /// </value>
        public H264SettingsViewModel H264Settings => ServiceLocator.Current.GetInstance<H264SettingsViewModel>();

        /// <summary>
        /// Gets the MainViewModel.
        /// </summary>
        /// <value>
        /// The MainViewModel.
        /// </value>
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        /// <summary>
        /// Gets the MetadataViewModel.
        /// </summary>
        /// <value>
        /// The MetadataViewModel.
        /// </value>
        public MetadataViewModel Metadata => ServiceLocator.Current.GetInstance<MetadataViewModel>();

        /// <summary>
        /// Gets the RecorderSettingsViewModel.
        /// </summary>
        /// <value>
        /// The RecorderSettingsViewModel.
        /// </value>
        public RecorderSettingsViewModel RecorderSettings => ServiceLocator.Current.GetInstance<RecorderSettingsViewModel>();

        /// <summary>
        /// Gets the SettingsViewModel.
        /// </summary>
        /// <value>
        /// The SettingsViewModel.
        /// </value>
        public SecurityViewModel Security => ServiceLocator.Current.GetInstance<SecurityViewModel>();

        /// <summary>
        /// Gets the SettingsViewModel.
        /// </summary>
        /// <value>
        /// The SettingsViewModel.
        /// </value>
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
    }
}