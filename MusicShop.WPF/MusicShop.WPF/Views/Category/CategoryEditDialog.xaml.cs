using System.Windows;

namespace MusicShop.WPF.Views.Category
{
    public partial class CategoryEditDialog : Window
    {
        public int? CategoryId { get; set; } // null = create
        public string CategoryName { get; set; } = "";
        public string? CategoryDescription { get; set; }

        public CategoryEditDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                MessageBox.Show("Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }
    }
}
