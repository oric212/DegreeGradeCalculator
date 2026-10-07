using DegreeGradeCalculator.TextImport;
using Xunit;

namespace DegreeGradeCalculator.Tests;

public class TranscriptTests
{
    [Fact]
    public void EnglishCompletedExemptionKeepsCreditsButNoNumericGrade()
    {
        var row = Assert.Single(new TextCourseParser().Parse(new("Fall 899996 Demo Community Activity Lecture 2.0 Completed")).Courses);
        Assert.Equal("Demo Community Activity", row.Name);
        Assert.Equal(2, row.Credits); Assert.Null(row.Grade);
        Assert.Equal("Completed", row.RawValues["grade"]);
        Assert.Contains("non-numeric-status", row.Warnings);
        Assert.DoesNotContain("invalid-grade", row.Warnings);
    }
    [Theory]
    [InlineData("א 150034 סדנה: עיצוב מדיה סמינר/סדנא 6.0 6.0 91", "סדנה: עיצוב מדיה", 6, 91)]
    [InlineData("א 899996 פעילות קהילתית לדוגמה שיעור 2.0 2.0 השלים חובותיו", "פעילות קהילתית לדוגמה", 2, null)]
    [InlineData("ב 960006 מיומנויות יצירתיות לדוגמה שיעור 2.0 טרם", "מיומנויות יצירתיות לדוגמה", 2, null)]
    [InlineData("ק 142203 עיצוב אינטראקטיבי מבוסס ג'אווה שיעור 4.0 טרם", "עיצוב אינטראקטיבי מבוסס ג'אווה", 4, null)]
    [InlineData("ק 142234 מבוא ליצירה במשחקים שיעור 3.0 טרם", "מבוא ליצירה במשחקים", 3, null)]
    [InlineData("א 131111 יצירה מתקדמת שיעור 4.0 טרם", "יצירה מתקדמת", 4, null)]
    [InlineData("א 141418 מבוא לתקשורת חזותית שיעור 4.0 טרם", "מבוא לתקשורת חזותית", 4, null)]
    [InlineData("א 142239 מבוא לאמנות שימושית שיעור 3.0 טרם", "מבוא לאמנות שימושית", 3, null)]
    public void WorkshopAndStatusRows(string line, string name, int credits, int? grade)
    {
        var result = new TextCourseParser().Parse(new("שנת לימודים 2026\n" + line));
        Assert.Empty(result.UnparsedLines); var row = Assert.Single(result.Courses);
        Assert.Equal(name, row.Name); Assert.Equal(credits, row.Credits); Assert.Equal((decimal?)grade, row.Grade);
        Assert.DoesNotContain("invalid-grade", row.Warnings);
        if (grade is null) Assert.Contains("non-numeric-status", row.Warnings);
    }
    [Theory]
    [InlineData("Fall 131111 Advanced Creative Studies Lecture No grade", "Advanced Creative Studies")]
    [InlineData("Fall 141418 Introduction to Visual Communication Lecture No grade", "Introduction to Visual Communication")]
    public void EnglishNoGradeWithoutCredits(string line, string name)
    {
        var result = new TextCourseParser().Parse(new("ACADEMIC YEAR 2027\n" + line));
        Assert.Empty(result.UnparsedLines); var row = Assert.Single(result.Courses);
        Assert.Equal(name, row.Name); Assert.Null(row.Grade); Assert.Null(row.Credits);
        Assert.Contains("invalid-credits", row.Warnings); Assert.DoesNotContain("invalid-grade", row.Warnings);
    }
    [Theory]
    [InlineData("2024 - first year", 1)]
    [InlineData("ACADEMIC YEAR 2024 - second year", 2)]
    [InlineData("שנת לימודים 2024 - שנה ב", 2)]
    public void ExplicitCalendarYearMapping(string heading, int expected)
    {
        var row = Assert.Single(new TextCourseParser().Parse(new(heading + "\nFall 111112 Math 3.0 81")).Courses);
        Assert.Equal(expected, row.Year); Assert.Equal("2024", row.RawValues["calendarYear"]);
    }
    [Fact]
    public void CalendarYearsInTableAreSuggestedWithGapsPreserved()
    {
        var result = new TextCourseParser().Parse(new("Name|Credits|Grade|Year|Semester\nMath|3|81|2024|Fall\nOS|4|78|2026|Spring"));
        Assert.Equal(1, result.Courses[0].Year); Assert.Equal(3, result.Courses[1].Year);
        Assert.Equal("2026", result.Courses[1].RawValues["calendarYear"]);
        Assert.DoesNotContain("unknown-year", result.Courses[1].Warnings);
    }
    [Fact]
    public void EnglishTranscriptRetainsMissingCreditsAndTitleNumbers()
    {
        var result = new TextCourseParser().Parse(new("""
            ACADEMIC YEAR 2024
            Fall 111112 Creative Coding 3.0 81
            Fall 111113 Visual Design 1 6.0 72
            Fall 111115 Introduction to Creative Media 6.0 83
            Spring 111111 Design Thinking 4.0 74
            Spring 111117 Visual Stories 1 6.0 86
            Spring 111124 Creative Work in C Language 4.0 83
            Spring 111126 Visual Design 2 43
            Summer 111121 Color Studio 56
            Summer 111123 Introduction to Digital Arts 4.0 63
            Summer 111126 Visual Design 2 Lecture 4.0 78
            """));
        Assert.Equal(10, result.Courses.Count); Assert.Empty(result.UnparsedLines);
        Assert.All(result.Courses, c => Assert.Equal(1, c.Year));
        Assert.Equal("Visual Design 1", result.Courses[1].Name);
        Assert.Equal("Visual Design 2", result.Courses[6].Name);
        Assert.Equal(43, result.Courses[6].Grade); Assert.Null(result.Courses[6].Credits);
        Assert.Null(result.Courses[7].Credits); Assert.Equal(56, result.Courses[7].Grade);
        Assert.Contains("invalid-credits", result.Courses[6].Warnings);
        Assert.Contains("invalid-credits", result.Courses[7].Warnings);
        Assert.Equal("Visual Design 2", result.Courses[9].Name);
        Assert.Equal(4, result.Courses[9].Credits); Assert.Equal(78, result.Courses[9].Grade);
        Assert.Equal("Summer", result.Courses[9].Semester);
    }
    public const string Sample = """
        שנת לימודים 2024

        א 111112 יצירה דיגיטלית שיעור 3.0 3.0 81
        א 111113 עיצוב חזותי 1 שיעור 6.0 6.0 72
        א 111115 מבוא למדיה יצירתית שיעור 6.0 6.0 83
        ב 111111 חשיבה עיצובית שיעור 4.0 4.0 74
        ב 111117 סיפורים חזותיים 1 שיעור 6.0 6.0 86
        ב 111124 יצירה אינטראקטיבית בשפת C שיעור 4.0 4.0 83
        ב 111126 עיצוב חזותי 2 שיעור 4.0 43
        ק 111121 מעבדת צבע שיעור 4.0 56
        ק 111123 מבוא לאמנות דיגיטלית שיעור 4.0 4.0 63
        ק 111126 עיצוב חזותי 2 שיעור 4.0 4.0 78

        שנת לימודים 2025

        א 111121 מעבדת צבע שיעור 4.0 4.0 89
        א 121114 סיפורים חזותיים 2 שיעור 4.0 4.0 94
        א 121115 מבנה יצירות שיעור 4.0 4.0 62
        א 121119 עיצוב אינטראקטיבי ושפת ++C שיעור 4.0 4.0 84
        87 2.0 2.0 שיעור Creative Studio English 800011 א
        ב 121111 מבני סיפורים שיעור 4.0 4.0 73
        ב 121118 ארכיטקטורת עיצוב שיעור 4.0 4.0 62
        ב 121150 כלים יצירתיים למדיה - בסיס שיעור 1.0 1.0 95
        ב 142169 מעבדת יצירה בסביבת #C שיעור 4.0 4.0 85
        ק 121120 טכניקות יצירה שיעור 4.0 74
        ק 142180 מבוא לאנימציה שיעור 4.0 4.0 99
        """;

