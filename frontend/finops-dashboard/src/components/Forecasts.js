import React, { useState, useEffect } from 'react';
import { XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Area, ComposedChart } from 'recharts';

export default function Forecasts({ apiBase }) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetch(`${apiBase}/forecasts`)
      .then(res => res.json())
      .then(setData)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [apiBase]);

  if (loading) return <div className="loading">Loading forecasts...</div>;
  if (error) return <div className="error">Error: {error}</div>;
  if (!data || data.length === 0) return <div className="loading">No forecast data available. Need at least 7 days of cost data.</div>;

  const totalProjected = data.reduce((sum, d) => sum + d.projectedCost, 0);

  const chartData = data.map(d => ({
    date: d.forecastDate,
    projected: parseFloat(d.projectedCost.toFixed(4)),
    lower: parseFloat(d.lowerBound.toFixed(4)),
    upper: parseFloat(d.upperBound.toFixed(4))
  }));

  return (
    <div>
      <div className="metric-cards">
        <div className="metric-card">
          <div className="label">Projected Total (14 days)</div>
          <div className="value blue">${totalProjected.toFixed(2)}</div>
        </div>
        <div className="metric-card">
          <div className="label">Forecast Period</div>
          <div className="value">{data.length} days</div>
        </div>
      </div>

      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <ComposedChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" tick={{ fontSize: 12 }} />
            <YAxis tick={{ fontSize: 12 }} tickFormatter={v => `$${v}`} />
            <Tooltip formatter={v => `$${v.toFixed(4)}`} />
            <Area type="monotone" dataKey="upper" stroke="none" fill="#2563eb" fillOpacity={0.1} />
            <Area type="monotone" dataKey="lower" stroke="none" fill="#fff" fillOpacity={1} />
            <Line type="monotone" dataKey="projected" stroke="#2563eb" strokeWidth={2} dot={false} />
          </ComposedChart>
        </ResponsiveContainer>
      </div>

      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Projected</th>
            <th>Lower Bound</th>
            <th>Upper Bound</th>
            <th>Confidence</th>
          </tr>
        </thead>
        <tbody>
          {data.map((d, i) => (
            <tr key={i}>
              <td>{d.forecastDate}</td>
              <td>${d.projectedCost.toFixed(4)}</td>
              <td>${d.lowerBound.toFixed(4)}</td>
              <td>${d.upperBound.toFixed(4)}</td>
              <td>{(d.confidence * 100).toFixed(0)}%</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
