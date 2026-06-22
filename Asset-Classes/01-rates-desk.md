# Markets Deep-Dive — 01 — The RATES Desk

> Part of my Barclays GTSM Markets internship notes.
> Goal: understand each asset class deeply, with numbers, mapped to my DeFi/RWA experience.
> Format: easy language, hard terms explained in (brackets), worked numerical scenarios.

---

## 0. Where "Rates" sits (the big picture)

The bank's **Markets** division trades several asset classes: Rates, Credit, FX, Commodities, Equities, plus Prime Brokerage as a service layer.

A trade is born in the **Front Office** (traders), checked in the **Middle Office** (risk/controls), and finished in the **Back Office** (settlement, confirmation, reconciliation). My synthetic-monitoring job guards the systems along that whole journey.

**Rates = everything whose value comes from interest rates.** It is the biggest financial market in the world by volume. The three things traded:

1. **Government bonds** (lending to governments)
2. **Interest Rate Swaps / IRS** (the core OTC product)
3. **Rate futures & options** (exchange-traded bets on rates)

The constant truth across ALL desks (memorize this):
> The bank mostly earns the **spread + fees on client flow** and **margin on structuring** — it is a *market maker / facilitator*, NOT a gambler. Directional betting is the *risk it manages* (inventory risk), not the *business model*.

---

## 1. The one idea Rates is built on: money has a rental price

Interest is the **rent on money**. If you lend ₹1000 for a year at 7%, you get ₹70 rent.

Everything on the Rates desk is just trading this rent — locking it in, swapping fixed rent for floating rent, betting it goes up or down.

**DeFi bridge:** you already know this cold.
- A **fixed rate** = a fixed-APY position.
- A **floating rate** = a variable-APY lending position (e.g. supplying to Aave where the APR moves every block).
- Choosing "fixed vs variable borrow" on Aave **is** the core decision the entire Rates desk is built around.

The hard term you'll hear: **benchmark rate** = the public reference interest rate that floating products are priced against.
- **LIBOR** (London Interbank Offered Rate) = the OLD benchmark, based on banks *self-reporting* their borrowing rate. It got manipulated (Barclays was the first caught, 2012) and is now retired.
- **SOFR** (Secured Overnight Financing Rate) = the NEW benchmark that replaced it, based on *real* overnight lending transactions (harder to fake).
- **DeFi bridge:** a benchmark rate is exactly an **oracle price feed**, but for interest rates instead of asset prices. SOFR is the "Chainlink of interest rates." A stale SOFR = a stale oracle = everything priced off it is wrong.

---

## 2. Government Bonds — lending to a government

### What it is
A **bond** = you lend money, you get fixed interest payments (called **coupons**), and your principal back at the end (**maturity**).

Terms in brackets:
- **Face value / par** = the amount repaid at the end (the "principal").
- **Coupon** = the fixed interest payment.
- **Maturity** = when you get your principal back.
- **Yield** = your actual return % given the price you paid (this moves; the coupon is fixed).

### Scenario A — buying a simple government bond
You buy a UK gilt (UK government bond):
- Face value: **£1000**
- Coupon: **5% per year** → pays **£50/year**
- Maturity: **10 years**

You pay £1000 now. Every year you get £50. After 10 years you get your £1000 back.
Total received: £50 × 10 + £1000 = **£1500**. Profit £500 over 10 years.

**DeFi bridge:** this is a **fixed-APY lending position with a lockup**. You deposited £1000, you earn a fixed 5% "APY," principal returned at the end. A government bond is the original "stablecoin yield vault" — except the yield source is a government's taxes, not a protocol.

### The thing that confuses everyone: price and yield move OPPOSITE

This is THE concept to nail for Rates.

If you hold a bond paying £50/year (5% on £1000) and **market interest rates rise to 10%**, nobody wants your old 5% bond anymore — they can get 10% on a new one. So your bond's *price falls* until its effective yield matches ~10%.

- Rates UP → existing bond prices DOWN.
- Rates DOWN → existing bond prices UP.

**Why (intuition):** your bond's payments are fixed at £50. The only way a fixed £50 can yield more is if you pay *less* for the bond. So price drops to lift the yield.

