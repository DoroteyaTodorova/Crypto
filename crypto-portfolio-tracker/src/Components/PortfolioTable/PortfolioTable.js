import React from 'react';
import './PortfolioTable.css';

function PortfolioTable({ portfolio }) {
    return (
        <div className="portfolio-table-container">
            <table className="portfolio-table">
                <thead>
                    <tr>
                        <th>Coin</th>
                        <th>Amount</th>
                        <th>Buy Price</th>
                        <th>Current Price</th>
                        <th>Initial Value</th>
                        <th>Current Value</th>
                        <th>% Change</th>
                    </tr>
                </thead>
                <tbody>
                    {portfolio.map((item, index) => (
                        <tr key={index}>
                            <td>{item.coin}</td>
                            <td>{item.amount}</td>
                            <td>${item.buyPrice.toFixed(2)}</td>
                            <td>${item.currentPrice?.toFixed(2) ?? '—'}</td>
                            <td>${(item.amount * item.buyPrice).toFixed(2)}</td>
                            <td>${(item.amount * item.currentPrice).toFixed(2)}</td>
                            <td className={item.change >= 0 ? 'positive' : 'negative'}>
                                {item.change?.toFixed(2)}%
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default PortfolioTable;
