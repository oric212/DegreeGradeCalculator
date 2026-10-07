using DegreeGradeCalculator;
using DegreeGradeCalculator.TextImport;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using Xunit;

public class NumericTests
{
    private static Backup Example(decimal required = 120, decimal? grade = 82) => new() {
        Degrees = [new Degree { Name = "Numeric demo", RequiredCredits = required,
            Years = [new AcademicYear { Semesters = [new Semester { Courses = [new Course { Name = "Fractional credits", Credits = 2.5m, Grade = grade }] }] }] }]
    };
    [Fact] public void WholeRequiredCreditsAndFractionalCourseCreditsAreValid() => Validation.Check(Example());
    [Theory] [InlineData("120.1")] [InlineData("128.5")] [InlineData("159.9")]
    public void FractionalRequiredCreditsAreRejected(string value) => Assert.Throws<ArgumentException>(() => Validation.Check(Example(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture))));
    [Fact] public void FractionalDirectAndComponentGradesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Validation.Check(Example(120, 69.5m)));
        var data = Example(); var course = Calculation.Courses(data.Degrees[0]).Single();
        course.Grade = null; course.UsesComponents = true; course.Components = [new Component { Name = "Exam", Weight = 100, Grade = 82.4m }];
        Assert.Throws<ArgumentException>(() => Validation.Check(data));
    }
    [Fact] public void ComponentsStillRoundFinalAndCreditsKeepTheirPrecision()
    {
        var data = Example(); var course = Calculation.Courses(data.Degrees[0]).Single();
        course.Grade = null; course.UsesComponents = true;
        course.Components = [new Component { Name = "Exam", Weight = 60, Grade = 82 }, new Component { Name = "Project", Weight = 40, Grade = 83 }];
        Validation.Check(data); Assert.Equal(82, Calculation.Final(course)); Assert.Equal(2.5m, Calculation.Summarize([course]).Credits);
    }
    [Fact] public void FractionalParsedGradesRequireCorrectionButCreditsRemainFractional()
    {
        var row = Assert.Single(new TextCourseParser().Parse(new("Course | Credits | Grade\nDemo | 2.5 | 69.5")).Courses);
        Assert.Contains("invalid-grade", row.Warnings); Assert.Equal(2.5m, row.Credits);
    }
    [Theory] [InlineData("120.4", 120)] [InlineData("120.5", 121)] [InlineData("120.6", 121)] [InlineData("0.1", 1)]
    public void LegacyRequiredCreditsMigrateOnceWithoutChangingAcademicData(string required, int expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), "numeric-" + Guid.NewGuid());
        try {
            var store = new Store(directory); var original = Example(); store.Write(original);
            var courseJson = JsonSerializer.Serialize(Calculation.Courses(original.Degrees[0]).Single(), Store.Json);
            original.Degrees[0].RequiredCredits = decimal.Parse(required, System.Globalization.CultureInfo.InvariantCulture);
            using (var db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "grades.db") }.ToString())) {
                db.Open(); using var command = db.CreateCommand(); command.CommandText = "UPDATE state SET json=$json WHERE id=1";
                command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(original, Store.Json)); command.ExecuteNonQuery();
            }
            var migrated = new Store(directory).Read();
            Assert.Equal(expected, migrated.Degrees[0].RequiredCredits); Assert.Equal(original.Revision + 1, migrated.Revision);
            Assert.Equal(courseJson, JsonSerializer.Serialize(Calculation.Courses(migrated.Degrees[0]).Single(), Store.Json));
            Assert.Equal(migrated.Revision, new Store(directory).Read().Revision);
            Assert.Throws<RevisionConflictException>(() => store.Write(Example()));
        } finally { SqliteConnection.ClearAllPools(); Directory.Delete(directory, true); }
    }
    [Fact] public void AlreadyIntegralRequiredCreditsDoNotTriggerMigration() => Assert.False(Store.NormalizeLegacyRequiredCredits(Example()));
}
