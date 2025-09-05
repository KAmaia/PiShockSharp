namespace WSSTest {
    internal class PingCommand : ICommand {
        public string ToPublishJson(CommandContext ctx) {
            return ("\"Operation\":\"PING\"");
        }
    }
}
