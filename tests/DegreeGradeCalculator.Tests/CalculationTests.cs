using DegreeGradeCalculator;
using System.Text.Json;
using Xunit;
public class CalculationTests
{
    [Fact] public void LatestGradedExactNameRepeatCountsOnce()
    {
        var rows = new[] { new Course { Name = "Math", Credits = 4, Grade = 90 },
            new Course { Name = "Math", Credits = 3, Grade = 60 },
            new Course { Name = "Math", Credits = 5, Grade = null },
            new Course { Name = "math", Credits = 2, Grade = 80 } };
        var summary = Calculation.Summarize(rows);
        Assert.Equal(68, summary.Average); Assert.Equal(5, summary.Credits);
        Assert.Equal(4, summary.Courses); Assert.Equal(2, summary.Distribution.Sum());
    }
    [Fact] public void MissingBackupFieldsRejected() => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Backup>("{}", Store.Json));
    [Fact] public void MissingCourseGradeRejected() => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Course>("{\"name\":\"Course\",\"credits\":5}", Store.Json));
    private static Course C(decimal credits, decimal? grade) => new() { Name = $"Course {credits}/{grade}", Credits = credits, Grade = grade };
    private static Course Components(params (decimal weight, decimal? grade)[] ps) => new() { Name = "Components", Credits = 5, UsesComponents = true, Components = ps.Select(p => new Component { Name = "Part", Weight = p.weight, Grade = p.grade }).ToList() };
    private static Backup Fixture() => new() { Degrees = [new Degree { Name = "Computer science", Years = [new AcademicYear { Semesters = [new Semester { Courses = [C(5, 80), C(4, 90), C(2.5m, null), Components((70, 55), (30, 98))] }] }] }] };
    [Fact] public void WeightedAverage() => Assert.Equal(760m / 9m, Calculation.Summarize([C(5, 80), C(4, 90)]).Average);
    [Fact] public void BlankExcludedFromAverage() => Assert.Equal(80, Calculation.Summarize([C(5, 80), C(100, null)]).Average);
    [Fact] public void BlankExcludedFromCredits() => Assert.Equal(5, Calculation.Summarize([C(5, 80), C(100, null)]).Credits);
    [Fact] public void DecimalCredits() => Assert.Equal(82.5m, Calculation.Summarize([C(1.5m, 70), C(2.5m, 90)]).Average);
    [Fact] public void LowGradeCountsNormally() => Assert.Equal(45, Calculation.Summarize([C(5, 45)]).Average);
    [Fact] public void ComponentsRoundFinal() => Assert.Equal(68, Calculation.Final(Components((70, 55), (30, 98))));
    [Fact] public void HundredPercent() => Assert.Equal(83, Calculation.Final(Components((70, 80), (30, 90))));
    [Fact] public void BonusNotNormalized() => Assert.Equal(93, Calculation.Final(Components((70, 80), (30, 90), (10, 100))));
    [Fact] public void HalfRoundsAway() => Assert.Equal(81, Calculation.Final(Components((100, 80.5m))));
    [Fact] public void IncompleteHasNoFinal() => Assert.Null(Calculation.Final(Components((70, 80), (30, null))));
    [Fact] public void UnderweightNotNormalized() => Assert.Equal(40, Calculation.Final(Components((50, 80))));
    [Fact] public void SemesterAverage() => Assert.Equal(760m / 9m, Calculation.Summarize(new Semester { Courses = [C(5, 80), C(4, 90)] }.Courses).Average);
    [Fact] public void YearAverage() { var y = new AcademicYear { Semesters = [new Semester { Courses = [C(5, 80)] }, new Semester { Courses = [C(4, 90)] }] }; Assert.Equal(760m / 9m, Calculation.Summarize(y.Semesters.SelectMany(s => s.Courses)).Average); }
    [Fact] public void DegreeAverage() { var d = Fixture().Degrees[0]; Assert.Equal(1100m / 14m, Calculation.Summarize(Calculation.Courses(d)).Average); }
    [Fact] public void DistributionBoundaries() => Assert.Equal(new[] { 2, 2, 2, 2, 3 }, Calculation.Summarize(new decimal[] { 0, 59, 60, 69, 70, 79, 80, 89, 90, 100, 110 }.Select(g => C(1, g)).Append(C(5, null))).Distribution);
    [Fact] public void EmptyAverage() => Assert.Null(Calculation.Summarize([]).Average);
    [Fact] public void SimulationDoesNotMutateOriginal() { var b = Fixture(); var copy = JsonSerializer.Deserialize<Backup>(JsonSerializer.Serialize(b, Store.Json), Store.Json)!; copy.Degrees[0].Years[0].Semesters[0].Courses[0].Grade = 95; Assert.Equal(80, b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade); Assert.NotEqual(Calculation.Summarize(Calculation.Courses(b.Degrees[0])).Average, Calculation.Summarize(Calculation.Courses(copy.Degrees[0])).Average); }
    [Fact] public void ComponentSimulationPropagates() { var b = Fixture(); var c = b.Degrees[0].Years[0].Semesters[0].Courses[3]; c.Components[0].Grade = 60; Assert.Equal(71, Calculation.Final(c)); Assert.Equal(1115m / 14m, Calculation.Summarize(Calculation.Courses(b.Degrees[0])).Average); }
    [Fact] public void ImportExportAndSqlitePersistence() { var dir = Path.Combine(Path.GetTempPath(), "grade-test-" + Guid.NewGuid()); try { var b = Fixture(); b.Language = "he"; new Store(dir).Write(b); var read = new Store(dir).Read(); Assert.Equal(JsonSerializer.Serialize(b, Store.Json), JsonSerializer.Serialize(read, Store.Json)); Validation.Check(read); } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(dir, true); } }
    [Fact] public void InvalidImportRejectedWithoutReplacement() { var dir = Path.Combine(Path.GetTempPath(), "grade-test-" + Guid.NewGuid()); try { var store = new Store(dir); store.Write(Fixture()); var bad = Fixture(); bad.Degrees[0].Name = " "; Assert.Throws<ArgumentException>(() => store.Write(bad)); Assert.Equal("Computer science", store.Read().Degrees[0].Name); } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(dir, true); } }
    [Theory][InlineData(-1)][InlineData(101)] public void InvalidGradesRejected(int grade) { var b = Fixture(); b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade = grade; Assert.Throws<ArgumentException>(() => Validation.Check(b)); }
    [Fact] public void DuplicateIdsRejected() { var b = Fixture(); b.Degrees[0].Years[0].Id = b.Degrees[0].Id; Assert.Throws<ArgumentException>(() => Validation.Check(b)); }
    [Fact] public void BonusValidationAllowed() { var b = Fixture(); b.Degrees[0].Years[0].Semesters[0].Courses.Add(Components((70, 80), (30, 90), (10, 100))); Validation.Check(b); }
}
