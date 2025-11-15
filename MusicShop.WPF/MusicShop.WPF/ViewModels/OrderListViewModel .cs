namespace MusicShop.WPF.ViewModels
{
    using MusicShop.Common.DTOs.Order;
    using MusicShop.Common.Models;
    using MusicShop.Common.Transport;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class UpdateStatusResponse
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }
    }

    public class OrderListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private readonly TcpClientHelper _tcp = new();
        private readonly string _host;
        private readonly int _port;

        public OrderListViewModel(string host = "127.0.0.1", int port = 5055)
        {
            _host = host;
            _port = port;

            SearchCommand = new RelayCommand(async _ => { Page = 1; await LoadAsync(); });
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            NextPageCommand = new RelayCommand(async _ => { if (Page * PageSize < Total) { Page++; await LoadAsync(); } });
            PrevPageCommand = new RelayCommand(async _ => { if (Page > 1) { Page--; await LoadAsync(); } });

            OrderStatuses = new ObservableCollection<OrderStatus>(
                Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>()
            );
            UpdateStatusCommand = new RelayCommand(async _ => await OnUpdateStatusAsync(null), _ => NewSelectedStatus != null);
        }

        public ObservableCollection<OrderListItemOutDTO> Orders { get; } = new();

        private string? _query;
        public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

        private int _page = 1;
        public int Page { get => _page; set { _page = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        private int _pageSize = 12;
        public int PageSize { get => _pageSize; set { _pageSize = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        private int _total;
        public int Total { get => _total; set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(PageInfo)); } }

        public string PageInfo => $"Page {Page} / {Math.Max(1, (int)Math.Ceiling((double)Total / PageSize))}  •  {Total} orders";

        private string? _statusText;
        public string? StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }


        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public ICommand UpdateStatusCommand { get; }

        public ObservableCollection<OrderStatus> OrderStatuses { get; }

        private OrderStatus _newSelectedStatus;
        public OrderStatus NewSelectedStatus
        {
            get => _newSelectedStatus;
            set { _newSelectedStatus = value; OnPropertyChanged(); }
        }

        private OrderListItemOutDTO? _selectedOrder;
        public OrderListItemOutDTO? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();

                if (_selectedOrder != null)
                {
                    NewSelectedStatus = _selectedOrder.Status;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }


        public async Task InitAsync()
        {
            StatusText = "Connecting to server...";
            var ok = await _tcp.ConnectAsync(_host, _port);
            if (!ok) { StatusText = "Server not connected"; return; }

            await LoadAsync();
            StatusText = "Ready";
        }

        public async Task LoadAsync()
        {
            if (!_tcp.IsConnected)
            {
                StatusText = "Server not connected";
                return;
            }

            try
            {
                var payload = new GetListPayload(Query, Page, PageSize);
                var res = await _tcp.SendAsync<PagedResult<OrderListItemOutDTO>>("Order.GetList", payload);

                Orders.Clear();
                foreach (var o in res?.Items ?? Array.Empty<OrderListItemOutDTO>())
                {
                    Orders.Add(o);
                }

                Total = res?.TotalCount ?? 0;
                StatusText = $"Loaded {Orders.Count} / {Total} orders";
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
            }
        }

        private async Task OnUpdateStatusAsync(object? param)
        {
            if (SelectedOrder == null)
            {
                StatusText = "No order selected.";
                return;
            }

            var requestDto = new OrderUpdateStatusRequestDTO
            {
                OrderId = SelectedOrder.Id,
                NewStatus = NewSelectedStatus
            };

            StatusText = "Updating status...";
            try
            {
                var res = await _tcp.SendAsync<UpdateStatusResponse>("Order.UpdateStatus", requestDto);

                if (res != null && res.Ok)
                {
                    StatusText = "Status updated successfully. Refreshing list...";
                    await LoadAsync();
                }
                else
                {
                    StatusText = $"Update failed: {res?.Error ?? "Unknown error"}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
        }
    }
}
