namespace WSSTest {
    public sealed class BeepCommand : ICommand {
        public BeepCommand(int intensity, int durationMs, bool warning = false, bool hold = false) { Intensity = intensity; DurationMs = durationMs; Warning = warning; Hold = hold; }
        public int Intensity { get; }
        public int DurationMs { get; }
        public bool Warning { get; }
        public bool Hold { get; }
        public string ToPublishJson(CommandContext ctx)
            => PublishBuilder.BuildJson("b", Intensity, DurationMs,
                new CommandContext(ctx.Channel, ctx.DeviceId, ctx.Type, ctx.UserId, Warning, Hold, ctx.Origin));
    }
}


