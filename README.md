# SaaSquatch Lead Scoring & Prioritization System

A lead scoring API with a real-time prioritization dashboard, built for the Caprae Capital technical assessment.

## Overview

This system scores and tiers B2B leads based on signals that actually matter for a search-fund/ETA buyer — company size, ownership structure, revenue, operational maturity, and location — then surfaces them through a React dashboard with search, tier filtering, a per-lead score breakdown, and CSV export.

The scoring logic is deliberately built around **acquirability**, not growth-stage hype. A lead that looks impressive by SaaS standards (VC-funded, 800 employees, $75M revenue) is actively the _wrong_ target for this audience, and the model scores it accordingly.

## Architecture

### Backend: ASP.NET Core Minimal API

- **Framework**: .NET 10, minimal API pattern
- **Scoring engine**: rule-based, weighted, pure-function scoring (`LeadScoringService`) — fully testable, no black box
- **Data layer**: SQLite via EF Core (`LeadDbContext`) — a real, queryable database, not an in-memory list. `MockLeadData` now serves only as seed data, inserted once on first run by `DbSeeder`
- **Model**: `Models/Lead.cs` as the shared entity/DTO
- **Endpoints**:
  - `GET /api/leads` — all leads, scored and tiered
  - `GET /api/leads/{id}` — single scored lead
  - `GET /api/leads/tier/{tier}` — filter by tier (A/B/C/D)

### Frontend: React + Vite

- Sidebar with live tier counts (A–D) and total leads
- Search across company, industry, and location
- Tier filter chips
- Card grid with a circular score indicator per lead
- Detail modal with a per-category score breakdown (visual bar per factor)
- CSV export of the current filtered/sorted view

## Scoring Algorithm

Each lead receives 0–100 points across five categories, weighted by how much each one actually predicts acquirability for a search-fund buyer — not by how much VC money or headcount growth a company has.

| Category                 | Weight | Logic                                                                                                                                                                                            |
| ------------------------ | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Company Size**         | 25 pts | 10–150 employees = 25 (sweet spot — real business, still acquirable). <10 = 15. 150–300 = 12. 300+ = 3 (enterprise scale, not a realistic target)                                                |
| **Ownership Structure**  | 25 pts | Bootstrapped / Family-Owned / Self-Funded = 25 (actually for sale). Seed = 15. Series A = 8. Series B = 3. Series C+ = 0 (not acquirable this way — optimizing for growth, not a sale)           |
| **Revenue**              | 20 pts | $1M–$20M = 20 (classic lower-middle-market range). $300K–$1M = 8. $20M–$50M = 10. $50M+ = 3                                                                                                      |
| **Operational Maturity** | 15 pts | 5 pts per recognized small-business tool in use (QuickBooks, ServiceTitan, Salesforce, Xero, Shopify, Square, HubSpot), capped at 15 — signals the business runs on real systems, not just paper |
| **Location**             | 15 pts | US / Canada / UK / Germany = 15 — established search-fund and ETA-friendly jurisdictions                                                                                                         |

**Tier assignment**:

- **Tier A** (80–100): High-priority — small, owner-operated, revenue-generating, acquirable
- **Tier B** (60–79): Strong prospect, good fit with one or two weaker signals
- **Tier C** (40–59): Moderate fit — worth a longer-term watch
- **Tier D** (<40): Low priority — usually too large, too VC-backed, or too early to be a realistic acquisition target

## Technology Decisions & Rationale

### Why .NET Minimal API?