**Scenario B — rates move against you**
You bought the £1000 bond at 5% (£50/year). Next day, market rates jump to 10%.
- A new buyer wants a 10% yield. £50/year = 10% yield only if the bond costs **£500**.
- So your bond's market price drops toward ~£500. You're sitting on a paper loss of ~£500 if you sell now.
- If you HOLD to maturity, you still get all coupons + £1000 back — the loss is only realized if you sell early.

**DeFi bridge:** this is **interest-rate risk** = the same feeling as locking your funds in a fixed-APY vault right before variable rates spike. You're now stuck earning 5% while the market pays 10%. Your locked position is "worth less" because better yields exist elsewhere. Same economics, exact same regret.

### How Barclays makes money on bonds
The bank is a **market maker** — it quotes a price to **buy** (bid) and a price to **sell** (ask), and pockets the **spread** (the gap).

**Scenario C — the bank as bond market maker**
- Barclays quotes a gilt: **bid £999.50 / ask £1000.50** → spread **£1**.
- Client X wants to sell £1m face of gilts → Barclays buys at £999.50.
- Client Y wants to buy £1m face → Barclays sells at £1000.50.
- On £1m notional, capturing ~£1 per £1000 face = ~**£1000 profit**, and the bank ends flat (bought and sold the same amount).

It made money on **flow**, not on guessing direction. Do this across billions in daily volume = the bond desk's bread and butter.

**Inventory risk:** if only Client X shows up, Barclays now *holds* £1m of gilts. If rates rise before it offloads them, the bond price drops and the bank loses. So it **hedges** (offsets) the risk — usually with rate futures or swaps.
**DeFi bridge:** identical to an AMM/LP holding inventory of one token after a one-sided flow, exposed to price moves until it rebalances/hedges.

---

## 3. Interest Rate Swaps (IRS) — the core OTC product

This is the heart of the Rates desk and the thing the Rates team flagged (OTC swaps + bonds + SOFR/LIBOR).

### What it is
A **swap** = two parties **exchange interest payments** on an agreed amount, for an agreed period. Most common: **fixed-for-floating**.
- One side pays a **fixed** rate.
- The other pays a **floating** rate (tied to SOFR).
- They swap these payments periodically.

Terms in brackets:
- **OTC (Over The Counter)** = traded *directly between two parties*, NOT on a public exchange. Private, bilateral, customizable.
- **Notional** = the reference amount the interest is calculated on. It is **never exchanged** — only the interest payments are. (This is key and trips people up.)
- **Fixed leg** = the stream of fixed payments.
- **Floating leg** = the stream of payments that move with SOFR.

**DeFi bridge:**
- OTC = an **off-exchange, bilateral settlement** — *exactly the BTC OTC settlement work I did at Coded Estate on Liquid Network.* Two parties, agreed terms, settled directly, not on a public order book. Same concept, different asset.
- A swap itself = swapping a **fixed-APY position for a variable-APY position** with a counterparty. Like agreeing with someone: "I'll pay you a flat 5% APY, you pay me whatever Aave's variable APR is" — and we settle the difference each period.

### Why anyone wants a swap (the real-world use)

**Scenario D — a company hedges its loan (the classic use case)**

Company C took a **£10,000,000 floating-rate loan** from a bank. It pays **SOFR + 1%** on it.
- Right now SOFR = 4%, so it pays 5% → £500,000/year.
- Problem: if SOFR rises to 8%, it pays 9% → £900,000/year. The company hates this uncertainty — it can't budget.

So Company C does a swap with **Barclays**:
- Company C **pays Barclays a fixed 5%** on £10m notional → £500,000/year, locked.
- Barclays **pays Company C floating SOFR + 1%** → whatever the loan costs.

Now follow the cash:
- Company C pays its *actual loan* (SOFR + 1%) to its lender.
- Company C receives (SOFR + 1%) from Barclays → cancels out the loan cost exactly.
- Company C pays a flat 5% to Barclays.
- **Net effect: Company C now effectively pays a fixed 5%, no matter what SOFR does.** It converted a floating loan into a fixed one. Uncertainty killed.

