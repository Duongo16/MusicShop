using System.Net.Sockets;
using System.Text.Json;

namespace MusicShop.Common.Transport
{
    public class TcpClientHelper : IAsyncDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        public string Host { get; private set; } = TcpServerOptions.Host;
        public int Port { get; private set; } = TcpServerOptions.Port;

        public bool IsConnected => _client != null && _client.Connected && _stream != null;

        public async Task<bool> ConnectAsync(string? host = null, int? port = null)
        {
            try
            {
                Host = string.IsNullOrWhiteSpace(host) ? Host : host!;
                Port = (port is null or <= 0) ? Port : port.Value;

                _client = new TcpClient();
                await _client.ConnectAsync(Host, Port);
                _stream = _client.GetStream();
                return true;
            }
            catch
            {
                _client = null;
                _stream = null;
                return false;
            }
        }

        public async Task<T?> SendAsync<T>(string op, object? payload, CancellationToken ct)
        {
            if (_stream == null || _client == null || !_client.Connected)
            {
                var ok = await ConnectAsync();
                if (!ok) throw new Exception("Không thể kết nối đến server TCP.");
            }

            var req = new TcpRequest(Guid.NewGuid().ToString("N"), op, payload);

            await TcpFraming.WriteJsonAsync(_stream!, req, ct);
            var json = await TcpFraming.ReadJsonAsync(_stream!, ct) ?? throw new Exception("Mất kết nối đến server.");

            var resp = JsonSerializer.Deserialize<TcpResponse>(json, TcpFraming.Json)!;
            if (!resp.Ok) throw new Exception(resp.Error ?? "Lỗi không xác định.");

            var dataJson = JsonSerializer.Serialize(resp.Data, TcpFraming.Json);
            return JsonSerializer.Deserialize<T>(dataJson, TcpFraming.Json);
        }

        public async ValueTask DisposeAsync()
        {
            try { _stream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }
            await Task.CompletedTask;
        }
    }
}
