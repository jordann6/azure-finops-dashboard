import React, { useState, useEffect } from 'react';

// Azure Advisor cost recommendations: idle/underused resources, right-sizing,
// and reservation purchases, with an estimated monthly saving each.
export default function Optimization({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    fetch(`${apiBase}/optimization/waste`, { signal: controller.signal })
      .then(res => res.json())
      .then(setData)
      .catch(err => { if (err.name !== 'AbortError') setError(err.message); })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiBase]);

  if (loading) return <div className="loading">Loading optimization recommendations...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  const recs = (data && data.recommendations) || [];

  return (
    <div>
      <div style={{ display: 'flex', gap: 24, marginBottom: 16 }}>
        <div>
          <div style={{ fontSize: 12, color: '#64748b' }}>Est. monthly savings available</div>
          <div style={{ fontSize: 28, fontWeight: 700, color: '#059669' }}>
            ${Number((data && data.totalEstMonthlySavings) || 0).toFixed(2)}
          </div>
        </div>
        <div>
          <div style={{ fontSize: 12, color: '#64748b' }}>Recommendations</div>
          <div style={{ fontSize: 28, fontWeight: 700 }}>{(data && data.count) || 0}</div>
        </div>
      </div>

      {recs.length === 0 ? (
        <div className="loading">
          No Advisor cost recommendations right now. Advisor evaluates the
          subscription periodically; a brand-new deployment may show none until
          it has run.
        </div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Recommendation</th>
              <th>Resource</th>
              <th>Impact</th>
              <th>Est. $/mo</th>
            </tr>
          </thead>
          <tbody>
            {recs.map((r, i) => (
              <tr key={i}>
                <td>{r.problem}</td>
                <td>{r.resourceName}</td>
                <td>{r.impact}</td>
                <td>${Number(r.estMonthlySavings).toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
