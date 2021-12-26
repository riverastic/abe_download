using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ABE_Download
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup( object sender, StartupEventArgs e )
        {
            if ( e.Args.Length == 1 && e.Args[ 0 ] is not null && e.Args[ 0 ].Equals( "--download" ) || e.Args[ 0 ].Equals( "--dl" ) )
            {
                MainWindow wnd = new MainWindow();
                wnd.ShowDialog();
                wnd.BringIntoView();
            }
            else
            {
                Environment.Exit( 0 );
            }
        }
    }
}
