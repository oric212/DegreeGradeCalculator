# Verification

Verified on Windows x64 with .NET SDK 10.0.401.

- Solution build: zero warnings or errors.
- 119 xUnit tests: original calculation/storage coverage plus deterministic parsing, supported delimiters and headers, Hebrew/English destinations, preserved invalid input, size limits, manual mapping, normalized output, bulk defaults/overrides, exclusion, duplicates, atomic import, and a live HTTP test proving parse/preview are read-only and import requires confirmation.
- 7 browser-logic tests: exact decimal math, exclusions, component weighting, simulated propagation/isolation, distribution boundaries.
- Self-contained Windows x64 publish includes the runtime, SQLite native library, and all browser assets.
- Browser checks: degree creation, direct course editing, component course creation, warnings for excess weights, vertical semesters, simulation controls, immediate exact component entry, restoration after simulation, Hebrew RTL, English LTR, and a 390-pixel mobile layout.
- Live release checks: automatic loopback-port selection, persisted real component grades after restart, backup validation/restoration, malformed backup rejection, static assets, and a windowless executable launch. Browser launch completed without recording an error.
- Parser browser checks: mixed Hebrew/English input, editable correction, destination defaults, explicit destinations, excluded rows, repeat-import change summaries, normalized clipboard copy, confirmation, and successful import to the chosen semester. Manual column mapping was also verified in the published release.
- Transcript checks: both complete user examples (21 Hebrew courses and 10 English courses), calendar-year mapping, reversed English rows, summer semesters, missing credits, and differing credit values. The collapsed format guide was verified in the browser.
- Course actions now open from a three-dot menu; the Edit action was verified. Fictional demo screenshots are included under `screenshots/` and referenced in the README.

Reference Android screenshots were not attached; the implementation follows the written layout specification. No cloud sync, accounts, or hypothetical-course creation is implemented, as requested.

Calendar-year mapping controls verified in the browser: 2024 → Year 2 and 2025 → Year 3 update the course cards and normalized output without saving.

The original eleven screenshots were recaptured using an isolated fictional Digital Arts demo. Example transcript course names and grades were replaced with invented data, and the import textarea has no placeholder.

Binary-grade verification: Passed adds credits without changing numeric averages; backup preserves binary status. Browser import created missing Summer only after confirmation and preserved editable Pass/Fail grading. Preview and invalid imports create no structure.

Grouped import review verified in Hebrew: moving a course from Year 1 / A to Year 2 / B updates the group headings, course counts, editable fields, and normalized preview. Binary Passed rows remain editable in their destination group.

Repeat-import verification: identical rows are unchanged; updated grades and credits retain course IDs and require confirmation; other semesters stay separate; absent courses are preserved. Tests cover binary transitions, component preservation/replacement, legacy duplicate merging, stale confirmation rejection, atomic validation, and HTTP persistence. Browser checks confirmed one new / one updated / one unchanged course, followed by an identical no-op reimport. Two and 200 unsupported rows each produce one expandable review section, without repeated notifications. The current gallery uses fictional data.

Empty summary verified in Hebrew using a fictional degree: compact dashboard card, one progress bar, explanatory distribution empty state, and isolated numeric labels.

Release refresh verification: the complete executable and assets were republished together. HTML, JavaScript, and CSS return Cache-Control: no-store; updated HTML uses fresh asset URLs. Live HTTP integration coverage checks these headers. The real saved database hash was unchanged during replacement. All old captures were removed and replaced with eleven fresh screenshots from the exact release executable.

Screenshot gallery replaced again after framing review: nine English-only captures, including English degree names and transcript text. Every image was visually inspected. Long degree, simulation, and import screens use full-page captures, with complete cards and actions; dialogs show all fields and buttons. No personal grades or mixed-language examples appear in the gallery.

Import destination controls verified: target degree stands alone; fallback year and semester are collapsed when all destinations are detected and expanded for missing data. A multi-semester transcript keeps A, B, and Summer when fallback year changes. Calendar-year cards distinguish transcript year from degree year. Current English import screenshots recaptured.

Import grade-mode toggle verified in the browser: Passed to numeric 86 to Failed to numeric restores 86, then back to binary restores Failed. Another numeric row toggled to binary and back retains 94; neighboring grade 73 and all destinations remain unchanged. Preview and final confirmation reflect only active grading values. English import screenshots refreshed.

Average labels verified in English and Hebrew: degree summary uses Degree average / ממוצע תואר; year summary uses Year average / ממוצע שנתי; semester headers use Semester average / ממוצע סמסטר. Simulation uses the same scope labels. Affected English screenshots recaptured.

Grade distribution uses five widely spaced blue shades, from pale blue below 60 to navy at 90+, with thin segment separators and matching legend dots. Simulation legend text has improved contrast. Browser verification displayed all five categories using fictional courses. Missing-destination controls are absent for fully matched courses and reappear when a course destination is cleared. All affected English screenshots refreshed.

Degree deletion is hidden in a three-dot menu and requires a named confirmation with Cancel focused first. Browser verification confirmed cancellation preserves the degree and logo navigation returns home from a degree and empty importer. New English menu and confirmation captures use fictional data.

Last-year removal dialog verified: names Year 2, reports two courses, warns that the year and contents are removed, and focuses Cancel. Cancellation preserves the year. Semester Add/Edit uses predefined localized A/B/Summer options; English and Hebrew dropdowns verified. Existing custom names are preserved when editing.
