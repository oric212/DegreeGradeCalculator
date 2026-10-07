# Degree Grade Calculator · GradePilot

A local-first degree grade calculator for Windows, with a clean blue browser interface and first-class Hebrew and English support. No accounts, cloud services, or separate database installation.

## Why I built GradePilot

I built GradePilot out of frustration with the grade-calculation tools I tried. They had ads or other drawbacks that made a simple task harder than it needed to be. I wanted a clean, local app where I could track my degree, calculate averages, and try different grades without those distractions.

## Using the Windows release

Open `release/GradePilot/DegreeGradeCalculator.exe` by double-clicking. Keep the entire release folder together: it contains the bundled .NET runtime, SQLite library, and browser assets. The executable starts a loopback-only server on an available port and opens your default browser without a terminal window. Opening it again brings up the existing instance. Closing the final app tab/window shuts down the local server automatically after a short grace period. Multiple tabs are supported: closing one keeps the others running, and ordinary refresh reconnects safely. **Close app** shuts down immediately. Before updating, close the app, replace the complete release folder, and reopen the executable. The current release prevents cached browser assets from hiding UI updates; reload any previously open tab.

## Features

- Multiple degrees with required credits, nominal duration, and flexible academic years.
- Horizontal year navigation with arrows or pointer/touch swipes; vertically stacked semesters. New years start with Semester A and B; add semesters using predefined A, B, and Summer choices, displayed in the current language. Only semesters missing from the selected year are offered when adding; editing keeps the current choice available. Add semester is disabled once A, B, and Summer all exist. Existing custom semester names remain available when editing.
- Course dialogs with decimal credits, optional direct grades, or weighted components. Choose a semester or **Yearly** as the course period. Yearly courses have their own year-page section and enter year/degree totals once, without entering individual semester totals. Courses can move between periods and years.
- Course, semester, and degree Edit/Delete actions are grouped in compact three-dot menus. **Add course** stays directly visible in each semester and yearly section. Deleting a degree requires a separate confirmation naming the degree; Cancel is focused first. Removing the last year also requires a named confirmation with its course count. Clicking the logo returns to the dashboard.
- Clearly labeled degree, year, and semester averages, graded-credit progress, reports, and five-category grade distribution with distinct light-to-dark blue shades and matching legend markers.
- Temporary simulation with a light blue summary, prominent new average, smaller saved average, exact grade entry, ±1/±5, reset, and component controls. Only currently different courses receive a blue highlight; returning every value to its original value removes the highlight and modified count, exactly like Reset.
- Hebrew RTL and English LTR, with a persisted language choice. Numeric fields and signed simulation buttons stay LTR in both languages; typing does not replace the focused input.
- JSON export and validated restore; confirmation for restore and deletion.
- Offline text grade import: paste, parse, review editable course cards, correct fields, and confirm. No JSON editing or external AI service.

## Calculations

Repeated courses with exactly the same name use only the latest graded attempt within each summary (academic year, semester, then row order). A newer blank grade does not replace an earlier numeric grade. Its credits and grade distribution count once; all attempts remain visible.

Weighted average = `sum(final grade × credits) / sum(graded course credits)`. Blank grades count toward course counts but not averages, counted credits, or distributions. Binary Passed grades add credits without entering averages or distributions; binary Failed grades add no credits. There is no pass/fail threshold: 45 counts normally. Averages display two decimal places; empty averages display an em dash.

Component grades are `sum(weight percentage × component grade / 100)`, rounded to the nearest whole number with halves rounded upward. That whole-number result enters all summaries. Every component needs a grade before a final grade exists. Weights below 100% are used as entered; weights above 100% show a warning and are **not normalized**. For example, 70% × 80 + 30% × 90 + 10% × 100 = 93. Input grades range from 0 to 100; bonus-weighted final grades can exceed 100.

Simulation clones the selected degree in memory. Component edits recalculate the course and all summaries. Ending simulation discards the clone; exports always contain real saved data. Structural editing is available outside simulation. Changing a component highlights its parent course even if rounding leaves the final grade unchanged; highlights are derived from current differences and are never saved.

