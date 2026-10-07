# Verification

Verified on Windows x64 with .NET SDK 10.0.401.

- Solution build: zero warnings or errors.
- 26 xUnit tests: weighted/decimal-credit averages, blank grades, low grades, semester/year/degree aggregation, component rounding and bonuses, incomplete components, distribution boundaries, simulation cloning, SQLite persistence, JSON round-trip, rejected backups and missing fields.
- 5 browser-logic tests: exact decimal math, exclusions, component weighting, simulated propagation/isolation, distribution boundaries.
- Self-contained Windows x64 publish includes the runtime, SQLite native library, and all browser assets.
- Browser checks: degree creation, direct course editing, component course creation, warnings for excess weights, vertical semesters, simulation controls, immediate exact component entry, restoration after simulation, Hebrew RTL, English LTR, and a 390-pixel mobile layout.
- Live release checks: automatic loopback-port selection, persisted real component grades after restart, backup validation/restoration, malformed backup rejection, static assets, and a windowless executable launch. Browser launch completed without recording an error.

Reference Android screenshots were not attached; the implementation follows the written layout specification. No cloud sync, accounts, or hypothetical-course creation is implemented, as requested.
