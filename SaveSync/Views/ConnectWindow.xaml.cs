using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow
    {
        private DropBoxConfiguration _UsedConfig = null;
        private DropBoxRequestToken _CurrentRequestToken = null;
        private ICloudStorageAccessToken _GeneratedToken = null;

        string appKey = "2wu1tuvvzyqamal";
        string appSecret = "endwrjghvotzkd0";

        public ConnectWindow()
        {
            InitializeComponent();

            // get a standard config
            _UsedConfig = DropBoxConfiguration.GetStandardConfiguration();

            wcAuthenticate.Navigated += new NavigatedEventHandler(webBrowser_DocumentTitleChanged);

            //Start authentication
            Authenticate();
        }

        /// <summary>
        /// Starts the token exchange process
        /// </summary>
        private void Authenticate()
        {
            // 0. reset token
            _GeneratedToken = null;

            // 1. modify dropbox configuration            
            _UsedConfig.APIVersion = DropBoxAPIVersion.V1;

            // 2. get the request token
            _CurrentRequestToken = DropBoxStorageProviderTools.GetDropBoxRequestToken(_UsedConfig, appKey, appSecret);

            if (_CurrentRequestToken == null)
            {
                MessageBox.Show("Can't get request token. Check Application Key & Secret values.");
                return;
            }

            // 3. get the authorization url 
            var authUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(_UsedConfig, _CurrentRequestToken);

            // 4. navigate to the AuthUrl 
            wcAuthenticate.Navigate(authUrl);
        }

        /// <summary>
        /// finishes the token exchange process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webBrowser_DocumentTitleChanged(object sender, EventArgs e)
        {
            if (_GeneratedToken == null && wcAuthenticate.Source.ToString().StartsWith(_UsedConfig.AuthorizationCallBack.ToString()))
            {
                // 5. try to get the real token
                _GeneratedToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(_UsedConfig, appKey, appSecret, _CurrentRequestToken);

                // 6. store the real token to file
                var cs = new CloudStorage();
                if(!Directory.Exists("auth"))
                {
                    Directory.CreateDirectory("auth");
                }
                cs.SerializeSecurityTokenEx(_GeneratedToken, _UsedConfig.GetType(), null, "auth/token.xml");

                // 7. show message box
                Console.WriteLine(@"ConnectWindow>>> Authentication token stored.");

                //Show main window
                var mainWindow = new MainWindowSimple();
                mainWindow.Show();

                Close();
                //System.Windows.Forms.MessageBox.Show(@"Stored token into " + @"auth/token.xml");
            }
        }
    }
}
