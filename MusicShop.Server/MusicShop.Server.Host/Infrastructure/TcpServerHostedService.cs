using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicShop.Common.Transport;
using MusicShop.Server.Core.Services;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace MusicShop.Server.Host.Infrastructure
{
    public class TcpServerHostedService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<TcpServerHostedService> _log;
        private readonly int _port;

        public TcpServerHostedService(IServiceProvider sp, ILogger<TcpServerHostedService> log, IConfiguration cfg)
        {
            _sp = sp; _log = log;
            _port = cfg.GetValue("Tcp:Port", 5055);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            _log.LogInformation("TCP listening on {Port}", _port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(stoppingToken);
                    _ = HandleClientAsync(client, stoppingToken);
                }
            }
            finally { listener.Stop(); }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var itemService = scope.ServiceProvider.GetRequiredService<IItemService>();
            using var stream = client.GetStream();

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    var json = await TcpFraming.ReadJsonAsync(stream, ct);
                    if (json is null) break;

                    TcpResponse resp;
                    try
                    {
                        var req = JsonSerializer.Deserialize<TcpRequest>(json, TcpFraming.Json)
                                  ?? throw new Exception("Invalid request");
                        switch (req.Op) 
                        {
                            case "Item.GetList":
                                {
                                    var p = JsonSerializer.Deserialize<GetListPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)
                                            ?? new(null, 1, 12);
                                    var data = await itemService.GetListAsync(p.Q, p.Page, p.PageSize, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            case "Item.GetById":
                                {
                                    var p = JsonSerializer.Deserialize<GetByIdPayload>(req.Payload?.ToString() ?? "{}", TcpFraming.Json)!;
                                    var data = await itemService.GetByIdAsync(p.Id, ct);
                                    resp = new(req.RequestId, true, data, null);
                                    break;
                                }
                            default:
                                resp = new(req.RequestId, false, null, $"Unknown Op: {req.Op}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        resp = new("?", false, null, ex.Message);
                    }

                    await TcpFraming.WriteJsonAsync(stream, resp, ct);
                }
            }
            catch (Exception ex) { }
            finally { client.Close(); }
        }
    }
}
