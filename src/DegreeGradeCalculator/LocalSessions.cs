namespace DegreeGradeCalculator;

// Browser leases and data revisions are deliberately independent.
public sealed class LocalSessions(TimeProvider clock, TimeSpan timeout, TimeSpan grace, TimeSpan startupGrace)
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, DateTimeOffset> sessions = [];
    private readonly DateTimeOffset started = clock.GetUtcNow();
    private DateTimeOffset? emptySince;
    private bool everConnected;
    private bool stopping;
    public bool Heartbeat(Guid id)
    {
        lock (gate)
        {
            if (stopping || id == Guid.Empty) return false;
            sessions[id] = clock.GetUtcNow(); everConnected = true; emptySince = null; return true;
        }
    }
    public void Close(Guid id) { lock (gate) { sessions.Remove(id); } }
    public int Count { get { lock (gate) { return sessions.Count; } } }
    public bool ShouldStop()
    {
        lock (gate)
        {
            if (stopping) return true;
            var now = clock.GetUtcNow();
            foreach (var id in sessions.Where(s => now - s.Value >= timeout).Select(s => s.Key).ToArray()) sessions.Remove(id);
            if (sessions.Count > 0) { emptySince = null; return false; }
            if (!everConnected && now - started < startupGrace) return false;
            emptySince ??= now;
            if (now - emptySince < grace) return false;
            stopping = true; return true;
        }
    }
}
public sealed record BrowserSession(Guid Id);
