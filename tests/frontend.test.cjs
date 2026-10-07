// Optional developer checks; Node is not needed to run the application.
const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const vm = require("node:vm");
const source = fs.readFileSync(
  "src/DegreeGradeCalculator/wwwroot/app.js",
  "utf8",
);
const context = vm.createContext({
  structuredClone,
  crypto: require("node:crypto").webcrypto,
  document: {addEventListener() {}},
});
vm.runInContext(
  source.slice(0, source.indexOf("function toast")) +
    "\nthis.logic={final,summary,avg,clone,availableSemesters,yearCourses,courses,simulationChanged,modifiedCourses,simulationBumpEnabled,simulationBumpValue,degreeInsights,formatCredits,formatGrade,formatRequiredCredits,validRequiredCredits,validGrade,requiredCreditAttributes,gradeAttributes,courseCreditMinimum,courseCreditAttributes,gradeSheetText};",
  context,
);
const { final, summary, avg, clone, availableSemesters, yearCourses, courses, simulationChanged, modifiedCourses, simulationBumpEnabled, simulationBumpValue, degreeInsights, formatCredits, formatGrade, formatRequiredCredits, validRequiredCredits, validGrade, requiredCreditAttributes, gradeAttributes, courseCreditMinimum, courseCreditAttributes, gradeSheetText } = context.logic;
const course = (credits, grade) => ({
  credits,
  grade,
  usesComponents: false,
  components: [],
});
const component = (...parts) => ({
  credits: 5,
  grade: null,
  usesComponents: true,
  components: parts.map(([weight, grade]) => ({ weight, grade })),
});
test("insights rank saved grades and quantify signed GPA influence without mutations", () => {
  const a = {...course(6, 60), name: "Heavy course"}, b = {...course(2, 100), name: "High grade"};
  const degree = {years: [{semesters: [{name: "A", courses: [a]}, {name: "B", courses: [b]}], yearlyCourses: []}]};
  const before = JSON.stringify(degree), result = degreeInsights(degree);
  assert.equal(result.rankings[0].course.name, "High grade");
  assert.equal(result.impact[0].course.name, "Heavy course");
  assert.equal(result.impact[0].difference, -30);
  assert.equal(result.impact[1].difference, 10);
  assert.equal(result.impact[0].share, 75);
  assert.equal(result.timeline.map(point => point.formatted).join(","), "60.00,70.00");
  assert.equal(JSON.stringify(degree), before);
});
test("insight history uses latest completed attempts at each period, including yearly components", () => {
  const named = (grade, extra = {}) => ({...course(4, grade), name: "Repeated", ...extra});
  const annual = {...component([70, 80], [30, 90]), name: "Annual"};
  const degree = {years: [
    {semesters: [{name: "A", courses: [named(60), {...course(2, null), name: "Pass", passed: true}]}, {name: "B", courses: [named(90)]}], yearlyCourses: [annual]},
    {semesters: [{name: "A", courses: [named(null)]}], yearlyCourses: []}
  ]};
  const result = degreeInsights(degree);
  assert.equal(result.rankings.length, 3);
  assert.equal(result.rankings.find(entry => entry.grade === 60).counted, false);
  assert.equal(result.impact.length, 2);
  assert.equal(result.timeline.length, 3);
  assert.equal(result.timeline[0].formatted, "60.00");
  assert.equal(result.timeline[1].formatted, "90.00");
  assert.equal(result.timeline[2].formatted, avg(summary(courses(degree)).average));
});
test("insights handle empty, single-course, decimal credits, and pass/fail replacing a numeric attempt", () => {
  const degree = cs => ({years: [{semesters: [{name: "A", courses: cs}]}]});
  assert.equal(degreeInsights(degree([])).rankings.length, 0);
  const single = degreeInsights(degree([{...course(.1, 82), name: "One"}]));
  assert.equal(single.impact[0].difference, null);
  assert.equal(single.impact[0].share, 100);
  assert.equal(single.timeline[0].formatted, "82.00");
  const replaced = degreeInsights(degree([{...course(4, 70), name: "Same"}, {...course(4, null), name: "Same", passed: true}]));
  assert.equal(replaced.impact.length, 0);
  assert.equal(replaced.rankings[0].counted, false);
});
test("browser weighted average and decimal credit arithmetic", () => {
  assert.equal(avg(summary([course(5, 80), course(4, 90)]).average), "84.44");
  assert.equal(summary([course(0.1, 80), course(0.2, 90)]).credits, 0.3);
  assert.equal(
    avg(summary([course(0.1, 80), course(0.2, 90)]).average),
    "86.67",
  );
});
test("blank grades are excluded from average, credits and distribution", () => {
  const s = summary([course(5, 45), course(20, null)]);
  assert.equal(avg(s.average), "45.00");
  assert.equal(s.credits, 5);
  assert.equal(s.bins.join(","), "1,0,0,0,0");
});
test("component rounding, bonuses, incomplete data and exact halves", () => {
  assert.equal(final(component([70, 55], [30, 98])), 68);
  assert.equal(final(component([70, 80], [30, 90], [10, 100])), 93);
  assert.equal(final(component([100, 80.5])), 81);
  assert.equal(final(component([50, 80])), 40);
  assert.equal(final(component([70, 80], [30, null])), null);
});
test("component simulation clone propagates without mutating saved course", () => {
  const real = [course(5, 80), component([70, 55], [30, 98])],
    simulated = clone(real);
  simulated[1].components[0].grade = 60;
  assert.equal(final(simulated[1]), 71);
  assert.equal(avg(summary(simulated).average), "75.50");
  assert.equal(final(real[1]), 68);
  assert.equal(avg(summary(real).average), "74.00");
});
test("distribution boundaries including bonus grades", () => {
  assert.equal(
    summary(
      [59, 60, 69, 70, 79, 80, 89, 90, 100, 110].map((g) => course(1, g)),
    ).bins.join(","),
    "1,2,2,2,3",
  );
});