- **Production alignment**: matches my professional stack (C#, Azure, EF Core)
- **Performance**: lightweight, fast cold-start for serverless deployment
- **Maintainability**: clear separation between models, services, and data

### Why rule-based scoring instead of ML?

- **Time constraint**: a 5-hour challenge doesn't allow for proper ML training/validation
- **Interpretability**: a business user can read the five categories and immediately understand — and adjust — why a lead scored the way it did. That transparency matters more here than marginal accuracy gains, especially for a first version of a scoring tool a non-technical buyer will actually use
- **No MLOps overhead**: no model drift, no retraining pipeline, nothing to monitor
- **Right-sized**: for a few dozen to a few hundred leads, well-chosen rules perform comparably to a basic model, without the operational cost

### Why SQLite (via EF Core)?

- **Real persistence, zero setup cost**: satisfies an actual data storage requirement without needing a hosted SQL Server/Postgres instance for a 5-hour demo
- **Same code path as production**: because it's EF Core, not a custom in-memory shim, the queries (`ToListAsync`, `FindAsync`, LINQ filtering) are the same shape they'd be against Azure SQL
- **Trivial production swap**: changing the provider from `UseSqlite(...)` to `UseSqlServer(...)` (plus a new connection string) is the entire migration — no rewrite of the data access layer
- **Realistic seed data**: `MockLeadData` is inserted once via `DbSeeder` on first run (guarded so restarts don't duplicate rows), giving a real `.db` file with an inspectable schema rather than a hardcoded list

### Live scraping vs. seed data

Real-time scraping against SaaSquatch's actual sources (Apollo, LinkedIn, Crunchbase, Google Maps, Growjo) isn't feasible without API access to those platforms, so the 5 hours went into the scoring logic and the storage architecture instead of authentication plumbing. The seed data mirrors the shape and variety of what those sources would return.

### Deployment strategy (production)

**Backend**

- Azure App Service (.NET runtime) or Azure Functions (consumption plan, cost-efficient for spiky scoring workloads)
- Azure SQL as the production database (drop-in EF Core provider swap from SQLite)
- Application Insights for monitoring and telemetry

**Frontend**

- Azure Static Web Apps (global CDN, auto-HTTPS)

**CI/CD** — GitHub Actions:

```yaml
- Backend: dotnet build → dotnet test → az webapp deploy
- Frontend: npm build → az staticwebapp deploy
```

**Infrastructure as code**: Bicep templates for provisioning; Key Vault for secrets (connection strings, API keys)

## Running Locally

### Prerequisites

- .NET 10 SDK
- Node.js 18+

### Backend

```bash
cd LeadScoringApi
dotnet restore
dotnet run --launch-profile http
# API runs on http://localhost:5001
# On first run, a leads.db SQLite file is created and seeded automatically
```

### Frontend

```bash
cd lead-scoring-ui
npm install
npm run dev
# UI runs on http://localhost:5173
```

## Testing

```bash
# API smoke test
curl http://localhost:5001/api/leads
curl http://localhost:5001/api/leads/tier/A
```

Inspect the database directly with DB Browser for SQLite — open `leads.db` from the project root, check the **Database Structure** tab for the schema and **Browse Data** for the seeded rows.

## Business Value

This system directly addresses a core search-fund pain point: SaaSquatch surfaces hundreds of leads from Apollo, LinkedIn, Crunchbase, Google Maps, and Growjo, but not all leads are equally worth a searcher's time.

1. **Prioritization**: automatically surfaces the businesses that are actually small enough, bootstrapped enough, and profitable enough to be realistic acquisition targets — not just the biggest or most "impressive" companies in the dataset
2. **Time savings**: a searcher can focus outreach on Tier A leads first instead of manually filtering hundreds of rows
3. **Transparency**: the score breakdown means a searcher can see _why_ a lead ranked where it did, and challenge or tune the weights if their thesis differs
4. **Data-driven, not vibes-driven**: removes the guesswork of "does this company look good" and replaces it with explicit, adjustable criteria
5. **CRM-ready**: CSV export slots into existing outreach workflows

## Extensions for Production

- [ ] Swap SQLite for Azure SQL once wired to real SaaSquatch data sources (single provider-line change, per above)
- [ ] Authentication (Azure AD B2C)
- [ ] User-adjustable scoring weights (let each searcher tune the model to their own thesis)
- [ ] Historical score tracking (watch how a lead's score changes as it's updated)
- [ ] CRM webhooks (HubSpot, Salesforce, Pipedrive)
- [ ] Validate/recalibrate weights against actual closed acquisitions over time

## Project Stats

- **Backend**: `Models/Lead.cs`, `Services/LeadScoringService.cs`, `Data/MockLeadData.cs` (seed data), `Data/LeadDbContext.cs`, `Data/DbSeeder.cs`, `Program.cs`
- **Database**: SQLite (`leads.db`), created and seeded automatically on first run
- **Seed data**: 28 leads — a deliberate mix of acquirable small businesses and a handful of VC-scale companies included specifically to demonstrate the model correctly deprioritizing them
- **API response time**: <50ms

---

Built for the Caprae Capital Technical Assessment | September 2026
