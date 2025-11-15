using System.Windows;

namespace MusicShop.WPF.Views.Item
{
    public partial class ItemListWindow 
    {
        private readonly ItemListViewModel _vm = new();
        public ItemListWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.InitAsync();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}