    [Fact]
    public void FictionalTranscriptExtractsEveryCourseAndDestination()
    {
        var result = new TextCourseParser().Parse(new(Sample));
        Assert.Equal("academic-transcript", result.DetectedFormat);
        Assert.Equal(21, result.Courses.Count);
        Assert.Empty(result.UnparsedLines);
        Assert.All(result.Courses, c => Assert.Empty(c.Warnings));
        Assert.Equal(10, result.Courses.Count(c => c.Year == 1));
        Assert.Equal(11, result.Courses.Count(c => c.Year == 2));
        Assert.Equal(new[] { "A", "A", "A", "B", "B", "B", "B", "Summer", "Summer", "Summer", "A", "A", "A", "A", "A", "B", "B", "B", "B", "Summer", "Summer" }, result.Courses.Select(c => c.Semester));
        Assert.Equal(new decimal?[] {81,72,83,74,86,83,43,56,63,78,89,94,62,84,87,73,62,95,85,74,99}, result.Courses.Select(c => c.Grade));
        Assert.Equal(new decimal?[] {3,6,6,4,6,4,4,4,4,4,4,4,4,4,2,4,4,1,4,4,4}, result.Courses.Select(c => c.Credits));
        var english = result.Courses[14];
        Assert.Equal("Creative Studio English", english.Name);
        Assert.Equal("800011", english.RawValues["courseCode"]);
        Assert.Equal("עיצוב חזותי 2", result.Courses[6].Name);
        Assert.Equal("מעבדת יצירה בסביבת #C", result.Courses[18].Name);
        Assert.Contains(result.Warnings, w => w.Code == "calendar-years-mapped");
    }
    [Fact]
    public void CalendarYearsUseChronologicalOrderEvenWhenSectionsAreReversed()
    {
        var rows = new TextCourseParser().Parse(new("שנת לימודים 2025\nא 121111 Math שיעור 4 78\nשנת לימודים 2024\nב 111111 OS שיעור 3 89")).Courses;
        Assert.Equal(2, rows[0].Year); Assert.Equal(1, rows[1].Year);
    }
    [Theory]
    [InlineData("א 111111 Math שיעור 4 3 78")]
    [InlineData("78 3 4 שיעור Math 111111 א")]
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
