"use strict";
Object.assign(words.en, {
  textImport: "Import grades",
  pasteTitle: "Paste your courses",
  pasteHelp:
    "Paste a spreadsheet or course list. Review and correct the results before importing. Everything stays on this computer.",
  pasteLabel: "Course text",
  supportedFormats: "Supported formats",
  formatsTables: "Spreadsheet tabs, CSV, pipes or semicolons. Optional English or Hebrew headers: name, credits, grade, year, semester.",
  formatsLists: "Simple course lists: name, credits and grade. A blank grade is allowed.",
  formatsTranscripts: "Academic transcripts in Hebrew or English, including course codes and reversed English rows. Fall / א = A, Spring / ב = B, Summer / ק = Summer.",
  formatsYears: "Calendar-year headings map chronologically to Year 1, Year 2, etc. Review destinations. Missing or ambiguous credits must be filled in before import.",
  parseText: "Parse text",
  clearText: "Clear",
  reviewTitle: "Review courses",
  reviewHelp:
    "Check where each course will be saved, then review its details below. Nothing is saved until you confirm.",
  targetDegree: "Target degree",
  defaultYear: "Year for missing destinations",
  calendarMapping: "Match transcript years to your degree",
  detectedYearsHelp: "Years were detected in your text. Choose which degree year each one belongs to; this applies to all its courses.",
  transcriptYear: "Year in transcript", degreeYear: "Year in degree", individualYears: "Set per course",
  detectedSemesterHelp: "Detected semesters are kept for each course. You can change an individual destination below. Missing years or semesters will be created when you confirm.",
  fallbackTitle: "Courses with missing year or semester",
  fallbackHelp: "These choices fill in missing information only. Detected years and semesters stay as shown below.",
  fallbackUnused: "Every included course already has a year and semester. These choices are currently unused.",
  calendarMappingHelp: "Suggested destinations are editable. Changing a calendar year updates all its courses; you can still change individual courses below.",
  defaultSemester: "Semester for missing destinations",
  useDefault: "Use default",
  includeRow: "Include in import",
  sourceLine: "Original text",
  unparsedTitle: "Lines needing attention",
  unparsedHelp:
    "These lines were not understood. Add a review card or keep them excluded; the original text is preserved below.",
  addReviewRow: "Add review card",
  advancedMapping: "Adjust column mapping",
  mappingHelp:
    "Choose what each column means, then parse the original text again. This replaces the current review cards.",
  column: "Column",
  ignoreColumn: "Keep as unused text",
  applyMapping: "Reparse with mapping",
  normalizedTitle: "Normalized text",
  copyNormalized: "Copy normalized text",
  copied: "Copied",
  copyFailed: "Copy is unavailable. Select and copy the read-only text below.",
  importCourses: "Import courses",
  confirmImport: "Confirm course import",
  countToImport: "Courses to import",
  duplicatesTitle: "Possible duplicate courses",
  skipDuplicates: "Skip duplicates",
  importDuplicates: "Import anyway",
  duplicatesHelp:
    "Duplicates match a course name in the same destination semester. Existing courses will not be overwritten.",
  skippedDuplicates: "Duplicates skipped",
  unparsedRemaining: "Unparsed lines will not be imported",
  emptyImport: "Select at least one valid course to import.",
  imported: "Courses imported",
  discardDraft: "Discard this unsaved import draft?",
  confirmText: "Import these reviewed courses into the selected degree?",
  parseBusy: "Parsing…",
  importBusy: "Importing…",
  fixBeforeImport: "Correct highlighted fields before importing.",
  "empty-text": "Paste some course text first.",
  "text-too-large": "Text is limited to 50,000 characters.",
  "too-many-lines": "Use at most 1,000 non-empty lines per import.",
  "invalid-mapping":
    "Each column can map to one field, and each field can be used only once.",
  "unparsed-line": "This line could not be parsed safely.",
  "calendar-years-mapped": "Academic years are mapped chronologically to Year 1, Year 2, and so on. Review the destinations before importing.",
  "different-credit-values": "The transcript contains different credit values. Review the selected credits against the original line.",
  "non-numeric-status": "This course has a status instead of a numeric grade. It will be imported with a blank grade and excluded from averages. See the original text.",
  "inferred-columns":
    "Columns were inferred as name, credits, grade, year, semester. Please review them.",
  "invalid-name": "Enter a course name of 1–200 characters.",
  "invalid-credits": "Credits must be a positive number, at most 1,000.",
  "invalid-grade": "Enter a grade from 0 to 100, or leave it blank.",
  "unknown-year": "Year was not recognized. Select a year or use the default.",
  "unknown-semester":
    "Semester was not recognized. Select a semester or use the default.",
  "unmapped-column":
    "Extra text was preserved in the original line. Check the column mapping if needed.",
  "column-count": "The number of cells differs from the header.",
  "missing-degree": "Choose an existing degree.",
  "invalid-destination":
    "The selected year and semester must already exist in this degree.",
  "confirmation-required": "Review and confirm before importing.",
  "invalid-import": "The import request is invalid.",
});
Object.assign(words.he, {
  textImport: "ייבוא ציונים",
  pasteTitle: "הדבקת רשימת קורסים",
  pasteHelp:
    "הדביקו גיליון או רשימת קורסים. בדקו ותקנו את התוצאות לפני הייבוא. הכול נשאר במחשב הזה.",
  pasteLabel: "טקסט הקורסים",
  supportedFormats: "פורמטים נתמכים",
  formatsTables: "טאבים מגיליון, CSV, קווים אנכיים או נקודה־פסיק. ניתן להוסיף כותרות בעברית או באנגלית: שם, נק״ז, ציון, שנה וסמסטר.",
  formatsLists: "רשימות קורסים פשוטות: שם, נק״ז וציון. אפשר להשאיר ציון ריק.",
  formatsTranscripts: "גיליונות ציונים בעברית או באנגלית, כולל מספרי קורס ושורות באנגלית בסדר הפוך. Fall / א = א, Spring / ב = ב, Summer / ק = קיץ.",
  formatsYears: "כותרות שנות לימודים ממופות לפי הסדר הכרונולוגי לשנה 1, שנה 2 וכן הלאה. יש לבדוק את היעדים ולמלא נק״ז חסרות או לא ברורות לפני הייבוא.",
  parseText: "פענוח טקסט",
  clearText: "ניקוי",
  reviewTitle: "בדיקת הקורסים",
  reviewHelp:
    "בדקו לאן ייובא כל קורס, ואז עברו על הפרטים בהמשך. הקורסים יישמרו רק לאחר אישור.",
  targetDegree: "תואר יעד",
  defaultYear: "שנה למידע חסר",
  calendarMapping: "התאמת שנות הגיליון לשנים בתואר",
  detectedYearsHelp: "זוהו שנות לימודים בטקסט. בחרו לאיזו שנה בתואר שייכת כל שנת לימודים; הבחירה תחול על כל הקורסים שלה.",
  transcriptYear: "שנה בגיליון", degreeYear: "שנה בתואר", individualYears: "לפי קורס",
  detectedSemesterHelp: "הסמסטרים שזוהו נשמרים לכל קורס. ניתן לשנות יעד של קורס בהמשך. שנים או סמסטרים חסרים ייווצרו בעת האישור.",
  fallbackTitle: "קורסים ללא שנה או סמסטר",
  fallbackHelp: "הבחירות האלה משלימות רק מידע חסר. שנים וסמסטרים שזוהו נשארים כפי שמוצג בהמשך.",
  fallbackUnused: "לכל הקורסים שנכללו כבר יש שנה וסמסטר. הבחירות האלה אינן בשימוש כרגע.",
  calendarMappingHelp: "היעדים המוצעים ניתנים לשינוי. שינוי שנת לימודים מעדכן את כל הקורסים שלה; ניתן לשנות גם קורס בודד בהמשך.",
  defaultSemester: "סמסטר למידע חסר",
  useDefault: "ברירת מחדל",
  includeRow: "לכלול בייבוא",
  sourceLine: "הטקסט המקורי",
  unparsedTitle: "שורות שדורשות בדיקה",
  unparsedHelp:
    "שורות אלו לא פוענחו. ניתן להוסיף כרטיס לעריכה או להשאיר אותן מחוץ לייבוא. הטקסט המקורי נשמר למטה.",
  addReviewRow: "הוספת כרטיס לעריכה",
  advancedMapping: "התאמת מיפוי עמודות",
  mappingHelp:
    "בחרו מה מייצגת כל עמודה ואז פענחו שוב את הטקסט המקורי. פעולה זו מחליפה את הכרטיסים הנוכחיים.",
  column: "עמודה",
  ignoreColumn: "שמירה כטקסט לא משויך",
  applyMapping: "פענוח מחדש לפי המיפוי",
  normalizedTitle: "טקסט מסודר",
  copyNormalized: "העתקת הטקסט המסודר",
  copied: "הועתק",
  copyFailed: "העתקה אוטומטית אינה זמינה. ניתן לבחור ולהעתיק את הטקסט למטה.",
  importCourses: "ייבוא קורסים",
  confirmImport: "אישור ייבוא קורסים",
  countToImport: "קורסים לייבוא",
  duplicatesTitle: "קורסים כפולים אפשריים",
  skipDuplicates: "דילוג על כפולים",
  importDuplicates: "ייבוא בכל זאת",
  duplicatesHelp:
    "כפילות מזוהה לפי שם קורס באותו סמסטר יעד. קורסים קיימים לא יידרסו.",
  skippedDuplicates: "כפולים שדולגו",
  unparsedRemaining: "שורות שלא פוענחו לא ייובאו",
  emptyImport: "בחרו לפחות קורס תקין אחד לייבוא.",
  imported: "הקורסים יובאו",
  discardDraft: "לבטל את טיוטת הייבוא שלא נשמרה?",
  confirmText: "לייבא את הקורסים שנבדקו לתואר שנבחר?",
  parseBusy: "מפענח…",
  importBusy: "מייבא…",
  fixBeforeImport: "יש לתקן שדות מסומנים לפני הייבוא.",
  "empty-text": "יש להדביק טקסט קורסים תחילה.",
  "text-too-large": "הטקסט מוגבל ל־50,000 תווים.",
  "too-many-lines": "ניתן לייבא עד 1,000 שורות לא ריקות בכל פעם.",
  "invalid-mapping":
    "כל עמודה משויכת לשדה אחד, וכל שדה ניתן לשיוך פעם אחת בלבד.",
  "unparsed-line": "לא ניתן לפענח את השורה בבטחה.",
  "calendar-years-mapped": "שנות הלימודים ממופות לפי סדר כרונולוגי לשנה 1, שנה 2 וכן הלאה. יש לבדוק את היעדים לפני הייבוא.",
  "different-credit-values": "בשורה מופיעים ערכי נק״ז שונים. יש לבדוק את הנק״ז שנבחרו מול הטקסט המקורי.",
  "non-numeric-status": "לקורס יש סטטוס במקום ציון מספרי. הוא ייובא עם ציון ריק ולא ייכלל בממוצע. ניתן לראות את הסטטוס בטקסט המקורי.",
  "inferred-columns":
    "העמודות זוהו כשם, נק״ז, ציון, שנה וסמסטר. יש לבדוק אותן.",
  "invalid-name": "יש להזין שם קורס באורך 1–200 תווים.",
  "invalid-credits": "נק״ז חייבות להיות מספר חיובי שאינו עולה על 1,000.",
  "invalid-grade": "יש להזין ציון בין 0 ל־100 או להשאיר ריק.",
  "unknown-year": "השנה לא זוהתה. בחרו שנה או השתמשו בברירת המחדל.",
  "unknown-semester": "הסמסטר לא זוהה. בחרו סמסטר או השתמשו בברירת המחדל.",
  "unmapped-column":
    "טקסט נוסף נשמר בשורה המקורית. יש לבדוק את מיפוי העמודות במידת הצורך.",
  "column-count": "מספר התאים שונה ממספר העמודות בכותרת.",
  "missing-degree": "יש לבחור תואר קיים.",
  "invalid-destination": "בחרו שנה וסמסטר.",
  "confirmation-required": "יש לבדוק ולאשר לפני הייבוא.",
  "invalid-import": "בקשת הייבוא אינה תקינה.",
});
let importDraft = null,
  previewSequence = 0;
