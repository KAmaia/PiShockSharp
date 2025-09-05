PiShock WS Console — README
Overview

This console app connects to PiShock’s WebSocket V2 broker and periodically sends commands (vibrate, shock, or beep) using a small command abstraction. It loads configuration from appsettings.json, schedules each send at a randomized interval that is biased toward shorter waits, and can optionally play a brief “warning vibrate” before sending a shock. While it runs, you can reschedule the next send on demand by pressing n, and you can exit cleanly with Ctrl+C. The exact JSON sent to the broker matches the V2 PUBLISH envelope you confirmed, including numeric types where required and lowercase ty in the label block.

Requirements

You need .NET 7 or 8 and Visual Studio 2022 or the dotnet CLI. The app expects an API key generated on or after 2024‑10‑15 to authenticate with the V2 broker. The executable must sit beside an appsettings.json; in VS, set the file’s “Copy to Output Directory” to “Copy if newer.”

Build and Run

Open the project in Visual Studio and run the console app normally. If you prefer the CLI, use the following two commands from the project directory.

dotnet build
dotnet run

At startup the app builds the V2 URL wss://broker.pishock.com/v2?Username=…&ApiKey=…, connects, prints each outgoing PUBLISH, and then prints the broker’s one‑line reply. Press n at any time to pick a new randomized delay. Press Ctrl+C to cancel the loop and close the socket gracefully.

Configuration: appsettings.json

All runtime behavior is driven by the PiShock section. Username and ApiKey are required. Channel is optional; if you leave it empty the program derives it from ClientId and Type. DeviceId is sent in the body as id and identifies the device. ClientId feeds the channel name; in many setups it equals the device id, but if yours differ, set both explicitly. Type must be api for the ops channel or sc for a share‑code channel; when you use sc, you must also provide ShareCode. UserId is optional and can be omitted. Origin is a cosmetic label shown in logs. The remaining flags and limits control shocks, warnings, and delay ranges.

{
  "PiShock": {
    "Username": "<Your UserName",
    "ApiKey": "<Your API Key>",
    "ClientId": Client ID,  //NO QUOTES!
    "DeviceId": Device ID,  //NO QUOTES!
    "UseShareCode": false,  //NO QUOTES!
    "UserID": UserID,       //NO QUOTES!
    "Origin": "<How you want this to appear in the logs>",
    "Type": "api",
    "Channel": "c{clientID}-ops",
    "AllowShocks": true,    //NO QUOTES!
    "MaxShockIntensity": 5, //NO QUOTES!
    "MaxShockDuration": 1000,  //NO QUOTES!
    "SendWarnBeforeShock": true, //NO QUOTES!
    "SendWarnRandomly": true, //NO QUOTES!
    "MaxDelaySeconds": 21600,  //NO QUOTES!  
    "MinDelaySeconds": 300    //NO QUOTES!
  }
}

The program requests typed values, so numeric fields should be numbers in JSON, and booleans should be true or false. If Channel is blank, it becomes c{ClientId}-ops when Type is api or c{ClientId}-sops-{ShareCode} when Type is sc.

What gets sent

Each command renders a V2 PUBLISH envelope that the broker accepts. A typical vibrate looks like this, where the Target channel is formed from your client id and the body id is your device id.

{"Operation":"PUBLISH","PublishCommands":[{"Target":"c18215-ops","Body":{"id":25539,"m":"v","i":100,"d":2000,"r":true,"l":{"u":38033,"ty":"api","w":false,"h":false}}}]}

The mode m is "v" for vibrate, "s" for shock, and "b" for beep. Intensities and durations are numeric; r is a boolean. The label ty is the lowercase api or sc you configured. When you enable “warn before shock,” the app sends a short warning vibrate immediately before a shock; if “warn randomly” is also enabled, it flips a coin each time to decide whether to warn.

Scheduler behavior and hotkey

The scheduler samples the next delay between your MinDelaySeconds and MaxDelaySeconds, with a curved bias toward shorter waits. While it is waiting it prints a human‑readable countdown such as “Waiting 0 hours 10 minutes and 35 seconds,” then sleeps until the scheduled time. Pressing n resamples a fresh delay and resets the countdown immediately. Pressing Ctrl+C cancels the wait, exits the loop, and closes the socket.
