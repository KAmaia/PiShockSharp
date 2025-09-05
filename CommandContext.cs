namespace WSSTest {
    // Commands/CommandContext.cs
    public sealed class CommandContext {
        public CommandContext(string channel, int deviceId, string type, int userId, bool warning, bool hold, string origin) {
            Channel = channel;          // e.g., "c18215-ops"
            DeviceId = deviceId;        // shocker.id (25539)
            Type = type.ToLowerInvariant(); // "api" or "sc"
            UserId = userId;            // e.g., 38033 (optional)
            Warning = warning;          // w
            Hold = hold;                // h
            Origin = origin;
        }
        public string Channel { get; }
        public int DeviceId { get; }
        public string Type { get; }
        public int UserId { get; }
        public bool Warning { get; }
        public bool Hold { get; }
        public string Origin { get; }
    }
}