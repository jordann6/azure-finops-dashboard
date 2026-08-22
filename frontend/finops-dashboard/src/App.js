import React, { useState } from 'react';
import './App.css';
import DailyCosts from './components/DailyCosts';
import CostsByResource from './components/CostsByResource';
import CostsByTag from './components/CostsByTag';
import TagHygiene from './components/TagHygiene';
import Anomalies from './components/Anomalies';
import Forecasts from './components/Forecasts';
import Optimization from './components/Optimization';

const API_BASE = process.env.REACT_APP_API_BASE || 'http://localhost:7071/api';

function App() {
  const [activeTab, setActiveTab] = useState('daily');

  const tabs = [
    { id: 'daily', label: 'Daily Costs' },
    { id: 'resources', label: 'By Resource' },
    { id: 'bytag', label: 'By Owner' },
    { id: 'optimization', label: 'Optimization' },
    { id: 'tags', label: 'Tag Hygiene' },
    { id: 'anomalies', label: 'Anomalies' },
    { id: 'forecasts', label: 'Forecasts' },
  ];

  return (
    <div className="app">
      <header className="header">
        <h1>FinOps Cost Visibility Dashboard</h1>
        <p>Azure subscription cost monitoring, tagging hygiene, anomaly detection, and forecasting</p>
      </header>

      <nav className="tabs">
        {tabs.map(tab => (
          <button
            key={tab.id}
            className={`tab ${activeTab === tab.id ? 'active' : ''}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </nav>

      <main className="content">
        {activeTab === 'daily' && <DailyCosts apiBase={API_BASE} />}
        {activeTab === 'resources' && <CostsByResource apiBase={API_BASE} />}
        {activeTab === 'bytag' && <CostsByTag apiBase={API_BASE} />}
        {activeTab === 'optimization' && <Optimization apiBase={API_BASE} />}
        {activeTab === 'tags' && <TagHygiene apiBase={API_BASE} />}
        {activeTab === 'anomalies' && <Anomalies apiBase={API_BASE} />}
        {activeTab === 'forecasts' && <Forecasts apiBase={API_BASE} />}
      </main>
    </div>
  );
}

export default App;
