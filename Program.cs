// Program.cs
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using WSSTest;

static class Program {
    public static async Task Main() {
        // Load settings from appsettings.json (must be copied to output)
        var appsettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var username = config.GetValue<string>("PiShock:Username");
        var apiKey = config.GetValue<string>("PiShock:ApiKey");
        var deviceId = config.GetValue<int>("PiShock:DeviceId");
        var clientId = config.GetValue<string>("PiShock:ClientId");
        var channel = config.GetValue<string>("PiShock:Channel");
        var origin = config.GetValue<string>("PiShock:Origin");
        var type = config.GetValue<string>("PiShock:Type");
        var userID = config.GetValue<int>("PiShock:UserId");
        var allowShocks = config.GetValue<bool>("PiShock:AllowShocks");
        var maxShockIntensity = config.GetValue<int>("PiShock:MaxShockIntensity");
        var maxShockDuration = config.GetValue<int>("PiShock:MaxShockDuration");
        var sendWarnBeforeShock = config.GetValue<bool>("PiShock:SendWarnBeforeShock");
        var sendWarnRandomly = config.GetValue<bool>("PiShock:SendWarnRandomly");
        var maxDelaySeconds = config.GetValue<int>("PiShock:MaxDelaySeconds");
        var minDelaySeconds = config.GetValue<int>("PiShock:MinDelaySeconds");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) {
            Console.Error.WriteLine("Missing PiShock:Username or PiShock:ApiKey in appsettings.json.");
            return;
        }

        if (!allowShocks) {
            Console.WriteLine("Shocks are disabled in appsettings.json (PiShock:AllowShocks = false).");
            return;
        }

        var uri = new Uri($"wss://broker.pishock.com/v2?Username={Uri.EscapeDataString(username)}&ApiKey={Uri.EscapeDataString(apiKey)}");
        var picker = new CommandPicker(allowShocks, maxShockIntensity, maxShockDuration);
        var ctx = new CommandContext(channel, deviceId, type, userID, false, false, origin);

        using var ws = new ClientWebSocket();
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        await ws.ConnectAsync(uri, CancellationToken.None);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        var nextAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(picker.NextDelay(minDelaySeconds, maxDelaySeconds));

        while (!cts.IsCancellationRequested) {
            var wait = nextAt - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero) {
                Console.WriteLine(
                    $"{DateTime.Now}: Waiting {wait.Hours}{(wait.Hours == 1 ? " hour " : " hours ")}" +
                    $"{wait.Minutes}{(wait.Minutes == 1 ? " minute " : " minutes ")} and " +
                    $"{wait.Seconds}{(wait.Seconds == 1 ? " second " : " seconds")}");

                try {
                    await Task.Delay(wait, cts.Token);
                }
                catch (TaskCanceledException) {
                    Console.WriteLine("Operation canceled.");
                    break;
                }

                continue;
            }

            ICommand command = picker.PickCommand();

            if (command is ShockCommand && sendWarnBeforeShock) {
                bool doWarn = !sendWarnRandomly || Random.Shared.Next(2) == 0; // 50% if random, otherwise always warn
                if (doWarn) {
                    var warnCommand = new VibeCommand(20, 1000, warning: true, hold: false);
                    await Publisher.SendCommand(ws, warnCommand, ctx, cts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
                }
            }

            var reply = await Publisher.SendCommand(ws, command, ctx, cts.Token);
            Console.WriteLine("\n===========\n" + reply + "\n===========\n");

            nextAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(picker.NextDelay(minDelaySeconds, maxDelaySeconds));
        }

        if (ws.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }
}
