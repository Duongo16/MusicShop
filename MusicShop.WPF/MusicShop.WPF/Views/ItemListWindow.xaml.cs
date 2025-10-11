using System.Windows;

namespace MusicShop.WPF.Views
{
    public partial class ItemListWindow : Window
    {
        private readonly ItemListViewModel _vm = new();
        public ItemListWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.InitAsync();
        }
    }
}
