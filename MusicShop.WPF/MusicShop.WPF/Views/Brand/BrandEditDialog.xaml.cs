using System.Windows;

namespace MusicShop.WPF.Views.Brand
{
    public partial class BrandEditDialog : Window
    {
        public int? BrandId { get; set; }
        public string BrandName { get; set; } = "";

        public BrandEditDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BrandName))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
    }
}