**DeFi bridge:** Company C had a *variable-APR debt* (like a variable borrow on Aave) and was scared of rates spiking. It "bought certainty" by swapping into fixed — exactly like wishing you could convert your variable Aave borrow into a fixed-rate one so a rate spike can't hurt you. The swap is the instrument that does that conversion.

### How the numbers play out when rates move

**Scenario E — same swap, SOFR rises to 8%**
Notional £10m. Company C pays fixed 5% (£500k). Barclays pays floating SOFR+1% = 9% (£900k).
- They only settle the **difference (netting)**: Barclays owes Company C £900k − £500k = **£400k** this year.
- Company C is happy: it's protected. Its loan now costs £900k but it receives £400k net from Barclays + the structure leaves it paying an effective £500k. Certainty achieved.
- Barclays is *down* £400k on this leg — BUT Barclays doesn't sit naked. It **hedges** by doing the opposite swap with another client or in the market.

**Scenario F — same swap, SOFR falls to 2%**
- Floating = SOFR+1% = 3% → Barclays owes £300k.
- Fixed = 5% → Company C owes £500k.
- Net: **Company C pays Barclays £200k.** Company C is still fine — it locked 5% and accepts it "overpaid" vs the now-low market; that's the price of certainty (like buying insurance and not needing it).

**Key term: netting** = instead of both sides sending full payments, only the *difference* changes hands. Hugely reduces settlement volume and risk.
**DeFi bridge:** netting = the same idea as **batching/settling net balances** instead of gross transfers — like a payment channel that only settles the net at the end instead of every gross movement on-chain.

### How Barclays actually makes money on swaps

The bank doesn't want directional rate risk. It earns the **spread** by being the middle party for *both* sides.

**Scenario G — Barclays matched book (how the spread is earned)**
- Company C wants to **pay fixed** → Barclays quotes "I'll receive fixed at **5.02%**."
- Pension Fund D wants to **receive fixed** (they want steady income) → Barclays quotes "I'll pay fixed at **4.98%**."
- Barclays is now in the middle: it **receives 5.02%** from C and **pays 4.98%** to D, on matched £10m notional.
- Spread captured = 5.02% − 4.98% = **0.04% (4 basis points)** on £10m = **£4000/year**, with the floating legs cancelling and **near-zero net rate risk**.

Term: **basis point (bp)** = 0.01%. So 4bp = 0.04%. Rates people talk entirely in bps.
**DeFi bridge:** this "matched book" is exactly an **AMM/market maker capturing the bid-ask spread** while staying delta-neutral (no net directional exposure). The bank is the liquidity layer between two counterparties and skims the spread.

This is the whole Rates-desk business model in one line:
> **Sit between clients who want fixed and clients who want floating, match them, skim the spread in basis points, and hedge whatever doesn't match.**

---

## 4. Rate futures & options (quick, since they mirror what I know)

- **Rate futures** = exchange-traded bets on where rates go. Standardized, on an exchange (not OTC). Mechanics = the futures mechanics I already know (margin, daily mark-to-market, expiry).
- **Rate options** = the right (not obligation) to enter a rate/swap at a set level. Greeks apply, like equity options.

**DeFi bridge:** futures = the perp/futures mechanics I mapped (margin = collateral, mark-to-market = continuous PnL, liquidation = margin call). The bank market-makes these too and earns spread.

These are used by the desk mainly to **hedge** the swap and bond inventory risk from Scenarios C–G.

---

## 5. Putting the whole desk together (story mode)

A single day on the Rates desk, simplified:
1. Corporate clients come in wanting to **lock their loan rates** → they pay fixed via swaps (Scenario D).
2. Pension funds / insurers come in wanting **steady fixed income** → they receive fixed (Scenario G).
3. Barclays **matches** them and skims the **bps spread**, staying rate-neutral.
4. Where flows don't match, the bank **hedges** with bonds/futures so it isn't exposed to rates moving.
5. It also **market-makes government bonds**, earning the bid-ask spread on huge volume (Scenario C).
6. All of this is priced off **SOFR** (the benchmark/oracle). Get SOFR wrong and *everything* above is mispriced.

