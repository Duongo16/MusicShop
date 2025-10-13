using MusicShop.WPF.ViewModels;
using System.Windows;

namespace MusicShop.WPF.Views.Category
{
    public partial class CategoryListWindow : Window
    {
        private readonly CategoryListViewModel _vm = new();
        public CategoryListWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.InitAsync();
        }
    }
}
