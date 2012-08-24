using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;
using SaveSync.Controllers;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for ChooseProvider.xaml
    /// </summary>
    public partial class ChooseProvider : Window
    {
        public ChooseProvider()
        {
            InitializeComponent();

            //Setup the webcomponent
            wcProvider.FlushAlpha = false;
            wcProvider.IsTransparent = true;
            wcProvider.ContextMenu = new ContextMenu { IsEnabled = false, Visibility = Visibility.Hidden };
            wcProvider.CreateObject("guiController");

            wcProvider.SetObjectCallback("guiController", "setProvider", onSetProviderCall);
        }

        private void onSetProviderCall(object sender, JSCallbackEventArgs e)
        {
            wcProvider.CallJavascriptFunction("","lightsOff");
            Console.WriteLine(@"ChooseProvider>>> Called JS function; Setting provider: " + e.Arguments[0].ToString());
            if(e.Arguments[0].ToString().Equals("dropbox"))
            {
                GlobalController.Provider = GlobalController.ProviderEnum.DropBox;

                if (!File.Exists("auth/token.xml"))
                {
                    //Your first time setting up the program?
                    Console.WriteLine(@"App.cs>>> No token found, please authenticate the application.");

                    var connectwindow = new ConnectWindow();
                    connectwindow.Show();
                }
                else
                {
                    Console.WriteLine(@"App.cs>>> Token found, no special authentication required");

                    var mainwindow = new MainWindowSimple();
                    mainwindow.Show();
                }
            }
            else if(e.Arguments[0].ToString().Equals("box"))
            {
                GlobalController.Provider = GlobalController.ProviderEnum.BoxNet;

                var loginwindow = new LoginWindow();
                loginwindow.Show();
            }

            Close();
        }

        private void windowProvider_Loaded(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("file://" + Environment.CurrentDirectory + "/gui/providers.html");
            wcProvider.Source = uri;
        }
    }
}
