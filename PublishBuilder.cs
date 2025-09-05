// Commands/PublishBuilder.cs
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WSSTest;

internal static class PublishBuilder {
    private static readonly JsonSerializerOptions Opt = new() {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string BuildJson(string mode, int intensity, int durationMs, CommandContext ctx) {
        var env = new Envelope {
            Operation = "PUBLISH",
            PublishCommands = new[]
            {
                new PublishItem
                {
                    Target = ctx.Channel,            // e.g., "c18215-ops"
                    Body = new Body
                    {
                        id = ctx.DeviceId,           // 25539
                        m  = mode,                   // "v" | "s" | "b"
                        i  = intensity,              // 100
                        d  = durationMs,             // 2000
                        r  = true,                   // per docs, always true
                        l  = new Labels
                        {
                            u  = ctx.UserId,         // 38033 (optional; omitted if null)
                            ty = ctx.Type,           // "api" (lowercase)
                            w  = ctx.Warning,        // false
                            h  = ctx.Hold,           // false
                            o = ctx.Origin
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(env, Opt);
    }

    private sealed class Envelope {
        public string Operation { get; set; } = "";           // "PUBLISH"
        public PublishItem[] PublishCommands { get; set; } = Array.Empty<PublishItem>();
    }
    private sealed class PublishItem {
        public string Target { get; set; } = "";
        public Body Body { get; set; } = new();
    }
    private sealed class Body {
        public int id { get; set; }       // numeric
        public string m { get; set; } = "";
        public int i { get; set; }       // numeric
        public int d { get; set; }       // numeric (ms)
        public bool r { get; set; }       // boolean
        public Labels l { get; set; } = new();
    }
    private sealed class Labels {
        public int? u { get; set; }       // numeric or null (omit if null)
        public string ty { get; set; } = ""; // "api" or "sc"
        public bool w { get; set; }       // boolean
        public bool h { get; set; }       // boolean

        public string o { get; set; } = ""; // not documented; always empty string
    }
}