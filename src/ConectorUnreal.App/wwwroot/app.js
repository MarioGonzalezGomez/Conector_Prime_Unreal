const statusEl = document.getElementById("status");
const buttonsEl = document.getElementById("buttons");
const eventsEl = document.getElementById("events");
const manualForm = document.getElementById("manualForm");
const signalInput = document.getElementById("signalInput");
const reconnectBtn = document.getElementById("reconnectBtn");
const refreshBtn = document.getElementById("refreshBtn");

async function getJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json();
}

async function postJson(url, body) {
  const response = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body || {})
  });

  if (!response.ok && response.status !== 202) {
    throw new Error(`Request failed: ${response.status}`);
  }
}

function renderStatus(health, metrics) {
  const rows = [
    { label: "TCP listener", value: health.tcpListening ? "Running" : "Stopped" },
    { label: "Unreal Endpoint", value: health.unrealConnected ? "Connected" : "Disconnected" },
    { label: "Total events", value: metrics.totalEvents },
    { label: "Received", value: metrics.receivedSignals },
    { label: "Sent", value: metrics.sentSignals },
    { label: "Errors", value: metrics.errorSignals }
  ];

  statusEl.innerHTML = rows
    .map((row) => `<div class="status-pill"><strong>${row.label}</strong>${row.value}</div>`)
    .join("");
}

function renderButtons(commands) {
  buttonsEl.innerHTML = "";

  for (const command of commands) {
    const btn = document.createElement("button");
    btn.textContent = command;
    btn.addEventListener("click", () => sendSignal(command));
    buttonsEl.appendChild(btn);
  }
}

function renderEvents(events) {
  eventsEl.innerHTML = events
    .slice()
    .reverse()
    .map((event) => {
      const cls = event.success === false || event.stage === 4 ? "err" : "ok";
      const meta = [
        `Origin: ${event.origin}`,
        `Stage: ${event.stage}`,
        event.normalizedSignal ? `Signal: ${event.normalizedSignal}` : null,
        event.actionName ? `Action: ${event.actionName}` : null,
        event.jsonPayload ? `Payload: ${event.jsonPayload}` : null
      ].filter(Boolean).join(" | ");

      return `
        <div class="event-row ${cls}">
          <div class="event-main">
            <strong>${event.message}</strong>
            <span>${new Date(event.timestampUtc).toLocaleTimeString()}</span>
          </div>
          <div class="event-meta">${meta}</div>
        </div>
      `;
    })
    .join("");
}

async function sendSignal(signal) {
  await postJson("/api/manual", { signal });
  await refresh();
}

async function refresh() {
  const [health, metrics, commands, events] = await Promise.all([
    getJson("/api/health"),
    getJson("/api/metrics"),
    getJson("/api/commands"),
    getJson("/api/events?take=200")
  ]);

  renderStatus(health, metrics);
  renderButtons(commands);
  renderEvents(events);
}

manualForm.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const value = signalInput.value.trim();
  if (!value) {
    return;
  }

  await sendSignal(value);
  signalInput.value = "";
});

reconnectBtn.addEventListener("click", async () => {
  await postJson("/api/unreal/reconnect", {});
  await refresh();
});

refreshBtn.addEventListener("click", refresh);

setInterval(() => {
  refresh().catch(() => {});
}, 2500);

refresh().catch((err) => {
  eventsEl.innerHTML = `<div class="event-row err">${err.message}</div>`;
});
