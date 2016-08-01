using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tricentis.TCAPIObjects.Objects;

namespace TosCode.Connector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public TCFolder Folder { get; private set; }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void LoadWithData(TCFolder folder)
        {
            TosCode.Connector.App app = new TosCode.Connector.App();
            app.InitializeComponent();
            app.Folder = folder;
            app.Run();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }
    }
}
