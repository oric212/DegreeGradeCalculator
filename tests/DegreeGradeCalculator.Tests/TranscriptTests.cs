using DegreeGradeCalculator.TextImport;
using Xunit;

namespace DegreeGradeCalculator.Tests;

public class TranscriptTests
{
    [Theory]
    [InlineData("2024 - first year", 1)]
    [InlineData("ACADEMIC YEAR 2024 - second year", 2)]
    [InlineData("שנת לימודים 2024 - שנה ב", 2)]
    public void ExplicitCalendarYearMapping(string heading, int expected)
    {
        var row = Assert.Single(new TextCourseParser().Parse(new(heading + "\nFall 111112 Math 3.0 77")).Courses);
        Assert.Equal(expected, row.Year); Assert.Equal("2024", row.RawValues["calendarYear"]);
    }
    [Fact]
    public void CalendarYearsInTableAreSuggestedWithGapsPreserved()
    {
        var result = new TextCourseParser().Parse(new("Name|Credits|Grade|Year|Semester\nMath|3|77|2024|Fall\nOS|4|80|2026|Spring"));
        Assert.Equal(1, result.Courses[0].Year); Assert.Equal(3, result.Courses[1].Year);
        Assert.Equal("2026", result.Courses[1].RawValues["calendarYear"]);
        Assert.DoesNotContain("unknown-year", result.Courses[1].Warnings);
    }
    [Fact]
    public void EnglishTranscriptRetainsMissingCreditsAndTitleNumbers()
    {
        var result = new TextCourseParser().Parse(new("""
            ACADEMIC YEAR 2024
            Fall 111112 Mathematical Reasoning 3.0 77
            Fall 111113 Calculus 1 6.0 69
            Fall 111115 Introduction to Computer Science 6.0 76
            Spring 111111 Introduction to Logic and Set Theory 4.0 66
            Spring 111117 Linear algebra 1 6.0 85
            Spring 111124 Advanced Programming in C Language 4.0 76
            Spring 111126 Calculus 2 47
            Summer 111121 Discrete Mathematics 53
            Summer 111123 Introduction To Probability Theory 4.0 63
            Summer 111126 Calculus 2 Lecture 4.0 80
            """));
        Assert.Equal(10, result.Courses.Count); Assert.Empty(result.UnparsedLines);
        Assert.All(result.Courses, c => Assert.Equal(1, c.Year));
        Assert.Equal("Calculus 1", result.Courses[1].Name);
        Assert.Equal("Calculus 2", result.Courses[6].Name);
        Assert.Equal(47, result.Courses[6].Grade); Assert.Null(result.Courses[6].Credits);
        Assert.Null(result.Courses[7].Credits); Assert.Equal(53, result.Courses[7].Grade);
        Assert.Contains("invalid-credits", result.Courses[6].Warnings);
        Assert.Contains("invalid-credits", result.Courses[7].Warnings);
        Assert.Equal("Calculus 2", result.Courses[9].Name);
        Assert.Equal(4, result.Courses[9].Credits); Assert.Equal(80, result.Courses[9].Grade);
        Assert.Equal("Summer", result.Courses[9].Semester);
    }
    public const string Sample = """
        שנת לימודים 2024

        א 111112 תורת ההנמקה שיעור 3.0 3.0 77
        א 111113 חשבון דיפרנציאלי ואינטגרלי 1 שיעור 6.0 6.0 69
        א 111115 מבוא למדעי המחשב שיעור 6.0 6.0 76
        ב 111111 מבוא ללוגיקה ולתורת הקבוצות שיעור 4.0 4.0 66
        ב 111117 אלגברה לינארית 1 שיעור 6.0 6.0 85
        ב 111124 תכנות מתקדם בשפת C שיעור 4.0 4.0 76
        ב 111126 חשבון דיפרנציאלי ואינטגרלי 2 שיעור 4.0 47
        ק 111121 מתמטיקה בדידה שיעור 4.0 53
        ק 111123 מבוא להסתברות שיעור 4.0 4.0 63
        ק 111126 חשבון דיפרנציאלי ואינטגרלי 2 שיעור 4.0 4.0 80

        שנת לימודים 2025

        א 111121 מתמטיקה בדידה שיעור 4.0 4.0 90
        א 121114 אלגברה ליניארית 2 שיעור 4.0 4.0 97
        א 121115 מבנה מחשבים שיעור 4.0 4.0 60
        א 121119 תכנות מכוון עצמים ושפת ++C שיעור 4.0 4.0 79
        92 2.0 2.0 שיעור Hacking Tech English 800011 א
        ב 121111 מבני נתונים שיעור 4.0 4.0 67
        ב 121118 ארכיטקטורת מחשבים שיעור 4.0 4.0 60
        ב 121150 כלים פרקטיים לתעשייה - בסיס שיעור 1.0 1.0 96
        ב 142169 תכנות מונחה עצמים בסביבת דוט-נט ושפת #C שיעור 4.0 4.0 88
        ק 121120 אלגוריתמים שיעור 4.0 66
        ק 142180 מבוא לפונקציות מרוכבות שיעור 4.0 4.0 98
        """;