## Local data and backups

Multiple open tabs use revision-based optimistic concurrency. If another tab saves newer data, a stale save is rejected and **Reload latest data** lets you load the current state before reapplying your edit. Existing data migrates automatically; restoring a backup advances the live revision rather than using the backup’s old token.

The database is `%LOCALAPPDATA%\DegreeGradeCalculator\grades.db`. Replacing the release folder does not replace your data. SQLite stores the validated application document transactionally. Back up through **Export backup**; **Restore backup** validates the document before asking to replace all degrees. Backups contain grades and course names, so store them where you would store personal academic records. Maximum backup size is 5 MiB.

## Import grades from text

Choose **Import grades** on the dashboard or degree page. Paste copied text and select **Parse text**, then review course names, credits, grades, years, and semesters in editable cards grouped by destination year and semester. Changing a destination moves the card into its matching group. Choose a target degree. Each course keeps its own detected year and semester. Fallback year/semester controls appear under **Courses with missing year or semester** only when an included course needs them. The section is hidden when all courses are matched; fallback values never override detected destinations. Exclude unwanted rows, fix highlighted fields, and confirm **Import courses** to save. Parsing and preview never write to the database.

Supported formats include pipes (`Creative Coding | 3 | 81`), CSV including quoted names, spreadsheet tabs, semicolons, spaced columns, `Creative Coding 3 credits 81`, and `Creative Coding - 3 - 81`. English/Hebrew headers and year/semester names are recognized. Decimal credits and blank grades are supported. Decimal commas work in non-comma-separated cells or quoted CSV cells.

Hebrew and English academic transcripts are also supported: `שנת לימודים 2030` / `ACADEMIC YEAR 2030`, course codes, א/ב/ק or Fall/Spring/Summer, optional repeated credits, and reversed English rows. Calendar-year sections and table year cells receive suggested degree-year destinations, preserving year gaps. Use the editable Academic year mapping controls to set, for example, 2030 → Year 2 and 2031 → Year 3 for all courses in those years. Explicit headings such as `2030 - first year` are recognized. Missing or ambiguous credits remain blank and require correction. The paste screen includes a collapsed supported-format guide.

This is a conservative local heuristic parser, not arbitrary natural-language understanding. Uncertain values retain their original text and warnings; unsupported rows appear once in an expandable section and can become manual review cards. An advanced column-mapping fallback appears when needed. Unknown destinations can use defaults; missing years and standard semesters (A, B, Summer) are created only after confirmed import. Reimporting the same course in the same year and semester leaves identical data unchanged. Changed credits or grades update the existing course only after a final confirmation showing the old and new values. Courses in other semesters remain separate, and courses absent from the import are preserved. If the paste repeats a course in one destination, the last included row wins. Existing component grades are preserved when their final grade matches; replacing a different component grade is explicitly flagged in the confirmation. The read-only normalized preview can be copied. Limits: 50,000 characters and 1,000 non-empty lines per paste.

Transcript course types include lectures, seminars and workshops. "Completed" and "השלים חובותיו" are imported as binary Passed grades: their credits count toward progress, while they stay out of numeric averages and grade distributions. "טרם" and "No grade" remain ungraded. Each import card has a toggle between numeric and pass/fail grading. Switching back restores the previous number or pass/fail choice on that card without changing other courses. Passed/Failed can also be selected in the course editor. Missing credits still require correction.

## Developers

Stack: .NET 10, ASP.NET Core/Kestrel, Microsoft.Data.Sqlite, SQLite, vanilla HTML/CSS/JavaScript, xUnit. No Node build or runtime requirement.

```powershell
dotnet build
dotnet test
node --test tests/frontend.test.cjs # Optional browser-calculation checks
dotnet run --project src/DegreeGradeCalculator
dotnet publish src/DegreeGradeCalculator -c Release -r win-x64 --self-contained true -o release/GradePilot
```

