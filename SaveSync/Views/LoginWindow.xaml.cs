using System;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            //Setup the webcomponent
            wcLogin.CreateObject("guiController");

            wcLogin.JSConsoleMessageAdded += OnJSConsole;
            wcLogin.SetObjectCallback("guiController", "login", onLoginCall);

            wcLogin.FlushAlpha = false;
            wcLogin.IsTransparent = true;
            wcLogin.ContextMenu = new ContextMenu { IsEnabled = false, Visibility = Visibility.Hidden };
        }

        private void OnJSConsole(Object sender, JSConsoleMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("file://" + Environment.CurrentDirectory + "/gui/login.html");
            wcLogin.Source = uri;
        }

        private void onLoginCall(object sender, JSCallbackEventArgs e)
        {
            Console.WriteLine("LoginWindow>>> Login function called from Javascript");
            var username = e.Arguments[0].ToString();
            var password = e.Arguments[1].ToString();

            Console.WriteLine("LoginWindow>>> " + username + " : " + password);

            try
            {
                var mainwindow = new MainWindowSimple(username, password);
                mainwindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                wcLogin.CallJavascriptFunction("", "lightsOn");
            }
        }
    }
}
