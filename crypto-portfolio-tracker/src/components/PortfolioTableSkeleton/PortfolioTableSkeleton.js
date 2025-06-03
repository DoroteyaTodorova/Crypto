import React from 'react';
import './PortfolioTableSkeleton.css';

function PortfolioTableSkeleton({ rows = 5 }) {
  return (
    <table className="portfolio-table-skeleton">
      <thead>
        <tr>
          <th>Coin</th>
          <th>Amount</th>
          <th>Buy Price</th>
          <th>Current Price</th>
          <th>Change (%)</th>
          <th>Sentiment</th>
        </tr>
      </thead>
      <tbody>
        {[...Array(rows)].map((_, i) => (
          <tr key={i} className="skeleton-row">
            <td><div className="skeleton-box" /></td>
            <td><div className="skeleton-box" /></td>
            <td><div className="skeleton-box" /></td>
            <td><div className="skeleton-box" /></td>
            <td><div className="skeleton-box" /></td>
            <td><div className="skeleton-box" /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default PortfolioTableSkeleton;
