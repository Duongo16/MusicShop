using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicShop.WPF;

public class ItemListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private string? _query;
    public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

    private int _page = 1;
    public int Page { get => _page; set { _page = value; OnPropertyChanged(); } }

    private int _pageSize = 12;
    public int PageSize { get => _pageSize; set { _pageSize = value; OnPropertyChanged(); } }

    private int _total;
    public int Total { get => _total; set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

    public string PageInfo => $"Page {Page} / {Math.Max(1, (int)Math.Ceiling((double)Total / PageSize))}  •  {Total} items";

    public ObservableCollection<ItemDetailOutDto> Items { get; } = new();

    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand RefreshCommand { get; }

    readonly TcpClientHelper _tcp = new();
    readonly string _host;
    readonly int _port;

    public ItemListViewModel(string host = "127.0.0.1", int port = 5055)
    {
        _host = host; _port = port;
        SearchCommand = new RelayCommand(async _ => { Page = 1; await LoadAsync(); });
        NextPageCommand = new RelayCommand(async _ => { if (Page * PageSize < Total) { Page++; await LoadAsync(); } });
        PrevPageCommand = new RelayCommand(async _ => { if (Page > 1) { Page--; await LoadAsync(); } });
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
    }

    public async Task InitAsync()
    {
        await _tcp.ConnectAsync(_host, _port);
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        var res = await _tcp.SendAsync<PagedResult<ItemDetailOutDto>>(
            "Item.GetList", new GetListPayload(Query, Page, PageSize));

        Items.Clear();
        foreach (var it in res?.Items ?? [])
            Items.Add(it);

        Total = res?.TotalCount ?? 0;
        OnPropertyChanged(nameof(Items));
    }
}

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _execAsync;
    private readonly Predicate<object?>? _can;
    public RelayCommand(Func<object?, Task> execAsync, Predicate<object?>? can = null)
    { _execAsync = execAsync; _can = can; }

    public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public async void Execute(object? parameter) => await (_execAsync?.Invoke(parameter) ?? Task.CompletedTask);
}
