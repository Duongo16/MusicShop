using System.Windows;

namespace MusicShop.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainFrame_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/Item/ItemListWindow.xaml", UriKind.Relative));
        }


        private void NavOrder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/Order/OrderListWindow.xaml", UriKind.Relative));
        }

        private void NavItems_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/Item/ItemListWindow.xaml", UriKind.Relative));
        }

        private void NavCategory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/Category/CategoryListWindow.xaml", UriKind.Relative));
        }

        private void NavBrand_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Views/Brand/BrandListWindow.xaml", UriKind.Relative));
        }
    }
}