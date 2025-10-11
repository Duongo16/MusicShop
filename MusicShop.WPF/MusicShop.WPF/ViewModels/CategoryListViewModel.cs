
using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicShop.WPF.ViewModels;
public class CategoryListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

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

    public string? StatusText { get; set; }

    public ICommand SearchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task InitAsync()
    {
        var ok = await _tcp.ConnectAsync(_host, _port);
        if (!ok) { StatusText = "Server chưa được kết nối"; OnPropertyChanged(nameof(StatusText)); return; }
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            var res = await _tcp.SendAsync<PagedResult<CategoryDetailOutDto>>(
                "Category.GetList", new CategoryGetListPayload(Query, Page, PageSize));

            Items.Clear();
            foreach (var c in res?.Items ?? []) Items.Add(c);
            Total = res?.TotalCount ?? 0;
        }
        catch (Exception ex)
        {
            StatusText = ex.Message; OnPropertyChanged(nameof(StatusText));
        }
    }

    private async Task AddAsync()
    {
        var dlg = new CategoryEditDialog(); // rỗng = create
        if (dlg.ShowDialog() == true)
        {
            var p = new CategoryUpsertPayload(null, dlg.CategoryName, dlg.CategoryDescription);
            try
            {
                var ok = await _tcp.SendAsync<bool>("Category.Create", p);
                if (ok == true) await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; OnPropertyChanged(nameof(StatusText)); }
        }
    }

    private async Task EditAsync(CategoryDetailOutDto? row)
    {
        if (row is null) return;
        var dlg = new CategoryEditDialog
        {
            CategoryId = row.Id,
            CategoryName = row.Name,
            CategoryDescription = row.Description
        };
        if (dlg.ShowDialog() == true)
        {
            var p = new CategoryUpsertPayload(row.Id, dlg.CategoryName, dlg.CategoryDescription);
            try
            {
                var ok = await _tcp.SendAsync<bool>("Category.Update", p);
                if (ok == true) await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; OnPropertyChanged(nameof(StatusText)); }
        }
    }

    private async Task DeleteAsync(CategoryDetailOutDto? row)
    {
        if (row is null) return;
        if (System.Windows.MessageBox.Show($"Delete category '{row.Name}'?", "Confirm", System.Windows.MessageBoxButton.YesNo,
                                           System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var ok = await _tcp.SendAsync<bool>("Category.Delete", new CategoryDeletePayload(row.Id));
            if (ok == true) await LoadAsync();
        }
        catch (Exception ex) { StatusText = ex.Message; OnPropertyChanged(nameof(StatusText)); }
    }
}

// RelayCommand tối giản
public class RelayCommand : ICommand
{
    private readonly Func<object?, Task> _exec; private readonly Predicate<object?>? _can;
    public RelayCommand(Func<object?, Task> exec, Predicate<object?>? can = null) { _exec = exec; _can = can; }
    public bool CanExecute(object? p) => _can?.Invoke(p) ?? true;
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public async void Execute(object? p) => await _exec(p);
}
