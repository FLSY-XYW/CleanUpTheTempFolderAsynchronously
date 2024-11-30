using NLog.Targets;

namespace NLog.Shared;

public class MemoryTarget : TargetWithLayout
{
    public List<string> Logs { get; } = new List<string>();

    protected override void Write(LogEventInfo logEvent)
    {
        Logs.Add(this.Layout.Render(logEvent));
    }
}