using System.Globalization;
using System.Text.Json;
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
        data.Degrees.Single(d => d.Id == request.DegreeId).Years = prepared.Staged.Years;
        return prepared.Preview;
    }
    private static (ImportPreview Preview, List<(Semester Semester, Course Course)> Pending, Degree Staged) Prepare(Backup data, CourseImportRequest r)
    {
        if (r.Rows is null || r.Rows.Count is < 1 or > 1000 || r.DuplicatePolicy is not ("skip" or "import")) throw new ArgumentException("invalid-import");
        var original = data.Degrees.SingleOrDefault(d => d.Id == r.DegreeId) ?? throw new ArgumentException("missing-degree");
        var degree = JsonSerializer.Deserialize<Degree>(JsonSerializer.Serialize(original, Store.Json), Store.Json)!;
        var pending = new List<(Semester, Course)>(); var duplicates = new List<string>(); var normalized = new List<string>(); int skipped = 0;
        var names = degree.Years.SelectMany(y => y.Semesters).ToDictionary(s => s.Id, s => s.Courses.Select(c => c.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase));
        foreach (var row in r.Rows)
        {
            if (row is null) throw new ArgumentException("invalid-import");
            if (!row.Included) continue;
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
            if (!names.ContainsKey(semester.Id)) names[semester.Id] = [];
            if (string.IsNullOrWhiteSpace(row.Name) || row.Name.Length > 200) throw new ArgumentException("invalid-name");
            if (row.Credits is null or <= 0 or > 1000) throw new ArgumentException("invalid-credits");
            if (row.Grade is < 0 or > 100) throw new ArgumentException("invalid-grade");
            if (row.Passed.HasValue && row.Grade.HasValue) throw new ArgumentException("invalid-grade");
            var name = row.Name.Trim();
            if (!names[semester.Id].Add(name)) { duplicates.Add(name); if (r.DuplicatePolicy == "skip") { skipped++; continue; } }
            pending.Add((semester, new Course { Name = name, Credits = row.Credits.Value, Grade = row.Grade, Passed = row.Passed }));
            string Cell(string value) => value.Contains('|') || value.Contains('"') || value.Contains('\n') || value.Contains('\r') ? '"' + value.Replace("\"", "\"\"") + '"' : value;
            normalized.Add(string.Join(" | ", Cell(name), row.Credits.Value.ToString(CultureInfo.InvariantCulture), row.Passed.HasValue ? (row.Passed.Value ? "Completed" : "Failed") : row.Grade?.ToString(CultureInfo.InvariantCulture) ?? "", $"Year {year}", Cell(semester.Name is "A" or "B" ? $"Semester {semester.Name}" : semester.Name == "Summer" ? "Summer" : semester.Name)));
        }
        return (new(pending.Count, skipped, duplicates, string.Join('\n', normalized)), pending, degree);
    }
}
