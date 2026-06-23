# PUMA PDC Synthetic Monitoring — Project Context & Build Plan

> **What this is:** the single source of truth for the PUMA DropCopy (PDC) synthetic-monitoring automation I'm building during my Barclays GTSM/Markets/Equities SRE internship. Anyone (or any Claude Code session) reading this should be able to understand the system, the manual check being automated, what already exists vs what I build, the tooling, and the plan — without extra context.
>
> **⚠️ Sensitive-data policy (read first):** real production **hostnames, absolute file paths, test-client IDs, ports, and credentials** are written as placeholders — `<HOST_X>`, `<LOGDIR_X>`, `<TEST_CLIENT_X>`, `<PORT>`. The *structure* here is exact; real literals are filled in **only locally on the work PC**, never committed. System/component names (PUMA, PDC, Stomp, etc.) are internal names kept for accuracy in this **private** repo. Do not add real hostnames/paths/creds to any committed file.

---

## 1. TL;DR (the whole project in one paragraph)

**PDC (Puma DropCopy)** is the read-only "drop-copy" pipeline that carries equity trades out of the **PUMA** trading system to downstream consumers: **regulatory reporting (ACT → US/FINRA)**, **Street-Side Booking → Netting**, and the **Data Warehouse**. A *silent* break here means trades stop reaching regulatory reporting **while every screen still looks healthy** — a compliance gap nobody sees. To catch that, the desk runs **synthetic monitoring** every shift: inject known **test orders** into PUMA, then trace each one through **PDC → KDB (StompDB) → each component's log** across **6 PDC instances**, and report **RED/GREEN**. The **verify + report** half is already automated (a checker script + AutoSys jobs). **My job:** automate the still-manual **test-order injection** (UiPath driving the PUMA desktop GUI) and **surface live health in Geneos (ITRS)** — ideally wiring the whole **inject → verify → visualize** loop hands-off.

---

## 2. The mental model to never lose: **Inject → Verify → Visualize**

| Stage | What it means | Status / owner |
|---|---|---|
| **Inject** | Put known synthetic trades into PUMA (6 test orders, one per instance) | **MANUAL → ME (UiPath)** |
| **Verify** | Trace each trade through PDC → KDB → every component log across 6 instances | **Exists** (checker script + AutoSys) |
| **Visualize** | Live RED/GREEN in Geneos so anyone sees PDC health at a glance; RED = L2 acts | **Gap → ME (Geneos design)** |

**The blockchain analogy (my edge):** this is *exactly* tracing a transaction through a pipeline — **submit a known tx → confirm it's recorded in the DB → confirm every downstream subscriber received it.** A missing log entry = a subscriber that silently dropped the event. I've built this pattern for on-chain indexers/relayers; PDC is the TradFi version.

---

## 3. My role & org context

- **Team:** GTSM (Global Technology Service Management) → **Markets** → **Equities**, supporting the **SRE** function.
- **Discipline:** Site Reliability Engineering — kill **toil** (repetitive manual work) via automation. This is **RTB** (Run The Bank), **not BTB** (Build The Bank). It is **production reliability automation / synthetic monitoring**, *not* QA/testing.
- **Project:** "Synthetic Health Check Automation for Trading Applications." Lead: **Pawan Kommuri**. Teammate: **Shashank Sathish**.
- **Scope reality:** ~400+ manual checks across desks; mandate is "automate as much as you can." I own the **Equities / PDC** area.
- **My edge:** finance literacy (DeFi/RWA — perps, options, structured products, OTC settlement, tx tracing/indexing). It lets me add a **correctness layer**: checking *meaning*, not just "is it up." E.g. I understand *why* a missing ACT-reporting log entry is a **regulatory** gap, not just a failed grep.

---

## 4. System glossary (real component names — use freely)