test("exact-name repeats count only the latest graded attempt", () => {
  const rows = [{...course(4, 90), name:"Math"}, {...course(3, 60), name:"Math"}, {...course(5, null), name:"Math"}, {...course(2,80),name:"math"}];
  const s = summary(rows);
  assert.equal(avg(s.average), "68.00");
  assert.equal(s.credits, 5);
  assert.equal(s.count, 4);
  assert.equal(s.bins.reduce((a,b)=>a+b,0),2);
});

test("binary passes count credits without entering numeric averages", () => {
  const rows=[{...course(4,80),name:"Studio"},{...course(2,null),name:"Activity",passed:true},{...course(3,null),name:"Pending"},{...course(5,null),name:"Failed",passed:false}];
  const s=summary(rows);
  assert.equal(avg(s.average),"80.00"); assert.equal(s.credits,6);
  assert.equal(s.bins.reduce((a,b)=>a+b,0),1);
  assert.equal(final(rows[1]),null);
});

test("semester choices exclude existing semesters per year and retain the edited semester", () => {
  const a = { id: "a", name: "A" }, b = { id: "b", name: "B" }, summer = { id: "summer", name: "Summer" };
  const year = { semesters: [a, b] };
  assert.deepEqual(Array.from(availableSemesters(year)), ["Summer"]);
  assert.deepEqual(Array.from(availableSemesters(year, b)), ["B", "Summer"]);
  assert.deepEqual(Array.from(availableSemesters({ semesters: [a, b, summer] })), []);
  assert.deepEqual(Array.from(availableSemesters({ semesters: [] })), ["A", "B", "Summer"]);
  assert.deepEqual(Array.from(availableSemesters({ semesters: [a] })), ["B", "Summer"]);
  assert.deepEqual(Array.from(availableSemesters({ semesters: [{ id: "other", name: " b " }] })), ["A", "Summer"]);
});
test("yearly courses enter year and degree summaries once, never semester summaries", () => {
  const semester = {...course(2, 70), id:"s", name:"Semester course"};
  const yearly = {...course(4, 100), id:"y", name:"Yearly course"};
  const year = {semesters:[{courses:[semester]}], yearlyCourses:[yearly]};
  const degree = {years:[year]};
  assert.equal(avg(summary(yearCourses(year)).average), "90.00");
  assert.equal(avg(summary(courses(degree)).average), "90.00");
  assert.equal(summary(courses(degree)).credits, 6);
  assert.equal(avg(summary(year.semesters[0].courses).average), "70.00");
  const simulated = clone(degree);
  simulated.years[0].yearlyCourses[0].grade = 85;
  assert.equal(avg(summary(courses(simulated)).average), "80.00");
  assert.equal(modifiedCourses(simulated, degree).length, 1);
  assert.equal(year.yearlyCourses[0].grade, 100);
});
test("simulation highlight tracks current differences and manual reversion equals reset", () => {
  const real = {...course(3, 80), id:"direct"};
  const current = clone(real);
  for (const [grade, changed] of [[85,true],[81,true],[80,false],[85,true],[80,false]]) {
    current.grade = grade; assert.equal(simulationChanged(current, real), changed);
  }
  assert.deepEqual(current, clone(real));
  const degree = {years:[{semesters:[{courses:[real]}]}]};
  assert.equal(modifiedCourses(clone(degree), degree).length, 0);
});
test("component changes highlight parent even when final rounded grade is unchanged", () => {
  const real = component([70,70],[30,90]), current = clone(real);
  current.components[0].grade = 75;
  assert.equal(simulationChanged(current,real), true);
  current.components[1].grade = 91;
  current.components[0].grade = 70;
  assert.equal(simulationChanged(current,real), true);
  current.components[1].grade = 90;
  assert.equal(simulationChanged(current,real), false);
  current.components[0].grade = 70.1;
  assert.equal(final(current), final(real));
  assert.equal(simulationChanged(current,real), true);
});

