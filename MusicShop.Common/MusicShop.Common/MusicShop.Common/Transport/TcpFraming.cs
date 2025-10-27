using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MusicShop.Common.Transport;

public static class TcpFraming
{
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteJsonAsync(NetworkStream s, object obj, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(obj, Json);
        var bytes = Encoding.UTF8.GetBytes(json);
        var len = BitConverter.GetBytes(bytes.Length); 
        await s.WriteAsync(len.AsMemory(0, 4), ct);
        await s.WriteAsync(bytes.AsMemory(0, bytes.Length), ct);
        await s.FlushAsync(ct);
    }

    public static async Task<string?> ReadJsonAsync(NetworkStream s, CancellationToken ct)
    {
        var lenBuf = new byte[4];
        if (!await ReadExactlyAsync(s, lenBuf, 4, ct)) return null;
        int len = BitConverter.ToInt32(lenBuf, 0);
        var buf = new byte[len];
        if (!await ReadExactlyAsync(s, buf, len, ct)) return null;
        return Encoding.UTF8.GetString(buf);
    }

    static async Task<bool> ReadExactlyAsync(NetworkStream s, byte[] buf, int len, CancellationToken ct)
    {
        int read = 0;
        while (read < len)
        {
            var n = await s.ReadAsync(buf.AsMemory(read, len - read), ct);
            if (n == 0) return false;
            read += n;
        }
        return true;
    }
}
