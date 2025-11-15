using MusicShop.WPF.ViewModels;

namespace MusicShop.WPF.Views.Order
{
    /// <summary>
    /// Interaction logic for OrderListWindow.xaml
    /// </summary>
    public partial class OrderListWindow
    {
        private readonly OrderListViewModel _vm = new();
        public OrderListWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.InitAsync();
        }
    }
}
