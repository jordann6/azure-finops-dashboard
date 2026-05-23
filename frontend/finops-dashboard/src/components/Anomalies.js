import React, { useState, useEffect } from 'react';

export default function Anomalies({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    fetch(`${apiBase}/anomalies`, { signal: controller.signal })
      .then(res => res.json())
      .then(setData)
      .catch(err => { if (err.name !== 'AbortError') setError(err.message); })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiBase]);

  if (loading) return <div className="loading">Loading anomalies...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data || data.length === 0) return <div className="loading">No anomalies detected. This is good!</div>;

  const highCount = data.filter(a => a.severity === 'High').length;
  const medCount = data.filter(a => a.severity === 'Medium').length;

  return (
    <div>
      <div className="metric-cards">
        <div className="metric-card">
          <div className="label">Total Anomalies</div>
          <div className="value red">{data.length}</div>
        </div>
        <div className="metric-card">
          <div className="label">High Severity</div>
          <div className="value red">{highCount}</div>
        </div>
        <div className="metric-card">
          <div className="label">Medium Severity</div>
          <div className="value" style={{ color: '#d97706' }}>{medCount}</div>
        </div>
      </div>

      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Resource</th>
            <th>Expected</th>
            <th>Actual</th>
            <th>Deviation</th>
            <th>Severity</th>
          </tr>
        </thead>
        <tbody>
          {data.map((a, i) => (
            <tr key={i}>
              <td>{a.detectedDate}</td>
              <td>{a.resourceName}</td>
              <td>${a.expectedCost.toFixed(4)}</td>
              <td>${a.actualCost.toFixed(4)}</td>
              <td>{a.deviationPercent.toFixed(1)}%</td>
              <td className={`severity-${a.severity.toLowerCase()}`}>{a.severity}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
