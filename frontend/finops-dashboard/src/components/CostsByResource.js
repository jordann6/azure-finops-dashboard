import React, { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from 'recharts';

const COLORS = ['#2563eb', '#059669', '#d97706', '#dc2626', '#7c3aed', '#0891b2', '#be185d', '#65a30d'];

export default function CostsByResource({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetch(`${apiBase}/costs/by-resource`)
      .then(res => res.json())
      .then(setData)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [apiBase]);

  if (loading) return <div className="loading">Loading resource costs...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data || data.length === 0) return <div className="loading">No resource cost data available.</div>;

  const chartData = data.slice(0, 8).map(d => ({
    name: d.resourceName,
    value: parseFloat(d.totalCost.toFixed(4))
  }));

  return (
    <div>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie data={chartData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={100} label>
              {chartData.map((_, i) => (
                <Cell key={i} fill={COLORS[i % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip formatter={v => `$${v.toFixed(4)}`} />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </div>

      <table>
        <thead>
          <tr>
            <th>Resource</th>
            <th>Type</th>
            <th>Total Cost</th>
            <th>Avg Daily</th>
          </tr>
        </thead>
        <tbody>
          {data.map((r, i) => (
            <tr key={i}>
              <td>{r.resourceName}</td>
              <td>{r.resourceType}</td>
              <td>${r.totalCost.toFixed(4)}</td>
              <td>${r.avgDailyCost.toFixed(4)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
