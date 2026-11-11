"use strict";

// Szín a technológiához (entitáshoz) kötve — a sorrend/darabszám változása nem színez át.
const PROVIDERS = {
  "AdoNet": { name: "ADO.NET", color: "var(--series-1)" },
  "Dapper": { name: "Dapper", color: "var(--series-2)" },
  "EfCore": { name: "EF Core", color: "var(--series-3)" },
  "EfCore+tracking": { name: "EF Core (tracking)", color: "var(--series-4)" }
};

const METRICS = { meanMs: "Átlag", medianMs: "Medián", p95Ms: "P95" };

const el = id => document.getElementById(id);
const fmt = (ms, digits = 2) => ms.toLocaleString("hu-HU", { minimumFractionDigits: digits, maximumFractionDigits: digits });
const fmtInt = n => n.toLocaleString("hu-HU");

let scenarios = [];
let lastResult = null;
let metric = "meanMs";
const history = [];

function providerKey(r) {
  return r.provider === "EfCore" && r.efCoreTracking ? "EfCore+tracking" : r.provider;
}

async function api(path, options) {
  const response = await fetch(path, options);
  if (!response.ok) {
    const body = await response.text();
    throw new Error(`${response.status} ${response.statusText}${body ? " — " + body.slice(0, 300) : ""}`);
  }
  return response.json();
}

async function loadStatus() {
  try {
    const status = await api("/api/admin/status");
    const c = status.rowCounts;
    el("db-status").textContent =
      `Adatbázis: ${status.database} — ` +
      `${fmtInt(c.Product)} termék, ${fmtInt(c.Customer)} vevő, ${fmtInt(c.Order)} rendelés, ${fmtInt(c.OrderItem)} tétel`;
  } catch (err) {
    el("db-status").textContent = "Az adatbázis-státusz nem érhető el: " + err.message;
  }
}

async function loadScenarios() {
  scenarios = await api("/api/benchmark/scenarios");
  const select = el("scenario");
  select.innerHTML = "";
  for (const s of scenarios) {
    const option = document.createElement("option");
    option.value = s.key;
    option.textContent = s.name;
    select.appendChild(option);
  }
  select.addEventListener("change", onScenarioChange);
  onScenarioChange();
}

function onScenarioChange() {
  const s = scenarios.find(x => x.key === el("scenario").value);
  if (!s) return;
  el("scenario-desc").textContent = s.description;
  el("tracking-wrap").style.display = s.supportsTracking ? "" : "none";
  el("recent-count-wrap").hidden = s.key !== "GetRecentOrdersWithItems";
  el("page-size-wrap").hidden = s.key !== "GetPagedProducts";
}

async function runCompare() {
  const runButton = el("run");
  runButton.disabled = true;
  el("status").textContent = "Mérés fut…";
  try {
    const request = {
      scenario: el("scenario").value,
      iterations: Number(el("iterations").value) || 20,
      warmup: Number(el("warmup").value) || 0,
      includeEfCoreTracking: el("tracking").checked,
      recentOrdersCount: Number(el("recent-count").value) || 100,
      pageSize: Number(el("page-size").value) || 50
    };
    lastResult = await api("/api/benchmark/compare", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request)
    });
    el("status").textContent = `Kész ${fmt(lastResult.totalElapsedMs / 1000, 1)} s alatt.`;
    addHistory(lastResult);
    render();
  } catch (err) {
    el("status").textContent = "Hiba: " + err.message;
  } finally {
    runButton.disabled = false;
  }
}

function render() {
  if (!lastResult) return;
  const scenario = scenarios.find(s => s.key === lastResult.scenario);
  el("results").hidden = false;
  el("result-title").textContent =
    `${scenario ? scenario.name : lastResult.scenario} — ${lastResult.iterations} iteráció`;
  renderValidity();
  renderChart();
  renderTable();
}

function renderValidity() {
  const target = el("validity");
  if (lastResult.rowCountsMatch) {
    target.innerHTML = `<span class="validity ok">✓ Minden provider azonos sorszámot adott vissza (${fmtInt(lastResult.results[0].rowsReturned)})</span>`;
  } else {
    const counts = lastResult.results.map(r => `${PROVIDERS[providerKey(r)].name}: ${fmtInt(r.rowsReturned)}`).join(", ");
    target.innerHTML = `<span class="validity bad">⚠ Eltérő sorszámok — a mérés nem összevethető! (${counts})</span>`;
  }
}

