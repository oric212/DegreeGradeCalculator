using Microsoft.Data.Sqlite;
using System.Text.Json;
namespace DegreeGradeCalculator;

public sealed class Store
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly string connection; private readonly object gate = new();
    public Store(string directory)
    {
        Directory.CreateDirectory(directory); connection = new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "grades.db") }.ToString();
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "CREATE TABLE IF NOT EXISTS state (id INTEGER PRIMARY KEY CHECK(id=1), json TEXT NOT NULL)"; cmd.ExecuteNonQuery();
    }
    private SqliteConnection Open() { var db = new SqliteConnection(connection); db.Open(); return db; }
    public Backup Read() { lock (gate) { using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT json FROM state WHERE id=1"; return cmd.ExecuteScalar() is string s ? JsonSerializer.Deserialize<Backup>(s, Json)! : new(); } }
    public void Write(Backup data) { Validation.Check(data); lock (gate) { using var db = Open(); using var tx = db.BeginTransaction(); using var cmd = db.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = "INSERT INTO state(id,json) VALUES(1,$json) ON CONFLICT(id) DO UPDATE SET json=excluded.json"; cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(data, Json)); cmd.ExecuteNonQuery(); tx.Commit(); } }
}
