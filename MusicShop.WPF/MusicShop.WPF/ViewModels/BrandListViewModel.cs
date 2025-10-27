using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using MusicShop.WPF.ViewModels;
using MusicShop.WPF.Views.Brand;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicShop.WPF.Views.Brands
{
    public class BrandListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private readonly TcpClientHelper _tcp = new();
        private readonly string _host;
        private readonly int _port;

        public BrandListViewModel(string host = "127.0.0.1", int port = 5055)
        {
            _host = host; _port = port;
            SearchCommand = new RelayCommand(async _ => { Page = 1; await LoadAsync(); });
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            NextPageCommand = new RelayCommand(async _ => { if (Page * PageSize < Total) { Page++; await LoadAsync(); } });
            PrevPageCommand = new RelayCommand(async _ => { if (Page > 1) { Page--; await LoadAsync(); } });

            AddCommand = new RelayCommand(async _ => await AddAsync());
            EditCommand = new RelayCommand(async row => await EditAsync(row as BrandDetailOutDto));
            DeleteCommand = new RelayCommand(async row => await DeleteAsync(row as BrandDetailOutDto));
        }

        public ObservableCollection<BrandDetailOutDto> Items { get; } = new();

        private string? _query;
        public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

        private int _page = 1;
        public int Page { get => _page; set { _page = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        private int _pageSize = 12;
        public int PageSize { get => _pageSize; set { _pageSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        private int _total;
        public int Total { get => _total; set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        private string? _statusText;
        public string? StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        public string PageInfo => $"Page {Page} / {Math.Max(1, (int)Math.Ceiling((double)Total / PageSize))}  •  {Total} brands";

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public async Task InitAsync()
        {
            StatusText = "Connecting...";
            var ok = await _tcp.ConnectAsync(_host, _port);
            if (!ok) { StatusText = "Server chưa được kết nối"; return; }

            await LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var res = await _tcp.SendAsync<PagedResult<BrandDetailOutDto>>(
                    "Brand.GetList", new GetListPayload(Query, Page, PageSize));

                Items.Clear();
                foreach (var b in res?.Items ?? []) Items.Add(b);
                Total = res?.TotalCount ?? 0;
                StatusText = $"Loaded {Items.Count} / {Total}";
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
            }
        }

        private async Task AddAsync()
        {
            var dlg = new BrandEditDialog();
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var newId = await _tcp.SendAsync<int>("Brand.Create", new BrandUpsertPayload(null, dlg.BrandName));
                    StatusText = newId > 0 ? $"Created brand #{newId}" : "Create failed";
                    await LoadAsync();
                }
                catch (Exception ex) { StatusText = ex.Message; }
            }
        }

        private async Task EditAsync(BrandDetailOutDto? row)
        {
            if (row is null) return;
            var dlg = new BrandEditDialog { BrandId = row.Id, BrandName = row.Name };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var ok = await _tcp.SendAsync<bool>("Brand.Update", new BrandUpsertPayload(row.Id, dlg.BrandName));
                    StatusText = ok ? "Updated" : "Update failed";
                    await LoadAsync();
                }
                catch (Exception ex) { StatusText = ex.Message; }
            }
        }

        private async Task DeleteAsync(BrandDetailOutDto? row)
        {
            if (row is null) return;
            if (System.Windows.MessageBox.Show($"Delete brand '{row.Name}'?", "Confirm",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                var ok = await _tcp.SendAsync<bool>("Brand.Delete", new DeletePayload(row.Id));
                StatusText = ok ? "Deleted" : "Delete failed (maybe brand has items)";
                await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; }
        }
    }
}
