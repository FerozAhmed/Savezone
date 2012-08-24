using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shell;
using Awesomium.Core;
using SaveSync.Controllers;
using SaveSync.Models;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for MainWindowSimple.xaml
    /// </summary>
    public partial class MainWindowSimple : Window
    {
        private MainController _mc;
        private BackgroundWorker _worker = new BackgroundWorker();

        private NotifyIcon _mNotifyIcon;

        public MainWindowSimple()
        {
            InitializeComponent();

            //Set the main controller
            _mc = new MainController();
            //DataContext = _mc;

            Setup();
        }

        public MainWindowSimple(String username, String password)
        {
            InitializeComponent();

            //Set the main controller
            _mc = new MainController(username, password);
            //DataContext = _mc;

            Setup();
        }

        private void Setup()
        {
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            //Set the systemtray icon
            var streamResourceInfo = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Savezone;component/Cloud48x48.ico"));
            if (streamResourceInfo != null)
            {
                Stream iconStream = streamResourceInfo.Stream;
                _mNotifyIcon = new NotifyIcon
                {
                    BalloonTipText = @"The app has been minimised. Click the tray icon to show.",
                    BalloonTipTitle = @"SaveSync",
                    Text = @"SaveSync",
                    Icon = new System.Drawing.Icon(iconStream)
                };
                var notifyContextMenu = new ContextMenu();
            }
            _mNotifyIcon.MouseClick += m_notifyIcon_Click;

            //Setup the webcomponent
            wbtnBackup.JSConsoleMessageAdded += OnJSConsole;
            wbtnBackup.PreviewMouseLeftButtonDown += OnPreviewMouseLeftDown;

            wbtnBackup.FlushAlpha = false;
            wbtnBackup.IsTransparent = true;
            wbtnBackup.ContextMenu = new ContextMenu { IsEnabled = false, Visibility = Visibility.Hidden };
            wbtnBackup.CreateObject("guiController");

            //When the Javascript call the "backup" method
            wbtnBackup.SetObjectCallback("guiController", "backup", onBackupCall);

            //Initialize webkit components
            var buttonuri = new Uri("file://" + Environment.CurrentDirectory + "/gui/button_backup_round.html");
            Console.WriteLine(buttonuri);
            wbtnBackup.Source = buttonuri;
        }

        private void OnPreviewMouseLeftDown(Object sender, MouseButtonEventArgs e)
        {
            // Ignore the event if DOM is not yet ready or the control is disabled/crashed.
            if (!wbtnBackup.IsEnabled)
                return;

            // Get the coordinates.
            var deviceTransform = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            var pos = e.GetPosition(this);
            var x = (int)(pos.X * deviceTransform.M11);
            var y = (int)(pos.Y * deviceTransform.M22);

            // Execute simple JS that will tell us if there's a valid DOM object at these coordinates.
            var val = wbtnBackup.ExecuteJavascriptWithResult(String.Format(@"($(document.elementFromPoint({0},{1})).attr('class') == 'draghandle')", x, y));

            if (val == null) return;
            using (val)
            {
                if (val.ToBoolean())
                {
                    // Prevent the event from being processed by the control.
                    e.Handled = true;
                    // Start DragMove.
                    DragMove();
                }
            }
        }

        private void OnJSConsole(Object sender, JSConsoleMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void OnClose(object sender, CancelEventArgs args)
        {
            WebCore.Shutdown();

            if(_worker.IsBusy)
                _worker.CancelAsync();

            _worker.Dispose();

            _mNotifyIcon.Dispose();
            _mNotifyIcon = null;

            //Dispatcher.InvokeShutdown();
            //Application.Current.Shutdown(0);
        }
        private WindowState _mStoredWindowState = WindowState.Normal;
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (_mNotifyIcon != null)
                    _mNotifyIcon.ShowBalloonTip(2000);
            }
            else
                _mStoredWindowState = WindowState;
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }
        void m_notifyIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var menu = (ContextMenu)FindResource("NotifierContextMenu");
                menu.IsOpen = true;
            }
            else if(e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = _mStoredWindowState;   
            }
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }
        void ShowTrayIcon(bool show)
        {
            if (_mNotifyIcon != null)
                _mNotifyIcon.Visible = show;
        }

        private void Backup()
        {
            if (!_worker.IsBusy)
            {
                //Set the button text
                wbtnBackup.CallJavascriptFunction("", "gameProgress", new JSValue(0));
                //wbtnBackup.CallJavascriptFunction("", "disableButton");
                wbtnBackup.DeferInput();
                windowMain.ForceCursor = true;

                tbinfo.ProgressState = TaskbarItemProgressState.Normal;

                _worker.DoWork += delegate(object s, DoWorkEventArgs args)
                {
                    _mc.Backup(GamesController.GetDatabaseData(), _worker);
                };

                _worker.ProgressChanged += delegate(object sender1, ProgressChangedEventArgs e1)
                {
                    var progressinfo = (ProgressInfo) e1.UserState;

                    switch (progressinfo.ProgressType)
                    {
                            case ProgressTypeEnum.ProgressType.Games:
                            JSValue[] p = { new JSValue(progressinfo.CurrentIndex), new JSValue(progressinfo.Total), new JSValue(progressinfo.Gamename),  };
                            wbtnBackup.CallJavascriptFunction("", "gameProgress", p);
                            p = null;
                            tbinfo.ProgressValue = (double)e1.ProgressPercentage/100;
                            break;

                            case ProgressTypeEnum.ProgressType.Upload:
                            JSValue[] p2 = { new JSValue(e1.ProgressPercentage)};
                            wbtnBackup.CallJavascriptFunction("", "uploadProgress", p2);
                            p2 = null;
                            break;

                            case ProgressTypeEnum.ProgressType.Files:
                            JSValue[] p3 = {
                                                    new JSValue(1),
                                                    new JSValue(progressinfo.CurrentIndex),
                                                    new JSValue(progressinfo.Total)
                                                };
                            wbtnBackup.CallJavascriptFunction("", "animation", p3);
                            p3 = null;
                            break;
                    }
                };

                _worker.RunWorkerCompleted += (sender2, e2) => {
                    windowMain.ForceCursor = false;
                    Close();
                };

                //Do your work!
                _worker.RunWorkerAsync();

                _mNotifyIcon.BalloonTipText =
                    @"The app is minimized to the tray during backup. Click the tray icon to show.";
                //WindowState = WindowState.Minimized;
            }
        }

        private void onBackupCall(object sender, JSCallbackEventArgs e)
        {
            Backup();
            Console.WriteLine(@"MainWindowSimple>>> Javascript called the backup method.");
        }

        private void Menu_Quit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
