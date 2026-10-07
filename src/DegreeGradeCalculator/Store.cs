using Microsoft.Data.Sqlite;
using System.Text.Json;
namespace DegreeGradeCalculator;

public sealed class RevisionConflictException(long currentRevision) : Exception("revision_conflict")
{
    public long CurrentRevision { get; } = currentRevision;
}
public sealed class Store
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly string connection;
    private readonly object gate = new();
    public Store(string directory)
    {
        Directory.CreateDirectory(directory);
        connection = new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "grades.db") }.ToString();
        using var db = Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS state (id INTEGER PRIMARY KEY CHECK(id=1), json TEXT NOT NULL)";
        cmd.ExecuteNonQuery();
        using var tx = db.BeginTransaction();
        using var existing = db.CreateCommand(); existing.Transaction = tx;
        existing.CommandText = "SELECT json FROM state WHERE id=1";
        var raw = existing.ExecuteScalar() as string;
        using var parsed = raw is null ? null : JsonDocument.Parse(raw);
        if (parsed is null || !parsed.RootElement.TryGetProperty("revision", out var revision) || revision.GetInt64() < 1)
            Save(db, tx, Read(db, tx)); // Initialize only empty or legacy revision metadata.
        tx.Commit();
    }
    private SqliteConnection Open() { var db = new SqliteConnection(connection); db.Open(); return db; }
    private static Backup Read(SqliteConnection db, SqliteTransaction? tx = null)
    {
        using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "SELECT json FROM state WHERE id=1";
        var data = cmd.ExecuteScalar() is string s ? JsonSerializer.Deserialize<Backup>(s, Json)! : new();
        if (data.Revision < 1) data.Revision = 1;
        return data;
    }
    private static void Save(SqliteConnection db, SqliteTransaction tx, Backup data)
    {
        using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "INSERT INTO state(id,json) VALUES(1,$json) ON CONFLICT(id) DO UPDATE SET json=excluded.json";
        cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(data, Json)); cmd.ExecuteNonQuery();
    }
    public Backup Read() { lock (gate) { using var db = Open(); return Read(db); } }
    public T Update<T>(long expectedRevision, Func<Backup, T> update)
    {
        lock (gate)
        {
            using var db = Open(); using var tx = db.BeginTransaction();
            var data = Read(db, tx);
            if (expectedRevision != data.Revision) throw new RevisionConflictException(data.Revision);
            var result = update(data);
            Validation.Check(data);
            data.Revision = checked(data.Revision + 1);
            Save(db, tx, data); tx.Commit();
            return result;
        }
    }
    public long Write(Backup data)
    {
        Validation.Check(data);
        var expected = data.Revision;
        var revision = Update(expected, current =>
        {
            current.Version = data.Version; current.Language = data.Language; current.Degrees = data.Degrees;
            return checked(current.Revision + 1);
        });
        data.Revision = revision;
        return revision;
    }
}
