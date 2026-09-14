import { useState, useEffect, useMemo } from 'react';
import './App.css';

function App() {
  const [leads, setLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sortConfig, setSortConfig] = useState({ key: 'Score', direction: 'desc' });
  const [filterTier, setFilterTier] = useState('All');
  const [selectedLead, setSelectedLead] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetch('http://localhost:5001/api/leads')
      .then(response => {
        if (!response.ok) throw new Error('Failed to fetch leads');
        return response.json();
      })
      .then(data => {
        setLeads(data);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  const filteredLeads = useMemo(() => {
    let filtered = leads;
    
    if (filterTier !== 'All') {
      filtered = filtered.filter(lead => lead.tier === filterTier);
    }
    
    if (searchTerm) {
      filtered = filtered.filter(lead => 
        lead.companyName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        lead.industry.toLowerCase().includes(searchTerm.toLowerCase()) ||
        lead.location.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
    
    return filtered;
  }, [leads, filterTier, searchTerm]);

  const sortedLeads = useMemo(() => {
    const sorted = [...filteredLeads];
    sorted.sort((a, b) => {
      let aVal = a[sortConfig.key.toLowerCase()];
      let bVal = b[sortConfig.key.toLowerCase()];
      
      if (sortConfig.key === 'Score') {
        aVal = a.score;
        bVal = b.score;
      } else if (sortConfig.key === 'Company') {
        aVal = a.companyName;
        bVal = b.companyName;
      } else if (sortConfig.key === 'Employees') {
        aVal = a.employeeCount;
        bVal = b.employeeCount;
      } else if (sortConfig.key === 'Revenue') {
        aVal = a.revenue;
        bVal = b.revenue;
      }

      if (typeof aVal === 'string') {
        return sortConfig.direction === 'asc' 
          ? aVal.localeCompare(bVal)
          : bVal.localeCompare(aVal);
      }
      
      return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal;
    });
    return sorted;
  }, [filteredLeads, sortConfig]);

  const handleSort = (key) => {
    setSortConfig(prev => ({
      key,
      direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc'
    }));
  };

  const getTierBadgeClass = (tier) => {
    const classes = {
      'A': 'tier-badge tier-a',
      'B': 'tier-badge tier-b',
      'C': 'tier-badge tier-c',
      'D': 'tier-badge tier-d'
    };
    return classes[tier] || 'tier-badge';
  };

  const exportToCSV = () => {
    const headers = ['Company', 'Tier', 'Score', 'Industry', 'Employees', 'Location', 'Funding Stage', 'Revenue'];
    const rows = sortedLeads.map(lead => [
      lead.companyName,
      lead.tier,
      lead.score,
      lead.industry,
      lead.employeeCount,
      lead.location,
      lead.fundingStage,
      lead.revenue
    ]);
    
    const csv = [headers, ...rows].map(row => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'scored-leads.csv';
    a.click();
  };

  if (loading) return (
    <div className="loading-container">
      <div className="spinner"></div>
      <p>Loading leads...</p>
    </div>
  );
  
  if (error) return (
    <div className="error-container">
      <div className="error-icon">⚠</div>
      <h2>Error Loading Leads</h2>
      <p>{error}</p>
    </div>
  );

  return (
    <div className="app">
      <div className="sidebar">
        <div className="logo">
          <h2>SaaSquatch</h2>
          <p>Lead Intelligence</p>
        </div>
        
        <div className="sidebar-stats">
          <h3>Overview</h3>
          <div className="sidebar-stat-item">
            <span className="stat-icon">📊</span>
            <div>
              <div className="stat-number">{leads.length}</div>
              <div className="stat-text">Total Leads</div>
            </div>
          </div>
          
          <div className="sidebar-stat-item tier-stat-a">
            <span className="stat-icon">⭐</span>
            <div>
              <div className="stat-number">{leads.filter(l => l.tier === 'A').length}</div>
              <div className="stat-text">Tier A - High Priority</div>
            </div>
          </div>
          
          <div className="sidebar-stat-item tier-stat-b">
            <span className="stat-icon">🎯</span>
            <div>
              <div className="stat-number">{leads.filter(l => l.tier === 'B').length}</div>
              <div className="stat-text">Tier B - Strong</div>
            </div>
          </div>
          
          <div className="sidebar-stat-item tier-stat-c">
            <span className="stat-icon">📈</span>
            <div>
              <div className="stat-number">{leads.filter(l => l.tier === 'C').length}</div>
              <div className="stat-text">Tier C - Moderate</div>
            </div>
          </div>
          
          <div className="sidebar-stat-item tier-stat-d">
            <span className="stat-icon">📋</span>
            <div>
              <div className="stat-number">{leads.filter(l => l.tier === 'D').length}</div>
              <div className="stat-text">Tier D - Watch</div>
            </div>
          </div>
        </div>

        <div className="sidebar-actions">
          <button className="export-btn" onClick={exportToCSV}>
            <span>📥</span> Export CSV
          </button>
        </div>
      </div>

      <div className="main-content">
        <header className="header">
          <div className="header-left">
            <h1>Lead Scoring Dashboard</h1>
            <p className="subtitle">AI-powered lead prioritization • {filteredLeads.length} leads shown</p>
          </div>
        </header>

        <div className="controls-bar">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input 
              type="text" 
              placeholder="Search by company, industry, or location..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          
          <div className="filter-chips">
            <button 
              className={`chip ${filterTier === 'All' ? 'active' : ''}`}
              onClick={() => setFilterTier('All')}
            >
              All Tiers
            </button>
            <button 
              className={`chip chip-a ${filterTier === 'A' ? 'active' : ''}`}
              onClick={() => setFilterTier('A')}
            >
              Tier A
            </button>
            <button 
              className={`chip chip-b ${filterTier === 'B' ? 'active' : ''}`}
              onClick={() => setFilterTier('B')}
            >
              Tier B
            </button>
            <button 
              className={`chip chip-c ${filterTier === 'C' ? 'active' : ''}`}
              onClick={() => setFilterTier('C')}
            >
              Tier C
            </button>
            <button 
              className={`chip chip-d ${filterTier === 'D' ? 'active' : ''}`}
              onClick={() => setFilterTier('D')}
            >
              Tier D
            </button>
          </div>
        </div>

        <div className="leads-grid">
          {sortedLeads.map(lead => (
            <div key={lead.id} className="lead-card" onClick={() => setSelectedLead(lead)}>
              <div className="lead-card-header">
                <div className="lead-company-info">
                  <h3>{lead.companyName}</h3>
                  <a href={lead.website} target="_blank" rel="noopener noreferrer" onClick={(e) => e.stopPropagation()}>
                    {lead.website}
                  </a>
                </div>
                <div className={`tier-badge-large tier-${lead.tier.toLowerCase()}`}>
                  {lead.tier}
                </div>
              </div>
              
              <div className="lead-score-section">
                <div className="score-circle">
                  <svg viewBox="0 0 100 100">
                    <circle cx="50" cy="50" r="45" className="score-bg"></circle>
                    <circle 
                      cx="50" 
                      cy="50" 
                      r="45" 
                      className={`score-progress score-progress-${lead.tier.toLowerCase()}`}
                      style={{ strokeDashoffset: 283 - (283 * lead.score) / 100 }}
                    ></circle>
                  </svg>
                  <div className="score-text">
                    <div className="score-number">{lead.score}</div>
                    <div className="score-label">Score</div>
                  </div>
                </div>
                
                <div className="lead-key-metrics">
                  <div className="metric">
                    <span className="metric-icon">👥</span>
                    <div>
                      <div className="metric-value">{lead.employeeCount.toLocaleString()}</div>
                      <div className="metric-label">Employees</div>
                    </div>
                  </div>
                  <div className="metric">
                    <span className="metric-icon">💰</span>
                    <div>
                      <div className="metric-value">${(lead.revenue / 1_000_000).toFixed(1)}M</div>
                      <div className="metric-label">Revenue</div>
                    </div>
                  </div>
                </div>
              </div>
              
              <div className="lead-details">
                <div className="detail-row">
                  <span className="detail-icon">🏢</span>
                  <span className="detail-text">{lead.industry}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-icon">📍</span>
                  <span className="detail-text">{lead.location}</span>
                </div>
                <div className="detail-row">
                  <span className="detail-icon">🚀</span>
                  <span className="detail-text">{lead.fundingStage}</span>
                </div>
              </div>
              
              <div className="tech-stack-section">
                <div className="tech-stack-grid">
                  {lead.techStack.slice(0, 4).map((tech, idx) => (
                    <span key={idx} className="tech-pill">{tech}</span>
                  ))}
                  {lead.techStack.length > 4 && (
                    <span className="tech-pill more">+{lead.techStack.length - 4}</span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedLead && (
        <div className="modal-overlay" onClick={() => setSelectedLead(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setSelectedLead(null)}>×</button>
            
            <div className="modal-header">
              <div>
                <h2>{selectedLead.companyName}</h2>
                <a href={selectedLead.website} target="_blank" rel="noopener noreferrer">{selectedLead.website}</a>
              </div>
              <div className={`tier-badge-large tier-${selectedLead.tier.toLowerCase()}`}>
                Tier {selectedLead.tier}
              </div>
            </div>
            
            <div className="modal-score">
              <div className="modal-score-main">
                <div className="modal-score-number">{selectedLead.score}</div>
                <div className="modal-score-label">Overall Score</div>
              </div>
              <div className="score-breakdown">
                <h4>Score Breakdown</h4>
                {Object.entries(selectedLead.scoreBreakdown).map(([key, value]) => (
                  <div key={key} className="breakdown-item">
                    <span className="breakdown-label">{key}</span>
                    <div className="breakdown-bar">
                      <div 
                        className="breakdown-fill"
                        style={{ width: `${(value / 30) * 100}%` }}
                      ></div>
                    </div>
                    <span className="breakdown-value">{value}</span>
                  </div>
                ))}
              </div>
            </div>
            
            <div className="modal-details">
              <div className="modal-detail-grid">
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Industry</div>
                  <div className="modal-detail-value">{selectedLead.industry}</div>
                </div>
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Employees</div>
                  <div className="modal-detail-value">{selectedLead.employeeCount.toLocaleString()}</div>
                </div>
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Revenue</div>
                  <div className="modal-detail-value">${(selectedLead.revenue / 1_000_000).toFixed(1)}M</div>
                </div>
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Location</div>
                  <div className="modal-detail-value">{selectedLead.location}</div>
                </div>
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Funding Stage</div>
                  <div className="modal-detail-value">{selectedLead.fundingStage}</div>
                </div>
                <div className="modal-detail-item">
                  <div className="modal-detail-label">Contact</div>
                  <div className="modal-detail-value">{selectedLead.contactEmail}</div>
                </div>
              </div>
              
              <div className="modal-tech-stack">
                <h4>Technology Stack</h4>
                <div className="modal-tech-grid">
                  {selectedLead.techStack.map((tech, idx) => (
                    <span key={idx} className="tech-pill">{tech}</span>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
