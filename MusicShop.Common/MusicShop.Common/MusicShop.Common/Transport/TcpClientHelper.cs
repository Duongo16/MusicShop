using System.Net.Sockets;
using System.Text.Json;

namespace MusicShop.Common.Transport
{
    public class TcpClientHelper : IAsyncDisposable
    {
        private readonly TcpClient _client = new();
        private NetworkStream _stream = default!;

        public bool IsConnected => _client.Connected && _stream != null;
        public string Host { get; private set; } = TcpServerOptions.Host;
        public int Port { get; private set; } = TcpServerOptions.Port;

        private void SafeClose()
        {
            try { _client.Close(); } catch {  }
        }

        public async Task<bool> ConnectAsync(string? host = null, int? port = null, CancellationToken ct = default)
        {
            Host = string.IsNullOrWhiteSpace(host) ? Host : host!;
            Port = (port is null or <= 0) ? Port : port.Value;

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                await _client.ConnectAsync(Host, Port, cts.Token).ConfigureAwait(false);
                _stream = _client.GetStream();

                _client.NoDelay = true;
                _client.ReceiveTimeout = 15000;
                _client.SendTimeout = 15000;

                return true;
            }
            catch (OperationCanceledException)
            {
                _stream = null;
                SafeClose();
                return false;
            }
            catch (SocketException)
            {
                _stream = null;
                SafeClose();
                return false;
            }
            catch
            {
                _stream = null;
                SafeClose();
                return false;
            }
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
