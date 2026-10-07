using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DegreeGradeCalculator.TextImport;

public sealed record ParseTextRequest(string? Text, List<string?>? ColumnMapping = null);
public sealed record ParseWarning(string Code, int? Line = null);
public sealed record UnparsedLine(int Line, string Text, string Reason);
public sealed class ParsedCourse
{
    public string Name { get; set; } = "";
    public decimal? Credits { get; set; }
    public decimal? Grade { get; set; }
    public bool? Passed { get; set; }
    public int? Year { get; set; }
    public string? Semester { get; set; }
    public string OriginalLine { get; set; } = "";
    public int Line { get; set; }
    public Dictionary<string, string> RawValues { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}
public sealed record ParseTextResult(bool Success, string DetectedFormat, List<ParsedCourse> Courses,
    List<ParseWarning> Warnings, List<UnparsedLine> UnparsedLines, List<string?> ColumnMapping,
    int ColumnCount, bool HasHeader, bool MappingRecommended);
public interface ITextCourseParser { ParseTextResult Parse(ParseTextRequest request); }

/// <summary>Conservative, deterministic parser. It has no persistence or network dependencies.</summary>
public sealed partial class TextCourseParser : ITextCourseParser
{
    public const int MaxCharacters = 50_000;
    private static readonly string[] Fields = ["name", "credits", "grade", "year", "semester"];
    private static readonly Dictionary<string, string> Headers = new()
    {
        ["course"] = "name", ["course name"] = "name", ["name"] = "name",
        ["קורס"] = "name", ["שם קורס"] = "name", ["שם הקורס"] = "name",
        ["credit"] = "credits", ["credits"] = "credits", ["credit points"] = "credits",
        ["נקז"] = "credits", ["נז"] = "credits", ["זיכוי"] = "credits",
        ["grade"] = "grade", ["final grade"] = "grade", ["ציון"] = "grade", ["ציון סופי"] = "grade",
        ["year"] = "year", ["academic year"] = "year", ["שנה"] = "year",
        ["semester"] = "semester", ["סמסטר"] = "semester"
    };
    private static string Key(string value) => Regex.Replace(value.Trim().ToLowerInvariant().Replace('״', '"').Replace('׳', '\''), "[\"']", "");
    public static decimal? Number(string value)
    {
        var text = value.Trim();
        // A decimal comma is safe inside an already separated cell, never a thousands separator.
        if (!Regex.IsMatch(text, @"^[+-]?\d+(?:[.,]\d+)?$")) return null;
        return decimal.TryParse(text.Replace(',', '.'), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) ? n : null;
    }
    public static int? Year(string value)
    {
        var key = Regex.Replace(Key(value), @"^(year|academic year|שנה)\s*", "");
        if (int.TryParse(key, out var n) && n is > 0 and <= 100) return n;
        return key switch { "א" => 1, "ב" => 2, "ג" => 3, "ד" => 4, "ה" => 5, "ו" => 6, "ז" => 7, "ח" => 8, "ט" => 9, "י" => 10, _ => null };
    }
    public static string? Semester(string value)
    {
        var key = Key(value);
        return key switch
        {
            "a" or "semester a" or "סמסטר א" or "א" or "fall" => "A",
            "b" or "semester b" or "סמסטר ב" or "ב" or "spring" => "B",
            "summer" or "summer semester" or "סמסטר קיץ" or "קיץ" or "ק" => "Summer",
            _ => null
        };
    }
    public ParseTextResult Parse(ParseTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) throw new ArgumentException("empty-text");
        if (request.Text.Length > MaxCharacters) throw new ArgumentException("text-too-large");
        var lines = request.Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
            .Select((text, i) => (Text: text, Line: i + 1)).Where(x => !string.IsNullOrWhiteSpace(x.Text)).ToList();
        if (lines.Count > 1_000) throw new ArgumentException("too-many-lines");
        var delimiter = new[] { '\t', '|', ';', ',' }.Select(c => (Char: c, Count: lines.Take(20).Count(l => l.Text.Contains(c) && (c != ',' || Heuristic(l.Text) is null))))
            .OrderByDescending(x => x.Count).First();
        char? separator = delimiter.Count > 0 ? delimiter.Char : null;
        var first = separator is { } sep ? Split(lines[0].Text, sep) : null;
        var detected = first?.Select(x => Headers.GetValueOrDefault(Key(x))).ToList() ?? [];
        var header = detected.Where(x => x is not null).Distinct().Count() >= 2;
        var mapping = request.ColumnMapping ?? (header ? detected : Fields.Select(x => (string?)x).ToList());
        if (mapping.Count > 30 || mapping.Any(x => x is not null && !Fields.Contains(x)) || mapping.Where(x => x is not null).Distinct().Count() != mapping.Count(x => x is not null)) throw new ArgumentException("invalid-mapping");
        var courses = new List<ParsedCourse>(); var unparsed = new List<UnparsedLine>(); var warnings = new List<ParseWarning>();
        var columns = first?.Count ?? 0; bool uncertain = separator is null || (!header && columns > 3);
        var calendarYears = lines.Select(l => TranscriptYear(l.Text)).Where(y => y.HasValue)
            .Select(y => y!.Value).Distinct().Order().ToList();
        int? transcriptYear = null; int? sourceCalendarYear = null; bool hasTranscript = false;
        var explicitYears = lines.Select(l => (Calendar: TranscriptYear(l.Text), Year: ExplicitTranscriptYear(l.Text)))
            .Where(x => x.Calendar.HasValue && x.Year.HasValue).GroupBy(x => x.Calendar!.Value)
            .ToDictionary(g => g.Key, g => g.Last().Year!.Value);
        foreach (var line in lines.Skip(header ? 1 : 0))
        {
            if (request.ColumnMapping is null && TranscriptYear(line.Text) is { } calendarYear)
            {
                sourceCalendarYear = calendarYear;
                transcriptYear = explicitYears.GetValueOrDefault(calendarYear, calendarYear - calendarYears[0] + 1);
                continue;
            }
            if (request.ColumnMapping is null && Transcript(line.Text, line.Line, transcriptYear) is { } transcript)
            {
                if (sourceCalendarYear is { } cy) transcript.RawValues["calendarYear"] = cy.ToString(CultureInfo.InvariantCulture);
                courses.Add(transcript); hasTranscript = true; continue;
            }
            List<string>? cells = separator is { } s ? Split(line.Text, s) : Heuristic(line.Text);
            if (cells is null || cells.Count < 2)
            {
                unparsed.Add(new(line.Line, line.Text, "unparsed-line")); warnings.Add(new("unparsed-line", line.Line)); continue;
            }
            columns = Math.Max(columns, cells.Count);
            var row = new ParsedCourse { OriginalLine = line.Text, Line = line.Line };
            for (var i = 0; i < cells.Count; i++)
            {
                var field = i < mapping.Count ? mapping[i] : null;
                if (field is null) { row.RawValues[$"column{i + 1}"] = cells[i]; if (cells[i].Trim().Length > 0) row.Warnings.Add("unmapped-column"); continue; }
                row.RawValues[field] = cells[i].Trim();
            }
            string Get(string field) => row.RawValues.GetValueOrDefault(field, "");
            row.Name = Get("name"); row.Credits = Number(Get("credits")); row.Grade = Number(Get("grade"));
            row.Year = Year(Get("year")); row.Semester = Semester(Get("semester"));
            if (string.IsNullOrWhiteSpace(row.Name) || row.Name.Length > 200) row.Warnings.Add("invalid-name");
            if (row.Credits is null or <= 0 or > 1000) row.Warnings.Add("invalid-credits");
            row.Passed = BinaryStatus(Get("grade"));
            if (Get("grade").Length > 0 && row.Passed is null && row.Grade is null or < 0 or > 100) row.Warnings.Add("invalid-grade");
            if (Get("year").Length > 0 && row.Year is null) row.Warnings.Add("unknown-year");
            if (Get("semester").Length > 0 && row.Semester is null) row.Warnings.Add("unknown-semester");
            if (header && cells.Count != first!.Count) row.Warnings.Add("column-count");
            courses.Add(row);
        }
        foreach (var course in courses)
        {
            var calendar = Regex.Match(course.RawValues.GetValueOrDefault("year", ""), @"^(?:(?:academic year|year|שנת לימודים|שנה)\s+)?(?<year>[12]\d{3})$", RegexOptions.IgnoreCase);
            if (calendar.Success) course.RawValues["calendarYear"] = calendar.Groups["year"].Value;
        }
        var allCalendarYears = courses.Where(c => c.RawValues.ContainsKey("calendarYear"))
            .Select(c => int.Parse(c.RawValues["calendarYear"], CultureInfo.InvariantCulture)).Concat(calendarYears).ToList();
        if (allCalendarYears.Count > 0)
        {
            var firstCalendarYear = allCalendarYears.Min();
            foreach (var course in courses.Where(c => c.RawValues.ContainsKey("calendarYear")))
            {
                var calendar = int.Parse(course.RawValues["calendarYear"], CultureInfo.InvariantCulture);
                course.Year = explicitYears.GetValueOrDefault(calendar, calendar - firstCalendarYear + 1);
                course.Warnings.Remove("unknown-year");
            }
            warnings.Add(new("calendar-years-mapped"));
        }
        if (courses.Any(x => x.Warnings.Count > 0)) uncertain = true;
        if (!header && separator is not null && request.ColumnMapping is null) warnings.Add(new("inferred-columns"));
        return new(courses.Count > 0, hasTranscript ? "academic-transcript" : separator is null ? "heuristic" : "tabular", courses, warnings, unparsed,
            mapping.Take(columns).Concat(Enumerable.Repeat<string?>(null, Math.Max(0, columns - mapping.Count))).ToList(), columns, header, uncertain);
    }
    private static int? TranscriptYear(string line)
    {
        var match = Regex.Match(line.Trim(), @"^(?:(?:שנת\s+לימודים|academic\s+year)\s+)?(?<year>[12]\d{3})(?:\s*[-–—:]\s*(?:first|second|third|fourth|fifth|\d+|ראשונה|שנייה|שניה|שלישית|רביעית|חמישית|א|ב|ג|ד|ה)(?:\s+year)?|\s*[-–—:]\s*(?:year|שנה)\s+\S+)?\s*$", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture) : null;
    }
    private static int? ExplicitTranscriptYear(string line)
    {
        var suffix = Regex.Match(line, @"[-–—:]\s*(?<value>.+?)\s*$").Groups["value"].Value;
        var value = Regex.Replace(suffix.ToLowerInvariant(), @"\byear\b|שנה", "").Trim();
        return value switch { "first" or "ראשונה" => 1, "second" or "שנייה" or "שניה" => 2,
            "third" or "שלישית" => 3, "fourth" or "רביעית" => 4, "fifth" or "חמישית" => 5, _ => Year(value) };
    }
    private static ParsedCourse? Transcript(string line, int lineNumber, int? year)
    {
        const string number = @"[+-]?\d+(?:[.,]\d+)?";
        const string status = @"טרם|השלים\s+חובותיו|No\s+grade|Completed|Failed|נכשל";
        const string kind = @"שיעור|סמינר\s*/\s*סדנ[אה]|סמינר|סדנ[אה]|Lecture|Seminar|Workshop";
        var match = Regex.Match(line.Trim(), $@"^(?<semester>[אבק])\s+(?<code>\d{{5,10}})\s+(?<name>.+?)\s+(?<kind>{kind})\s+(?<credits>{number})(?:\s+(?<extra>{number}))?\s+(?<grade>{number}|{status})\s*$", RegexOptions.IgnoreCase);
        if (!match.Success)
            match = Regex.Match(line.Trim(), $@"^(?<grade>{number}|{status})\s+(?:(?<extra>{number})\s+)?(?<credits>{number})\s+(?<kind>{kind})\s+(?<name>.+?)\s+(?<code>\d{{5,10}})\s+(?<semester>[אבק])\s*$", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            // A trailing integer in a title (Calculus 2) is not safely distinguishable
            // from integer credits. English transcript credits require a decimal token
            // or an explicit Lecture marker; otherwise ask the user to supply credits.
            match = Regex.Match(line.Trim(), $@"^(?<semester>Fall|Spring|Summer)\s+(?<code>\d{{5,10}})\s+(?<name>.+?)\s+(?:(?<kind>{kind})\s+(?:(?<credits>{number})\s+)?|(?<credits>\d+[.,]\d+)\s+)?(?<grade>{number}|{status})\s*$", RegexOptions.IgnoreCase);
        }
        if (!match.Success) return null;
        string Get(string key) => match.Groups[key].Value;
        var row = new ParsedCourse
        {
            Name = Get("name"), Credits = Number(Get("credits")), Grade = Number(Get("grade")),
            Year = year, Semester = Semester(Get("semester")), OriginalLine = line, Line = lineNumber,
            RawValues = new() { ["name"] = Get("name"), ["credits"] = Get("credits"), ["grade"] = Get("grade"),
                ["courseCode"] = Get("code"), ["semester"] = Get("semester"), ["additionalCredits"] = Get("extra"), ["courseType"] = Get("kind") }
        };
        if (row.Credits is null or <= 0 or > 1000) row.Warnings.Add("invalid-credits");
        var hasStatus = Regex.IsMatch(Get("grade"), $@"^(?:{status})$", RegexOptions.IgnoreCase);
        if (!hasStatus && row.Grade is null or < 0 or > 100) row.Warnings.Add("invalid-grade");
        row.Passed = BinaryStatus(Get("grade"));
        if (hasStatus && row.Passed is null) row.Warnings.Add("non-numeric-status");
        if (row.Name.Length > 200) row.Warnings.Add("invalid-name");
        if (Get("extra").Length > 0 && Number(Get("extra")) != row.Credits) row.Warnings.Add("different-credit-values");
        return row;
    }
    private static bool? BinaryStatus(string text) => Regex.IsMatch(text.Trim(), @"^(?:Completed|השלים\s+חובותיו)$", RegexOptions.IgnoreCase) ? true : Regex.IsMatch(text.Trim(), @"^(?:Failed|נכשל)$", RegexOptions.IgnoreCase) ? false : null;
    private static List<string>? Split(string line, char separator)
    {
        var cells = new List<string>(); var cell = new StringBuilder(); bool quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"' && (quoted || cell.ToString().Trim().Length == 0))
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { cell.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == separator && !quoted) { cells.Add(cell.ToString()); cell.Clear(); }
            else cell.Append(c);
        }
        if (quoted) return null;
        cells.Add(cell.ToString()); return cells;
    }
    private static List<string>? Heuristic(string line)
    {
        var match = Regex.Match(line.Trim(), @"^(?<name>.+?)\s+(?<credits>\d+(?:[.,]\d+)?)\s+(?:credits?|credit points|נק[״""']?ז|נ[״""']?ז)\s*(?<grade>\d+(?:[.,]\d+)?)?\s*$", RegexOptions.IgnoreCase);
        if (!match.Success) match = Regex.Match(line.Trim(), @"^(?<name>.+?)\s+-\s+(?<credits>\d+(?:[.,]\d+)?)\s+-\s*(?<grade>\d+(?:[.,]\d+)?)?\s*$");
        if (match.Success) return [match.Groups["name"].Value, match.Groups["credits"].Value, match.Groups["grade"].Value];
        var cells = Regex.Split(line.Trim(), @"\s{2,}");
        return cells.Length is >= 2 and <= 5 ? cells.ToList() : null;
    }
}