**How the money adds up:** thin margins (basis points) × gigantic volume (billions in notional) + structuring fees for bespoke client swaps. Steady, flow-driven, technology-and-risk-intensive income.

---

## 6. Why this is MY edge (DeFi/RWA connections, collected)

- **OTC swaps = bilateral off-exchange settlement** → directly maps to my **BTC OTC settlement work on Liquid Network (Coded Estate / Sovereign Protocol)**. Two parties, agreed terms, settled directly. I've built this.
- **Fixed vs floating = fixed vs variable APY** → Aave-style lending intuition I already have.
- **Benchmark rate (SOFR) = an oracle price feed** → I understand oracle freshness/staleness deeply from DeFi. SOFR is "the Chainlink of interest rates."
- **Netting = batched net settlement / payment channel settlement** → familiar from payment-rail and channel design.
- **Matched book spread = delta-neutral AMM market making** → the LP-earning-the-spread model.
- **Bonds = fixed-APY lending with lockup + interest-rate risk** → tokenized-bond / RWA yield product intuition (this is literally the "tokenize the world" thesis — bonds are the #1 RWA being tokenized).

---

## 7. The "what breaks" layer — my synthetic-monitoring angle for Rates

Most people check "is the app up?" My finance-literate checks verify "is the answer CORRECT?" — the real differentiator.

Rates-specific synthetic checks worth building:
1. **Benchmark freshness** — is SOFR (and the curve) updating? A stale SOFR = every swap and floating bond mispriced. This is the single highest-value check on the desk. (App can be "up" while feeding yesterday's rate.)
2. **Yield curve sanity** — does the curve look coherent (no negative gaps where there shouldn't be, no missing tenors)? A broken curve = mispriced everything.
3. **Swap repricing** — fire a synthetic swap valuation and confirm it reprices when the benchmark moves (not frozen).
4. **Bond price/yield consistency** — does quoted price imply a yield that matches the curve? A crossed or arbitrage-impossible quote = broken feed.
5. **Settlement/confirmation flow** — can a synthetic trade flow through capture → validation → confirmation → settlement (the trade lifecycle), not just "submit" succeeding?

The one-liner to use in a design discussion:
> "For the Rates apps, the synthetic checks should validate that the benchmark (SOFR) is fresh and the curve is coherent — not just that the app responds. A stale rate is a worse failure than an outage because every swap and bond silently misprices off it, and the screen still looks green."

Real-world proof this matters: **LIBOR rigging** (benchmark integrity) and the **$361m structured-note over-issuance** (a missing automated check on a limit) — both are "system looked fine but the number was wrong" failures. That is exactly the class my checks catch.

---

## 8. Glossary (fast recall)

- **Interest** — rent on money.
- **Benchmark rate** — public reference rate (SOFR now, LIBOR retired); = an interest-rate oracle.
- **SOFR** — Secured Overnight Financing Rate; the modern benchmark, based on real transactions.
- **Coupon** — a bond's fixed interest payment.
- **Face value / par** — the principal repaid at maturity.
- **Maturity** — when principal is returned.
- **Yield** — actual return % given the price paid (moves inversely to price).
- **Interest-rate risk** — risk that rate moves change a bond/position's value.
- **IRS (Interest Rate Swap)** — exchange of fixed vs floating interest payments.
- **OTC** — traded directly between two parties, off-exchange (= bilateral settlement).
- **Notional** — the reference amount interest is calculated on; never exchanged.
- **Fixed leg / floating leg** — the two payment streams in a swap.
- **Netting** — settling only the difference between what each side owes.
- **Matched book** — the bank sits between two opposite clients, skims the spread, stays neutral.
- **Basis point (bp)** — 0.01%.
- **Spread** — the gap between buy and sell prices; the bank's core income.
- **Inventory risk** — risk from holding a position after one-sided client flow, until hedged.
- **Hedge** — an offsetting position that cancels a risk.

---

*End of Rates notes. Next desks to append: FX, Equities, Credit, Commodities, Prime Brokerage.*
