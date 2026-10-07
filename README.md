# Degree Grade Calculator · GradePilot

A local-first degree grade calculator for Windows, with a clean blue browser interface and first-class Hebrew and English support. No accounts, cloud services, or separate database installation.

## Using the Windows release

Open `release/win-x64/DegreeGradeCalculator.exe` by double-clicking. Keep the entire release folder together: it contains the bundled .NET runtime, SQLite library, and browser assets. The executable starts a loopback-only server on an available port and opens your default browser without a terminal window. Opening it again brings up the existing instance. Use **Close app** to shut down; closing the browser alone leaves the server running.

## Features

- Multiple degrees with required credits, nominal duration, and flexible academic years.
- Horizontal year navigation with arrows or pointer/touch swipes; vertically stacked semesters. New years start with Semester A and B; add summer or custom semesters as needed.
- Course dialogs with decimal credits, optional direct grades, or weighted components. Courses can move between semesters and years.
- Degree, year, and semester averages, graded-credit progress, reports, and five-category grade distribution.
- Temporary simulation with exact grade entry, ±1/±5, reset, and component-level controls in the normal degree layout.
- Hebrew RTL and English LTR, with a persisted language choice.
- JSON export and validated restore; confirmation for restore and deletion.

## Calculations

Weighted average = `sum(final grade × credits) / sum(graded course credits)`. Blank grades count toward course counts but not averages, graded credits, or distributions. There is no pass/fail threshold: 45 counts normally. Averages display two decimal places; empty averages display an em dash.

Component grades are `sum(weight percentage × component grade / 100)`, rounded to the nearest whole number with halves rounded upward. That whole-number result enters all summaries. Every component needs a grade before a final grade exists. Weights below 100% are used as entered; weights above 100% show a warning and are **not normalized**. For example, 70% × 80 + 30% × 90 + 10% × 100 = 93. Input grades range from 0 to 100; bonus-weighted final grades can exceed 100.

Simulation clones the selected degree in memory. Component edits recalculate the course and all summaries. Ending simulation discards the clone; exports always contain real saved data. Structural editing is available outside simulation.

## Local data and backups

The database is `%LOCALAPPDATA%\DegreeGradeCalculator\grades.db`. Replacing the release folder does not replace your data. SQLite stores the validated application document transactionally. Back up through **Export backup**; **Restore backup** validates the document before asking to replace all degrees. Backups contain grades and course names, so store them where you would store personal academic records. Maximum backup size is 5 MiB.

## Developers

Stack: .NET 10, ASP.NET Core/Kestrel, Microsoft.Data.Sqlite, SQLite, vanilla HTML/CSS/JavaScript, xUnit. No Node build or runtime requirement.

```powershell
dotnet build
dotnet test
dotnet run --project src/DegreeGradeCalculator
dotnet publish src/DegreeGradeCalculator -c Release -r win-x64 --self-contained true -o release/win-x64
```

`--no-browser` suppresses browser launch for integration checks. `GRADEPILOT_DATA` overrides the data directory for isolated test instances. Each data directory has an exclusive instance lock. The server accepts only its loopback host and rejects cross-origin requests.

## Screenshots

The interface uses white rounded cards, navy controls, blue distribution bars, and responsive desktop/mobile layouts. Screenshots can be added here after capturing a degree with representative course data.