test("only +5 initializes a blank simulation grade to 100 and reset restores blank", () => {
  const original = {...course(3, null), id:"blank"}, simulated = clone(original);
  for (const delta of [1,-1,-5]) {
    assert.equal(simulationBumpEnabled(null,delta), false);
    assert.equal(simulationBumpValue(null,delta), null);
  }
  assert.equal(simulationBumpEnabled(null,5),true);
  simulated.grade = simulationBumpValue(simulated.grade,5);
  assert.equal(simulated.grade,100);
  assert.equal(simulationChanged(simulated,original),true);
  assert.equal(original.grade,null);
  assert.equal(simulationBumpValue(80,5),85);
  assert.equal(simulationBumpValue(100,5),100);
  assert.equal(simulationBumpValue(2,-5),0);
  simulated.grade = original.grade;
  assert.equal(simulationChanged(simulated,original),false);
  assert.equal(simulationBumpEnabled(simulated.grade,5),true);
  const parts = component([100,null]);
  parts.components[0].grade = simulationBumpValue(parts.components[0].grade,5);
  assert.equal(final(parts),100);
});

test("semantic numeric formatting keeps credits precise, grades whole and averages at two decimals", () => {
  assert.equal(validRequiredCredits(120), true);
  assert.equal(validRequiredCredits(120.1), false);
  assert.equal(formatRequiredCredits(120), "120");
  assert.equal(formatCredits(2.5), "2.5");
  assert.equal(formatCredits(3.00), "3");
  assert.equal(formatCredits(3.25), "3.25");
  assert.equal(formatCredits(summary([course(3,82), course(4,82), course(2,82)]).credits), "9");
  assert.equal(formatCredits(summary([course(3,82), course(4,82), course(2.5,82)]).credits), "9.5");
  assert.equal(formatCredits(summary([course(.1,82), course(.2,82)]).credits), "0.3");
  assert.equal(formatGrade(82.00), "82");
  assert.equal(validGrade(69.5), false);
  assert.equal(validGrade(69), true);
  assert.equal(validGrade(101), false);
  assert.equal(validGrade(null), true);
  assert.equal(formatGrade(final(component([60,82],[40,83]))), "82");
  const degree = {years:[{semesters:[{name:"A",courses:[course(2.5,82)]}]}]};
  for (const rows of [courses(degree), yearCourses(degree.years[0]), degree.years[0].semesters[0].courses, courses(clone(degree))]) {
    assert.equal(avg(summary(rows).average), "82.00");
  }
  assert.match(requiredCreditAttributes, /step="1"/);
  assert.match(requiredCreditAttributes, /min="1"/);
  assert.match(gradeAttributes, /step="1"/);
  assert.match(gradeAttributes, /dir="ltr"/);
  assert.equal(degreeInsights(degree).timeline[0].formatted, "82.00");
});

test("course-credit spinner steps by one while fractional inputs remain valid", () => {
  for (const value of [3, 4, 120]) assert.equal(courseCreditMinimum(value), 1);
  assert.equal(courseCreditMinimum(2.5), .5);
  assert.equal(courseCreditMinimum(3.25), .25);
  assert.equal(courseCreditMinimum(2.3), .3);
  assert.equal(courseCreditMinimum(.01), .01);
  assert.equal(courseCreditMinimum(0), 1);
  assert.equal(courseCreditMinimum(-2.5), 1);
  assert.match(courseCreditAttributes(3), /min="1"/);
  assert.match(courseCreditAttributes(2.5), /min="0.5"/);
  assert.match(courseCreditAttributes(2.5), /step="1"/);
});

test("grade sheets group semesters in order without changing saved data", () => {
  const degree = {name:"Demo degree", years:[{semesters:[
    {name:"Summer", courses:[{...course(2,null),name:"Pending"}]},
    {name:"B", courses:[{...course(2,null),name:"Workshop",passed:true}]},
    {name:"A", courses:[{...component([70,90],[30,80]),name:"Studio"},{...course(2.5,81),name:"Coding | practice\nPart 2"}]}
  ],yearlyCourses:[{...course(1,null),name:"Annual",passed:false}]},
  {semesters:[{name:"A",courses:[]}]}]};
  const before = JSON.stringify(degree);
  const text = gradeSheetText(degree,"en");
  assert.ok(text.indexOf("Semester A") < text.indexOf("Semester B"));
  assert.ok(text.indexOf("Semester B") < text.indexOf("Summer"));
  assert.match(text,/Studio \| 5 \| 87/);
  assert.match(text,/Coding   practice Part 2 \| 2.5 \| 81/);
  assert.match(text,/Workshop \| 2 \| Passed/);
  assert.match(text,/Pending \| 2 \| —/);
  assert.match(text,/Annual \| 1 \| Failed/);
  assert.match(text,/Yearly courses/);
  assert.match(text,/Year 2/);
  assert.ok(text.includes("\r\n"));
  assert.equal(JSON.stringify(degree),before);
  const hebrew = gradeSheetText(degree,"he");
  assert.match(hebrew,/גיליון ציונים/);
  assert.match(hebrew,/קיץ/);
  assert.match(hebrew,/Workshop \| 2 \| עבר/);
  assert.equal(JSON.stringify(degree),before);
});
