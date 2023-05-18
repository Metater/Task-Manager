using System.Diagnostics;
using System.Speech.Synthesis;
using Task_Manager;

#pragma warning disable CA1416 // Validate platform compatibility
static void Beep(int frequency = 300, int duration = 1000) => Task.Run(() => Console.Beep(frequency, duration));
static Task BeepAsync(int frequency = 300, int duration = 1000) => Task.Run(() => Console.Beep(frequency, duration));

Console.WriteLine("Hello, World!");

bool isSynthEnabled = true;
SpeechSynthesizer synth = new();
synth.SetOutputToDefaultAudioDevice();

BeepAsync(250, 200)
    .ContinueWith((_) => BeepAsync(600, 200))
    .ContinueWith((_) => Beep(1200, 200));

object l = new();
List<Interval> intervals = new();
Guid currentGuid = Guid.Empty;
Stopwatch stopwatch = new();

Interval NextInterval()
{
    Interval active = intervals.Find(i => i.guid == currentGuid)!;
    int nextIndex = intervals.IndexOf(active) + 1;
    if (nextIndex >= intervals.Count)
    {
        active = intervals.First();
    }
    else
    {
        active = intervals[nextIndex];
    }

    return active;
}

Task.Run(async () =>
{
    while (true)
    {
        lock (l)
        {
            if (intervals.Count == 0)
            {
                Console.Title = "Task Manager - type help";
            }
            else
            {
                Interval? active = intervals.Find(i => i.guid == currentGuid);

                if (active == null)
                {
                    active = intervals.First();
                    stopwatch.Restart();
                }

                TimeSpan elapsion = active.TimeSpan - stopwatch.Elapsed;
                int id = 2 * (intervals.IndexOf(active) + 1);
                Interval next = NextInterval();
                int nextId = 2 * (intervals.IndexOf(next) + 1);

                if (elapsion.TotalSeconds <= 0)
                {
                    Console.WriteLine(
                        $"[{DateTime.Now}]\n" +
                        $"\t{id}. {active.Name} elapsed after {double.Round(active.TimeSpan.TotalSeconds)} seconds\n" +
                        $"\t{nextId}. {next.Name} is now running for {double.Round(next.TimeSpan.TotalSeconds)} seconds"
                    );
                    active = next;
                    stopwatch.Restart();
                    Beep();
                    if (isSynthEnabled)
                    {
                        synth.SpeakAsync($"Begin interval {active.Name}");
                    }
                }

                Console.Title = $"Task Manager - {id}. {active.Name} elapsion in {double.Round(elapsion.TotalSeconds)} seconds, {nextId}. {next.Name} will be the next interval for {double.Round(next.TimeSpan.TotalSeconds)} seconds";

                currentGuid = active.guid;
            }
        }

        await Task.Delay(100);
    }
});

while (true)
{
    string? input = Console.ReadLine();
    if (input == null) continue;
    input = input.Trim();
    string[] split = input.Split(' ', StringSplitOptions.TrimEntries);
    switch (split[0].ToLower())
    {
        case "help":
            Console.WriteLine("\tcreate <minutes> <name>: creates an interval");
            Console.WriteLine("\tlist: list all intervals");
            Console.WriteLine("\tupdate <id> <minutes>: updates an interval's length");
            Console.WriteLine("\tdelete <id>: deletes an interval");
            break;
        case "create":
            lock (l)
            {
                if (split.Length < 3)
                {
                    continue;
                }

                if (!double.TryParse(split[1], out double minutes))
                {
                    continue;
                }

                string name = string.Join(' ', split, 2, split.Length - 2);

                Interval interval = new(name)
                {
                    TimeSpan = TimeSpan.FromMinutes(minutes)
                };

                intervals.Add(interval);
            }
            break;
        case "list":
            lock (l)
            {
                for (int i = 0; i < intervals.Count; i++)
                {
                    int id = 2 * (i + 1);
                    Interval interval = intervals[i];
                    Console.WriteLine($"\t{id}. {interval.Name}: {double.Round(interval.TimeSpan.TotalMinutes, 3)} minute(s)");
                }
            }
            break;
        case "update":
            break;
        case "delete":
            break;
        case "pause":
            break;
        case "unpause":
            break;
        case "jump":
            break;
        case "next":
            break;
        case "mute":
            isSynthEnabled = false;
            break;
        case "unmute":
            isSynthEnabled = true;
            break;
    }
}
#pragma warning restore CA1416 // Validate platform compatibility