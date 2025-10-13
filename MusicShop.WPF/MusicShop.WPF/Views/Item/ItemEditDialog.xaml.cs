using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicShop.WPF.Views.Item
{
    public partial class ItemEditDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public Guid? Id { get; set; }

        private string _sku = "";
        public string Sku { get => _sku; set { _sku = value; OnPropertyChanged(); } }

        private string _itemName = "";
        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(); }
        }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private ItemType _itemType;
        public ItemType ItemType { get => _itemType; set { _itemType = value; OnPropertyChanged(); } }

        private ItemStatus _status;
        public ItemStatus Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private decimal _price;
        public decimal Price { get => _price; set { _price = value; OnPropertyChanged(); } }

        private decimal? _salePrice;
        public decimal? SalePrice { get => _salePrice; set { _salePrice = value; OnPropertyChanged(); } }

        private int _stockQty;
        public int StockQty { get => _stockQty; set { _stockQty = value; OnPropertyChanged(); } }

        private int _reorderLevel;
        public int ReorderLevel { get => _reorderLevel; set { _reorderLevel = value; OnPropertyChanged(); } }

        private string? _imageUrl;
        public string? ImageUrl { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }

        private BrandDetailOutDto? _selectedBrand;
        public BrandDetailOutDto? SelectedBrand
        {
            get => _selectedBrand;
            set { _selectedBrand = value; OnPropertyChanged(); }
        }

        private CategoryDetailOutDto? _selectedCategory;
        public CategoryDetailOutDto? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BrandDetailOutDto> Brands { get; } = new();
        public ObservableCollection<CategoryDetailOutDto> Categories { get; } = new();

        public IEnumerable<ItemType> ItemTypes { get; } = (ItemType[])Enum.GetValues(typeof(ItemType));
        public IEnumerable<ItemStatus> ItemStatuses { get; } = (ItemStatus[])Enum.GetValues(typeof(ItemStatus));

        public ItemEditDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ForceUpdateBindings();

            if (string.IsNullOrWhiteSpace(Sku) || string.IsNullOrWhiteSpace(ItemName))
            {
                MessageBox.Show("SKU và Name là bắt buộc.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Price < 0)
            {
                MessageBox.Show("Price phải ≥ 0.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SalePrice is < 0)
            {
                MessageBox.Show("SalePrice phải ≥ 0 (hoặc để trống).", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StockQty < 0 || ReorderLevel < 0)
            {
                MessageBox.Show("StockQty/ReorderLevel phải ≥ 0.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void ForceUpdateBindings()
        {
            var focused = FocusManager.GetFocusedElement(this) as FrameworkElement;
            if (focused != null)
            {
                if (focused is TextBox tb)
                {
                    BindingExpression? be = tb.GetBindingExpression(TextBox.TextProperty);
                    be?.UpdateSource();
                }

                if (focused is ComboBox cb)
                {
                    BindingExpression? be = cb.GetBindingExpression(Selector.SelectedItemProperty);
                    be?.UpdateSource();
                }
            }
        }
    }
}
