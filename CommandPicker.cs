namespace WSSTest {
    internal class CommandPicker {
        private bool allowShocks;
        private int maxShockValue;
        private int maxShockDuration;

        public CommandPicker(bool allowShocks, int maxShockValue, int maxShockDuration) {
            this.allowShocks = allowShocks;
            this.maxShockValue = maxShockValue;
            this.maxShockDuration = maxShockDuration;
        }

        public ICommand PickCommand() {
            ICommand command = null; 
            int chooser = Random.Shared.Next(0, 3); // Adjusted range to include all cases
            Console.WriteLine($"Chooser value: {chooser}");
            if (chooser == 0) {
                if (allowShocks) {
                    int intensity = Random.Shared.Next(1, maxShockValue + 1);
                    int duration = Random.Shared.Next(100, maxShockDuration + 1);
                    command = new ShockCommand(intensity, duration);
                }
                else {
                    int intensity = Random.Shared.Next(1, 100);
                    int duration = Random.Shared.Next(100, 2000);
                    command = new VibeCommand(intensity, duration);
                }
            }
            else if (chooser == 1) {
                int intensity = Random.Shared.Next(20, 100);
                int duration = Random.Shared.Next(100, 2000);
                command = new VibeCommand(intensity, duration, false, false);
            }
            else if (chooser == 2) {
                int duration = Random.Shared.Next(500, 3000);
                command = new BeepCommand(100, duration);
            }

            if (command == null) { // Ensure command is assigned to fix potential null reference
                command = new BeepCommand(100, 1000);
            }
            var i = command.GetIntensity();
            var d = command.GetDurationMs();
            Console.WriteLine(
                i.HasValue && d.HasValue
                    ? $"Picked command: {command.GetType().Name}, intensity {i}, duration {d}ms"
                    : $"Picked command: {command.GetType().Name}");
            return command;
        }



        internal int NextDelay(int min, int max) {
            int innerMin = min;    // 1 minute 60 
            int innerMax = max;  // 30 minutes 1800
            double u = Random.Shared.NextDouble();          // uniform [0,1)
            double skew = Math.Pow(u, 1.5);                 // >1 biases toward smaller values; tweak 1.5–4.0 to taste
            int delay = innerMin + (int)Math.Floor(skew * (innerMax - innerMin + 1)); // inclusive of both ends
            return delay;
        }
    }
    public static class CommandIntrospection {
        public static int? GetIntensity(this ICommand command) => command switch {
            VibeCommand v => v.Intensity,
            ShockCommand s => s.Intensity,
            BeepCommand b => b.Intensity, // or null if you treat beep as “no intensity”
            _ => null
        };

        public static int? GetDurationMs(this ICommand command) => command switch {
            VibeCommand v => v.DurationMs,
            ShockCommand s => s.DurationMs,
            BeepCommand b => b.DurationMs,
            _ => null
        };
    }

}
