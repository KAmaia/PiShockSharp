namespace WSSTest {
    public interface ICommand {
        string ToPublishJson(CommandContext ctx);
    }
}