words.en.switchToBinary = "Use pass / fail";
words.en.switchToNumeric = "Use numeric grade";
words.he.switchToBinary = "מעבר לעובר / נכשל";
words.he.switchToNumeric = "מעבר לציון מספרי";
words.en.acceptBlankGrade = "Leave grade blank";
words.he.acceptBlankGrade = "השארת הציון ריק";
Object.assign(words.en, { addedCourses: "New courses", updatedCourses: "Courses to update", unchangedCourses: "Unchanged courses", updateDetails: "Review changes to existing courses", alreadyCurrent: "These courses are already up to date. Nothing needs to be imported.", repeatedRows: "Repeated rows in this draft use the last included occurrence.", replacesComponents: "The imported grade will replace the component calculation.", mergesDuplicates: "Existing duplicate entries will be merged into one course.", "import-changed": "Saved courses changed after review. Close this confirmation and review the import again." });
Object.assign(words.he, { addedCourses: "קורסים חדשים", updatedCourses: "קורסים לעדכון", unchangedCourses: "קורסים ללא שינוי", updateDetails: "בדיקת השינויים בקורסים קיימים", alreadyCurrent: "הקורסים כבר מעודכנים. אין צורך בייבוא נוסף.", repeatedRows: "שורות חוזרות בטיוטה משתמשות בשורה האחרונה שנכללה.", replacesComponents: "הציון המיובא יחליף את החישוב לפי רכיבים.", mergesDuplicates: "רשומות כפולות קיימות יאוחדו לקורס אחד.", "import-changed": "הקורסים השמורים השתנו מאז הבדיקה. סגרו את האישור ובדקו את הייבוא שוב." });
function openTextImport() {
  if (simulation || !data.degrees.length) return;
  importDraft = {
    text: "",
    degreeId: selected || data.degrees[0].id,
    defaultYear: selected ? yearIndex + 1 : 1,
    defaultSemester: "A",
    rows: [],
    result: null,
    duplicatePolicy: "update",
    preview: null,
  };
  importDraft.defaultSemester =
    importDegree().years[importDraft.defaultYear - 1].semesters[0]?.name || "";
  render();
}
const importDegree = () =>
  data.degrees.find((d) => d.id === importDraft.degreeId);
