# Quick Start Guide

## Running the Application

### Terminal 1 - Start Backend API
```powershell
cd "C:\Users\HP EliteBook 840 G8\Documents\personal\‫ImpDoc\personal\New folder\LeadScoringApi"
dotnet run --launch-profile http
```
Wait for: `Now listening on: http://localhost:5001`

### Terminal 2 - Start Frontend
```powershell
cd "C:\Users\HP EliteBook 840 G8\Documents\personal\‫ImpDoc\personal\New folder\lead-scoring-ui"
npm run dev
```

### Access the Application
Open your browser to: **http://localhost:5173**

---

## What You'll See

### Modern Dark-Themed Dashboard with:
- **Sidebar Stats**: Live overview of lead distribution across tiers
- **Search Bar**: Real-time filtering by company, industry, or location
- **Tier Filter Chips**: Quick filtering by A/B/C/D tiers
- **Card Grid Layout**: Each lead displayed in a modern card with:
  - Circular progress score visualization
  - Color-coded tier badges (A=Green, B=Blue, C=Orange, D=Gray)
  - Key metrics (employees, revenue)
  - Tech stack pills
  - Industry, location, funding stage
- **Modal Detail View**: Click any card to see full lead breakdown with score components
- **Export Button**: Download filtered leads as CSV

### Features Working:
✅ Real-time search across 50 leads
✅ Sort by tier/score (highest priority first by default)
✅ Animated score circles showing 0-100 scoring
✅ Responsive design (works on mobile/tablet/desktop)
✅ Smooth animations and hover effects
✅ Dark theme optimized for demos

---

## API Endpoints Available

- `GET http://localhost:5001/api/leads` - All scored leads
- `GET http://localhost:5001/api/leads/1` - Single lead by ID
- `GET http://localhost:5001/api/leads/tier/A` - Filter by tier

---

Your application is ready to demo!