`--no-browser` suppresses browser launch for integration checks. `GRADEPILOT_DATA` overrides the data directory for isolated test instances. Each data directory has an exclusive instance lock. Windowed launches default to local app mode. Set configuration/environment **LocalAppMode=false** (or run with --no-browser, which defaults to non-local mode) to disable session-driven shutdown for unattended/hosted execution. Explicit **LocalAppMode=true** enables it for local testing. The server accepts only its loopback host and rejects cross-origin requests.

Parser contract: `POST /api/import/parse-text` accepts `{ "text": "..." }` and an optional `columnMapping` array. The injected `ITextCourseParser` service returns courses, raw values, warning codes, and unparsed lines. Import preview and confirmed import use separate endpoints. These API objects are internal; users edit ordinary controls.

## Screenshots

A walkthrough using a fictional **Digital Arts · Demo** degree. All course names, transcript text, and grades are invented. These fresh English-only captures focus on the relevant controls.

**Start here:** open a degree, import grades, or create a new degree from the dashboard.

![Dashboard with fictional demo degrees](screenshots/01-home.jpg)

<details>
<summary>Explore averages, edit courses, and simulate grades</summary>

**Open a degree.** Degree and year averages have separate labels, with five distinct blue grade ranges.

![Degree and year summaries](screenshots/02-degree.jpg)

**Open a course’s three-dot menu** to edit or delete that course. Each semester shows its own average.

![Semester and course options](screenshots/03-course-menu.jpg)

**Edit weighted components.** Choose a grading method, enter component weights and grades, then save.

![Component fields and Save and Cancel actions](screenshots/04-components.jpg)

**Start simulation** to compare a prominent hypothetical average with the smaller saved average in a light blue card.

![Simulation comparison](screenshots/05-simulation.jpg)

**Changed courses receive a blue highlight.** Manually returning to the original grade or pressing Reset removes the highlight and changed count.

![A changed yearly course in simulation](screenshots/18-simulation-change.jpg)

**Choose Yearly in the course form** for an annual course. It appears after the semesters and contributes only to year and degree summaries.

![Dedicated yearly course section](screenshots/15-yearly-courses.jpg)

</details>

<details>
<summary>Import grades: paste → map years → review → confirm</summary>

**1. Paste your course text and select Parse text.** Supported formats stay in a collapsed guide.

![Clean paste screen](screenshots/06-import-paste.jpg)

**2. Choose the degree and map academic years.** Each course retains its own semester; mappings can be adjusted.

![Target degree and adjustable academic year mapping](screenshots/07-import-destinations.jpg)

**3. Review courses grouped by year and semester.** Edit an individual destination or toggle only that course between numeric and pass/fail grading.

![Grouped review with a binary course](screenshots/08-course-review.jpg)

**If some rows need attention,** open one consolidated section to view them or add an editable review card.

![One expandable section for unsupported rows](screenshots/14-unsupported-rows.jpg)

**4. Confirm updates.** Reimported changes show old and new values before existing courses are replaced.

![Before and after import confirmation](screenshots/09-import-confirmation.jpg)

</details>

<details>
<summary>Manage degrees, years, and available semesters</summary>

**Add course stays visible.** A semester’s three-dot menu holds only Edit semester and Delete semester; deletion requires confirmation.

![Visible Add course with semester management menu](screenshots/17-semester-menu.jpg)

**Degree settings and deletion** live inside the three-dot menu.

![Degree options](screenshots/10-degree-menu.jpg)

**Deleting a degree requires confirmation** naming the affected degree.

![Named degree deletion confirmation](screenshots/11-delete-confirmation.jpg)

**Removing the last year also requires confirmation,** including its course count.

![Year removal confirmation](screenshots/12-remove-year-confirmation.jpg)

**Add only missing semesters.** This demo year already has A and B, so the dropdown offers only Summer. Choices are localized in Hebrew mode.

![Only Summer is available in this year](screenshots/13-semester-choices.jpg)

</details>

<details>
<summary>Saving from multiple tabs</summary>

**Newer data is protected.** If another tab has saved, a stale save is blocked. Select Reload latest data, then reapply your intended edit.

![Save conflict with reload action](screenshots/16-save-conflict.jpg)

</details>
