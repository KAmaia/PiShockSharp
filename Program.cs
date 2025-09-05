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


        var uri = new Uri($"wss://broker.pishock.com/v2?Username={Uri.EscapeDataString(username)}&ApiKey={Uri.EscapeDataString(apiKey)}");
        CommandPicker picker = new CommandPicker(allowShocks, maxShockIntensity, maxShockDuration);

        CommandContext ctx = new CommandContext(channel, deviceId, type, userID, false, false, origin);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) {
            Console.Error.WriteLine("Missing PiShock:Username or PiShock:ApiKey in appsettings.json.");
            return;
        }


        if (!allowShocks) {
            Console.WriteLine("Shocks are disabled in appsettings.json (PiShock:AllowShocks = false).");
            return;
        }


        using var ws = new System.Net.WebSockets.ClientWebSocket();
        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        await ws.ConnectAsync(uri, CancellationToken.None);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        var hotkeyCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
        bool rescheduleRequested = false;

        var hotkeyTask = Task.Run(async () => {
            Console.WriteLine("Press 'n' to choose a new random delay.");
            try {
                while (!hotkeyCts.Token.IsCancellationRequested) {
                    if (Console.KeyAvailable) {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.N) {
                            rescheduleRequested = true;
                        }
                        else {
                            await Task.Delay(50, hotkeyCts.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }, hotkeyCts.Token);

        var nextAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(picker.NextDelay(minDelaySeconds, maxDelaySeconds));
        while (!cts.IsCancellationRequested) {

            if (rescheduleRequested) {
                Console.WriteLine("Rescheduling...");
                rescheduleRequested = false;
                nextAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(picker.NextDelay(minDelaySeconds, maxDelaySeconds));
                var ts = nextAt - DateTimeOffset.UtcNow;
                Console.WriteLine(
                    $"Rescheduled: {ts.Hours:0} {(ts.Hours == 1 ? "hour" : "hours")} " +
                    $"{ts.Minutes:0} {(ts.Minutes == 1 ? "minute" : "minutes")} and " +
                    $"{ts.Seconds:0} {(ts.Seconds == 1 ? "second" : "seconds")}");
            }

            var wait = nextAt - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero) {
                Console.WriteLine($"Waiting {wait.Hours}{(wait.Hours == 1 ? " hour " : " hours ")}"
                    + $"{wait.Minutes}{(wait.Minutes == 1 ? " minute " : " minutes ")} and "
                    + $"{wait.Seconds}{(wait.Seconds == 1 ? " second " : " seconds")}");

                var chunk = TimeSpan.FromMilliseconds(250);
                try {
                    // loop in small chunks so we can notice 'n' promptly
                    while ((nextAt - DateTimeOffset.UtcNow) > TimeSpan.Zero && !rescheduleRequested)
                        await Task.Delay(chunk, cts.Token);
                }
                catch (TaskCanceledException) { break; }

                continue;

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
                //warning beeps enabled?
                bool doWarn = !sendWarnRandomly || Random.Shared.Next(2) == 0; // 50% if random, always true if not
                if (doWarn) {
                    ICommand warnCommand = new VibeCommand(20, 1000, warning: true, hold: false);
                    await Publisher.SendCommand(ws, warnCommand, ctx);
                    await Task.Delay(2000);
                }
            }


            var reply = await Publisher.SendCommand(ws, command, ctx);
            Console.WriteLine("\n===========\n" + reply + "\n===========\n");
            nextAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(picker.NextDelay(minDelaySeconds, maxDelaySeconds));

            if (ws.State == WebSocketState.Open)
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
    }
    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e) {
        throw new NotImplementedException();
    }
}