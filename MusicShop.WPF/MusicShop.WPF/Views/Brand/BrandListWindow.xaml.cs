// WPF/Views/Brands/BrandListWindow.xaml.cs
using MusicShop.WPF.Views.Brands;
using System.Windows;

namespace MusicShop.WPF.Views.Brand
{
    public partial class BrandListWindow
    {
        private readonly BrandListViewModel _vm = new();
        public BrandListWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.InitAsync();
        }
    }
}
