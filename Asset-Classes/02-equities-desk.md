# Markets Deep-Dive — 02 — The EQUITIES Desk

> Part of my Barclays GTSM Markets internship notes.
> Goal: understand each asset class deeply, with numbers, mapped to my DeFi/RWA experience.
> Format: easy language, hard terms explained in (brackets), worked numerical scenarios.

---

## 0. Where "Equities" sits (the big picture)

The Markets division trades Rates, Credit, FX, Commodities, **Equities**, plus Prime Brokerage as a service layer. A trade is born **Front Office** (traders/sales), checked **Middle Office** (risk), finished **Back Office** (settlement/confirmation/reconciliation).

**Equities = ownership of companies, and everything derived from that ownership.** What the desk trades:

1. **Cash equities** — actual shares/stocks (the spot market).
2. **Equity derivatives** — futures, options on stocks and indices.
3. **Structured products** — derivatives packaged into a wrapper and sold for a margin (my PPN wheelhouse).
4. Plus the **service rails**: execution channels (high/low/zero touch), securities lending, prime.

The constant truth across ALL desks (same as Rates):
> The bank mostly earns **commission + spread on client flow** and **margin on structuring** — it is a *facilitator / market maker*, NOT a gambler. On equities the agency/commission piece is bigger than on Rates, because a huge part of the business is just *executing client orders well* and charging for it.

**The unique flavor of Equities vs Rates:** Rates is mostly OTC (bilateral swaps). Equities is mostly **exchange-traded and execution-speed-obsessed** — so a giant part of the desk's edge and income is *how fast and how cleverly it routes and executes orders*. This is why latency, algos, and the "touch" spectrum matter so much here and barely came up in Rates.

---

## 1. The one idea Equities is built on: a share is a slice of a company

Buy 1 share = you own a tiny slice of a real business: a claim on its profits (**dividends**) and its growth (**price appreciation**).

Terms in brackets:
- **Equity / stock / share** — a unit of ownership in a company.
- **Dividend** — a cash payout of profits to shareholders (optional; many firms pay none).
- **Market cap** — share price × number of shares = the whole company's market value.
- **Index** — a basket of stocks tracked as one number (e.g. **Nifty 50**, **Bank Nifty**, S&P 500, FTSE 100). You can trade the basket itself via index futures/options.

**DeFi bridge:** a share ≈ a **governance/ownership token** with a real cash-flow claim. A dividend ≈ a **staking reward / protocol revenue distribution**. An index ≈ an **index token / on-chain basket** (like a DeFi index that rebalances a set of tokens). Market cap is identical to a token's FDV-style valuation (price × supply).

**India specifics I already know:** whole shares only (no fractional), no perpetuals (SEBI bans perps), derivatives are **expiry-based** (weekly + monthly), index derivatives are **cash-settled**, single-stock derivatives are **physically settled**. Nifty 50 / Bank Nifty dominate flow.

---

## 2. Before you trade: the two ways to decide WHAT to trade

The desk taught this explicitly — **before trading you look at fundamentals AND charts.** These are the two analysis schools:

### 2a. Fundamental analysis — "what is the company actually worth?"
You read the business: earnings, revenue growth, debt, margins, the famous ratios.
- **EPS (Earnings Per Share)** = profit ÷ number of shares.
- **P/E ratio (Price ÷ Earnings)** = how many years of earnings you're paying for. High P/E = market expects growth; low P/E = cheap or troubled.
- Also: revenue, debt load, cash flow, management quality, sector outlook.

**Scenario A — fundamentals**
Company A: share price **₹500**, EPS **₹25**.
- P/E = 500 ÷ 25 = **20** → you pay 20 years of current earnings for the stock.
- If a competitor with the same growth trades at P/E 15, Company A looks *expensive* on fundamentals (or the market expects it to grow faster).