function renderChart() {
  const chart = el("chart");
  chart.innerHTML = "";
  const max = Math.max(...lastResult.results.map(r => r[metric]));
  // A leghosszabb sáv is csak a sáv-terület 82%-áig ér, így az értékcímke
  // mindig a sávon kívülre, szöveg-színnel kerül (a szín csak identitást hordoz).
  const scale = 82;
  for (const r of lastResult.results) {
    const p = PROVIDERS[providerKey(r)];
    const value = r[metric];
    const pct = max > 0 ? (value / max) * scale : 0;
    const row = document.createElement("div");
    row.className = "bar-row";
    row.innerHTML = `
      <div class="bar-label">${p.name}</div>
      <div class="bar-track">
        <div class="bar-fill" style="width:${pct}%;background:${p.color}"></div>
        <div class="bar-value" style="left:${pct}%">${fmt(value)} ms</div>
      </div>`;
    attachTooltip(row, r, p);
    chart.appendChild(row);
  }
}

function attachTooltip(row, r, p) {
  const tooltip = el("tooltip");
  row.addEventListener("mousemove", e => {
    tooltip.hidden = false;
    tooltip.innerHTML = `
      <strong>${p.name}</strong>
      <table>
        <tr><td>Min</td><td>${fmt(r.minMs)} ms</td></tr>
        <tr><td>Medián</td><td>${fmt(r.medianMs)} ms</td></tr>
        <tr><td>Átlag</td><td>${fmt(r.meanMs)} ms</td></tr>
        <tr><td>P95</td><td>${fmt(r.p95Ms)} ms</td></tr>
        <tr><td>Max</td><td>${fmt(r.maxMs)} ms</td></tr>
        <tr><td>Összesen</td><td>${fmt(r.totalMs)} ms</td></tr>
      </table>`;
    const x = Math.min(e.clientX + 14, window.innerWidth - tooltip.offsetWidth - 8);
    const y = Math.min(e.clientY + 14, window.innerHeight - tooltip.offsetHeight - 8);
    tooltip.style.left = x + "px";
    tooltip.style.top = y + "px";
  });
  row.addEventListener("mouseleave", () => { tooltip.hidden = true; });
}

function renderTable() {
  const tbody = el("stats").querySelector("tbody");
  tbody.innerHTML = "";
  for (const r of lastResult.results) {
    const p = PROVIDERS[providerKey(r)];
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td><span class="swatch" style="background:${p.color}"></span>${p.name}</td>
      <td>${fmt(r.minMs)}</td><td>${fmt(r.medianMs)}</td><td>${fmt(r.meanMs)}</td>
      <td>${fmt(r.p95Ms)}</td><td>${fmt(r.maxMs)}</td><td>${fmt(r.stdDevMs)}</td>
      <td>${fmtInt(r.rowsReturned)}</td>`;
    tbody.appendChild(tr);
  }
}

function addHistory(result) {
  history.unshift(result);
  el("history-card").hidden = false;
  renderHistory();
}

function renderHistory() {
  el("history-metric-head").textContent = `${METRICS[metric]} (ms)`;
  const tbody = el("history").querySelector("tbody");
  tbody.innerHTML = "";
  for (const h of history.slice(0, 20)) {
    const scenario = scenarios.find(s => s.key === h.scenario);
    const summary = h.results
      .map(r => `${PROVIDERS[providerKey(r)].name}: ${fmt(r[metric])}`)
      .join(" · ");
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${new Date(h.startedAtUtc).toLocaleTimeString("hu-HU")}</td>
      <td>${scenario ? scenario.name : h.scenario}</td>
      <td>${h.iterations}</td>
      <td style="text-align:left">${summary}</td>`;
    tbody.appendChild(tr);
  }
}

async function reseed() {
  if (!confirm("Minden adat törlődik és újragenerálódik a Seed beállítások szerint. Ez akár perceken át tarthat. Folytatod?")) {
    return;
  }
  const button = el("reseed");
  button.disabled = true;
  el("status").textContent = "Újratöltés fut… (hosszú művelet)";
  try {
    await api("/api/admin/reseed", { method: "POST" });
    el("status").textContent = "Újratöltés kész.";
    await loadStatus();
  } catch (err) {
    el("status").textContent = "Hiba: " + err.message;
  } finally {
    button.disabled = false;
  }
}

document.querySelectorAll("button.metric").forEach(button => {
  button.addEventListener("click", () => {
    metric = button.dataset.metric;
    document.querySelectorAll("button.metric").forEach(b => b.classList.toggle("active", b === button));
    render();
    renderHistory();
  });
});

el("run").addEventListener("click", runCompare);
el("reseed").addEventListener("click", reseed);

loadStatus();
loadScenarios().catch(err => { el("status").textContent = "Hiba: " + err.message; });
