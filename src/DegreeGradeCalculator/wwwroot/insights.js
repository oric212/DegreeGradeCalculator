"use strict";
function insightPeriod(period) {
  return yearName(period.yearIndex) + " · " + (period.semester ? semName(period.semester) : t("yearlyCourses"));
}
function insightGraph(points) {
  if (!points.length) return `<p>${esc(t("insightsEmpty"))}</p>`;
  const width = 900, height = 270, left = 54, right = 30, top = 24, bottom = 46;
  const min = Math.max(0, Math.floor((Math.min(...points.map(p => p.average)) - 5) / 5) * 5);
  const max = Math.max(min + 10, Math.ceil((Math.max(...points.map(p => p.average)) + 5) / 5) * 5);
  const x = i => points.length === 1 ? width / 2 : left + i / (points.length - 1) * (width - left - right);
  const y = value => top + (max - value) / (max - min) * (height - top - bottom);
  const line = points.map((p, i) => `${x(i)},${y(p.average)}`).join(" ");
  const ticks = Array.from({length: 5}, (_, i) => min + (max - min) * i / 4);
  return `<svg class="insight-graph" viewBox="0 0 ${width} ${height}" role="img" aria-label="${esc(t("cumulative"))}" xmlns="http://www.w3.org/2000/svg">
    <title>${esc(t("cumulative"))}</title><desc>${esc(points.map(p => insightPeriod(p.period) + ": " + p.formatted).join("; "))}</desc>
    ${ticks.map(value => `<line x1="${left}" y1="${y(value)}" x2="${width-right}" y2="${y(value)}" class="graph-grid"/><text x="${left-12}" y="${y(value)+4}" text-anchor="end">${value.toFixed(2)}</text>`).join("")}
    ${points.length > 1 ? `<polygon points="${x(0)},${height-bottom} ${line} ${x(points.length-1)},${height-bottom}" fill="#245dc114"/><polyline points="${line}" fill="none" stroke="#245dc1" stroke-width="3" stroke-linejoin="round"/>` : ""}
    ${points.map((p, i) => `<circle cx="${x(i)}" cy="${y(p.average)}" r="5" fill="#245dc1" stroke="white" stroke-width="2"><title>${esc(insightPeriod(p.period))}: ${p.formatted}</title></circle>${i === 0 || i === points.length-1 || points.length <= 8 ? `<text x="${x(i)}" y="${height-14}" text-anchor="middle">${i+1}</text>` : ""}`).join("")}
  </svg>`;
}
function renderInsights(degree) {
  const result = degreeInsights(degree);
  const rows = (entries, impact = false) => `<ol class="insight-list">${entries.map((entry, index) => `<li>
    <span class="insight-rank"><bdi>${index+1}</bdi></span><div class="insight-course"><h3>${esc(entry.course.name)}</h3><small>${esc(insightPeriod(entry.period))} · <bdi>${formatCredits(entry.course.credits)}</bdi> ${esc(t("creditPoints"))}</small>
    ${impact ? `<div class="insight-weight"><span style="width:${entry.share}%"></span></div><small>${esc(t("gpaShare"))}: <bdi>${entry.share.toFixed(1)}%</bdi> · ${esc(t("grade"))}: <bdi>${formatGrade(entry.grade)}</bdi></small>` : `<small class="insight-status ${entry.counted ? "" : "muted"}">${esc(t(entry.counted ? "countedAttempt" : "previousAttempt"))}</small>`}</div>
    <div class="insight-value"><strong><bdi>${impact ? entry.difference === null ? "—" : (entry.difference >= 0 ? "+" : "−") + Math.abs(entry.difference).toFixed(2) : formatGrade(entry.grade)}</bdi></strong>${impact ? `<small>${esc(t(entry.difference === null ? "onlyCourse" : "impactPoints"))}</small>` : ""}</div>
  </li>`).join("")}</ol>`;
  $("#app").innerHTML = `<div class="heading"><div>${button("degreeCourses", (data.language === "he" ? "› " : "‹ ") + t("returnToDegree"))}<h1>${esc(t("insights"))}</h1><p>${esc(degree.name)}</p></div><span class="insights-badge">${esc(t("readOnly"))}</span></div>
    <p class="insights-intro">${esc(t("insightsIntro"))}</p>
    <section class="card insights-summary"><div class="metrics">${metric(t("average"), avg(result.current.average))}${metric(t("numericCourses"), result.impact.length)}</div></section>
    ${!result.rankings.length ? `<section class="card empty"><h2>${esc(t("insightsEmpty"))}</h2></section>` : `<section class="card insights-chart"><h2>${esc(t("cumulative"))}</h2><p>${esc(t("timelineHelp"))}</p>${insightGraph(result.timeline)}
    <div class="insight-periods">${result.timeline.map((point, index) => `<span><bdi>${index+1}</bdi> · ${esc(insightPeriod(point.period))}</span>`).join("")}</div><details class="insight-values"><summary>${esc(t("timelineDetails"))}</summary><ol>${result.timeline.map(point => `<li><span>${esc(insightPeriod(point.period))}</span><strong><bdi>${point.formatted}</bdi></strong></li>`).join("")}</ol></details></section>
    <div class="insights-grid"><section class="card"><h2>${esc(t("gradeRanking"))}</h2>${rows(result.rankings)}</section><section class="card"><h2>${esc(t("impactRanking"))}</h2><p class="insight-explanation">${esc(t("impactHelp"))}</p>${result.impact.length ? rows(result.impact, true) : `<p>${esc(t("insightsEmpty"))}</p>`}</section></div>`}`;
}
