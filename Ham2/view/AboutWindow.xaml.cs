using System;
using System.Linq;
using System.Windows;

namespace Ham2.view
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() => InitializeComponent();

        private void ButtonClick(object sender, RoutedEventArgs e) => Close();
    }
}
