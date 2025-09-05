Pishock Sharp


YOU MUST CREATE AN APPSETTINGS.JSON file in the same directory as the executable with the following format:


{
  "PiShock": {
    "Username": "<PISHOCK USERNAME>",
    "ApiKey": "<PISHOCK APIKEY>",
    "ClientId": <ClientID>,
    "DeviceId": <DeviceID>,
    "UseShareCode": false,
    "UserID": <USERID>,
    "Origin": "Your Origin Point",
    "Type": "api",
    "Channel": "c<ClientID>-ops",
    "AllowShocks": true,
    "MaxShockIntensity": 5,
    "MaxShockDuration": 1000,
    "SendWarnBeforeShock": true,
    "SendWarnRandomly": true,
    "MaxDelaySeconds": 21600,
    "MinDelaySeconds": 300
  }
}
