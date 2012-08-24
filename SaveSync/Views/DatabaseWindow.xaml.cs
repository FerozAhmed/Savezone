using System.Windows;
using Awesomium.Windows.Controls;

namespace SaveSync.Views
{
    /// <summary>
    /// Interaction logic for DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : Window
    {
        public DatabaseWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //tbResponse.Text = GamesController.GetDatabaseData();
            wc1.ContextMenu = new WebControlContextMenu();
        }
    }
}