| Name | What it is |
|---|---|
| **PUMA** | Equities trading / order-management system. Orders entered via the **PUMA GUI** (desktop thick client). |
| **Puma DropCopy (PDC)** | Read-only drop-copy of PUMA's trade flow, built on the **Stomp** framework. Listens to PUMA replication traffic; fans data to consumers. **6 instances:** `PDC, PDC1, 2PDC, 3PDC, 4PDC, 5PDC`. |
| **Stomp** | Internal messaging/object framework PDC is built on. |
| **OrderManager (OM)** | Ingests data, normalizes it into Stomp objects. |
| **OMAgent** | Listens to transactions from OrderManager, commits them to StompDB. |
| **StompDB / KDB** | kdb+/Q time-series database where all transactions are recorded. The "did it get captured?" checkpoint. |
| **DataDistributor** | Fans transactions out to the gateways. |
| **ActReportingEngine (ACT)** | Gateway → US/FINRA trade reporting (regulatory). One per affinity (AFF6/7/8/9…). |
| **SSB (Street Side Booking)** | Gateway → publishes to **Netting**. |
| **OESDistributor** | Gateway → publishes via OES → Compass. |
| **DDMsgRouter** | Message-router component. |
| **PumaToPOG** | Component writing to POG. |
| **Affinity / AFFINITY_ID** | Which instance/partition a process runs as (AFF6, AFF7, …). |
| **TIBCO / TIB / RV (Rendezvous)** | Messaging middleware moving data between components. |
| **Checker script** | `Puma_pdc_check.ksh` — verifies a test trade across KDB + all component logs; driven by a config file (one row per PDC instance). |
| **AutoSys** | Job scheduler running the checker jobs (`pum_c_gl_pdc_weekday/weekend_checks_report`). |
| **Geneos (ITRS)** | Bank-standard monitoring dashboard. Live "dataviews" with RED/AMBER/GREEN cells. Pieces: **Netprobe** (agent on host), **Gateway** (central brain/rules), **Active Console** (display). |
| **UiPath** | Desktop automation (RPA) — for driving the PUMA GUI. |
| **Playwright** | Browser automation — for any web-based checks (Node.js). |
| **DWH** | Data Warehouse (downstream consumer). |
| **L1 / L2 / L3** | Support tiers. L1 = first responders/runbooks; L2 = deeper fixes; L3 = engineers. |

---

## 5. Architecture & data flow

### 5.1 The pipeline (the "relay race")

```
   PUMA AppServer  (trade is born; entered via PUMA GUI)
        |
        | replication traffic (ODBC -> TT -> XLA -> RepServer -> RepClient -> Sybase),
        | and TIBCO (TIB/RV) messaging
        v
   ┌─────────────────────── Puma DropCopy (PDC) ───────────────────────┐
   │   OrderManager (normalizes -> Stomp objects)                      │
   │        | SST                                                      │
   │   OMAgent (commits to DB)                                         │
   │        | socket                                                   │
   │   StompDB / KDB  ("SDB PDC")   <-- KDB_STATUS checkpoint          │
   │        | socket                                                   │
   │   DataDistributor (fans out)                                      │
   │     /        |          |            |           \               │
   │  VolData  DDMsgRouter  SSB(AFF6)  ACTReporting  OESDistributor    │
   │     |        |          |          (AFFx)         |              │
   └─────┼────────┼──────────┼────────────┼────────────┼──────────────┘
         v        v          v            v            v
      EU TRM   Netting   ActGateway     OES -> GPM   ... -> DWH
                                       (regulatory)
   GUIs: PUMA GUI, PW GUI  (desktop thick clients)
```