const yearOptions = (value, allowDefault) =>
  `${allowDefault ? `<option value="">${t("useDefault")}</option>` : ""}${importDegree()
    .years.map(
      (y, i) =>
        `<option value="${i + 1}" ${value === i + 1 ? "selected" : ""}>${yearName(i)}</option>`,
    )
    .join(
      "",
    )}${value && value > importDegree().years.length ? `<option value="${value}" selected>${yearName(value - 1)}</option>` : ""}`;
function semesterOptions(year, value, allowDefault) {
  const semesters =
    importDegree().years[(year || importDraft.defaultYear) - 1]?.semesters ||
    [];
  return `${allowDefault ? `<option value="">${t("useDefault")}</option>` : ""}${semesters.map((s) => `<option value="${esc(s.name)}" ${value === s.name ? "selected" : ""}>${esc(semName(s))}</option>`).join("")}${value && !semesters.some((s) => s.name === value) ? `<option value="${esc(value)}" selected>${esc(semName({ name: value }))}</option>` : ""}`;
}
function renderTextImport() {
  const draft = importDraft;
  $("#app").innerHTML =
    `<div class="heading"><div>${button("leaveTextImport", t("back"))}<h1>${t("textImport")}</h1><p>${t("pasteHelp")}</p></div></div><section class="card"><div class="paste-heading"><h2>${t("pasteTitle")}</h2></div><details class="format-guide"><summary>${t("supportedFormats")}</summary><div><p>${t("formatsTables")}</p><pre dir="ltr">Creative Coding | 3 | 81</pre><p>${t("formatsLists")}</p><pre dir="ltr">Creative Coding 3 credits 81</pre><p>${t("formatsTranscripts")}</p><pre dir="auto">שנת לימודים 2030&#10;א 410101 יצירה דיגיטלית שיעור 3.0 3.0 81</pre><pre dir="ltr">ACADEMIC YEAR 2030&#10;Fall 410101 Creative Coding 3.0 81</pre><p>${t("formatsYears")}</p></div></details>${label(t("pasteLabel"), `<textarea id="pasteText" maxlength="50000" rows="7" dir="auto">${esc(draft.text)}</textarea>`)}<div class="actions">${button("parseText", t("parseText"), 'class="primary"')}${button("clearText", t("clearText"))}</div><p id="parseError" role="alert"></p></section>${draft.result ? `<section class="card"><h2>${t("reviewTitle")}</h2><p>${t("reviewHelp")}</p><div class="import-target">${label(t("targetDegree"), `<select id="importDegree">${data.degrees.map((d) => `<option value="${d.id}" ${d.id === draft.degreeId ? "selected" : ""}>${esc(d.name)}</option>`).join("")}</select>`)}</div>${calendarMappingPanel()}${fallbackDestinationPanel()}${[...new Set(draft.result.warnings.map(w => w.code).filter(code => !["calendar-years-mapped", "unparsed-line"].includes(code)))].map(code => `<p class="import-note">${esc(t(code))}</p>`).join("")}${draft.result.mappingRecommended && draft.result.columnCount > 0 ? `<details class="mapping"><summary>${t("advancedMapping")}</summary><p>${t("mappingHelp")}</p><div class="import-defaults">${draft.result.columnMapping.map((m, i) => label(t("column") + " " + (i + 1), `<select data-map="${i}"><option value="">${t("ignoreColumn")}</option>${["name", "credits", "grade", "year", "semester"].map((field) => `<option value="${field}" ${m === field ? "selected" : ""}>${t(field === "credits" ? "creditPoints" : field)}</option>`).join("")}</select>`)).join("")}</div>${button("applyMapping", t("applyMapping"))}</details>` : ""}</section><form id="reviewForm"><div id="reviewRows">${groupedReviewCards()}</div></form>${draft.result.unparsedLines.length ? `<section class="card"><details class="unparsed-section"><summary>${t("unparsedTitle")} (${draft.result.unparsedLines.length})</summary><p>${t("unparsedHelp")}</p>${draft.result.unparsedLines.map((l, i) => `<div class="unparsed-line"><pre>${esc(l.text)}</pre>${button("recoverLine", t("addReviewRow"), `data-line="${i}"`)}</div>`).join("")}</details></section>` : ""}<section class="card"><h3>${t("normalizedTitle")}</h3><p id="previewError" role="status"></p><div id="duplicateWarnings"></div>${label(t("normalizedTitle"), `<textarea id="normalizedText" readonly rows="4" dir="auto"></textarea>`)}<div class="actions">${button("copyNormalized", t("copyNormalized"))}${button("reviewImport", t("importCourses"), 'class="primary"')}</div><p id="importCount" aria-live="polite"></p></section>` : ""}`;
  if (draft.result) {
    validateReview();
    refreshImportPreview();
  }
}
function calendarMappingPanel() {
  const years = [...new Set(importDraft.rows.map(r => r.rawValues?.calendarYear).filter(Boolean))].sort();
  if (!years.length) return "";
  return `<section class="calendar-destinations"><h3>${t("calendarMapping")}</h3><p>${t("detectedYearsHelp")}</p><div class="calendar-grid">${years.map(calendar => {
    const rows = importDraft.rows.filter(r => r.rawValues?.calendarYear === calendar);
    const value = rows.every(r => r.year === rows[0].year) ? rows[0].year : null;
    return `<div class="calendar-destination"><div><span class="destination-label">${t("transcriptYear")}</span><strong><bdi>${esc(calendar)}</bdi></strong><small>${rows.length} ${t("courses")}</small></div><span class="mapping-arrow" aria-hidden="true">${data.language === "he" ? "←" : "→"}</span>${label(t("degreeYear"), `<select data-calendar-year="${calendar}" aria-label="${esc(calendar + ' · ' + t('degreeYear'))}">${value === null ? `<option value="" selected>${t("individualYears")}</option>` : ""}${yearOptions(value, false)}</select>`)}</div>`;
  }).join("")}</div><p class="destination-note">${t("detectedSemesterHelp")}</p></section>`;
}
function fallbackDestinationPanel() {
  const missing = importDraft.rows.filter(row => row.included && (!row.year || !row.semester)).length;
  if (!missing) return "";
  return `<details class="fallback-destinations" open><summary>${t("fallbackTitle")}${missing ? ` · ${missing} ${t("courses")}` : ""}</summary><p>${t(missing ? "fallbackHelp" : "fallbackUnused")}</p><div class="fallback-fields">${label(t("defaultYear"), `<select id="defaultYear">${yearOptions(importDraft.defaultYear, false)}</select>`)}${label(t("defaultSemester"), `<select id="defaultSemester">${semesterOptions(importDraft.defaultYear, importDraft.defaultSemester, false)}</select>`)}</div></details>`;
}
function groupedReviewCards() {
  const years = new Map();
  importDraft.rows.forEach((row, index) => {
    const year = row.year || importDraft.defaultYear;
    const semester = row.semester || importDraft.defaultSemester;
    if (!years.has(year)) years.set(year, new Map());
    const semesters = years.get(year);
    if (!semesters.has(semester)) semesters.set(semester, []);
    semesters.get(semester).push({ row, index });
  });
  const order = name => {
    const standard = ["A", "B", "Summer"].indexOf(name);
    return standard < 0 ? 3 : standard;
  };
  return [...years].sort((a, b) => a[0] - b[0]).map(([year, semesters]) =>
    `<section class="import-year-group"><h2 class="import-year-heading">${esc(yearName(year - 1))}</h2>${[...semesters].sort((a,b) => order(a[0]) - order(b[0])).map(([semester, rows]) =>
      `<section class="import-semester-group"><div class="import-semester-heading"><h3>${esc(semName({ name: semester }))}</h3><span>${rows.length} ${t("courses")}</span></div>${rows.map(({ row, index }) => reviewCard(row, index)).join("")}</section>`
    ).join("")}</section>`
  ).join("");
}
function reviewCard(row, i) {
  return `<article class="card import-row ${row.warnings.length ? "uncertain" : ""}" data-row="${i}"><div class="import-row-head"><h3 data-row-title>${esc(row.name || t("courses"))}</h3><label class="include-row"><input type="checkbox" data-field="included" ${row.included ? "checked" : ""}>${t("includeRow")}</label></div><div class="import-fields">${label(t("name"), `<input data-field="name" value="${esc(row.name)}" required maxlength="200">`)}${label(t("creditPoints"), `<input data-field="credits" type="number" min="0.01" max="1000" step="any" required value="${esc(row.credits ?? "")}">`)}<div class="import-grade-field">${row.passed != null ? label(t("binary"), `<select data-field="passed"><option value="true" ${row.passed ? "selected" : ""}>${t("passed")}</option><option value="false" ${!row.passed ? "selected" : ""}>${t("failed")}</option></select>`) : label(t("grade"), `<input data-field="grade" type="number" min="0" max="100" step="1" value="${esc(row.grade ?? "")}">`)}${button("toggleImportGradeMode", t(row.passed != null ? "switchToNumeric" : "switchToBinary"), `class="grade-mode-toggle" data-row-index="${i}" aria-pressed="${row.passed != null}"`)}</div>${label(t("year"), `<select data-field="year">${yearOptions(row.year, true)}</select>`)}${label(t("semester"), `<select data-field="semester">${semesterOptions(row.year, row.semester, true)}</select>`)}</div><div class="row-validation" aria-live="polite"></div>${row.invalidGradeUnresolved ? button("acceptBlankGrade", t("acceptBlankGrade"), `data-row-index="${i}"`) : ""}${row.warnings.map((w) => `<p class="import-note" data-warning="${w}">${esc(t(w))}${["invalid-credits", "invalid-grade", "unknown-year", "unknown-semester"].includes(w) ? ` <span dir="auto">(${esc(row.rawValues?.[{ "invalid-credits": "credits", "invalid-grade": "grade", "unknown-year": "year", "unknown-semester": "semester" }[w]] || "")})</span>` : ""}</p>`).join("")}<details><summary>${t("sourceLine")}</summary><pre>${esc(row.originalLine)}</pre></details></article>`;
}
function importRequest(confirmed = false) {
  return {
    revision: data.revision,
    degreeId: importDraft.degreeId,
    defaultYear: importDraft.defaultYear,
    defaultSemester: importDraft.defaultSemester,
    rows: importDraft.rows.map((r) => ({
      name: r.name,
      credits: r.credits,
      grade: r.grade,
      passed: r.passed ?? null,
      year: r.year,
      semester: r.semester,
      included: r.included,
    })),
    duplicatePolicy: importDraft.duplicatePolicy,
    reviewToken: confirmed ? importDraft.preview?.reviewToken : null,
    confirmed,
  };
}
function importChangeSummary(p) {
  const grade = (number, passed) => passed != null ? t(passed ? "passed" : "failed") : number ?? "—";
  return `<div class="import-change-counts"><span>${t("addedCourses")}: <strong>${p.added}</strong></span><span>${t("updatedCourses")}: <strong>${p.updated}</strong></span><span>${t("unchangedCourses")}: <strong>${p.unchanged}</strong></span></div>${p.updates.length ? `<details class="mapping" open><summary>${t("updateDetails")}</summary>${p.updates.map(u => `<article class="import-update"><strong dir="auto">${esc(u.name)}</strong><small>${esc(yearName(u.year - 1))} · ${esc(semName({name:u.semester}))}</small><p>${t("grade")}: <bdi>${esc(grade(u.oldGrade,u.oldPassed))} → ${esc(grade(u.newGrade,u.newPassed))}</bdi> · ${t("creditPoints")}: <bdi>${formatCredits(u.oldCredits)} → ${formatCredits(u.newCredits)}</bdi></p>${u.replacesComponents ? `<p class="import-note">${t("replacesComponents")}</p>` : ""}${u.removedDuplicates ? `<p class="import-note">${t("mergesDuplicates")}</p>` : ""}</article>`).join("")}</details>` : ""}${p.repeatedRows.length ? `<p class="import-note">${t("repeatedRows")} <span dir="auto">${p.repeatedRows.map(esc).join(", ")}</span></p>` : ""}${p.count === 0 ? `<p>${t("alreadyCurrent")}</p>` : ""}`;
}
function validateReview() {
  let valid = true;
  $("#reviewRows")
    ?.querySelectorAll("[data-row]")
    .forEach((card) => {
      const row = importDraft.rows[Number(card.dataset.row)],
        errors = [];
      card.classList.toggle("excluded", !row.included);
      card
        .querySelectorAll("[data-field]:not([type=checkbox])")
        .forEach((field) => {
          field.disabled = !row.included;
          field.setCustomValidity("");
        });
      if (row.included) {
        if (!row.name.trim() || row.name.length > 200)
          errors.push("invalid-name");
        if (
          row.credits === null ||
          !Number.isFinite(row.credits) ||
          row.credits <= 0 ||
          row.credits > 1000
        )
          errors.push("invalid-credits");
        if (
          row.invalidGradeUnresolved ||
          (row.grade !== null &&
            (!validGrade(row.grade)))
        )
          errors.push("invalid-grade");
        const y =
          importDegree().years[(row.year || importDraft.defaultYear) - 1];
        if (
          !(["A", "B", "Summer"].includes(row.semester || importDraft.defaultSemester) && (row.year || importDraft.defaultYear) >= 1 && (row.year || importDraft.defaultYear) <= 100) && !y?.semesters.some(
            (s) => s.name === (row.semester || importDraft.defaultSemester),
          )
        )
          errors.push("invalid-destination");
        if ([...card.querySelectorAll("input")].some((f) => !f.validity.valid))
          errors.push("fixBeforeImport");
      }
      card.classList.toggle("invalid", errors.length > 0);
      card.querySelector(".row-validation").textContent = errors
        .map(t)
        .join(" ");
      if (errors.length) valid = false;
    });
  return valid;
}
async function refreshImportPreview() {
  const sequence = ++previewSequence;
  importDraft.preview = null;
  if (!importDraft.rows.length || !validateReview()) {
    $("#previewError").textContent = importDraft.rows.length ? t("fixBeforeImport") : "";
    document.querySelector("[data-action=reviewImport]").disabled = true;
    $("#normalizedText").value = "";
    $("#importCount").textContent = "";
    $("#duplicateWarnings").replaceChildren();
    return;
  }
  try {
    const result = await request(
      "/api/import/preview",
      "POST",
      importRequest(),
    );
    if (sequence !== previewSequence || !importDraft) return;
    importDraft.preview = result;
    $("#previewError").textContent = "";
    $("#normalizedText").value = result.normalizedText;
    $("#importCount").textContent = `${t("countToImport")}: ${result.count}`;
    $("#duplicateWarnings").innerHTML = importChangeSummary(result);
    document.querySelector("[data-action=reviewImport]").disabled = result.count === 0;
  } catch (error) {
    if (sequence === previewSequence && importDraft)
      $("#previewError").textContent = t(error.message);
  }
}
$("#app").addEventListener("input", (event) => {
  if (!importDraft) return;
  const el = event.target;
  if (el.id === "pasteText") {
    importDraft.text = el.value;
    return;
  }
  const card = el.closest("[data-row]");
  if (card && el.dataset.field) {
    const row = importDraft.rows[Number(card.dataset.row)],
      field = el.dataset.field;
    if (field === "passed") { row.passed = el.value === "true"; row.savedPassed = row.passed; row.grade = null; validateReview(); refreshImportPreview(); return; }
    row[field] =
      field === "included"
        ? el.checked
        : ["credits", "grade", "year"].includes(field)
          ? el.value === ""
            ? null
            : Number(el.value)
          : el.value || null;
    if (field === "name") row.name = el.value;
    if (field === "grade") row.invalidGradeUnresolved = false;
    const corrected = {
      name: "invalid-name",
      credits: "invalid-credits",
      grade: "invalid-grade",
      year: "unknown-year",
      semester: "unknown-semester",
    }[field];
    if (
      corrected &&
      el.validity.valid &&
      (field !== "name" || row.name.trim())
    ) {
      row.warnings = row.warnings.filter((w) => w !== corrected);
      card.querySelector(`[data-warning="${corrected}"]`)?.remove();
      card.classList.toggle("uncertain", row.warnings.length > 0);
    }
    if (field === "year")
      card.querySelector("[data-field=semester]").innerHTML = semesterOptions(
        row.year,
        row.semester,
        true,
      );
    card.querySelector("[data-row-title]").textContent =
      row.name || t("courses");
    if (field === "year" || field === "semester") {
      renderTextImport();
      return;
    }
    validateReview();
    refreshImportPreview();
  }
});
$("#app").addEventListener("submit", (event) => {
  if (event.target.id === "reviewForm") event.preventDefault();
});
$("#app").addEventListener("change", (event) => {
  if (!importDraft) return;
  const el = event.target;
  if (el.dataset.calendarYear) {
    if (!el.value) return;
    importDraft.rows.filter(r => r.rawValues?.calendarYear === el.dataset.calendarYear).forEach(row => { row.year = Number(el.value); row.warnings = row.warnings.filter(w => w !== "unknown-year"); });
    renderTextImport();
  } else if (el.id === "importDegree") {
    importDraft.degreeId = el.value;
    importDraft.defaultYear = 1;
    importDraft.defaultSemester =
      importDegree().years[0].semesters[0]?.name || "";
    renderTextImport();
  } else if (el.id === "defaultYear") {
    importDraft.defaultYear = Number(el.value);
    importDraft.defaultSemester =
      importDegree().years[importDraft.defaultYear - 1].semesters[0]?.name ||
      "";
    renderTextImport();
  } else if (el.id === "defaultSemester") {
    importDraft.defaultSemester = el.value;
    renderTextImport();
  } else if (el.id === "duplicatePolicy") {
    importDraft.duplicatePolicy = el.value;
    refreshImportPreview();
  }
});
$("#app").addEventListener("click", async (event) => {
  const button = event.target.closest("[data-action]");
  if (!button || !importDraft) return;
  const action = button.dataset.action;
  try {
    if (action === "leaveTextImport") {
      if (
        (!importDraft.text && !importDraft.rows.length) ||
        confirm(t("discardDraft"))
      ) {
        importDraft = null;
        previewSequence++;
        render();
      }
    } else if (action === "clearText") {
      if (!importDraft.rows.length || confirm(t("discardDraft"))) {
        importDraft.text = "";
        importDraft.rows = [];
        importDraft.result = null;
        previewSequence++;
        renderTextImport();
      }
    } else if (action === "parseText" || action === "applyMapping") {
      const mapping =
        action === "applyMapping"
          ? [...document.querySelectorAll("[data-map]")].map(
              (s) => s.value || null,
            )
          : null;
      if (importDraft.result && !confirm(t("mappingHelp"))) return;
      button.disabled = true;
      button.textContent = t("parseBusy");
      const result = await request("/api/import/parse-text", "POST", {
        text: importDraft.text,
        columnMapping: mapping,
      });
      importDraft.result = result;
      importDraft.rows = result.courses.map((c) => ({
        ...c,
        included: true,
        invalidGradeUnresolved:
          c.grade === null && c.warnings.includes("invalid-grade"),
      }));
      renderTextImport();
    } else if (action === "toggleImportGradeMode") {
      const row = importDraft.rows[Number(button.dataset.rowIndex)];
      if (row.passed != null) {
        row.savedPassed = row.passed;
        row.passed = null;
        row.grade = row.savedNumericGrade ?? null;
      } else {
        row.savedNumericGrade = row.grade;
        row.passed = row.savedPassed ?? true;
        row.grade = null;
        row.invalidGradeUnresolved = false;
        row.warnings = row.warnings.filter(w => w !== "invalid-grade");
      }
      renderTextImport();
    } else if (action === "acceptBlankGrade") {
      const row = importDraft.rows[Number(button.dataset.rowIndex)];
      row.invalidGradeUnresolved = false;
      row.grade = null;
      row.warnings = row.warnings.filter((w) => w !== "invalid-grade");
      renderTextImport();
    } else if (action === "recoverLine") {
      const line =
        importDraft.result.unparsedLines[Number(button.dataset.line)];
      importDraft.rows.push({
        name: line.text,
        credits: null,
        grade: null,
        year: null,
        semester: null,
        included: true,
        originalLine: line.text,
        warnings: [],
        rawValues: {},
      });
      importDraft.result.unparsedLines.splice(Number(button.dataset.line), 1);
      renderTextImport();
    } else if (action === "copyNormalized") {
      await refreshImportPreview();
      if (!importDraft.preview) throw Error("fixBeforeImport");
      try {
        await navigator.clipboard.writeText($("#normalizedText").value);
        toast(t("copied"));
      } catch {
        $("#normalizedText").focus();
        $("#normalizedText").select();
        toast(t("copyFailed"));
      }
    } else if (action === "reviewImport") {
      if (!importDraft.rows.length || !validateReview()) {
        $("#reviewForm").reportValidity();
        throw Error("fixBeforeImport");
      }
      await refreshImportPreview();
      const p = importDraft.preview;
      if (!p || p.count === 0) throw Error(p?.unchanged ? "alreadyCurrent" : "emptyImport");
      const req = importRequest(true),
        target = importDegree().name;
      dialog(
        t("confirmImport"),
        `<h3>${esc(target)}</h3><p>${t("countToImport")}: <strong>${p.count}</strong></p><p>${t("confirmText")}</p>${importChangeSummary(p)}${importDraft.rows.filter((r) => r.included).some((r) => r.warnings.length) ? `<p class="import-note">${[...new Set(importDraft.rows.filter((r) => r.included).flatMap((r) => r.warnings))].map((w) => esc(t(w))).join("<br>")}</p>` : ""}${importDraft.result.unparsedLines.length ? `<p class="import-note">${t("unparsedRemaining")}: ${importDraft.result.unparsedLines.length}</p>` : ""}`,
        async () => {
          $("#save").disabled = true;
          try {
            await request("/api/import/courses", "POST", req);
            data = await request("/api/data");
            selected = req.degreeId;
            yearIndex = req.defaultYear - 1;
            importDraft = null;
            previewSequence++;
            render();
            toast(t("imported"));
          } finally {
            $("#save").disabled = false;
          }
        },
      );
      $("#save").textContent = t("importCourses");
    }
  } catch (error) {
    toast(t(error.message));
    const area = $("#parseError");
    if (area) area.textContent = t(error.message);
    button.disabled = false;
    if (action === "parseText") button.textContent = t("parseText");
    if (action === "applyMapping") button.textContent = t("applyMapping");
  }
});
if (data) render();
