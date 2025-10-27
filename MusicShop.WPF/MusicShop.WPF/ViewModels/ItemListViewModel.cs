using MusicShop.Common.DTOs;
using MusicShop.Common.Models;
using MusicShop.Common.Transport;
using MusicShop.WPF.ViewModels;
using MusicShop.WPF.Views.Item;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MusicShop.WPF;

public class ItemListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private string? _query;
    public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

    private int _page = 1;
    public int Page
    {
        get => _page;
        set { _page = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); }
    }

    private int _pageSize = 12;
    public int PageSize
    {
        get => _pageSize;
        set { _pageSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); }
    }

    private int _total;
    public int Total
    {
        get => _total;
        set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); }
    }

    public string PageInfo => $"Page {Page} / {Math.Max(1, (int)Math.Ceiling((double)Total / PageSize))}  •  {Total} items";

    private string? _statusText;
    public string? StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ItemDetailOutDto> Items { get; } = new();

    public ObservableCollection<BrandDetailOutDto> Brands { get; } = new();
    public ObservableCollection<CategoryDetailOutDto> Categories { get; } = new();

    private BrandDetailOutDto? _selectedBrand;
    public BrandDetailOutDto? SelectedBrand { get => _selectedBrand; set { _selectedBrand = value; OnPropertyChanged(); } }

    private CategoryDetailOutDto? _selectedCategory;
    public CategoryDetailOutDto? SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

    public IEnumerable<ItemStatus> Statuses { get; } = Enum.GetValues(typeof(ItemStatus)).Cast<ItemStatus>();
    public IEnumerable<ItemType> ItemTypes { get; } = Enum.GetValues(typeof(ItemType)).Cast<ItemType>();

    private ItemStatus? _selectedStatus;
    public ItemStatus? SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); } }

    private ItemType? _selectedItemType;
    public ItemType? SelectedItemType { get => _selectedItemType; set { _selectedItemType = value; OnPropertyChanged(); } }

    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

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

        AddCommand = new RelayCommand(async _ => await AddAsync());
        EditCommand = new RelayCommand(async row => await EditAsync(row as ItemDetailOutDto));
        DeleteCommand = new RelayCommand(async row => await DeleteAsync(row as ItemDetailOutDto));
    }

    public async Task InitAsync()
    {
        StatusText = "Đang kết nối server...";
        var ok = await _tcp.ConnectAsync(_host, _port);
        if (!ok) { StatusText = "Server chưa được kết nối"; return; }

        StatusText = "Đang tải bộ lọc...";
        await LoadFiltersAsync();

        StatusText = "Đang tải dữ liệu...";
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
            var res = await _tcp.SendAsync<PagedResult<ItemDetailOutDto>>(
                "Item.GetList", new GetListPayload(Query, Page, PageSize));

            var rows = res?.Items ?? Array.Empty<ItemDetailOutDto>();

            if (SelectedBrand != null)
                rows = rows.Where(x => x.Brand?.Id == SelectedBrand.Id).ToArray();

            if (SelectedCategory != null)
                rows = rows.Where(x => x.Category?.Id == SelectedCategory.Id).ToArray();

            if (SelectedStatus != null)
                rows = rows.Where(x => x.Status == SelectedStatus.Value).ToArray();

            if (SelectedItemType != null)
                rows = rows.Where(x => x.ItemType == SelectedItemType.Value).ToArray();

            Items.Clear();
            foreach (var it in rows) Items.Add(it);

            Total = res?.TotalCount ?? 0;
            StatusText = $"Loaded {Items.Count} / {Total}";
        }
        catch (Exception ex)
        {
            StatusText = ex.Message;
        }
    }

    private async Task LoadFiltersAsync()
    {
        try
        {
            var brands = await _tcp.SendAsync<PagedResult<BrandDetailOutDto>>(
                "Brand.GetList", new GetListPayload(null, 1, 500));

            Brands.Clear();
            foreach (var b in brands?.Items ?? Array.Empty<BrandDetailOutDto>())
                Brands.Add(b);

            var cats = await _tcp.SendAsync<PagedResult<CategoryDetailOutDto>>(
                "Category.GetList", new GetListPayload(null, 1, 500));

            Categories.Clear();
            foreach (var c in cats?.Items ?? Array.Empty<CategoryDetailOutDto>())
                Categories.Add(c);
        }
        catch (Exception ex)
        {
            StatusText = "Không tải được filters: " + ex.Message;
        }
    }

    private async Task AddAsync()
    {
        var dlg = new ItemEditDialog();

        await FillDialogFiltersAsync(dlg);
        dlg.DataContext = dlg;

        if (dlg.ShowDialog() == true)
        {
            var p = new ItemUpsertPayload(
                null, dlg.Sku, dlg.ItemName, dlg.Description,
                dlg.ItemType, dlg.Status,
                dlg.Price, dlg.SalePrice,
                dlg.StockQty, dlg.ReorderLevel,
                dlg.ImageUrl,
                dlg.SelectedBrand?.Id, dlg.SelectedCategory?.Id
            );

            try
            {
                var newId = await _tcp.SendAsync<Guid>("Item.Create", p);
                StatusText = newId != Guid.Empty ? $"Created item {newId}" : "Create failed";
                await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; }
        }
    }

    private async Task EditAsync(ItemDetailOutDto? row)
    {
        if (row is null) return;

        var dlg = new ItemEditDialog
        {
            Id = row.Id,
            Sku = row.Sku,
            ItemName = row.Name,
            Description = row.Description,
            ItemType = row.ItemType,
            Status = row.Status,
            Price = row.Price,
            SalePrice = row.SalePrice,
            StockQty = row.StockQty,
            ReorderLevel = row.ReorderLevel,
            ImageUrl = row.ImageUrl
        };

        await FillDialogFiltersAsync(dlg);
        if (row.Brand != null)
            dlg.SelectedBrand = dlg.Brands.FirstOrDefault(b => b.Id == row.Brand.Id);
        if (row.Category != null)
            dlg.SelectedCategory = dlg.Categories.FirstOrDefault(c => c.Id == row.Category.Id);
        dlg.DataContext = dlg;
        if (dlg.ShowDialog() == true)
        {
            var p = new ItemUpsertPayload(
                dlg.Id, dlg.Sku, dlg.ItemName, dlg.Description,
                dlg.ItemType, dlg.Status,
                dlg.Price, dlg.SalePrice,
                dlg.StockQty, dlg.ReorderLevel,
                dlg.ImageUrl,
                dlg.SelectedBrand?.Id, dlg.SelectedCategory?.Id
            );

            try
            {
                var ok = await _tcp.SendAsync<bool>("Item.Update", p);
                StatusText = ok ? "Updated" : "Update failed";
                if (ok) await LoadAsync();
            }
            catch (Exception ex) { StatusText = ex.Message; }
        }
    }

    private async Task DeleteAsync(ItemDetailOutDto? row)
    {
        if (row is null) return;

        if (System.Windows.MessageBox.Show(
                $"Delete item '{row.Name}'?",
                "Confirm",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            var ok = await _tcp.SendAsync<bool>("Item.Delete", new DeleteGuidPayload(row.Id));
            StatusText = ok ? "Deleted" : "Delete failed (maybe referenced in orders/carts/ledgers)";
            if (ok) await LoadAsync();
        }
        catch (Exception ex) { StatusText = ex.Message; }
    }

    private async Task FillDialogFiltersAsync(ItemEditDialog dlg)
    {
        try
        {
            // Brand list
            var brands = await _tcp.SendAsync<PagedResult<BrandDetailOutDto>>(
                "Brand.GetList", new GetListPayload(null, 1, 500));
            dlg.Brands.Clear();
            foreach (var b in brands?.Items ?? Array.Empty<BrandDetailOutDto>())
                dlg.Brands.Add(b);

            // Category list
            var cats = await _tcp.SendAsync<PagedResult<CategoryDetailOutDto>>(
                "Category.GetList", new GetListPayload(null, 1, 500));
            dlg.Categories.Clear();
            foreach (var c in cats?.Items ?? Array.Empty<CategoryDetailOutDto>())
                dlg.Categories.Add(c);
        }
        catch (Exception ex)
        {
            StatusText = "Không tải được danh mục/nhãn hiệu cho form: " + ex.Message;
        }
    }
}


