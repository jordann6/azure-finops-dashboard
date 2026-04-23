import React, { useState, useEffect } from 'react';

export default function TagHygiene({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetch(`${apiBase}/tags/hygiene`)
      .then(res => res.json())
      .then(setData)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [apiBase]);

  if (loading) return <div className="loading">Evaluating tag compliance...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data) return <div className="loading">No tag data available.</div>;

  const complianceColor = data.compliancePercent >= 80 ? 'green' : data.compliancePercent >= 50 ? 'red' : 'red';

  return (
    <div>
      <div className="metric-cards">
        <div className="metric-card">
          <div className="label">Compliance</div>
          <div className={`value ${complianceColor}`}>{data.compliancePercent}%</div>
        </div>
        <div className="metric-card">
          <div className="label">Total Resources</div>
          <div className="value">{data.totalResources}</div>
        </div>
        <div className="metric-card">
          <div className="label">Properly Tagged</div>
          <div className="value green">{data.taggedResources}</div>
        </div>
        <div className="metric-card">
          <div className="label">Missing Tags</div>
          <div className="value red">{data.untaggedResources}</div>
        </div>
      </div>

      <p style={{ fontSize: 14, color: '#6b7280', marginBottom: 12 }}>
        Required tags: {data.requiredTags.join(', ')}
      </p>

      {data.findings.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Resource</th>
              <th>Type</th>
              <th>Missing Tags</th>
            </tr>
          </thead>
          <tbody>
            {data.findings.map((f, i) => (
              <tr key={i}>
                <td>{f.resourceName}</td>
                <td>{f.resourceType}</td>
                <td>{f.missingTags.join(', ')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
