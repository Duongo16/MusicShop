using System.Net.Sockets;
using System.Text.Json;

namespace MusicShop.Common.Transport
{
    public class TcpClientHelper : IAsyncDisposable
    {
        private readonly TcpClient _client = new();
        private NetworkStream _stream = default!;

        public async Task ConnectAsync(string host, int port, CancellationToken ct = default)
        {
            if (String.IsNullOrEmpty(host)) host = TcpServerOptions.Host;
            ArgumentNullException.ThrowIfNull(host);
            if(String.IsNullOrEmpty(port.ToString())) port = TcpServerOptions.Port;
            ArgumentNullException.ThrowIfNull(port);
            await _client.ConnectAsync(host, port, ct);
            _stream = _client.GetStream();
        }

        public async Task<T?> SendAsync<T>(string op, object? payload, CancellationToken ct = default)
        {
            var req = new TcpRequest(Guid.NewGuid().ToString("N"), op, payload);
            await TcpFraming.WriteJsonAsync(_stream, req, ct);

            var json = await TcpFraming.ReadJsonAsync(_stream, ct) ?? throw new Exception("Disconnected");
            var resp = JsonSerializer.Deserialize<TcpResponse>(json, TcpFraming.Json)!;
            if (!resp.Ok) throw new Exception(resp.Error ?? "Unknown TCP error");

            var dataJson = JsonSerializer.Serialize(resp.Data, TcpFraming.Json);
            return JsonSerializer.Deserialize<T>(dataJson, TcpFraming.Json);
        }

        public async ValueTask DisposeAsync()
        {
            _stream?.Dispose();
            _client.Close();
            await Task.CompletedTask;
        }
    }
}