    [Fact]
    public void FullUserTranscriptExtractsEveryCourseAndDestination()
    {
        var result = new TextCourseParser().Parse(new(Sample));
        Assert.Equal("academic-transcript", result.DetectedFormat);
        Assert.Equal(21, result.Courses.Count);
        Assert.Empty(result.UnparsedLines);
        Assert.All(result.Courses, c => Assert.Empty(c.Warnings));
        Assert.Equal(10, result.Courses.Count(c => c.Year == 1));
        Assert.Equal(11, result.Courses.Count(c => c.Year == 2));
        Assert.Equal(new[] { "A", "A", "A", "B", "B", "B", "B", "Summer", "Summer", "Summer", "A", "A", "A", "A", "A", "B", "B", "B", "B", "Summer", "Summer" }, result.Courses.Select(c => c.Semester));
        Assert.Equal(new decimal?[] {77,69,76,66,85,76,47,53,63,80,90,97,60,79,92,67,60,96,88,66,98}, result.Courses.Select(c => c.Grade));
        Assert.Equal(new decimal?[] {3,6,6,4,6,4,4,4,4,4,4,4,4,4,2,4,4,1,4,4,4}, result.Courses.Select(c => c.Credits));
        var english = result.Courses[14];
        Assert.Equal("Hacking Tech English", english.Name);
        Assert.Equal("800011", english.RawValues["courseCode"]);
        Assert.Equal("חשבון דיפרנציאלי ואינטגרלי 2", result.Courses[6].Name);
        Assert.Equal("תכנות מונחה עצמים בסביבת דוט-נט ושפת #C", result.Courses[18].Name);
        Assert.Contains(result.Warnings, w => w.Code == "calendar-years-mapped");
    }
    [Fact]
    public void CalendarYearsUseChronologicalOrderEvenWhenSectionsAreReversed()
    {
        var rows = new TextCourseParser().Parse(new("שנת לימודים 2025\nא 121111 Math שיעור 4 80\nשנת לימודים 2024\nב 111111 OS שיעור 3 90")).Courses;
        Assert.Equal(2, rows[0].Year); Assert.Equal(1, rows[1].Year);
    }
    [Theory]
    [InlineData("א 111111 Math שיעור 4 3 80")]
    [InlineData("80 3 4 שיעור Math 111111 א")]
    public void DifferentCreditValuesRequireReview(string text)
    {
        var row = Assert.Single(new TextCourseParser().Parse(new(text)).Courses);
        Assert.Equal(4, row.Credits); Assert.Contains("different-credit-values", row.Warnings);
        Assert.Equal(text, row.OriginalLine); Assert.Null(row.Year);
    }
    [Fact]
    public void OutOfRangeTranscriptGradeIsNotSilentlyAccepted()
    {
        var row = Assert.Single(new TextCourseParser().Parse(new("ק 111111 Math שיעור 4 101")).Courses);
        Assert.Contains("invalid-grade", row.Warnings);
    }
}
