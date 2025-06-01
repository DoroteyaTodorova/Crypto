import React, { useEffect } from 'react';
import './RefreshButton.css';
import { FaSyncAlt } from 'react-icons/fa';

function RefreshButton({ onClick, refreshInterval = 300000, setRefreshInterval }) {
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
              value={refreshInterval / 60000}
              onChange={(e) => setRefreshInterval(Number(e.target.value) * 60000)}
            />
            min
          </label>
          <button
            className="refresh-button"
            onClick={onClick}
            title={`Refresh (every ${refreshInterval / 60000} min)`}
          >
            <FaSyncAlt className="refresh-icon" />
          </button>
        </>
      );      
}

export default RefreshButton;