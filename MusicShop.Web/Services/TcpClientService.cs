using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MusicShop.Web.Services
{
    public sealed class TcpClientService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public TcpClientService(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task<JsonElement> SendRequestAsync(string command, object? data = null, CancellationToken ct = default)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port, ct);
            using var stream = client.GetStream();

            var reqObj = new
            {
                command,
                correlationId = Guid.NewGuid().ToString(),
                data = data ?? new { }
            };

            var json = JsonSerializer.Serialize(reqObj, _jsonOptions);
            var payload = Encoding.UTF8.GetBytes(json);

            var header = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(), payload.Length);

            await stream.WriteAsync(header, 0, header.Length, ct);
            await stream.WriteAsync(payload, 0, payload.Length, ct);
            await stream.FlushAsync(ct);

            client.Client.Shutdown(SocketShutdown.Send);

            var headerBuf = new byte[4];
            await ReadExactlyAsync(stream, headerBuf, 0, 4, ct);
            int len = BinaryPrimitives.ReadInt32BigEndian(headerBuf);

            var body = new byte[len];
            await ReadExactlyAsync(stream, body, 0, len, ct);

            var respJson = Encoding.UTF8.GetString(body);
            var doc = JsonDocument.Parse(respJson);
            return doc.RootElement.Clone();
        }


        private static async Task<bool> ReadExactlyAsync(NetworkStream s, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int total = 0;

            while (total < count)
            {
                int n = 0;
                try
                {
                    n = await s.ReadAsync(buffer.AsMemory(offset + total, count - total), ct);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"⚠️ IO Error while reading: {ex.Message}");
                    return false;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("⚠️ Stream has been closed before reading completed.");
                    return false;
                }

                if (n == 0)
                {
                    Console.WriteLine($"⚠️ Remote closed connection before reading {count - total} remaining bytes.");
                    return false; 
                }

                total += n;
            }

            return true; 
        }
    }
}
