using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace DegreeGradeCalculator.TextImport;

public sealed class ReviewedCourse
{
    public string Name { get; set; } = "";
    public decimal? Credits { get; set; }
    public decimal? Grade { get; set; }
    public bool? Passed { get; set; }
    public int? Year { get; set; }
    public string? Semester { get; set; }
    public bool Included { get; set; } = true;
}
public sealed record CourseImportRequest(Guid DegreeId, int DefaultYear, string DefaultSemester,
    List<ReviewedCourse> Rows, string DuplicatePolicy = "update", bool Confirmed = false, string? ReviewToken = null, long? Revision = null);
public sealed record CourseUpdate(string Name, int Year, string Semester, decimal OldCredits, decimal NewCredits,
    decimal? OldGrade, decimal? NewGrade, bool? OldPassed, bool? NewPassed, bool ReplacesComponents, int RemovedDuplicates);
public sealed record ImportPreview(int Count, int SkippedDuplicates, List<string> DuplicateNames, string NormalizedText,
    int Added, int Updated, int Unchanged, List<CourseUpdate> Updates, List<string> RepeatedRows, string ReviewToken, long Revision = 0);
public static class CourseTextImport
{
    public static ImportPreview Preview(Backup data, CourseImportRequest request) => Prepare(data, request).Preview;
    public static ImportPreview Apply(Backup data, CourseImportRequest request)
    {
        if (!request.Confirmed) throw new ArgumentException("confirmation-required");
        var prepared = Prepare(data, request);
        if (request.ReviewToken is not null && request.ReviewToken != prepared.Preview.ReviewToken) throw new ArgumentException("import-changed");
        if (prepared.Preview.Count > 0) data.Degrees.Single(d => d.Id == request.DegreeId).Years = prepared.Staged.Years;
        return prepared.Preview;
    }
    private static string NameKey(string name) => Regex.Replace(name.Trim(), @"\s+", " ").ToUpperInvariant();
    private static (ImportPreview Preview, Degree Staged) Prepare(Backup data, CourseImportRequest r)
    {
        if (r.Rows is null || r.Rows.Count is < 1 or > 1000 || r.DuplicatePolicy is not ("skip" or "update" or "import")) throw new ArgumentException("invalid-import");
        var original = data.Degrees.SingleOrDefault(d => d.Id == r.DegreeId) ?? throw new ArgumentException("missing-degree");
        var serialized = JsonSerializer.Serialize(original, Store.Json);
        var token = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(serialized)));
        var degree = JsonSerializer.Deserialize<Degree>(serialized, Store.Json)!;
        var rows = new Dictionary<(Guid, string), (Semester Semester, ReviewedCourse Row, int Year)>();
        var repeats = new List<string>();
        foreach (var row in r.Rows)
        {
            if (row is null) throw new ArgumentException("invalid-import");
            if (!row.Included) continue;
            if (string.IsNullOrWhiteSpace(row.Name) || row.Name.Length > 200) throw new ArgumentException("invalid-name");
            if (row.Credits is null or <= 0 or > 1000) throw new ArgumentException("invalid-credits");
            if (row.Grade is < 0 or > 100 || (row.Passed.HasValue && row.Grade.HasValue)) throw new ArgumentException("invalid-grade");
            var year = row.Year ?? r.DefaultYear;
            if (year < 1 || year > 100) throw new ArgumentException("invalid-destination");
            while (degree.Years.Count < year) degree.Years.Add(new AcademicYear { Semesters = [new Semester { Name = "A" }, new Semester { Name = "B" }] });
            var rawSemester = row.Semester ?? r.DefaultSemester;
            var semesterName = TextCourseParser.Semester(rawSemester) ?? rawSemester;
            var semester = degree.Years[year - 1].Semesters.FirstOrDefault(s => (TextCourseParser.Semester(s.Name) ?? s.Name) == semesterName);
            if (semester is null)
            {
                if (semesterName is not ("A" or "B" or "Summer")) throw new ArgumentException("invalid-destination");
                semester = new Semester { Name = semesterName }; degree.Years[year - 1].Semesters.Add(semester);
            }
            var key = (semester.Id, NameKey(row.Name));
            if (rows.ContainsKey(key)) repeats.Add(row.Name.Trim());
            rows[key] = (semester, row, year); // Last included occurrence is the reviewed value.
        }
        int added = 0, updated = 0, unchanged = 0, skipped = 0;
        var duplicates = new List<string>(); var updates = new List<CourseUpdate>(); var normalized = new List<string>();
        foreach (var (semester, row, year) in rows.Values)
        {
            var matches = semester.Courses.Where(c => NameKey(c.Name) == NameKey(row.Name)).ToList();
            var existing = matches.LastOrDefault();
            if (existing is not null)
            {
                duplicates.Add(existing.Name);
                if (r.DuplicatePolicy == "skip") { skipped++; continue; }
                var sameGrade = Calculation.Final(existing) == row.Grade && existing.Passed == row.Passed;
                if (sameGrade && existing.Credits == row.Credits && matches.Count == 1) unchanged++;
                else
                {
                    updates.Add(new(existing.Name, year, semester.Name, existing.Credits, row.Credits!.Value,
                        Calculation.Final(existing), row.Grade, existing.Passed, row.Passed,
                        existing.UsesComponents && !sameGrade, matches.Count - 1));
                    existing.Credits = row.Credits!.Value;
                    if (!sameGrade) { existing.Grade = row.Grade; existing.Passed = row.Passed; existing.UsesComponents = false; existing.Components = []; }
                    foreach (var duplicate in matches.Where(c => c.Id != existing.Id)) semester.Courses.Remove(duplicate);
                    updated++;
                }
            }
            else
            {
                semester.Courses.Add(new Course { Name = row.Name.Trim(), Credits = row.Credits!.Value, Grade = row.Grade, Passed = row.Passed });
                added++;
            }
            string Cell(string value) => value.Contains('|') || value.Contains('"') || value.Contains('\n') || value.Contains('\r') ? '"' + value.Replace("\"", "\"\"") + '"' : value;
            normalized.Add(string.Join(" | ", Cell(row.Name.Trim()), row.Credits!.Value.ToString(CultureInfo.InvariantCulture),
                row.Passed.HasValue ? (row.Passed.Value ? "Completed" : "Failed") : row.Grade?.ToString(CultureInfo.InvariantCulture) ?? "",
                $"Year {year}", Cell(semester.Name is "A" or "B" ? $"Semester {semester.Name}" : semester.Name == "Summer" ? "Summer" : semester.Name)));
        }
        return (new(added + updated, skipped, duplicates, string.Join('\n', normalized), added, updated, unchanged, updates, repeats.Distinct().ToList(), token), degree);
    }
}
