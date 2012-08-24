using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using SaveSync.Controllers;
using SaveSync.Models;
using System.IO;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly MainController mc;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        private NotifyIcon m_notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            //Initialize web components
            //var buttonuri = new Uri("file://" + Environment.CurrentDirectory + "/gui/button_backup.html");
            //Console.WriteLine(buttonuri);
            //wbtnBackup.Source = buttonuri;

            // initialise code here
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/SaveSync;component/Cloud48x48.ico")).Stream;
            m_notifyIcon = new NotifyIcon
                               {
                                   BalloonTipText = @"The app has been minimised. Click the tray icon to show.",
                                   BalloonTipTitle = @"SaveSync",
                                   Text = @"SaveSync",
                                   Icon = new System.Drawing.Icon( iconStream )
                               };
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);

            mc = new MainController();
        }

        void OnClose(object sender, CancelEventArgs args)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = mc;
        }
        private WindowState m_storedWindowState = WindowState.Normal;
        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (m_notifyIcon != null)
                    m_notifyIcon.ShowBalloonTip(2000);
            }
            else
                m_storedWindowState = WindowState;
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }
        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }
        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        private void filter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(lstSavegameInfo.ItemsSource);

            view.Filter = null;
            view.Filter = new Predicate<object>(FilterGameItem);  
        }

        private bool FilterGameItem(object obj)
        {
            var item = obj as SavegameInfo;
            if (item == null) return false;

            Boolean availableFilter = true;

            // apply the filter  
            if (item.LocationExists) return true;
            return false;
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            BackupAllGames();
        }

        private void BackupAllGames()
        {
            //Reset progress bars
            pbProgress.Value = 0;
            pbNumberofGames.Value = 0;
            pbFiles.Value = 0;

            _worker.WorkerReportsProgress = true;

            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                //mc.Backup(lstSavegameInfo.Items, _worker);
            };

            _worker.ProgressChanged += delegate(object sender1, ProgressChangedEventArgs e1)
            {
                if (Equals(e1.UserState, "games"))
                {
                    lblStatus.Text = "Backing up: " + e1.ProgressPercentage + "%";
                    pbNumberofGames.Value = e1.ProgressPercentage;
                }
                else if (Equals(e1.UserState, "upload"))
                {
                    pbProgress.Value = e1.ProgressPercentage;
                }
                else if (Equals(e1.UserState, "files"))
                {
                    pbFiles.Value = e1.ProgressPercentage;
                }
            };

            _worker.RunWorkerCompleted += delegate(object sender2, RunWorkerCompletedEventArgs e2)
            {
                Close();
            };

            lblStatus.Text = "Preparing backup...";
            _worker.RunWorkerAsync();

            WindowState = WindowState.Minimized;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var dbw = new DatabaseWindow();
            dbw.Show();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            filter();
        }
    }
}
