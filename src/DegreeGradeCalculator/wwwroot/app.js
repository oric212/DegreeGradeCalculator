"use strict";
let data,
  selected = null,
  yearIndex = 0,
  simulation = null;
const $ = (s) => document.querySelector(s),
  esc = (s) =>
    String(s).replace(
      /[&<>"']/g,
      (c) =>
        ({
          "&": "&amp;",
          "<": "&lt;",
          ">": "&gt;",
          '"': "&quot;",
          "'": "&#39;",
        })[c],
    );
const words = {
  en: {
    home: "Your degrees",
    intro: "A clearer view of your academic journey.",
    addDegree: "Add degree",
    export: "Export backup",
    import: "Restore backup",
    shutdown: "Close app",
    open: "Open degree",
    average: "Degree average", yearAverage: "Year average", semesterAverage: "Semester average",
    creditPoints: "Credit points",
    noGradedCourses: "Add grades to see the grade distribution.",
    credits: "Counted credits", binary: "Pass / fail", passed: "Passed", failed: "Failed",
    courses: "Courses",
    courseOptions: "Course options", degreeOptions: "Degree options", deleteDegreeTitle: "Delete degree?", deleteDegreeHelp: "This permanently deletes this degree and all its courses. This cannot be undone.",
    required: "Required credits",
    edit: "Edit",
    delete: "Delete",
    name: "Name",
    duration: "Nominal duration / initial years",
    save: "Save",
    cancel: "Cancel",
    back: "All degrees",
    year: "Year",
    semester: "Semester",
    summer: "Summer semester",
    addYear: "Add year",
    removeYear: "Remove last year", removeYearTitle: "Remove last year?", removeYearHelp: "This permanently removes this year, its semesters, and all its courses. This cannot be undone.",
    addSemester: "Add semester",
    addCourse: "Add course",
    report: "Degree report",
    simulate: "Simulate grades",
    end: "End simulation",
    simulation: "Simulation mode · temporary changes",
    current: "Current average",
    newAverage: "New average",
    grade: "Grade",
    direct: "Direct grade",
    components: "Component calculation",
    mode: "Grade method",
    weight: "Weight %",
    addComponent: "Add component",
    totalWeight: "Total weighting",
    warning:
      "Component weights exceed 100%. Bonus weighting is applied without normalization.",
    reset: "Reset",
    confirm:
      "This permanently deletes the selected item and its contents. Continue?",
    restoreConfirm:
      "Replace all saved degrees with this backup? Export a backup first if needed.",
    empty: "Your next chapter starts here.",
    emptyText: "Create a degree, then add your courses and grades.",
    none: "No courses yet",
    saved: "Saved",
    error: "Unable to complete this action",
    closeConfirm:
      "Close the local application? You can restart it by opening the executable.",
    below: "Below 60",
    incomplete: "Incomplete components — no final grade",
    remove: "Remove",
    semesterName: "Semester name",
    language: "Language",
    reportTitle: "Academic overview",
  },
  he: {
    home: "התארים שלך",
    intro: "תמונה ברורה של הדרך האקדמית שלך.",
    addDegree: "הוספת תואר",
    export: "ייצוא גיבוי",
    import: "שחזור גיבוי",
    shutdown: "סגירת היישום",
    open: "פתיחת תואר",
    average: "ממוצע תואר", yearAverage: "ממוצע שנתי", semesterAverage: "ממוצע סמסטר",
    creditPoints: "נק״ז",
    noGradedCourses: "התפלגות הציונים תופיע לאחר הוספת ציונים.",
    credits: "נק״ז לחישוב", binary: "עובר / נכשל", passed: "עבר", failed: "נכשל",
    courses: "קורסים",
    courseOptions: "אפשרויות קורס", degreeOptions: "אפשרויות תואר", deleteDegreeTitle: "למחוק את התואר?", deleteDegreeHelp: "התואר וכל הקורסים שלו יימחקו לצמיתות. לא ניתן לבטל את המחיקה.",
    required: "נק״ז נדרשות",
    edit: "עריכה",
    delete: "מחיקה",
    name: "שם",
    duration: "משך התואר / מספר שנים התחלתי",
    save: "שמירה",
    cancel: "ביטול",
    back: "כל התארים",
    year: "שנה",
    semester: "סמסטר",
    summer: "קיץ",
    addYear: "הוספת שנה",
    removeYear: "הסרת השנה האחרונה", removeYearTitle: "להסיר את השנה האחרונה?", removeYearHelp: "השנה, הסמסטרים וכל הקורסים שלה יימחקו לצמיתות. לא ניתן לבטל את המחיקה.",
    addSemester: "הוספת סמסטר",
    addCourse: "הוספת קורס",
    report: "סיכום התואר",
    simulate: "סימולציית ציונים",
    end: "סיום סימולציה",
    simulation: "מצב סימולציה · שינויים זמניים",
    current: "ממוצע נוכחי",
    newAverage: "ממוצע חדש",
    grade: "ציון",
    direct: "ציון ישיר",
    components: "חישוב לפי רכיבים",
    mode: "שיטת חישוב הציון",
    weight: "משקל %",
    addComponent: "הוספת רכיב",
    totalWeight: "משקל כולל",
    warning: "משקל הרכיבים עולה על 100%. משקל הבונוס מחושב ללא נרמול.",
    reset: "איפוס",
    confirm: "הפריט וכל תוכנו יימחקו לצמיתות. להמשיך?",
    restoreConfirm:
      "להחליף את כל התארים השמורים בגיבוי זה? מומלץ לייצא גיבוי לפני כן.",
    empty: "הפרק הבא שלך מתחיל כאן.",
    emptyText: "הוסיפו תואר ולאחר מכן קורסים וציונים.",
    none: "עדיין אין קורסים",
    saved: "נשמר",
    error: "לא ניתן להשלים את הפעולה",
    closeConfirm:
      "לסגור את היישום המקומי? ניתן להפעילו שוב באמצעות קובץ ההפעלה.",
    below: "מתחת ל־60",
    incomplete: "רכיבים לא שלמים — אין ציון סופי",
    remove: "הסרה",
    semesterName: "שם הסמסטר",
    language: "שפה",
    reportTitle: "סקירה אקדמית",
  },
};
const t = (k) => words[data?.language || "en"][k] || k,
  uid = () => crypto.randomUUID(),
  clone = (o) => structuredClone(o);
// Decimal arithmetic keeps weights and credits exact until presentation.
const fraction = (n) => {
  let text = String(n),
    [base, exp] = text.toLowerCase().split("e"),
    places = (base.split(".")[1] || "").length - Number(exp || 0),
    digits = BigInt(base.replace(".", ""));
  return places >= 0
    ? [digits, 10n ** BigInt(places)]
    : [digits * 10n ** BigInt(-places), 1n];
};
const gcd = (a, b) => (b === 0n ? a : gcd(b, a % b)),
  reduce = (a) => {
    let g = gcd(a[0], a[1]);
    return [a[0] / g, a[1] / g];
  },
  plus = (a, b) => reduce([a[0] * b[1] + b[0] * a[1], a[1] * b[1]]),
  times = (a, b) => [a[0] * b[0], a[1] * b[1]],
  divide = (a, b) => [a[0] * b[1], a[1] * b[0]],
  exactSum = (items) => items.reduce(plus, [0n, 1n]);
const final = (c) => {
  if (c.passed != null) return null;
  if (!c.usesComponents) return c.grade;
  if (!c.components.length || c.components.some((p) => p.grade === null))
    return null;
  let raw = divide(
    exactSum(
      c.components.map((p) => times(fraction(p.weight), fraction(p.grade))),
    ),
    [100n, 1n],
  );
  return Number((raw[0] * 2n + raw[1]) / (raw[1] * 2n));
};
const courses = (d) =>
  d.years.flatMap((y) => y.semesters.flatMap((s) => s.courses));
function summary(cs) {
  const latest = new Map();
  cs.filter((c) => final(c) !== null || c.passed === true).forEach((c) => latest.set(c.name ?? c, c));
  const completed = [...latest.values()],
    counted = exactSum(completed.map(c => fraction(c.credits))),
    graded = completed.filter(c => final(c) !== null),
    creditFraction = exactSum(graded.map((c) => fraction(c.credits))),
    credits = Number(creditFraction[0]) / Number(creditFraction[1]),
    bins = [0, 0, 0, 0, 0];
  graded.forEach((c) => {
    let g = final(c);
    bins[g < 60 ? 0 : g < 70 ? 1 : g < 80 ? 2 : g < 90 ? 3 : 4]++;
  });
  return {
    average: credits
      ? divide(
          exactSum(
            graded.map((c) => times(fraction(final(c)), fraction(c.credits))),
          ),
          creditFraction,
        )
      : null,
    credits: Number(counted[0]) / Number(counted[1]),
    count: cs.length,
    bins,
  };
}
const avg = (n) => {
    if (n === null) return "—";
    let rounded = (n[0] * 200n + n[1]) / (n[1] * 2n);
    return (
      String(rounded / 100n) + "." + String(rounded % 100n).padStart(2, "0")
    );
  },
  colors = ["#d1e2f8", "#9bbfe9", "#6097d6", "#286bb5", "#103d78"];
function distribution(s) {
  let total = s.bins.reduce((a, b) => a + b, 0);
  if (!total) return `<p class="distribution-empty">${esc(t("noGradedCourses"))}</p>`;
  return `<div class="distribution" aria-label="${esc(t("grade"))}">${s.bins.map((n, i) => `<span style="width:${total ? (n / total) * 100 : 0}%;background:${colors[i]}"></span>`).join("")}</div><div class="legend">${s.bins.map((n, i) => `<span><i class="dot" style="background:${colors[i]}"></i><bdi>${[t("below"), "60–69", "70–79", "80–89", "90+"][i]}</bdi> · <bdi>${n}</bdi></span>`).join("")}</div>`;
}
const metric = (label, value) =>
  `<div class="metric"><small>${esc(label)}</small><strong>${value}</strong></div>`;
const button = (action, label, extra = "") =>
  `<button data-action="${action}" ${extra}>${esc(label)}</button>`;
const yearName = (i) =>
  data.language === "he"
    ? `שנה ${["א׳", "ב׳", "ג׳", "ד׳", "ה׳", "ו׳", "ז׳", "ח׳", "ט׳", "י׳"][i] || i + 1}`
    : `Year ${i + 1}`;
const semName = (s) =>
  s.name === "A"
    ? data.language === "he"
      ? "סמסטר א׳"
      : "Semester A"
    : s.name === "B"
      ? data.language === "he"
        ? "סמסטר ב׳"
        : "Semester B"
      : s.name === "Summer"
        ? t("summer")
        : s.name;
function toast(message) {
  $("#toast").textContent = message;
  $("#toast").style.display = "block";
  setTimeout(() => ($("#toast").style.display = "none"), 4000);
}
async function request(path, method = "GET", body) {
  let r = await fetch(path, {
    method,
    headers: { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!r.ok) {
    let b = await r.json().catch(() => ({}));
    throw Error(b.error || t("error"));
  }
  return r.status === 200 && r.headers.get("content-type")?.includes("json")
    ? r.json()
    : null;
}
async function persist() {
  await request("/api/data", "PUT", data);
}
async function change(fn) {
  let before = clone(data);
  try {
    fn();
    await persist();
    render();
  } catch (e) {
    data = before;
    render();
    throw e;
  }
}
function render() {
  document.documentElement.lang = data.language;
  document.documentElement.dir = data.language === "he" ? "rtl" : "ltr";
  $("#language").setAttribute("aria-label", t("language"));
  $("#language").title = t("language");
  $("#languageTitle").textContent = t("language");
  ["export", "import", "shutdown"].forEach(
    (k) => ($("#" + k).textContent = t(k)),
  );
  let root = $("#app");
  if (typeof importDraft !== "undefined" && importDraft) {
    renderTextImport();
    return;
  }
  if (!selected) {
    root.innerHTML = `<div class="heading"><div><div class="section-label">GradePilot</div><h1>${t("home")}</h1><p>${t("intro")}</p></div><div class="actions">${button("textImport", t("textImport"), data.degrees.length ? "" : "disabled")}${button("newDegree", t("addDegree"), 'class="primary"')}</div></div>${
      !data.degrees.length
        ? `<div class="card empty"><h2>${t("empty")}</h2><p>${t("emptyText")}</p>${button("newDegree", t("addDegree"), 'class="primary"')}</div>`
        : `<div class="grid">${data.degrees
            .map((d) => {
              let s = summary(courses(d)),
                p = (s.credits / d.requiredCredits) * 100;
              return `<article class="card degree-card"><div class="degree-card-head"><h2>${esc(d.name)}</h2>${degreeOptions(d)}</div><div class="metrics">${metric(t("average"), avg(s.average))}${metric(t("credits"), `${s.credits} <span class="unit">/ ${d.requiredCredits}</span>`)}</div><div class="progress"><div style="width:${Math.min(p, 100)}%"></div></div><small class="progress-caption"><bdi>${p.toFixed(1)}%</bdi></small>${distribution(s)}<div class="actions sub-actions">${button("open", t("open"), `class="primary" data-id="${d.id}"`)}</div></article>`;
            })
            .join("")}</div>`
    }`;
    return;
  }
  let real = data.degrees.find((d) => d.id === selected),
    d = simulation || real;
  yearIndex = Math.min(yearIndex, d.years.length - 1);
  let y = d.years[yearIndex],
    s = summary(courses(d)),
    ys = summary(y.semesters.flatMap((s) => s.courses));
  root.innerHTML = `<div class="heading"><div>${button("back", (data.language === "he" ? "› " : "‹ ") + t("back"))}<h1>${esc(d.name)}</h1></div><div class="actions">${simulation ? button("end", t("end"), 'class="primary"') : button("simulate", t("simulate"), 'class="primary"')}${button("report", t("report"))}${!simulation ? button("textImport", t("textImport")) : ""}</div></div>${simulation ? `<section class="simulation"><h2>${t("simulation")}</h2><div class="metrics">${metric(t("newAverage"), avg(s.average))}${metric(t("current"), avg(summary(courses(real)).average))}</div>${distribution(s)}</section>` : `<section class="card"><div class="metrics">${metric(t("average"), avg(s.average))}${metric(t("credits"), `${s.credits} <span class="unit">/ ${d.requiredCredits}</span>`)}</div>${distribution(s)}</section>`}<div class="year-nav">${button("prev", data.language === "he" ? "→" : "←", yearIndex === 0 ? "disabled" : "")}<h2>${yearName(yearIndex)}</h2>${button("next", data.language === "he" ? "←" : "→", yearIndex === d.years.length - 1 ? "disabled" : "")}</div><section class="card year-summary"><h3>${yearName(yearIndex)}</h3><div class="metrics">${metric(t("yearAverage"), avg(ys.average))}${metric(t("credits"), ys.credits)}${metric(t("courses"), ys.count)}</div></section>${!simulation ? `<div class="actions sub-actions">${button("addYear", t("addYear"))}${button("removeYear", t("removeYear"), d.years.length === 1 ? "disabled" : "")}${button("editDegree", t("edit"), `data-id="${d.id}"`)}${button("addSemester", t("addSemester"))}</div>` : ""}${y.semesters
    .map((sem) => {
      let ss = summary(sem.courses);
      return `<section class="semester"><div class="semester-head"><div><h2>${esc(semName(sem))}</h2><p>${ss.credits} ${t("credits")} · ${t("semesterAverage")} ${avg(ss.average)}</p></div>${!simulation ? `<div class="actions">${button("addCourse", t("addCourse"), `data-sem="${sem.id}"`)}${button("editSemester", t("edit"), `data-sem="${sem.id}"`)}${button("deleteSemester", t("delete"), `data-sem="${sem.id}"`)}</div>` : ""}</div>${!sem.courses.length ? `<p class="muted">${t("none")}</p>` : sem.courses.map((c) => `<article class="course ${simulation ? "sim-course" : ""}"><div class="sim-fields"><h3>${esc(c.name)}</h3><small>${c.credits} ${t("creditPoints")}${c.usesComponents && final(c) === null ? " · " + t("incomplete") : ""}</small>${simulation && c.passed == null ? (c.usesComponents ? c.components.map((p, i) => `<div class="sim-component"><small>${esc(p.name)} · ${p.weight}%</small>${simControls(c, p.grade, i)}</div>`).join("") : simControls(c, c.grade, -1)) : ""}</div><div class="course-end"><span class="course-grade">${c.passed != null ? t(c.passed ? "passed" : "failed") : final(c) ?? "—"}</span>${!simulation ? courseOptions(c) : ""}</div></article>`).join("")}</section>`;
    })
    .join("")}`;
}
function simControls(c, g, i) {
  return `<div class="quick" data-course="${c.id}" data-component="${i}"><input aria-label="${esc(c.name + " " + (i >= 0 ? c.components[i].name + " " : "") + t("grade"))}" type="number" min="0" max="100" step="0.01" value="${g ?? ""}" data-sim-input>${[5, 1, -1, -5].map((n) => button("bump", (n > 0 ? "+" : "") + n, `data-delta="${n}" ${g === null ? "disabled" : ""}`)).join("")}${button("reset", t("reset"))}</div>`;
}
function degreeOptions(d) {
  return `<details class="course-menu degree-menu"><summary aria-label="${esc(t("degreeOptions") + ": " + d.name)}" title="${esc(t("degreeOptions"))}"><svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="5" r="2"/><circle cx="12" cy="12" r="2"/><circle cx="12" cy="19" r="2"/></svg></summary><div class="course-menu-actions">${button("editDegree", t("edit"), `data-id="${d.id}"`)}${button("deleteDegree", t("delete"), `class="danger" data-id="${d.id}"`)}</div></details>`;
}
function courseOptions(c) {
  return `<details class="course-menu"><summary aria-label="${esc(t("courseOptions") + ": " + c.name)}" title="${esc(t("courseOptions"))}"><svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="5" r="2"/><circle cx="12" cy="12" r="2"/><circle cx="12" cy="19" r="2"/></svg></summary><div class="course-menu-actions">${button("editCourse", t("edit"), `data-id="${c.id}"`)}${button("deleteCourse", t("delete"), `class="danger" data-id="${c.id}"`)}</div></details>`;
}
document.addEventListener("click", (event) => {
  document.querySelectorAll(".course-menu[open]").forEach((menu) => {
    if (
      !menu.contains(event.target) ||
      event.target.closest(".course-menu-actions button")
    )
      menu.open = false;
  });
});
document.addEventListener("keydown", (event) => {
  if (event.key === "Escape")
    document.querySelectorAll(".course-menu[open]").forEach((menu) => {
      menu.open = false;
      menu.querySelector("summary").focus();
    });
});
function label(text, input) {
  return `<label>${esc(text)}${input}</label>`;
}
function input(name, value = "", attrs = "") {
  return `<input name="${name}" value="${esc(value ?? "")}" ${attrs}>`;
}
function dialog(title, html, onSave) {
  $("#fields").innerHTML = `<h2>${esc(title)}</h2>${html}`;
  $("#cancel").textContent = t("cancel");
  $("#save").textContent = t("save");
  $("#save").className = "primary";
  $("#save").hidden = !onSave;
  $("#form").onsubmit = async (e) => {
    e.preventDefault();
    try {
      if (onSave) await onSave(new FormData($("#form")));
      $("#dialog").close();
    } catch (err) {
      toast(err.message);
    }
  };
  $("#dialog").showModal();
}
$("#cancel").onclick = () => $("#dialog").close();
function degreeForm(id) {
  let d = data.degrees.find((x) => x.id === id);
  dialog(
    d ? t("edit") : t("addDegree"),
    label(t("name"), input("name", d?.name, 'required maxlength="200"')) +
      label(
        t("duration"),
        input(
          "duration",
          d?.duration || 3,
          'type="number" min="1" max="100" step="1" required',
        ),
      ) +
      label(
        t("required"),
        input(
          "requiredCredits",
          d?.requiredCredits || 120,
          'type="number" min="0.01" max="10000" step="any" required',
        ),
      ),
    async (f) => {
      await change(() => {
        let values = {
          name: f.get("name").trim(),
          duration: Number(f.get("duration")),
          requiredCredits: Number(f.get("requiredCredits")),
        };
        if (d) Object.assign(d, values);
        else
          data.degrees.push({
            id: uid(),
            ...values,
            years: Array.from({ length: values.duration }, () => ({
              id: uid(),
              semesters: [
                { id: uid(), name: "A", courses: [] },
                { id: uid(), name: "B", courses: [] },
              ],
            })),
          });
      });
    },
  );
}
function locate(d, id) {
  for (let y of d.years)
    for (let s of y.semesters) {
      let c = s.courses.find((c) => c.id === id);
      if (c) return { s, c };
    }
}
function courseForm(id, semId) {
  let d = data.degrees.find((d) => d.id === selected),
    found = id ? locate(d, id) : null,
    c = found?.c || {
      name: "",
      credits: 3,
      grade: null,
      usesComponents: false,
      components: [],
    },
    ps = clone(c.components);
  let options = d.years
    .flatMap((y, i) =>
      y.semesters.map(
        (s) =>
          `<option value="${s.id}" ${s.id === (found?.s.id || semId) ? "selected" : ""}>${esc(yearName(i) + " · " + semName(s))}</option>`,
      ),
    )
    .join("");
  dialog(
    id ? t("edit") : t("addCourse"),
    label(t("name"), input("name", c.name, 'required maxlength="200"')) +
      `<div class="form-row">${label(t("creditPoints"), input("credits", c.credits, 'type="number" min="0.01" max="1000" step="any" required'))}${label(t("semester"), `<select name="semester">${options}</select>`)}</div>` +
      label(
        t("mode"),
        `<select id="mode"><option value="direct">${t("direct")}</option><option value="binary" ${c.passed != null ? "selected" : ""}>${t("binary")}</option><option value="components" ${c.usesComponents ? "selected" : ""}>${t("components")}</option></select>`,
      ) +
      `<div id="direct">${label(t("grade"), input("grade", c.grade, 'type="number" min="0" max="100" step="any"'))}</div><div id="binaryGrade">${label(t("grade"), `<select name="passed"><option value="true" ${c.passed !== false ? "selected" : ""}>${t("passed")}</option><option value="false" ${c.passed === false ? "selected" : ""}>${t("failed")}</option></select>`)}</div><div id="componentEditor"></div>`,
    async (f) => {
      let usesComponents = $("#mode").value === "components";
      let components = [...document.querySelectorAll(".component-row")].map(
        (row) => ({
          name: row.querySelector("[data-name]").value.trim(),
          weight: Number(row.querySelector("[data-weight]").value),
          grade:
            row.querySelector("[data-grade]").value === ""
              ? null
              : Number(row.querySelector("[data-grade]").value),
        }),
      );
      await change(() => {
        let updated = {
          id: id || uid(),
          name: f.get("name").trim(),
          credits: Number(f.get("credits")),
          usesComponents,
          passed: $("#mode").value === "binary" ? f.get("passed") === "true" : null,
          grade: usesComponents || $("#mode").value === "binary"
            ? null
            : f.get("grade") === ""
              ? null
              : Number(f.get("grade")),
          components: usesComponents ? components : [],
        };
        if (found) found.s.courses = found.s.courses.filter((x) => x.id !== id);
        d.years
          .flatMap((y) => y.semesters)
          .find((s) => s.id === f.get("semester"))
          .courses.push(updated);
      });
    },
  );
  function readPs() {
    ps = [...document.querySelectorAll(".component-row")].map((r) => ({
      name: r.querySelector("[data-name]").value,
      weight: r.querySelector("[data-weight]").value,
      grade: r.querySelector("[data-grade]").value,
    }));
  }
  function weights() {
    let n = [...document.querySelectorAll("[data-weight]")].reduce(
      (s, x) => s + Number(x.value),
      0,
    );
    $("#weightTotal").textContent = t("totalWeight") + ": " + n + "%";
    $("#weightWarning").hidden = n <= 100;
  }
  function draw() {
    let component = $("#mode").value === "components";
    $("#direct").hidden = $("#mode").value !== "direct";
    $("#binaryGrade").hidden = $("#mode").value !== "binary";
    $("#direct input").disabled = $("#mode").value !== "direct";
    $("#componentEditor").hidden = !component;
    $("#componentEditor").innerHTML =
      ps
        .map(
          (p, i) =>
            `<div class="component-row">${label(t("name"), `<input data-name value="${esc(p.name)}" required maxlength="200">`)}${label(t("weight"), `<input data-weight type="number" value="${esc(p.weight)}" min="0.01" max="1000" step="any" required>`)}${label(t("grade"), `<input data-grade type="number" value="${esc(p.grade ?? "")}" min="0" max="100" step="any">`)}<button type="button" data-remove="${i}" aria-label="${t("remove")}">×</button></div>`,
        )
        .join("") +
      `<p id="weightTotal"></p><p id="weightWarning" class="weight-warning">${t("warning")}</p><button type="button" id="addComponent">${t("addComponent")}</button>`;
    $("#componentEditor")
      .querySelectorAll("input")
      .forEach((x) => (x.disabled = !component));
    $("#addComponent").onclick = () => {
      readPs();
      ps.push({ name: "", weight: 100, grade: null });
      draw();
    };
    $("#componentEditor")
      .querySelectorAll("[data-remove]")
      .forEach(
        (b) =>
          (b.onclick = () => {
            readPs();
            ps.splice(Number(b.dataset.remove), 1);
            draw();
          }),
      );
    $("#componentEditor").oninput = weights;
    weights();
  }
  $("#mode").onchange = () => {
    readPs();
    if (!ps.length) ps.push({ name: "", weight: 100, grade: null });
    draw();
  };
  draw();
}
function applySim(el, action, value) {
  let box = el.closest(".quick"),
    found = locate(simulation, box.dataset.course),
    original = locate(
      data.degrees.find((d) => d.id === selected),
      box.dataset.course,
    ),
    index = Number(box.dataset.component),
    target = index < 0 ? found.c : found.c.components[index],
    real = index < 0 ? original.c : original.c.components[index];
  target.grade =
    action === "reset"
      ? real.grade
      : value === ""
        ? null
        : Math.max(0, Math.min(100, Number(value)));
  render();
}
$("#app").addEventListener("input", (e) => {
  if (e.target.matches("[data-sim-input]")) {
    if (!e.target.validity.valid) {
      e.target.reportValidity();
      return;
    }
    let box = e.target.closest(".quick"),
      courseId = box.dataset.course,
      component = box.dataset.component;
    applySim(e.target, "set", e.target.value);
    document
      .querySelector(
        `[data-course="${courseId}"][data-component="${component}"] input`,
      )
      .focus();
  }
});
$(".brand").addEventListener("click", (event) => {
  event.preventDefault();
  if (typeof importDraft !== "undefined" && importDraft) {
    if ((importDraft.text || importDraft.rows.length) && !confirm(t("discardDraft"))) return;
    importDraft = null;
    previewSequence++;
  }
  selected = null;
  simulation = null;
  yearIndex = 0;
  render();
});
$("#app").addEventListener("click", async (e) => {
  let b = e.target.closest("[data-action]");
  if (!b) return;
  let a = b.dataset.action,
    d = data.degrees.find((d) => d.id === selected),
    y = d?.years[yearIndex];
  try {
    switch (a) {
      case "textImport":
        openTextImport();
        break;
      case "newDegree":
        degreeForm();
        break;
      case "editDegree":
        degreeForm(b.dataset.id);
        break;
      case "open":
        selected = b.dataset.id;
        yearIndex = 0;
        render();
        break;
      case "back":
        selected = null;
        simulation = null;
        render();
        break;
      case "deleteDegree": {
        const degree = data.degrees.find(d => d.id === b.dataset.id);
        if (!degree) break;
        dialog(t("deleteDegreeTitle"), `<h3>${esc(degree.name)}</h3><p>${esc(t("deleteDegreeHelp"))}</p>`, async () => {
          await change(() => { data.degrees = data.degrees.filter(d => d.id !== degree.id); });
        });
        $("#save").textContent = t("delete");
        $("#save").className = "danger";
        $("#cancel").focus();
        break;
      }
      case "prev":
        yearIndex--;
        render();
        break;
      case "next":
        yearIndex++;
        render();
        break;
      case "addYear":
        await change(() => {
          d.years.push({
            id: uid(),
            semesters: [
              { id: uid(), name: "A", courses: [] },
              { id: uid(), name: "B", courses: [] },
            ],
          });
          yearIndex = d.years.length - 1;
        });
        break;
      case "removeYear": {
        if (d.years.length <= 1) break;
        const last = d.years[d.years.length - 1];
        const count = last.semesters.reduce((total, semester) => total + semester.courses.length, 0);
        dialog(t("removeYearTitle"), `<h3>${esc(yearName(d.years.length - 1))}</h3><p>${count} ${esc(t("courses"))}</p><p>${esc(t("removeYearHelp"))}</p>`, async () => {
          await change(() => d.years.pop());
        });
        $("#save").textContent = t("removeYear");
        $("#save").className = "danger";
        $("#cancel").focus();
        break;
      }
      case "addSemester":
      case "editSemester": {
        let s = y.semesters.find((s) => s.id === b.dataset.sem);
        dialog(
          t("semester"),
          label(
            t("semesterName"),
            `<select name="name">${[...new Set(["A", "B", "Summer", ...(s && !["A", "B", "Summer"].includes(s.name) ? [s.name] : [])])].map(name => `<option value="${esc(name)}" ${name === (s?.name || "Summer") ? "selected" : ""}>${esc(semName({name}))}</option>`).join("")}</select>`,
          ),
          async (f) =>
            change(() => {
              if (s) s.name = f.get("name").trim();
              else
                y.semesters.push({
                  id: uid(),
                  name: f.get("name").trim(),
                  courses: [],
                });
            }),
        );
        break;
      }
      case "deleteSemester":
        if (confirm(t("confirm")))
          await change(
            () =>
              (y.semesters = y.semesters.filter((s) => s.id !== b.dataset.sem)),
          );
        break;
      case "addCourse":
        courseForm(null, b.dataset.sem);
        break;
      case "editCourse":
        courseForm(b.dataset.id);
        break;
      case "deleteCourse":
        if (confirm(t("confirm")))
          await change(() => {
            let f = locate(d, b.dataset.id);
            f.s.courses = f.s.courses.filter((c) => c.id !== b.dataset.id);
          });
        break;
      case "simulate":
        simulation = clone(d);
        render();
        break;
      case "end":
        simulation = null;
        render();
        break;
      case "bump": {
        let box = b.closest(".quick"),
          f = locate(simulation, box.dataset.course),
          i = Number(box.dataset.component),
          g = i < 0 ? f.c.grade : f.c.components[i].grade;
        applySim(b, "set", g + Number(b.dataset.delta));
        break;
      }
      case "reset":
        applySim(b, "reset");
        break;
      case "report": {
        let target = simulation || d;
        dialog(
          t("reportTitle"),
          `<h3>${esc(target.name)}</h3>${target.years
            .map((y, i) => {
              let s = summary(y.semesters.flatMap((s) => s.courses));
              return `<div class="report-line"><strong>${yearName(i)}</strong> · ${avg(s.average)} · ${s.credits} ${t("credits")} · ${s.count} ${t("courses")}</div>`;
            })
            .join("")}${distribution(summary(courses(target)))}`,
          null,
        );
        break;
      }
    }
  } catch (err) {
    toast(err.message);
  }
});
$("#language").onclick = () => {
  document
    .querySelectorAll("[data-language]")
    .forEach((button) =>
      button.setAttribute(
        "aria-pressed",
        String(button.dataset.language === data.language),
      ),
    );
  const panel = $("#languageDialog"),
    anchor = $("#language").getBoundingClientRect();
  panel.style.left =
    Math.max(12, Math.min(anchor.left, window.innerWidth - 232)) + "px";
  panel.style.top = anchor.bottom + 8 + "px";
  panel.togglePopover();
};
$("#languageDialog").addEventListener("toggle", (event) => {
  $("#language").setAttribute(
    "aria-expanded",
    String(event.newState === "open"),
  );
});
document.querySelectorAll("[data-language]").forEach((button) => {
  button.onclick = async () => {
    try {
      await change(() => {
        data.language = button.dataset.language;
      });
      $("#languageDialog").hidePopover();
      $("#language").focus();
    } catch (error) {
      toast(error.message);
    }
  };
});
$("#export").onclick = () => {
  let url = URL.createObjectURL(
      new Blob([JSON.stringify(data, null, 2)], { type: "application/json" }),
    ),
    a = document.createElement("a");
  a.href = url;
  a.download =
    "degree-backup-" + new Date().toISOString().slice(0, 10) + ".json";
  a.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
};
$("#import").onclick = () => $("#file").click();
$("#file").onchange = async (e) => {
  try {
    let file = e.target.files[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) throw Error(t("error"));
    let imported = JSON.parse(await file.text());
    await request("/api/validate", "POST", imported);
    if (confirm(t("restoreConfirm"))) {
      await request("/api/data", "PUT", imported);
      data = await request("/api/data");
      selected = null;
      simulation = null;
      if (typeof importDraft !== "undefined") importDraft = null;
      render();
      toast(t("saved"));
    }
  } catch (err) {
    toast(err.message);
  } finally {
    e.target.value = "";
  }
};
$("#shutdown").onclick = async () => {
  if (confirm(t("closeConfirm"))) {
    await request("/api/shutdown", "POST");
    document.body.innerHTML =
      "<main><h1>" +
      (data.language === "he" ? "היישום נסגר" : "Application closed") +
      "</h1></main>";
  }
};
let start;
$("#app").addEventListener("pointerdown", (e) => {
  if (e.target.closest("button,input,select,a")) return;
  start = { x: e.clientX, y: e.clientY };
});
$("#app").addEventListener("pointerup", (e) => {
  if (!start || !selected) return;
  let dx = e.clientX - start.x,
    dy = e.clientY - start.y;
  start = null;
  if (Math.abs(dx) > 90 && Math.abs(dy) < 50) {
    let next =
        yearIndex + (dx * (data.language === "he" ? -1 : 1) < 0 ? 1 : -1),
      d = simulation || data.degrees.find((d) => d.id === selected);
    if (next >= 0 && next < d.years.length) {
      yearIndex = next;
      render();
    }
  }
});
request("/api/data")
  .then((b) => {
    data = b;
    render();
  })
  .catch((e) => {
    $("#app").textContent = e.message;
  });

