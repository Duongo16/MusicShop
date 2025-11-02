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

    public static async Task WriteJsonAsync(NetworkStream s, object obj)
    {
        var json = JsonSerializer.Serialize(obj, Json);
        var bytes = Encoding.UTF8.GetBytes(json);
        var len = BitConverter.GetBytes(bytes.Length); 
        await s.WriteAsync(len.AsMemory(0, 4));
        await s.WriteAsync(bytes.AsMemory(0, bytes.Length));
        await s.FlushAsync();
    }

    public static async Task<string?> ReadJsonAsync(NetworkStream s)
    {
        var lenBuf = new byte[4];
        if (!await ReadExactlyAsync(s, lenBuf, 4)) return null;
        int len = BitConverter.ToInt32(lenBuf, 0);
        var buf = new byte[len];
        if (!await ReadExactlyAsync(s, buf, len)) return null;
        return Encoding.UTF8.GetString(buf);
    }

    static async Task<bool> ReadExactlyAsync(NetworkStream s, byte[] buf, int len)
    {
        int read = 0;
        while (read < len)
        {
            var n = await s.ReadAsync(buf.AsMemory(read, len - read));
            if (n == 0) return false;
            read += n;
        }
        return true;
    }
}
