using DegreeGradeCalculator;
using DegreeGradeCalculator.TextImport;
using System.Text.Json;
using Xunit;

public class TextImportTests
{
    private readonly TextCourseParser parser = new();
    private ParseTextResult Parse(string text, List<string?>? mapping = null) => parser.Parse(new(text, mapping));
    private static Backup Data() => new() { Degrees = [new Degree { Name = "Computer Science", Years = Enumerable.Range(0, 3).Select(_ => new AcademicYear { Semesters = [new Semester { Name = "A" }, new Semester { Name = "B" }, new Semester { Name = "Summer" }] }).ToList() }] };
    private static CourseImportRequest Request(Backup data, params ReviewedCourse[] rows) => new(data.Degrees[0].Id, 2, "B", rows.ToList(), Confirmed: true);

    [Theory]
    [InlineData("Algorithms | 4 | 82")]
    [InlineData("Algorithms, 4, 82")]
    [InlineData("Algorithms\t4\t82")]
    [InlineData("Algorithms;4;82")]
    [InlineData("Algorithms 4 credits 82")]
    [InlineData("Algorithms - 4 - 82")]
    public void CommonFormats(string text) { var row = Assert.Single(Parse(text).Courses); Assert.Equal("Algorithms", row.Name); Assert.Equal(4, row.Credits); Assert.Equal(82, row.Grade); }
    [Fact] public void EnglishReorderedHeaders() { var r = Parse("Final Grade|Course Name|Credit Points|Academic Year|Semester\n82|Algorithms|3.5|Year 2|Semester B"); Assert.True(r.HasHeader); var c = Assert.Single(r.Courses); Assert.Equal("Algorithms", c.Name); Assert.Equal(3.5m, c.Credits); Assert.Equal(2, c.Year); Assert.Equal("B", c.Semester); }
    [Fact] public void HebrewHeadersAndValues() { var r = Parse("שם הקורס|נק״ז|ציון סופי|שנה|סמסטר\nמערכות הפעלה|5|68|שנה ג׳|סמסטר א׳"); var c = Assert.Single(r.Courses); Assert.True(r.HasHeader); Assert.Equal("מערכות הפעלה", c.Name); Assert.Equal(5, c.Credits); Assert.Equal(3, c.Year); Assert.Equal("A", c.Semester); }
    [Theory]
    [InlineData("נ״ז")]
    [InlineData("נקז")]
    [InlineData("זיכוי")]
    [InlineData("Credit")]
    [InlineData("Credits")]
    public void HeaderAliases(string credits) => Assert.Equal(2.5m, Assert.Single(Parse($"Name|{credits}|Grade\nMath|2.5|45").Courses).Credits);
    [Fact] public void DecimalCommaCell() => Assert.Equal(2.5m, Assert.Single(Parse("Math;2,5;45").Courses).Credits);
    [Fact] public void DecimalCommaInHeuristic() => Assert.Equal(2.5m, Assert.Single(Parse("Math 2,5 credits 45").Courses).Credits);
    [Fact] public void QuotedCommaName() => Assert.Equal("Math, advanced", Assert.Single(Parse("\"Math, advanced\",2.5,80").Courses).Name);
    [Fact] public void BlankGradeRemainsNull() { var c = Assert.Single(Parse("Math|2.5|").Courses); Assert.Null(c.Grade); Assert.Empty(c.Warnings); }
    [Theory]
    [InlineData("Year 4", 4)]
    [InlineData("Academic Year 2", 2)]
    [InlineData("1", 1)]
    [InlineData("שנה א׳", 1)]
    [InlineData("שנה ב", 2)]
    [InlineData("שנה ג׳", 3)]
    [InlineData("שנה ד׳", 4)]
    public void Years(string value, int year) => Assert.Equal(year, TextCourseParser.Year(value));
    [Theory]
    [InlineData("Semester A", "A")]
    [InlineData("B", "B")]
    [InlineData("Summer Semester", "Summer")]
    [InlineData("סמסטר א׳", "A")]
    [InlineData("סמסטר ב", "B")]
    [InlineData("קיץ", "Summer")]
    public void Semesters(string text, string semester) => Assert.Equal(semester, TextCourseParser.Semester(text));
    [Fact] public void UncertainDestinationPreservesRawValue() { var c = Assert.Single(Parse("Math|4|80|next year|winter").Courses); Assert.Null(c.Year); Assert.Null(c.Semester); Assert.Equal("winter", c.RawValues["semester"]); Assert.Contains("unknown-year", c.Warnings); Assert.Contains("unknown-semester", c.Warnings); }
    [Fact] public void UnparsedLinesPreservedWithLineNumber() { var r = Parse("Math|4|80\n\nA line I cannot understand"); var l = Assert.Single(r.UnparsedLines); Assert.Equal(3, l.Line); Assert.Equal("A line I cannot understand", l.Text); }
    [Theory]
    [InlineData("?")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1001")]
    public void InvalidCreditsWarn(string credits) { var c = Assert.Single(Parse($"Math|{credits}|80").Courses); Assert.Contains("invalid-credits", c.Warnings); Assert.Equal(credits, c.RawValues["credits"]); }
    [Theory]
    [InlineData("bad")]
    [InlineData("-1")]
    [InlineData("101")]
    public void InvalidGradesWarn(string grade) => Assert.Contains("invalid-grade", Assert.Single(Parse($"Math|4|{grade}").Courses).Warnings);
    [Fact] public void MeaningfulNamePunctuationPreserved() => Assert.Equal("C# 2 - Systems (advanced)", Assert.Single(Parse("C# 2 - Systems (advanced)|4|80").Courses).Name);
    [Fact] public void ExtraBlankLinesAndCrLf() => Assert.Equal(2, Parse("\r\nMath|4|80\r\n\r\nOS|5|68\r\n").Courses.Count);
    [Fact] public void MixedValidInvalidAndUnparsed() { var r = Parse("Math|4|80\nOS|?|bad\nUnstructured note"); Assert.Equal(2, r.Courses.Count); Assert.Equal(2, r.Courses[1].Warnings.Count); Assert.Single(r.UnparsedLines); }
    [Fact] public void SizeLimit() => Assert.Equal("text-too-large", Assert.Throws<ArgumentException>(() => Parse(new string('x', 50_001))).Message);
    [Fact] public void EmptyTextRejected() => Assert.Throws<ArgumentException>(() => Parse(" \n "));
    [Fact] public void ExcessRowsRejected() => Assert.Throws<ArgumentException>(() => Parse(string.Join('\n', Enumerable.Repeat("a|1|80", 1001))));
    [Fact] public void ManualMapping() { var c = Assert.Single(Parse("82|Algorithms|4", ["grade", "name", "credits"]).Courses); Assert.Equal("Algorithms", c.Name); Assert.Equal(82, c.Grade); }
    [Fact] public void DuplicateMappingRejected() => Assert.Throws<ArgumentException>(() => Parse("A|4|80", ["name", "name", "grade"]));
    [Fact] public void UnmappedInputIsNotDiscarded() { var c = Assert.Single(Parse("Math|4|80|Year 1|A|notes").Courses); Assert.Contains("unmapped-column", c.Warnings); Assert.Equal("notes", c.RawValues["column6"]); }
    [Fact] public void MalformedQuotedLinePreserved() => Assert.Equal("\"Math|4|80", Assert.Single(Parse("\"Math|4|80").UnparsedLines).Text);
    [Fact] public void ParserDoesNotTouchStore() { var dir = Path.Combine(Path.GetTempPath(), "parser-test-" + Guid.NewGuid()); try { var s = new Store(dir); s.Write(Data()); var before = JsonSerializer.Serialize(s.Read(), Store.Json); Parse("Math|4|80"); Assert.Equal(before, JsonSerializer.Serialize(s.Read(), Store.Json)); } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(dir, true); } }
    [Fact] public void BulkDefaults() { var b = Data(); CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 4, Grade = 82 })); Assert.Equal("Math", Assert.Single(b.Degrees[0].Years[1].Semesters[1].Courses).Name); }
    [Fact] public void ExplicitDestinationWins() { var b = Data(); CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 4, Grade = 82, Year = 3, Semester = "Summer" })); Assert.Single(b.Degrees[0].Years[2].Semesters[2].Courses); Assert.Empty(b.Degrees[0].Years[1].Semesters[1].Courses); }
    [Fact] public void ExcludedRowsDoNotImportOrRequireValues() { var b = Data(); var p = CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 4 }, new ReviewedCourse() { Included = false })); Assert.Equal(1, p.Count); }
    [Fact] public void DuplicateSkipAndImportAnyway() { var b = Data(); var req = Request(b, new ReviewedCourse() { Name = "Math", Credits = 4, Grade = 80 }, new ReviewedCourse() { Name = " math ", Credits = 3, Grade = 90 }); var p = CourseTextImport.Apply(b, req); Assert.Equal(1, p.Count); Assert.Equal(1, p.SkippedDuplicates); var again = CourseTextImport.Apply(b, req with { DuplicatePolicy = "import" }); Assert.Equal(2, again.Count); Assert.Equal(3, b.Degrees[0].Years[1].Semesters[1].Courses.Count); Assert.Equal(80, b.Degrees[0].Years[1].Semesters[1].Courses[0].Grade); }
    [Fact] public void PreviewNeverChangesData() { var b = Data(); var before = JsonSerializer.Serialize(b, Store.Json); var p = CourseTextImport.Preview(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 2.5m, Grade = null })); Assert.Equal("Math | 2.5 |  | Year 2 | Semester B", p.NormalizedText); Assert.Equal(before, JsonSerializer.Serialize(b, Store.Json)); }
    [Fact] public void NormalizedTextRoundTripsQuotedNames() { var b = Data(); var p = CourseTextImport.Preview(b, Request(b, new ReviewedCourse() { Name = "Math | \"advanced\"", Credits = 2.5m, Grade = 80 })); Assert.Equal("Math | \"advanced\"", Assert.Single(Parse(p.NormalizedText).Courses).Name); }
    [Fact] public void ConfirmationRequired() { var b = Data(); Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 4 }) with { Confirmed = false })); Assert.Empty(Calculation.Courses(b.Degrees[0])); }
    [Fact] public void InvalidLaterRowIsAtomic() { var b = Data(); Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Good", Credits = 4 }, new ReviewedCourse() { Name = "Bad", Credits = 0 }))); Assert.Empty(Calculation.Courses(b.Degrees[0])); }
    [Fact] public void MissingDestinationDoesNotCreateStructure() { var b = Data(); Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, Request(b, new ReviewedCourse() { Name = "Math", Credits = 4, Year = 4 }))); Assert.Equal(3, b.Degrees[0].Years.Count); }
}