**Reading it:** a trade is born in PUMA → replicated into PDC → **OrderManager** normalizes it to a Stomp object → **OMAgent** commits it to **KDB** (checkpoint #1: *captured?*) → **DataDistributor** fans it to the gateways → each gateway writes its **own log** (checkpoint #2: *propagated?*). The two checkpoints — **KDB_STATUS** and **LOG_STATUS** — are the heart of the health check.

### 5.2 The 6 PDC instances (why config-driven is the right design)

Every instance is the **same shape** (OrderManager → OMAgent → SDB/KDB → DataDistributor → gateways) but with a different host, test client, log directory, and affinity. They're fed static data from a **Static Data Server (Slave mode)** over TIBCO RV. **Identical-shape × 6 is precisely why the solution must be config-driven: one routine + a 6-row config, never 6 hardcoded flows.**

| Instance | Host (placeholder) | Log dir (placeholder) | Notes |
|---|---|---|---|
| PDC   | `<HOST_PDC>`   | `<LOGDIR_PDC>/logs`   | AFF6 region |
| PDC1  | `<HOST_PDC1>`  | `<LOGDIR_PDC1>/logs`  | AFF7 |
| 2PDC  | `<HOST_2PDC>`  | `<LOGDIR_2PDC>/logs`  | AFF8 |
| 3PDC  | `<HOST_3PDC>`  | `<LOGDIR_3PDC>/logs`  | AFF9 |
| 4PDC  | `<HOST_4PDC>`  | `<LOGDIR_4PDC>/logs`  | |
| 5PDC  | `<HOST_5PDC>`  | `<LOGDIR_5PDC>/logs`  | checker host |

Each instance's log dir holds component logs like:
`ActReportingEngineAFFx.log`, `DataDistributorPumaDropCopy.log`, `OMAgentPumaDropCopy.log`, `PumaToPOG<INSTANCE>.log`, `SSBApp*.log`, `OESDistributorPumaDropCopy.log`, `DDMsgRouterX.log` (the exact component set varies slightly per instance — see Open Question #5).

### 5.3 Reference notes
- Static Data Server feeds static data to order managers in real time via **TIBCO RV**.
- Order managers get transaction data from the **PUMA Rep Server**.
- In prod some processes run **two affinities** (e.g. `PumaDropCopy` and `PumaDropCopyRSS2` / `PumaDropCopyCF`).
- **Startup scripts:** `startOrderManagerPumaDropCopy.sh`, `startOMAgent.sh -a AFFINITY_ID`, `startDataDistributor.sh -a AFFINITY_ID`, `startActReportingEngine.sh -a AFFINITY_ID`, `startOESDistributor.sh -a AFFINITY_ID`, `startStreetBookingApplication -a AFFINITY_ID`, etc.
- **Verify-startup strings** (usable as health assertions):
  - OrderManager: `"Completed initialization of QdbConfig"`
  - StompDB: `"Starting StaticDataServerEQ application now."`
  - ACTReporting / StreetSideBooking: `confirmingStream; ... state=RECEIVING`

---

## 6. The manual PDC check (what's being automated)

**Cadence:** once per **APAC / EMEA / AMER** shift at ~**05:30** each region. **All 6 test orders must be created within ~20 minutes.**

### Step 1 — Inject 6 test orders via PUMA GUI — *STILL MANUAL → my UiPath target*
For each PDC instance, create one test order with its instance-specific test client:

| Instance | Test client (placeholder) |
|---|---|
| pdc  | `<TEST_CLIENT_PDC>`  (e.g. format like `TEST_CF5878P1_CT10`) |
| pdc1 | `<TEST_CLIENT_PDC1>` |
| 2pdc | `<TEST_CLIENT_2PDC>` |
| 3pdc | `<TEST_CLIENT_3PDC>` |
| 4pdc | `<TEST_CLIENT_4PDC>` |
| 5pdc | `<TEST_CLIENT_5PDC>` |

**Order-entry field values** (constant across instances *except* the client):
- **Client** = `<TEST_CLIENT_x>`
- **RIC** = `ZVZZT.OQ` (dummy test ticker; ZVZZT is a recognized test symbol)
- **Direction** = Buy
- **Quantity** = 100
- **Order Type** = Market
- **Destination** = Program
- **Lehman Product** = Portfolio
- **Desk** = CASH

**GUI flow:**
1. Open **Order Entry**, fill the fields above.
2. Click **Create** (top-left of the order-entry form).
3. A **Validation Warning** popup appears: *"'Not Held' or 'Held' instruction is required for single stock!"* → click **Not Held**.
4. Repeat for all 6 instances.
5. Once all 6 are sent, they appear under the **Alerts** tab (with clientShortName, Server_Name, clientId, masterId, Business Area, Product Type, RIC, portfolioId, …).

### Step 2 — Verify in KDB — *ALREADY AUTOMATED*
Query the DB for the test order; take its **transaction ID**. (Manual variant: `select from Portfolio where LIST_ID=<id>`, then take the 5/6 transaction IDs.)

### Step 3 — Verify in component logs — *ALREADY AUTOMATED*
Per instance, `grep -l <TRANSACTION_ID> *.log` in that instance's log dir. The transaction ID must appear in each **expected** component log (ActReportingEngine, DataDistributor, OMAgent, PumaToPOG, SSBApp, OESDistributor — as applicable).

### Step 4 — Checker job + report — *ALREADY AUTOMATED*
- Run `Puma_pdc_check.ksh` (on the checker host) and/or AutoSys jobs `pum_c_gl_pdc_weekday/weekend_checks_report`.
- Config `pdc_conf_nav` drives it — **one row per instance**:
  `<TEST_CLIENT>,<short_name>,<instance>,<HOST>:<PORT>`
  → **This is already config-driven; I should mirror its shape on the injection side.**
- Connection check: `conncheck.sh`.
- Output = a **RED/GREEN email** to the support distro. RED → notify L2.

---

## 7. The status report → the Geneos dataview

Columns of the RED/GREEN report (and the target Geneos dataview):

| Column | Meaning |
|---|---|
| `APPSERVER` | App-server short name for the instance |
| `APPSERVER_STATUS` | OK / not OK — is the app server up |
| `LIST_ID` | The test order's list id |
| `PDC_INS` | Which PDC instance (pdc, pdc1, 2pdc, …) |
| `KDB_STATUS` | OK — recorded in KDB? |
| `TRANS_ID` | The transaction id traced |
| `LOG_STATUS` | OK, or `NOT OK, TRANSID not found in the logs of <component>` |

**GREEN (all healthy):**
```
APPSERVER  APPSERVER_STATUS  LIST_ID   PDC_INS  KDB_STATUS  TRANS_ID  LOG_STATUS
<srv1>     OK                <id1>     pdc      OK          <tid1>    OK
<srv2>     OK                <id2>     pdc1     OK          <tid2>    OK
<srv3>     OK                <id3>     2pdc     OK          <tid3>    OK
<srv4>     OK                <id4>     3pdc     OK          <tid4>    OK
<srv5>     OK                <id5>     4pdc     OK          <tid5>    OK
<srv6>     OK                <id6>     5pdc     OK          <tid6>    OK
```

**RED (one component dropped the trade):**
```
APPSERVER  APPSERVER_STATUS  LIST_ID   PDC_INS  KDB_STATUS  TRANS_ID  LOG_STATUS
<srv4>     OK                <id4>     3pdc     OK          <tid4>    NOT OK, TRANSID not found in
                                                                     logs of ActReportingEngineAFFx
```

**The correctness-layer insight (why PDC is tier-1):** `APPSERVER OK + KDB OK + LOG NOT OK on ACT` = the trade was **captured but did not propagate to regulatory reporting.** Every screen is green; regulatory reporting is silently incomplete. This is the exact **"up but wrong"** failure synthetic monitoring exists to catch. (Same failure family as the LIBOR/FX/structured-note scandals from my asset-class notes: the system looked fine while a number was silently wrong.)

---

## 8. What exists vs what I build

| Stage | Status | Owner |
|---|---|---|
| **Inject** 6 test orders via PUMA GUI | **MANUAL** | **ME — UiPath** |
| Verify in KDB | Automated (checker script) | exists |
| Verify in component logs (grep transID) | Automated (checker script) | exists |
| RED/GREEN report email | Automated (script + AutoSys) | exists |
| **Visualize** live health in Geneos | gap / improve | **ME — Geneos design** |
| **Orchestrate** inject → trigger checker → surface result | gap | **ME — tie it together** |

**My deliverables:**
1. **UiPath robot** that drives the PUMA GUI to create all 6 test orders (handling the **Not Held** popup), **config-driven** (one routine + a 6-row config mirroring `pdc_conf_nav`).
2. **Geneos dataview** surfacing per-instance status with proper RED/AMBER/GREEN, a **freshness ("last checked") cell**, and an **overall roll-up**. Colors so RED always = L2 action. Likely fed via a **Toolkit** plugin (script emits tabular rows) and/or **FKM** (File/Keyword Monitor) rules on the key log strings.
3. **(Stretch) Orchestration** so injection triggers the existing checker and the result lands in Geneos — full hands-off **inject → verify → visualize** loop.

---

## 9. Tooling split & POC environment

- **Browser-based apps → Node.js + Playwright** (my comfort zone). Keep language scoped to browser scripts; the harness/glue may end up Python- or ksh-shaped (the checker world is ksh/KDB) — stay flexible. *(Note: my standing tooling preference is Python-first, Node only as fallback; here Playwright is via Node because that's the team's existing browser-automation stack — confirm.)*
- **Desktop apps (PUMA GUI) → UiPath.** Unfamiliar → **highest learning priority.** Must confirm **PUMA is native desktop vs Citrix-streamed** (native = real selectors; Citrix = image/OCR).
- **No PUMA access yet** → practice against a **dummy order-entry form** (HTML mirroring PUMA's fields + the Not Held popup) for both Playwright and UiPath.
- **POC sync:** work flows through this private GitHub repo (`tradfi`); Claude Code remote-control pushes to it; I run/adapt on the work PC (signed permission for AI-assisted POC). The timestamped repo log (`logs/worklog.md`) is the durable memory.

---

## 10. Open questions to confirm with the team

1. Is **PUMA GUI native desktop or Citrix-streamed**? (Selector strategy hinges on this.)
2. Is my target **just GUI injection**, or **full orchestration** (inject → trigger checker → Geneos)?
3. Geneos: **enhance an existing PDC dataview or build new**? Fed via **Toolkit** script, **FKM**, or both? Who owns Netprobe/Gateway config (likely infra)?
4. Confirm **bank policy on AI-assisted code** for this work (have signed POC permission; confirm scope).
5. Which **components are mandatory in LOG_STATUS** per instance (the expected-log set varies)?

---

## 11. Why this is my edge — the DeFi / blockchain mapping

| PDC concept | DeFi/web3 analogue I already know |
|---|---|
| Inject a known test order | Submit a **known test transaction** to a chain/relayer |
| KDB capture checkpoint | Tx **confirmed / recorded in the indexed DB** |
| Component logs per gateway | **Downstream subscribers** (indexers, bridges, webhooks) each ack the event |
| Missing log entry | A **subscriber that silently dropped the event** |
| ACT regulatory reporting gap | A **compliance/settlement leg that never fired** (funds/records out of sync) |
| 6 identical instances + config | **Multi-chain / multi-instance relayer** driven by one config |
| "Up but wrong" failure | **Oracle stale / event dropped** while the dashboard reads green |
| Geneos RED/GREEN dataview | A **monitoring dashboard over a tx pipeline's health** |

This is the tracing-a-transaction-through-a-pipeline pattern I've built before — that's why I can add the **correctness layer** instead of just "is the process running."

---

## 12. Build roadmap (the 2-month plan)

> Goal across the internship: **automate as much of inject → verify → visualize as possible**, with a reusable, config-driven design. Reliable-and-simple beats clever-and-brittle (a flaky synthetic check that false-alarms is worse than none — SREs stop trusting it).

**Phase 0 — Foundation (now)**
- [x] Capture full context (this doc).
- [ ] Build the **dummy HTML order-entry form** (mirrors PUMA fields + Not Held popup) as the practice target.
- [ ] Confirm Open Questions #1 and #4 with the team (Citrix? AI-code scope?).

**Phase 1 — Injection POC (Playwright on the dummy form)**
- [ ] Playwright (Node) script that fills the order form, clicks Create, handles the Not Held popup, loops over a **6-row config** mirroring `pdc_conf_nav`.
- [ ] Prove the config-driven shape end-to-end against the dummy form.

**Phase 2 — Injection on real PUMA (UiPath)**
- [ ] Port the flow to **UiPath** for the PUMA desktop GUI (selector strategy per Citrix answer).
- [ ] Robust popup handling, retries, per-instance client switching, run all 6 within the 20-min window.

**Phase 3 — Visualize (Geneos)**
- [ ] Design the dataview (per-instance status, freshness cell, roll-up). Feed via Toolkit and/or FKM on the key log strings.
- [ ] RED = L2 action, AMBER = stale/degraded, GREEN = healthy.

**Phase 4 — Orchestrate (stretch)**
- [ ] Wire injection → trigger existing checker (`Puma_pdc_check.ksh` / AutoSys) → result into Geneos. Hands-off loop.

**Cross-cutting:** keep everything **config-driven**, log every step/approach in `logs/worklog.md`, never commit real hostnames/paths/creds, and add the **correctness layer** wherever possible (verify meaning, not just liveness).

---

*This doc is the project's living context. Update it as open questions get answered and the build progresses.*
