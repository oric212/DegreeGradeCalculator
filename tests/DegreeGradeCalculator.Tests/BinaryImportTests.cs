using DegreeGradeCalculator;
using DegreeGradeCalculator.TextImport;
using System.Text.Json;
using Xunit;

public class BinaryImportTests
{
    [Fact] public void PassedCoursesCountCreditsWithoutChangingAverage()
    {
        var summary = Calculation.Summarize([new Course { Name = "Studio", Credits = 4, Grade = 80 },
            new Course { Name = "Community", Credits = 2, Passed = true },
            new Course { Name = "Pending", Credits = 3 }, new Course { Name = "Failed", Credits = 5, Passed = false }]);
        Assert.Equal(80, summary.Average); Assert.Equal(6, summary.Credits); Assert.Equal(1, summary.Distribution.Sum());
    }
    [Fact] public void BinaryOnlyAverageIsBlankAndBackupRetainsPass()
    {
        var c = new Course { Name = "Community", Credits = 2, Passed = true };
        var saved = JsonSerializer.Deserialize<Course>(JsonSerializer.Serialize(c, Store.Json), Store.Json)!;
        Assert.True(saved.Passed); Assert.Null(Calculation.Summarize([saved]).Average);
        Assert.Equal(2, Calculation.Summarize([saved]).Credits);
    }
    [Fact] public void PreviewDoesNotCreateDetectedDestinationsButConfirmedImportDoes()
    {
        var degree = new Degree { Name = "Demo", Years = [new AcademicYear { Semesters = [new Semester { Name = "A" }, new Semester { Name = "B" }] }] };
        var b = new Backup { Degrees = [degree] };
        var request = new CourseImportRequest(degree.Id, 1, "A", [new ReviewedCourse { Name = "Community", Credits = 2, Passed = true, Year = 3, Semester = "Summer" }], Confirmed: true);
        var preview = CourseTextImport.Preview(b, request);
        Assert.Single(degree.Years); Assert.Contains("Completed", preview.NormalizedText);
        var roundTrip = Assert.Single(new TextCourseParser().Parse(new(preview.NormalizedText)).Courses);
        Assert.True(roundTrip.Passed);
        CourseTextImport.Apply(b, request);
        Assert.Equal(3, degree.Years.Count);
        Assert.True(Assert.Single(degree.Years[2].Semesters.Single(s => s.Name == "Summer").Courses).Passed);
    }
    [Fact] public void InvalidImportDoesNotCreateYearsOrSemesters()
    {
        var d = new Degree { Name = "Demo", Years = [new AcademicYear { Semesters = [new Semester()] }] };
        var b = new Backup { Degrees = [d] };
        var req = new CourseImportRequest(d.Id, 1, "A", [new ReviewedCourse { Name = "Good", Credits = 2, Year = 3, Semester = "Summer" }, new ReviewedCourse { Name = "Bad", Credits = 0 }], Confirmed: true);
        Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, req));
        Assert.Single(d.Years); Assert.Single(d.Years[0].Semesters); Assert.Empty(Calculation.Courses(d));
    }
}
