import React from 'react';
import './Logs.css';

function Logs({ logs }) {
  const lastLogs = logs.slice(-10); // Show only last 10 logs
  
  return (
    <div className="logs-container">
      <h3>Activity Logs</h3>
      {lastLogs.length === 0 ? (
        <p className="logs-empty">No logs yet.</p>
      ) : (
        <ul className="logs-list">
          {lastLogs.map((log, index) => (
            <li key={index} className="log-item">
              <span className="log-timestamp">
                {new Date(log.timestamp).toLocaleTimeString()}
              </span>
              <span className="log-message">{log.message}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Logs;
