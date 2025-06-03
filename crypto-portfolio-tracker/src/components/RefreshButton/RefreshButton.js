import React, { useEffect } from 'react';
import './RefreshButton.css';
import { FaSyncAlt } from 'react-icons/fa';

function RefreshButton({ onClick, refreshInterval = 300000, setRefreshInterval }) {
  const minuteInSeconds = 60000;
    useEffect(() => {
        const interval = setInterval(() => {
            onClick();
        }, refreshInterval);

        return () => clearInterval(interval);
    }, [onClick, refreshInterval]);

    return (
        <>
          <label className='input-interval'>
            Refresh interval:
            <input
              className='input-field'
              type="number"
              min="1"
              value={refreshInterval / minuteInSeconds}
              onChange={(e) => setRefreshInterval(Number(e.target.value) * minuteInSeconds)}
            />
            min
          </label>
          <button
            className="refresh-button"
            onClick={onClick}
            title={`Refresh (every ${refreshInterval / minuteInSeconds} min)`}
          >
            <FaSyncAlt className="refresh-icon" />
          </button>
        </>
      );      
}

export default RefreshButton;