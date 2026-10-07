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
    "\nthis.logic={final,summary,avg,clone};",
  context,
);
const { final, summary, avg, clone } = context.logic;
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
