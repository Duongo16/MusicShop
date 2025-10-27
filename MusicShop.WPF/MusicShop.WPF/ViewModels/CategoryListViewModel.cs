using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using MusicShop.WPF.Views.Category;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicShop.WPF.ViewModels;

public class CategoryListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly TcpClientHelper _tcp = new();
    private readonly string _host;
    private readonly int _port;

    public CategoryListViewModel(string host = "127.0.0.1", int port = 5055)
    {
        _host = host; _port = port;

        SearchCommand = new RelayCommand(async _ => { Page = 1; await LoadAsync(); });
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        NextPageCommand = new RelayCommand(async _ => { if (Page * PageSize < Total) { Page++; await LoadAsync(); } });
        PrevPageCommand = new RelayCommand(async _ => { if (Page > 1) { Page--; await LoadAsync(); } });

        AddCommand = new RelayCommand(async _ => await AddAsync());
        EditCommand = new RelayCommand(async row => await EditAsync(row as CategoryDetailOutDto));
        DeleteCommand = new RelayCommand(async row => await DeleteAsync(row as CategoryDetailOutDto));
    }

    public ObservableCollection<CategoryDetailOutDto> Items { get; } = new();

    private string? _query;
    public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

    private int _page = 1;
    public int Page { get => _page; set { _page = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

    private int _pageSize = 12;
    public int PageSize { get => _pageSize; set { _pageSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

    private int _total;
    public int Total { get => _total; set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

    public string PageInfo => $"Page {Page} / {Math.Max(1, (int)Math.Ceiling((double)Total / PageSize))}  •  {Total} categories";

    private string? _statusText;
    public string? StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task InitAsync()
    {
        StatusText = "Đang kết nối server...";
        var ok = await _tcp.ConnectAsync(_host, _port);
        if (!ok) { StatusText = "Server chưa được kết nối"; return; }

        await LoadAsync();
        StatusText = "Sẵn sàng.";
    }

    public async Task LoadAsync()
    {
        if (!_tcp.IsConnected)
        {
            StatusText = "Server chưa được kết nối";
            return;
        }

        try
        {
            var res = await _tcp.SendAsync<PagedResult<CategoryDetailOutDto>>(
                "Category.GetList", new GetListPayload(Query, Page, PageSize));

            Items.Clear();
            foreach (var c in res?.Items ?? []) Items.Add(c);
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
        var dlg = new CategoryEditDialog(); 
        if (dlg.ShowDialog() == true)
        {
            var p = new CategoryUpsertPayload(null, dlg.CategoryName);
            try
            {
                var newId = await _tcp.SendAsync<int>("Category.Create", p);
                StatusText = newId > 0 ? $"Created category #{newId}" : "Create failed";
                await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; }
        }
    }

    private async Task EditAsync(CategoryDetailOutDto? row)
    {
        if (row is null) return;

        var dlg = new CategoryEditDialog
        {
            CategoryId = row.Id,
            CategoryName = row.Name,
        };

        if (dlg.ShowDialog() == true)
        {
            var p = new CategoryUpsertPayload(row.Id, dlg.CategoryName);
            try
            {
                var ok = await _tcp.SendAsync<bool>("Category.Update", p);
                StatusText = ok ? "Updated" : "Update failed";
                if (ok) await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; }
        }
    }

    private async Task DeleteAsync(CategoryDetailOutDto? row)
    {
        if (row is null) return;

        if (System.Windows.MessageBox.Show(
                $"Delete category '{row.Name}'?",
                "Confirm",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            var ok = await _tcp.SendAsync<bool>("Category.Delete", new DeletePayload(row.Id));
            StatusText = ok ? "Deleted" : "Delete failed (maybe category has items)";
            if (ok) await LoadAsync();
        }
        catch (Exception ex) { StatusText = ex.Message; }
    }
}

