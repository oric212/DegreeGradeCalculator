# Degree Grade Calculator · GradePilot

A local-first degree grade calculator for Windows, with a clean blue browser interface and first-class Hebrew and English support. No accounts, cloud services, or separate database installation.

## Using the Windows release

Open `release/parser-win-x64/DegreeGradeCalculator.exe` by double-clicking. Keep the entire release folder together: it contains the bundled .NET runtime, SQLite library, and browser assets. The executable starts a loopback-only server on an available port and opens your default browser without a terminal window. Opening it again brings up the existing instance. Use **Close app** to shut down; closing the browser alone leaves the server running.

## Features

- Multiple degrees with required credits, nominal duration, and flexible academic years.
- Horizontal year navigation with arrows or pointer/touch swipes; vertically stacked semesters. New years start with Semester A and B; add summer or custom semesters as needed.
- Course dialogs with decimal credits, optional direct grades, or weighted components. Courses can move between semesters and years.
- Course Edit/Delete actions are grouped in a compact three-dot menu.
- Degree, year, and semester averages, graded-credit progress, reports, and five-category grade distribution.
- Temporary simulation with exact grade entry, ±1/±5, reset, and component-level controls in the normal degree layout.
- Hebrew RTL and English LTR, with a persisted language choice.
- JSON export and validated restore; confirmation for restore and deletion.
- Offline text grade import: paste, parse, review editable course cards, correct fields, and confirm. No JSON editing or external AI service.

## Calculations

Weighted average = `sum(final grade × credits) / sum(graded course credits)`. Blank grades count toward course counts but not averages, graded credits, or distributions. There is no pass/fail threshold: 45 counts normally. Averages display two decimal places; empty averages display an em dash.

Component grades are `sum(weight percentage × component grade / 100)`, rounded to the nearest whole number with halves rounded upward. That whole-number result enters all summaries. Every component needs a grade before a final grade exists. Weights below 100% are used as entered; weights above 100% show a warning and are **not normalized**. For example, 70% × 80 + 30% × 90 + 10% × 100 = 93. Input grades range from 0 to 100; bonus-weighted final grades can exceed 100.

Simulation clones the selected degree in memory. Component edits recalculate the course and all summaries. Ending simulation discards the clone; exports always contain real saved data. Structural editing is available outside simulation.

## Local data and backups

The database is `%LOCALAPPDATA%\DegreeGradeCalculator\grades.db`. Replacing the release folder does not replace your data. SQLite stores the validated application document transactionally. Back up through **Export backup**; **Restore backup** validates the document before asking to replace all degrees. Backups contain grades and course names, so store them where you would store personal academic records. Maximum backup size is 5 MiB.

## Import grades from text

Choose **Import grades** on the dashboard or degree page. Paste copied text and select **Parse text**, then review course names, credits, grades, years, and semesters in editable cards. Choose a target degree and default year/semester; explicit row destinations take priority. Exclude unwanted rows, fix highlighted fields, and confirm **Import courses** to save. Parsing and preview never write to the database.

Supported formats include pipes (`Algorithms | 4 | 82`), CSV including quoted names, spreadsheet tabs, semicolons, spaced columns, `Algorithms 4 credits 82`, and `Algorithms - 4 - 82`. English/Hebrew headers and year/semester names are recognized. Decimal credits and blank grades are supported. Decimal commas work in non-comma-separated cells or quoted CSV cells.

Hebrew and English academic transcripts are also supported: `שנת לימודים 2024` / `ACADEMIC YEAR 2024`, course codes, א/ב/ק or Fall/Spring/Summer, optional repeated credits, and reversed English rows. Calendar-year sections and table year cells receive suggested degree-year destinations, preserving year gaps. Use the editable Academic year mapping controls to set, for example, 2024 → Year 2 and 2025 → Year 3 for all courses in those years. Explicit headings such as `2024 - first year` are recognized. Missing or ambiguous credits remain blank and require correction. The paste screen includes a collapsed supported-format guide.

This is a conservative local heuristic parser, not arbitrary natural-language understanding. Uncertain values retain their original text and warnings; unparsed lines remain visible and can become manual review cards. An advanced column-mapping fallback appears when needed. Unknown destinations can use defaults; destination years and semesters must already exist. Likely duplicates in the same semester can be skipped or imported as additional courses, never overwritten. The read-only normalized preview can be copied. Limits: 50,000 characters and 1,000 non-empty lines per paste.

## Developers

Stack: .NET 10, ASP.NET Core/Kestrel, Microsoft.Data.Sqlite, SQLite, vanilla HTML/CSS/JavaScript, xUnit. No Node build or runtime requirement.

```powershell
dotnet build
dotnet test
node --test tests/frontend.test.cjs # Optional browser-calculation checks
dotnet run --project src/DegreeGradeCalculator
dotnet publish src/DegreeGradeCalculator -c Release -r win-x64 --self-contained true -o release/win-x64
```

`--no-browser` suppresses browser launch for integration checks. `GRADEPILOT_DATA` overrides the data directory for isolated test instances. Each data directory has an exclusive instance lock. The server accepts only its loopback host and rejects cross-origin requests.

Parser contract: `POST /api/import/parse-text` accepts `{ "text": "..." }` and an optional `columnMapping` array. The injected `ITextCourseParser` service returns courses, raw values, warning codes, and unparsed lines. Import preview and confirmed import use separate endpoints. These API objects are internal; users edit ordinary controls.

## Screenshots

Captured from the running application with fictitious demo data. No database or personal records are included.

![Dashboard](screenshots/dashboard.png)

<details>
<summary>Degree layout, components, and simulation</summary>

![Degree page](screenshots/degree-view.png)
![Course options](screenshots/course-options.png)
![Course components](screenshots/course-components.png)
![Simulation](screenshots/simulation.png)

</details>

<details>
<summary>Text import: paste, review, and confirm</summary>

![Text input](screenshots/text-parser.png)
![Editable parsed courses](screenshots/text-parser-preview.png)
![Import confirmation](screenshots/import-confirmation.png)
![Hebrew review](screenshots/text-parser-hebrew.png)

</details>

![Supported formats guide](screenshots/supported-formats.png)

![Adjustable academic year mapping](screenshots/year-mapping.png)

Transcript course types include lectures, seminars and workshops. Statuses "טרם", "השלים חובותיו", "No grade" and "Completed" produce blank numeric grades (excluded from averages), with the status preserved in the review source. Missing credits still require correction.
