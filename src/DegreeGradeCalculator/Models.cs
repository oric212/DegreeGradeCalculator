using System.Text.Json.Serialization;
namespace DegreeGradeCalculator;

public sealed class Backup
{
    public long Revision { get; set; } = 1;
    [JsonRequired] public int Version { get; set; } = 1;
    [JsonRequired] public string Language { get; set; } = "en";
    [JsonRequired] public List<Degree> Degrees { get; set; } = [];
}
public sealed class Degree
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "";
    [JsonRequired] public int Duration { get; set; } = 3;
    [JsonRequired] public decimal RequiredCredits { get; set; } = 120;
    [JsonRequired] public List<AcademicYear> Years { get; set; } = [];
}
public sealed class AcademicYear
{
    public List<Course> YearlyCourses { get; set; } = [];
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public List<Semester> Semesters { get; set; } = [];
}
public sealed class Semester
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "A";
    [JsonRequired] public List<Course> Courses { get; set; } = [];
}
public sealed class Course
{
    [JsonRequired] public Guid Id { get; set; } = Guid.NewGuid();
    [JsonRequired] public string Name { get; set; } = "";
    [JsonRequired] public decimal Credits { get; set; }
    [JsonRequired] public decimal? Grade { get; set; }
    public bool? Passed { get; set; }
    [JsonRequired] public bool UsesComponents { get; set; }
    [JsonRequired] public List<Component> Components { get; set; } = [];
}
public sealed class Component
{
    [JsonRequired] public string Name { get; set; } = "";
    [JsonRequired] public decimal Weight { get; set; }
    [JsonRequired] public decimal? Grade { get; set; }
}
public record Summary(decimal? Average, decimal Credits, int Courses, int[] Distribution);
public static class Calculation
{
    public static decimal? Final(Course c) => c.Passed.HasValue ? null : !c.UsesComponents ? c.Grade : c.Components.Count == 0 || c.Components.Any(x => x.Grade is null) ? null : decimal.Round(c.Components.Sum(x => x.Weight * x.Grade!.Value / 100m), 0, MidpointRounding.AwayFromZero);
    public static Summary Summarize(IEnumerable<Course> source)
    {
        var all = source.ToList(); var completed = all.Where(c => Final(c) is not null || c.Passed == true)
            .GroupBy(c => c.Name, StringComparer.Ordinal).Select(g => g.Last()).ToList(); var credits = completed.Sum(c => c.Credits);
        var graded = completed.Where(c => Final(c) is not null).ToList(); var numericCredits = graded.Sum(c => c.Credits);
        var bins = new int[5]; foreach (var c in graded) { var g = Final(c)!.Value; bins[g < 60 ? 0 : g < 70 ? 1 : g < 80 ? 2 : g < 90 ? 3 : 4]++; }
        return new(numericCredits == 0 ? null : graded.Sum(c => Final(c)!.Value * c.Credits) / numericCredits, credits, all.Count, bins);
    }
    public static IEnumerable<Course> Courses(AcademicYear y) => y.Semesters.SelectMany(s => s.Courses).Concat(y.YearlyCourses);
    public static IEnumerable<Course> Courses(Degree d) => d.Years.SelectMany(Courses);
}
public static class Validation
{
    public static void Check(Backup b)
    {
        if (b.Version != 1 || b.Language is not ("en" or "he") || b.Degrees is null || b.Degrees.Count > 100) throw new ArgumentException("Invalid backup format or language.");
        var ids = new HashSet<Guid>();
        void Id(Guid id) { if (id == Guid.Empty || !ids.Add(id)) throw new ArgumentException("Invalid or duplicate identifier."); }
        void Name(string n) { if (string.IsNullOrWhiteSpace(n) || n.Length > 200) throw new ArgumentException("Names must contain 1–200 characters."); }
        void Grade(decimal? g) { if (g is < 0 or > 100 || (g.HasValue && g.Value != decimal.Truncate(g.Value))) throw new ArgumentException("Input grades must be whole numbers between 0 and 100."); }
        foreach (var d in b.Degrees)
        {
            if (d is null) throw new ArgumentException("Degree cannot be null.");
            Id(d.Id); Name(d.Name); if (d.Duration is < 1 or > 100 || d.RequiredCredits is <= 0 or > 10000 || d.RequiredCredits != decimal.Truncate(d.RequiredCredits) || d.Years is null || d.Years.Count is < 1 or > 100) throw new ArgumentException("Degree required credits must be whole numbers; check degree structure and credit range.");
            foreach (var y in d.Years)
            {
                if (y is null) throw new ArgumentException("Year cannot be null."); Id(y.Id); if (y.Semesters is null || y.Semesters.Count > 50) throw new ArgumentException("Invalid semesters.");
                if (y.YearlyCourses is null || y.YearlyCourses.Count > 1000) throw new ArgumentException("Invalid yearly courses.");
                foreach (var s in y.Semesters)
                {
                    if (s is null) throw new ArgumentException("Semester cannot be null."); Id(s.Id); Name(s.Name); if (s.Courses is null || s.Courses.Count > 1000) throw new ArgumentException("Invalid courses.");
                }
                foreach (var c in Calculation.Courses(y))
                {
                    if (c is null) throw new ArgumentException("Course cannot be null."); Id(c.Id); Name(c.Name); if (c.Credits is <= 0 or > 1000 || c.Components is null || c.Components.Count > 100) throw new ArgumentException("Invalid course credits or components."); Grade(c.Grade); if (c.Passed.HasValue && (c.Grade is not null || c.UsesComponents || c.Components.Count > 0)) throw new ArgumentException("Binary courses cannot have numeric grades or components.");
                    if (c.UsesComponents && (c.Grade is not null || c.Components.Count == 0)) throw new ArgumentException("Component courses require components and no direct override.");
                    foreach (var p in c.Components) { if (p is null) throw new ArgumentException("Component cannot be null."); Name(p.Name); if (p.Weight is <= 0 or > 1000) throw new ArgumentException("Weights must be positive and at most 1000%."); Grade(p.Grade); }
                }
            }
        }
    }
}
