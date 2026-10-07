# Verification

Verified on Windows x64 with .NET SDK 10.0.401.

- Solution build: zero warnings or errors.
- 107 xUnit tests: original calculation/storage coverage plus deterministic parsing, supported delimiters and headers, Hebrew/English destinations, preserved invalid input, size limits, manual mapping, normalized output, bulk defaults/overrides, exclusion, duplicates, atomic import, and a live HTTP test proving parse/preview are read-only and import requires confirmation.
- 5 browser-logic tests: exact decimal math, exclusions, component weighting, simulated propagation/isolation, distribution boundaries.
- Self-contained Windows x64 publish includes the runtime, SQLite native library, and all browser assets.
- Browser checks: degree creation, direct course editing, component course creation, warnings for excess weights, vertical semesters, simulation controls, immediate exact component entry, restoration after simulation, Hebrew RTL, English LTR, and a 390-pixel mobile layout.
- Live release checks: automatic loopback-port selection, persisted real component grades after restart, backup validation/restoration, malformed backup rejection, static assets, and a windowless executable launch. Browser launch completed without recording an error.
- Parser browser checks: mixed Hebrew/English input, editable correction, destination defaults, explicit destinations, excluded rows, duplicate Skip/Import anyway choices, normalized clipboard copy, confirmation, and successful import to the chosen semester. Manual column mapping was also verified in the published release.
- Transcript checks: both complete user examples (21 Hebrew courses and 10 English courses), calendar-year mapping, reversed English rows, summer semesters, missing credits, and differing credit values. The collapsed format guide was verified in the browser.
- Course actions now open from a three-dot menu; the Edit action was verified. Eleven real demo screenshots are included under `screenshots/` and referenced in the README.

Reference Android screenshots were not attached; the implementation follows the written layout specification. No cloud sync, accounts, or hypothetical-course creation is implemented, as requested.

Calendar-year mapping controls verified in the browser: 2024 → Year 2 and 2025 → Year 3 update the course cards and normalized output without saving.
