using System.Globalization;
namespace DegreeGradeCalculator.TextImport;

public sealed class ReviewedCourse
{
    public string Name { get; set; } = "";
    public decimal? Credits { get; set; }
    public decimal? Grade { get; set; }
    public int? Year { get; set; }
    public string? Semester { get; set; }
    public bool Included { get; set; } = true;
}
public sealed record CourseImportRequest(Guid DegreeId, int DefaultYear, string DefaultSemester,
    List<ReviewedCourse> Rows, string DuplicatePolicy = "skip", bool Confirmed = false);
public sealed record ImportPreview(int Count, int SkippedDuplicates, List<string> DuplicateNames, string NormalizedText);
public static class CourseTextImport
{
    public static ImportPreview Preview(Backup data, CourseImportRequest request) => Prepare(data, request).Preview;
    public static ImportPreview Apply(Backup data, CourseImportRequest request)
    {
        if (!request.Confirmed) throw new ArgumentException("confirmation-required");
        var prepared = Prepare(data, request);
        foreach (var (semester, course) in prepared.Pending) semester.Courses.Add(course);
        return prepared.Preview;
    }
    private static (ImportPreview Preview, List<(Semester Semester, Course Course)> Pending) Prepare(Backup data, CourseImportRequest r)
    {
        if (r.Rows is null || r.Rows.Count is < 1 or > 1000 || r.DuplicatePolicy is not ("skip" or "import")) throw new ArgumentException("invalid-import");
        var degree = data.Degrees.SingleOrDefault(d => d.Id == r.DegreeId) ?? throw new ArgumentException("missing-degree");
        var pending = new List<(Semester, Course)>(); var duplicates = new List<string>(); var normalized = new List<string>(); int skipped = 0;
        var names = degree.Years.SelectMany(y => y.Semesters).ToDictionary(s => s.Id, s => s.Courses.Select(c => c.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase));
        foreach (var row in r.Rows)
        {
            if (row is null) throw new ArgumentException("invalid-import");
            if (!row.Included) continue;
            var year = row.Year ?? r.DefaultYear;
            if (year < 1 || year > degree.Years.Count) throw new ArgumentException("invalid-destination");
            var semesterName = row.Semester ?? r.DefaultSemester;
            var semester = degree.Years[year - 1].Semesters.FirstOrDefault(s => s.Name == semesterName) ?? throw new ArgumentException("invalid-destination");
            if (string.IsNullOrWhiteSpace(row.Name) || row.Name.Length > 200) throw new ArgumentException("invalid-name");
            if (row.Credits is null or <= 0 or > 1000) throw new ArgumentException("invalid-credits");
            if (row.Grade is < 0 or > 100) throw new ArgumentException("invalid-grade");
            var name = row.Name.Trim();
            if (!names[semester.Id].Add(name)) { duplicates.Add(name); if (r.DuplicatePolicy == "skip") { skipped++; continue; } }
            pending.Add((semester, new Course { Name = name, Credits = row.Credits.Value, Grade = row.Grade }));
            string Cell(string value) => value.Contains('|') || value.Contains('"') || value.Contains('\n') || value.Contains('\r') ? '"' + value.Replace("\"", "\"\"") + '"' : value;
            normalized.Add(string.Join(" | ", Cell(name), row.Credits.Value.ToString(CultureInfo.InvariantCulture), row.Grade?.ToString(CultureInfo.InvariantCulture) ?? "", $"Year {year}", Cell(semester.Name is "A" or "B" ? $"Semester {semester.Name}" : semester.Name == "Summer" ? "Summer" : semester.Name)));
        }
        return (new(pending.Count, skipped, duplicates, string.Join('\n', normalized)), pending);
    }
}
