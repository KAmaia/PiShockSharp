// Commands/Publisher.cs
using System.Net.WebSockets;
using System.Text;


namespace WSSTest;
public static class Publisher {
    public static async Task<string> SendCommand(ClientWebSocket ws, ICommand command, CommandContext ctx, CancellationToken ct = default) {
        var json = command.ToPublishJson(ctx);
        await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, ct);
        return await ReceiveOne(ws, TimeSpan.FromSeconds(10), ct);
    }

    private static async Task<string> ReceiveOne(ClientWebSocket ws, TimeSpan timeout, CancellationToken ct) {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linked.CancelAfter(timeout);
        var buf = new byte[8192];
        var sb = new StringBuilder();
        while (true) {
            var res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), linked.Token);
            if (res.MessageType == WebSocketMessageType.Close)
                return $"{{\"Close\":\"{res.CloseStatus}\",\"Reason\":\"{res.CloseStatusDescription}\"}}";
            sb.Append(Encoding.UTF8.GetString(buf, 0, res.Count));
            if (res.EndOfMessage) return sb.ToString();
        }
    }
}

