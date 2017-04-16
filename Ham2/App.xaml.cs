using System.Collections.Generic;
using System.Windows;

namespace Ham2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args == null)
            {
                return;
            }

            Args = new List<string>(e.Args);
        }

        public static List<string> Args
        {
            get;
            private set;
        }
    }
}