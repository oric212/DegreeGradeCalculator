using DegreeGradeCalculator;
using DegreeGradeCalculator.TextImport;
using System.Text.Json;
using Xunit;

public class ReimportTests
{
    private static Backup Data() => new() { Degrees = [new Degree { Name = "Demo", Years = [new AcademicYear { Semesters = [new Semester { Name = "A", Courses = [new Course { Name = "Design Studio", Credits = 3, Grade = 81 }, new Course { Name = "Other", Credits = 2, Grade = 70 }] }, new Semester { Name = "B" }] }] }] };
    private static CourseImportRequest Request(Backup b, ReviewedCourse row) => new(b.Degrees[0].Id, 1, "A", [row], Confirmed: true);
    [Fact] public void ChangedGradeAndCreditsPreserveIdAndRequireConfirmation()
    {
        var b = Data(); var id = b.Degrees[0].Years[0].Semesters[0].Courses[0].Id;
        var req = Request(b, new() { Name = " design   studio ", Credits = 4, Grade = 87 });
        var before = JsonSerializer.Serialize(b, Store.Json);
        var p = CourseTextImport.Preview(b, req);
        Assert.Equal(1, p.Updated); Assert.Equal(81, p.Updates[0].OldGrade); Assert.Equal(87, p.Updates[0].NewGrade);
        Assert.Equal(before, JsonSerializer.Serialize(b, Store.Json));
        Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, req with { Confirmed = false }));
        CourseTextImport.Apply(b, req with { ReviewToken = p.ReviewToken });
        var courses = b.Degrees[0].Years[0].Semesters[0].Courses;
        Assert.Equal(2, courses.Count); Assert.Equal(id, courses[0].Id); Assert.Equal(87, courses[0].Grade); Assert.Equal(4, courses[0].Credits);
        Assert.Equal(70, courses[1].Grade);
        var unchanged = CourseTextImport.Apply(b, req); Assert.Equal(0, unchanged.Count); Assert.Equal(1, unchanged.Unchanged);
    }
    [Fact] public void DifferentSemesterIsSeparateAttempt()
    {
        var b = Data(); var p = CourseTextImport.Apply(b, Request(b, new() { Name = "Design Studio", Credits = 3, Grade = 87, Semester = "B" }));
        Assert.Equal(1, p.Added); Assert.Equal(81, b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade);
    }
    [Fact] public void BlankAndBinaryUpdatesAreExplicitlyPreviewed()
    {
        var b = Data(); var req = Request(b, new() { Name = "Design Studio", Credits = 3 });
        var p = CourseTextImport.Preview(b, req); Assert.Equal(81, p.Updates[0].OldGrade); Assert.Null(p.Updates[0].NewGrade);
        var binary = req with { Rows = [new() { Name = "Design Studio", Credits = 3, Passed = true }] };
        p = CourseTextImport.Preview(b, binary); Assert.True(p.Updates[0].NewPassed);
        CourseTextImport.Apply(b, binary); Assert.True(b.Degrees[0].Years[0].Semesters[0].Courses[0].Passed);
    }
    [Fact] public void MatchingComponentGradePreservesBreakdown()
    {
        var b = Data(); var c = b.Degrees[0].Years[0].Semesters[0].Courses[0]; c.UsesComponents = true; c.Grade = null;
        c.Components = [new Component { Name = "Portfolio", Weight = 100, Grade = 81 }];
        var req = Request(b, new() { Name = c.Name, Credits = 4, Grade = 81 });
        var p = CourseTextImport.Preview(b, req); Assert.False(p.Updates[0].ReplacesComponents);
        CourseTextImport.Apply(b, req); Assert.Single(b.Degrees[0].Years[0].Semesters[0].Courses[0].Components);
        req = req with { Rows = [new() { Name = c.Name, Credits = 4, Grade = 90 }] };
        Assert.True(CourseTextImport.Preview(b, req).Updates[0].ReplacesComponents);
        CourseTextImport.Apply(b, req); Assert.Empty(b.Degrees[0].Years[0].Semesters[0].Courses[0].Components);
    }
    [Fact] public void StaleConfirmationCannotOverwriteNewerChanges()
    {
        var b = Data(); var req = Request(b, new() { Name = "Design Studio", Credits = 3, Grade = 87 });
        var token = CourseTextImport.Preview(b, req).ReviewToken;
        b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade = 90;
        Assert.Equal("import-changed", Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, req with { ReviewToken = token })).Message);
        Assert.Equal(90, b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade);
    }
    [Fact] public void OldDuplicatesAreMergedAfterReview()
    {
        var b = Data(); b.Degrees[0].Years[0].Semesters[0].Courses.Add(new() { Name = "Design Studio", Credits = 3, Grade = 87 });
        var req = Request(b, new() { Name = "Design Studio", Credits = 3, Grade = 87 });
        Assert.Equal(1, CourseTextImport.Preview(b, req).Updates[0].RemovedDuplicates);
        CourseTextImport.Apply(b, req); Assert.Equal(2, b.Degrees[0].Years[0].Semesters[0].Courses.Count);
    }
    [Fact] public void InvalidLaterRowDoesNotApplyEarlierUpdate()
    {
        var b = Data(); var req = Request(b, new() { Name = "Design Studio", Credits = 3, Grade = 87 }) with { Rows = [new() { Name = "Design Studio", Credits = 3, Grade = 87 }, new() { Name = "Bad", Credits = 0 }] };
        Assert.Throws<ArgumentException>(() => CourseTextImport.Apply(b, req)); Assert.Equal(81, b.Degrees[0].Years[0].Semesters[0].Courses[0].Grade);
    }
}
