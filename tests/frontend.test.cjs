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
});
vm.runInContext(
  source.slice(0, source.indexOf("function toast")) +
    "\nthis.logic={final,summary,avg,clone,availableSemesters,yearCourses,courses,simulationChanged,modifiedCourses};",
  context,
);
const { final, summary, avg, clone, availableSemesters, yearCourses, courses, simulationChanged, modifiedCourses } = context.logic;
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
