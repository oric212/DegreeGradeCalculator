using System.Text.Json;
using DegreeGradeCalculator;
using Microsoft.Data.Sqlite;
using Xunit;

public sealed class IntegrityTests
{
    private static Backup Example() => new() { Degrees = [new Degree { Name = "Fictional", Years = [new AcademicYear {
        Semesters = [new Semester { Courses = [new Course { Name = "Term", Credits = 2, Grade = 70 }] }],
        YearlyCourses = [new Course { Name = "Annual", Credits = 4, Grade = 100 }] }] }] };
    [Fact] public void YearlyCoursesAreCountedOnceOutsideSemester()
    {
        var b = Example(); Validation.Check(b);
        var y = b.Degrees[0].Years[0];
        Assert.Equal(90m, Calculation.Summarize(Calculation.Courses(y)).Average);
        Assert.Equal(90m, Calculation.Summarize(Calculation.Courses(b.Degrees[0])).Average);
        Assert.Equal(6m, Calculation.Summarize(Calculation.Courses(y)).Credits);
        Assert.Equal(70m, Calculation.Summarize(y.Semesters[0].Courses).Average);
        var restored = JsonSerializer.Deserialize<Backup>(JsonSerializer.Serialize(b, Store.Json), Store.Json)!;
        Assert.Single(restored.Degrees[0].Years[0].YearlyCourses);
        y.YearlyCourses[0].Credits = -1;
        Assert.Throws<ArgumentException>(() => Validation.Check(b));
    }
    [Fact] public void MatchingWriteAdvancesRevisionAndStaleCopyCannotOverwrite()
    {
        WithStore(store => {
            var a = store.Read(); var b = store.Read();
            Assert.Equal(1, a.Revision);
            a.Language = "he"; Assert.Equal(2, store.Write(a));
            var conflict = Assert.Throws<RevisionConflictException>(() => store.Write(b));
            Assert.Equal(2, conflict.CurrentRevision);
            Assert.Equal("he", store.Read().Language);
            Assert.Equal(2, store.Read().Revision);
            var latest = store.Read(); latest.Degrees = Example().Degrees;
            Assert.Equal(3, store.Write(latest));
        });
    }
    [Fact] public void OldBackupRestoresAsNewStateAndInvalidUpdateIsAtomic()
    {
        WithStore(store => {
            var current = store.Read(); store.Write(current);
            var old = Example(); old.Revision = current.Revision; // caller's token, not backup metadata
            Assert.Equal(3, store.Write(old));
            Assert.Equal(3, store.Read().Revision);
            Assert.Throws<ArgumentException>(() => store.Update(3, state => {
                state.Language = "he"; state.Degrees[0].Name = ""; return 0;
            }));
            Assert.Equal("en", store.Read().Language);
            Assert.Equal("Fictional", store.Read().Degrees[0].Name);
            Assert.Equal(3, store.Read().Revision);
        });
    }
    [Fact] public void RevisionCompareAndWriteAreAtomicAcrossStoreInstances()
    {
        var directory = Path.Combine(Path.GetTempPath(), "atomic-" + Guid.NewGuid());
        try {
            var stores = new[] { new Store(directory), new Store(directory) };
            var copies = stores.Select(s => s.Read()).ToArray();
            int accepted = 0, rejected = 0;
            Parallel.For(0, 2, i => {
                try { stores[i].Write(copies[i]); Interlocked.Increment(ref accepted); }
                catch (RevisionConflictException) { Interlocked.Increment(ref rejected); }
            });
            Assert.Equal(1, accepted); Assert.Equal(1, rejected);
            Assert.Equal(2, stores[0].Read().Revision);
        } finally { SqliteConnection.ClearAllPools(); Directory.Delete(directory,true); }
    }
    [Fact] public void LegacyJsonInitializesRevisionWithoutLosingData()
    {
        var dir = Path.Combine(Path.GetTempPath(), "legacy-" + Guid.NewGuid());
        try {
            new Store(dir);
            var json = System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(Example(), Store.Json))!;
            json.AsObject().Remove("revision");
            json["degrees"]![0]!["years"]![0]!.AsObject().Remove("yearlyCourses");
            using (var db = new SqliteConnection("Data Source=" + Path.Combine(dir,"grades.db"))) {
                db.Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "UPDATE state SET json=$json";
                cmd.Parameters.AddWithValue("$json", json.ToJsonString()); cmd.ExecuteNonQuery();
            }
            var migrated = new Store(dir).Read();
            Assert.Equal(1, migrated.Revision);
            Assert.Equal("Fictional", migrated.Degrees[0].Name);
            Assert.Equal(70, Calculation.Courses(migrated.Degrees[0]).Single().Grade);
            Assert.Empty(migrated.Degrees[0].Years[0].YearlyCourses);
        } finally { SqliteConnection.ClearAllPools(); Directory.Delete(dir,true); }
    }
    private static void WithStore(Action<Store> test) {
        var dir = Path.Combine(Path.GetTempPath(), "integrity-" + Guid.NewGuid());
        try { test(new Store(dir)); } finally { SqliteConnection.ClearAllPools(); Directory.Delete(dir,true); }
    }
}
public sealed class LeaseTests
{
    private sealed class Clock : TimeProvider {
        public DateTimeOffset Now = DateTimeOffset.Parse("2030-01-01T00:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
        public void Advance(int seconds) => Now = Now.AddSeconds(seconds);
    }
    [Fact] public void RegistrationHeartbeatsAndMultipleSessionsKeepAlive()
    {
        var clock = new Clock(); var leases = New(clock);
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        Assert.True(leases.Heartbeat(a)); Assert.True(leases.Heartbeat(b));
        clock.Advance(15); leases.Heartbeat(b); leases.Close(a);
        Assert.False(leases.ShouldStop()); Assert.Equal(1, leases.Count);
        clock.Advance(19); Assert.False(leases.ShouldStop());
        leases.Heartbeat(b); clock.Advance(19); Assert.False(leases.ShouldStop());
    }
    [Fact] public void ExpirationCleansSessionsAndStopsAfterGrace()
    {
        var clock = new Clock(); var leases = New(clock);
        leases.Heartbeat(Guid.NewGuid()); clock.Advance(20);
        Assert.False(leases.ShouldStop()); Assert.Equal(0, leases.Count);
        clock.Advance(5); Assert.False(leases.ShouldStop());
        clock.Advance(1); Assert.True(leases.ShouldStop());
    }
    [Fact] public void RefreshAndReconnectCancelPendingShutdown()
    {
        var clock = new Clock(); var leases = New(clock); var id = Guid.NewGuid();
        leases.Heartbeat(id); leases.Close(id); Assert.False(leases.ShouldStop());
        clock.Advance(5); leases.Heartbeat(Guid.NewGuid());
        Assert.False(leases.ShouldStop()); clock.Advance(10); Assert.False(leases.ShouldStop());
    }
    [Fact] public void StartupAllowsBrowserToOpenButDoesNotLeaveOrphanForever()
    {
        var clock = new Clock(); var leases = New(clock);
        clock.Advance(59); Assert.False(leases.ShouldStop());
        clock.Advance(1); Assert.False(leases.ShouldStop());
        clock.Advance(6); Assert.True(leases.ShouldStop());
        Assert.False(leases.Heartbeat(Guid.NewGuid()));
    }
    private static LocalSessions New(TimeProvider clock) => new(clock,TimeSpan.FromSeconds(20),TimeSpan.FromSeconds(6),TimeSpan.FromSeconds(60));
}