**DeFi bridge:** fundamental analysis = looking at a protocol's **real revenue / fees / TVL / token emissions** to judge if the token is over- or under-valued — the "is this protocol actually earning, or is it hype?" question. P/E ≈ **price-to-fees (P/F) ratio** people use for DeFi protocols.

### 2b. Technical analysis — "what is the price chart telling me?"
You read the **trading charts**: price patterns, trends, support/resistance levels, volume, moving averages, momentum indicators (RSI, MACD). It ignores the business and reads crowd behavior in the price.

**DeFi bridge:** identical to **TA on a token chart** — support/resistance, moving averages, RSI — the exact same charting you do on any crypto pair. The tools literally transfer 1:1.

**The takeaway:** fundamentals tell you *what* to buy and at what value; technicals tell you *when* to buy/sell and at what level. Traders use both. (My monitoring job doesn't pick trades — but it must keep the *data feeding both* fresh and correct: stale fundamentals or a frozen chart feed = a trader deciding on wrong info.)

---

## 3. The order book and order types (the mechanics)

I know the order book cold from DeFi/perps, so this is fast — but the **order types** below are the new, examable part.

**Order book recap:** **bid** = highest price a buyer will pay; **ask/offer** = lowest price a seller will accept; **spread** = ask − bid (the market maker's income). **Crossed book** (bid ≥ ask) = broken feed. **Market order** = certainty of execution, uncertain price. **Limit order** = certain price, uncertain execution. **Slippage** = gap between expected and actual fill.

### Order types by TIME-IN-FORCE (how long the order lives) — the desk flagged these:
- **Day order** — valid only for the current trading session; if unfilled by market close, it **expires automatically**. The default.
- **GTD (Good Till Date)** — stays live until a **specified future date**, then expires if unfilled.
- **GTC (Good Till Cancelled)** — stays live until you cancel it (brokers often cap it, e.g. 90 days).
- **IOC (Immediate Or Cancel)** — fill whatever you can *right now*, cancel the rest.
- **FOK (Fill Or Kill)** — fill the **whole** order immediately or cancel it entirely (no partial).

**Scenario B — day vs GTD**
You place a limit buy: 1,000 shares of Company A at **₹495** (market is at ₹500).
- As a **day order**: if ₹495 isn't hit by close, the order dies tonight. Tomorrow you must re-enter.
- As a **GTD order to 30-Jun**: it keeps working every session until 30 June; if ₹495 prints any day before then, it fills, otherwise it expires 30 June.

**DeFi bridge:** time-in-force = the **deadline / TTL on a signed limit order** in systems like 0x, CoW Swap, or any orderbook DEX. A GTD order = a signed limit order with `expiry = <timestamp>`. IOC/FOK = the "revert if not fully fillable" flag on a swap (slippage/`minAmountOut` + all-or-nothing). Exact same primitives.

---

## 4. The "touch" spectrum — HOW an order gets executed (the core of this desk)

This is the part the desk spent real time on. When a client wants to trade, *how much human/bank involvement* the order gets defines the channel — and the commission. This is the **high-touch → low-touch → zero-touch** spectrum. (The bracketed names are Barclays' internal/branded systems the desk named.)

### 4a. HIGH TOUCH — a human sales-trader handles it `[PUMA]`
A skilled human trader takes the order and works it with discretion: big, sensitive, illiquid, or complex orders where careless execution would **move the market against the client**. Maximum service, maximum commission.

**Scenario C — high-touch block trade**
Fund B wants to buy **2,000,000 shares** of a mid-cap that only trades ~500,000 shares/day. Dumping a market order would spike the price (huge slippage). So it goes high touch:
- The sales-trader quietly sources liquidity over hours/days, uses dark pools, finds natural sellers, minimizes **market impact**.
- Commission: high, say **15 bps** (0.15%). On a ₹1,000,000,000 (₹100 cr) notional that's **₹1,500,000** commission.

**DeFi bridge:** high touch = an **OTC desk negotiating a large block** so it doesn't wreck the AMM price — exactly the BTC OTC settlement work I did. A human (or careful algo) avoids the slippage a naive on-chain market order would cause. "Market impact" = "price impact / slippage on a thin pool."

### 4b. LOW TOUCH — algorithms execute it `[BATMAN / BLACKBIRD]`
The client sends the order electronically; **execution algorithms** chop and route it automatically with minimal human input. Cheaper than high touch. The classic algos:
- **VWAP** (Volume-Weighted Average Price) — spread the order across the day to match the average traded price.
- **TWAP** (Time-Weighted Average Price) — slice evenly over a time window.
- **POV** (Percentage of Volume) — trade a fixed % of market volume as it happens.
- **Implementation Shortfall** — minimize the gap between decision price and final fill.

**Scenario D — low-touch VWAP**
Fund B wants 200,000 shares but doesn't need a human. It sends a **VWAP order** via the algo suite `[BATMAN/BLACKBIRD]`:
- The algo slices it into hundreds of child orders across the session to track VWAP and hide intent.
- Commission: lower, say **3 bps** (0.03%). On ₹100,000,000 that's **₹30,000**.

**DeFi bridge:** low touch = using a **smart order router / execution aggregator** (1inch, CoW) or a **TWAP bot** that splits a big swap into many small ones over time to reduce slippage. VWAP/TWAP algos literally exist on-chain now (e.g. TWAP orders). Same idea: automate the slicing.

### 4c. DMA / ZERO TOUCH — the client's own system hits the market `[QBS, CEBUM]`
**DMA (Direct Market Access)**: the client's own systems send orders **straight to the exchange** through the bank's membership/infrastructure — the bank is a **pure pipe**, no human, no algo decisioning. Used by quant/HFT funds that want raw speed and control. Lowest commission (fractions of a bp, often per-share), highest volume.

This is where **speed becomes the product**:
- **Colocation (colo)** — the client/bank places its servers **physically inside the exchange's data center**, cables away from the matching engine, to shave microseconds. The desk quoted **~158 nanoseconds** for an execution hop — that's *nanoseconds* (1 ns = a billionth of a second); light travels ~30 cm in that time. Distance to the matching engine literally matters.
- **Smart Order Routing (SOR)** — splits/routes an order across multiple venues to get the best price instantly.

**Scenario E — zero-touch / DMA via colo**
A quant fund routes through DMA `[QBS/CEBUM]`, co-located:
- Order reaches the matching engine in ~**158 ns**; the fund's strategy fires thousands of orders/second.
- Commission: tiny, e.g. **0.1 bp** or a per-share fee — but on **enormous** volume. The desk cited a peak APAC day of **~$55,000,000,000 (≈ $55bn) traded in ~8 hours**. Even at 0.1 bp, $55bn × 0.001% = **~$550,000** in a single day from one region's flow — and that's before spread/market-making income.

**DeFi bridge:** DMA + colocation = an **MEV searcher / HFT bot co-located near the sequencer or with the lowest-latency mempool access**, racing to land transactions first. 158 ns colo ≈ the latency arms race to be first in the block. Smart Order Routing = exactly a **DEX aggregator splitting across pools** for best execution. Zero touch = "my bot signs and sends straight to the chain; the venue is just infrastructure."

### The touch spectrum in one table
| Channel | Who executes | Barclays system named | Cost to client | Volume | DeFi analogue |
|---|---|---|---|---|---|
| **High touch** | Human sales-trader | `PUMA` | Highest (e.g. 15 bps) | Low | OTC block desk |
| **Low touch** | Bank's algos | `BATMAN / BLACKBIRD` | Medium (e.g. 3 bps) | Medium | SOR / TWAP bot |
| **DMA / Zero touch** | Client's own system | `QBS, CEBUM` | Lowest (sub-bp) | Huge | Co-located MEV/HFT bot |

> Mental model: **the less the bank touches it, the cheaper it is and the more volume runs through it.** High touch = high margin / low volume; zero touch = razor margin / gigantic volume. The bank wants *all three* because together they capture the whole client spectrum.

---

## 5. OTC equities — bilateral, "no governing body"

Most equities trade on an **exchange** (NSE, BSE, LSE), where the exchange + a **clearing corporation** act as the central counterparty and *governing/guaranteeing body* — they enforce rules and guarantee settlement so neither side can default on you.

**OTC (Over The Counter) equities trade differently:** directly between two parties, **off-exchange, with no central governing body** sitting in the middle. This is what the desk meant by "OTC has no governing body."
- **Pros:** customizable size/terms, privacy (big blocks don't show on the public book), can trade things not listed.
- **Cons:** **counterparty risk** (the other side could default — no clearing house guaranteeing it), less price transparency, you self-manage settlement.

**Scenario F — OTC block vs exchange**
Fund B buys a huge stake directly from Fund C OTC at a negotiated price.
- No exchange order book is touched (no market impact, price stays private).
- BUT if Fund C fails to deliver the shares, there's **no clearing corp to make Fund B whole** — they bear the counterparty risk directly. On an exchange, the clearing corp would have guaranteed it.

**DeFi bridge:** this is the cleanest mapping I have. Exchange-traded = **trading on a DEX/AMM**, where the smart contract is the "governing body" guaranteeing atomic settlement (no counterparty risk — code escrows both legs). OTC equities = a **raw peer-to-peer swap with no escrow contract** — you trust the counterparty to deliver, exactly the counterparty risk I designed *around* with escrows/HTLCs in my OTC BTC settlement work. The whole point of my Liquid OTC settlement was to *remove* this "no governing body" risk via covenants. TradFi OTC equities still largely runs on bilateral trust + legal docs (ISDA-style) instead of code.

---

## 6. Equity derivatives & structured products (my home turf, mapped fast)

### Futures & options on stocks/indices
- **Equity future** = agreement to buy/sell a stock/index at a set price on a future date. In India: expiry-based (weekly/monthly), index futures **cash-settled**, single-stock **physically settled**, **no perps** (SEBI). **Basis** = futures − spot, converges to 0 at expiry.
- **Equity option** = right (not obligation) to buy (**call**) or sell (**put**) at a **strike** by expiry. Buyer's max loss = **premium**. **Premium = intrinsic value + time value**. Greeks: **delta, gamma, theta** (time decay), **vega** (vol sensitivity).

**Scenario G — an equity call option**
Stock at **₹500**. You buy a **₹520 strike call** for a **₹15 premium**.
- Intrinsic value now = max(500 − 520, 0) = **₹0** (it's **OTM**, out of the money). So the whole ₹15 is **time value**.
- If at expiry the stock is ₹560: intrinsic = 560 − 520 = ₹40. Profit = 40 − 15 = **₹25/share**.
- If at expiry the stock is ≤ ₹520: option expires worthless, you lose only the **₹15 premium** (asymmetric payoff — capped loss, like I know from perps-vs-options).
- **Sanity rule that becomes a monitoring check:** an option's premium must be **≥ its intrinsic value**, always. A ₹520 call when the stock is ₹560 can't trade below ₹40 — if a feed shows ₹35, the feed is broken (arbitrage-impossible).

**DeFi bridge:** options Greeks and payoff are exactly what I built in PPN.fi. Theta (time decay) ≈ the funding-like cost of holding; the asymmetric "max loss = premium" payoff is the convexity I used to construct principal protection.

### Structured products (the PPN wheelhouse)
The bank **packages** derivatives into a single wrapper and sells it to clients for a **margin**. Classic: a **Principal-Protected Note (PPN)** = a zero-coupon bond (returns your principal) + a call option (gives upside). The bank earns the spread between what the components cost it to build and what it sells the wrapper for.

**Scenario H — bank sells a PPN**
Bank packages a 1-year PPN: it costs the bank ₹96 of zero-coupon bond + ₹2 of options = **₹98 to build**, sells it at **₹100**. Margin = **₹2 (2%)** per note. Sell ₹1,000,000,000 of notes → **₹20,000,000** structuring margin.
- This is **exactly PPN.fi** — I combined zero-coupon bond pricing with options/perps to build principal-protected notes on-chain. The bank does the same thing off-chain and charges the wrapper margin. (Bonus irony from my context: Barclays' 2022 **$361m structured-note over-issuance** — they sold ~$17.7bn more notes than registered because *no system tracked issuance vs the legal cap*. That's a missing correctness check on exactly this product.)

---

## 7. How Barclays makes money on Equities (all the streams together)

Equities has **more income streams than Rates** because execution itself is a product:

1. **Commission (agency execution)** — charged per the touch spectrum: high touch (≈15 bps) > low touch (≈3 bps) > DMA/zero touch (sub-bp). Thin per-trade × the $55bn/day kind of volume.
2. **Bid-ask spread (principal market making)** — quoting both sides on cash equities and derivatives, skimming the spread while staying hedged.
3. **Algo & platform fees** — for the low-touch algo suite and DMA infrastructure/colo access.
4. **Structuring margin** — packaging structured products (Scenario H), the fattest margin.
5. **Securities lending** — lending shares to short-sellers for a **borrow fee** (overlaps Prime Brokerage). 
6. **Equity-derivatives market making** — spread on futures/options + managing the Greeks book.

**Scenario I — securities lending (quick)**
A short-seller needs to borrow 100,000 shares (price ₹500 = ₹50,000,000 value) to sell short. Barclays lends them from inventory at a **2% annual borrow fee** → **₹1,000,000/year** for shares it was already holding. **DeFi bridge:** identical to the **borrow APR a shorter pays on Aave/perp funding** — the lender earns a fee for supplying the asset to someone betting it falls.

**How the money adds up:** razor-thin commission and spread (sub-bp to a few bps) × *gigantic* volume ($55bn in a single region in 8 hours on a peak day), **plus** fat structuring margins on the bespoke end, **plus** securities-lending and platform fees. Flow-driven, technology-and-latency-intensive, with a high-margin structuring tail.

---

## 8. Why this is MY edge (DeFi/RWA connections, collected)

- **Order book / bid-ask / slippage** → native from DeFi perps and AMMs.
- **Touch spectrum** → high touch = OTC block desk (my BTC OTC work); low touch = SOR/TWAP bot (1inch/CoW); zero touch + colo = **MEV/HFT bot racing the sequencer**. I understand the latency arms race natively.
- **Smart Order Routing** → DEX aggregator splitting across pools for best execution.
- **OTC "no governing body"** → bilateral counterparty risk; the *exact* problem my Liquid OTC settlement (covenants/escrows) was built to remove. Exchange + clearing corp = the smart-contract escrow of TradFi.
- **Time-in-force (day/GTD/IOC/FOK)** → signed limit orders with expiry/TTL and all-or-nothing flags on orderbook DEXs.
- **Options/Greeks + structured products** → literally PPN.fi (zero-coupon bond + option = principal-protected note).
- **Securities lending** → borrow fee = Aave borrow APR / perp funding paid by shorts.
- **Index** → on-chain index token / rebalancing basket.

---

## 9. The "what breaks" layer — my synthetic-monitoring angle for Equities

Tier 3 (financially coherent) is my edge. Equities-specific correctness checks worth building:

1. **Book not crossed** — bid must be < ask. A crossed/locked book (bid ≥ ask) = a broken feed. App can be "up" and still publishing an impossible book.
2. **Price freshness** — last price / mark updating within expected cadence; a frozen tape on a live market is dangerous (decisions made on stale prices). Highest-value check, mirrors my SOFR-freshness logic from Rates.
3. **Option premium ≥ intrinsic value** — a call/put quoting below its intrinsic value is arbitrage-impossible = broken pricing (Scenario G).
4. **Greeks fresh & non-NaN** — delta/gamma/theta/vega present, numeric, and updating. NaN/stale Greeks = risk system flying blind.
5. **No missing strikes / expiries** — the option chain should have its expected strikes and expiry columns; gaps = a feed dropout.
6. **Execution latency / colo health** — for DMA/zero-touch, monitor round-trip latency against the ~ns/µs SLA; a latency spike *is* the outage for an HFT client even if the app "responds."
7. **Order lifecycle (synthetic transaction)** — can a synthetic order flow Order → Execution → Trade Capture → Validation → Confirmation → Settlement, not just "submit returns 200"? Confirm time-in-force is honored (a day order actually expires at close).
8. **Algo sanity (low touch)** — does a VWAP/TWAP child-order stream behave (slicing, not dumping)? A misbehaving algo silently causes market impact.
9. **Issuance-vs-limit checks (structured products)** — track notes issued against the registered/legal cap (the literal gap behind the $361m over-issuance). The clearest "screen green, number silently wrong" failure.

The one-liner for a design discussion:
> "For the equities apps, the synthetic checks shouldn't just confirm the screen loads — they should confirm the **book isn't crossed, the tape is fresh, option premiums clear intrinsic value, and the Greeks aren't NaN**, and for DMA flow that **latency is within SLA**. A frozen tape or a crossed book is a worse failure than an outage, because it looks green while handing away money."

Real-world proof: the **$361m structured-note over-issuance** (missing issuance-vs-cap check) is exactly the class of silent-number failure tier-3 checks catch.

---

## 10. Glossary (fast recall)

- **Equity / share / stock** — a unit of company ownership.
- **Dividend** — cash payout of profits to shareholders.
- **Market cap** — share price × shares outstanding.
- **Index** — a tradable basket of stocks (Nifty 50, Bank Nifty).
- **EPS / P/E** — earnings per share / price-to-earnings ratio (fundamental valuation).
- **Fundamental analysis** — valuing the business (earnings, ratios).
- **Technical analysis** — reading the price chart (trend, support/resistance, RSI).
- **Bid / ask / spread** — best buy / best sell / the gap (market-maker income).
- **Market vs limit order** — certainty of execution vs certainty of price.
- **Slippage / market impact** — gap between expected and actual fill; price moved by your own order.
- **Time-in-force** — how long an order lives: **Day** (today only), **GTD** (good till date), **GTC** (till cancelled), **IOC** (immediate-or-cancel), **FOK** (fill-or-kill).
- **High touch** — human sales-trader executes (Barclays: `PUMA`); highest commission.
- **Low touch** — bank's algos execute (Barclays: `BATMAN/BLACKBIRD`); medium commission. VWAP/TWAP/POV.
- **DMA / Zero touch** — client's own system hits the market via the bank's pipe (Barclays: `QBS, CEBUM`); lowest commission.
- **Colocation (colo)** — servers placed inside the exchange's data center for minimal latency (~158 ns execution hop quoted).
- **Smart Order Routing (SOR)** — splitting/routing an order across venues for best price.
- **OTC equities** — bilateral, off-exchange, **no central governing body** → counterparty risk (vs exchange + clearing corp guaranteeing settlement).
- **Equity future / basis** — futures on a stock/index; basis = futures − spot, → 0 at expiry.
- **Call / put / strike / premium** — right to buy / right to sell / agreed price / cost of the option (= intrinsic + time value).
- **Greeks** — delta, gamma, theta (time decay), vega (vol sensitivity).
- **Structured product / PPN** — derivatives packaged into a wrapper for a margin; PPN = zero-coupon bond + option.
- **Securities lending** — lending shares to short-sellers for a borrow fee.

---

*End of Equities notes. Next desks to append: FX, Credit, Commodities, Prime Brokerage.*
