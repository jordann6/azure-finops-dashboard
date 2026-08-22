import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';

// Spend attributed to each value of a cost allocation tag (default: project).
// The "(untagged)" bucket is drawn in amber to flag unallocated spend.
export default function CostsByTag({ apiBase }) {
  const [data, setData] = useState(null);
  const [tagKey, setTagKey] = useState('project');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    fetch(`${apiBase}/costs/by-tag`, { signal: controller.signal })
      .then(res => res.json())
      .then(res => { setData(res.costs || []); setTagKey(res.tagKey || 'project'); })
      .catch(err => { if (err.name !== 'AbortError') setError(err.message); })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiBase]);

  if (loading) return <div className="loading">Loading spend by tag...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data || data.length === 0) return <div className="loading">No tag-grouped spend available yet.</div>;

  const chartData = data.map(d => ({
    name: d.tagValue,
    value: parseFloat(Number(d.cost).toFixed(4)),
    untagged: d.tagValue === '(untagged)'
  }));

  return (
    <div>
      <p style={{ color: '#64748b', marginBottom: 12 }}>
        Spend over the last 30 days grouped by the <strong>{tagKey}</strong> tag.
      </p>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData} margin={{ top: 8, right: 16, bottom: 8, left: 8 }}>
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip formatter={v => `$${Number(v).toFixed(4)}`} />
            <Bar dataKey="value">
              {chartData.map((d, i) => (
                <Cell key={i} fill={d.untagged ? '#d97706' : '#2563eb'} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      <table>
        <thead>
          <tr>
            <th>{tagKey}</th>
            <th>30-Day Spend</th>
          </tr>
        </thead>
        <tbody>
          {data.map((r, i) => (
            <tr key={i}>
              <td style={r.tagValue === '(untagged)' ? { color: '#d97706' } : undefined}>{r.tagValue}</td>
              <td>${Number(r.cost).toFixed(4)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
