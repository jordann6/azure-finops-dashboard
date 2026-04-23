import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function DailyCosts({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetch(`${apiBase}/costs/daily`)
      .then(res => res.json())
      .then(setData)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [apiBase]);

  if (loading) return <div className="loading">Loading daily costs...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data || data.length === 0) return <div className="loading">No cost data available yet. Run the ingestion function to populate data.</div>;

  const totalCost = data.reduce((sum, d) => sum + d.total, 0);
  const avgDaily = totalCost / data.length;

  return (
    <div>
      <div className="metric-cards">
        <div className="metric-card">
          <div className="label">Total (30 days)</div>
          <div className="value blue">${totalCost.toFixed(2)}</div>
        </div>
        <div className="metric-card">
          <div className="label">Daily Average</div>
          <div className="value">${avgDaily.toFixed(2)}</div>
        </div>
        <div className="metric-card">
          <div className="label">Data Points</div>
          <div className="value">{data.length}</div>
        </div>
      </div>

      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} tickFormatter={v => `$${v}`} />
            <Tooltip formatter={v => [`$${v.toFixed(4)}`, 'Cost']} />
            <Bar dataKey="total" fill="#2563eb" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
