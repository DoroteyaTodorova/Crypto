import React from 'react';
import './SentimentToggle.css';

const SentimentToggle = ({ enabled, onToggle }) => {
  return (
    <div className="sentiment-toggle">
      <label className="switch">
        <input
          type="checkbox"
          checked={enabled}
          onChange={(e) => onToggle(e.target.checked)}
        />
        <span className="slider round"></span>
      </label>
      <span className="label-text">
        Sentiment Analysis {enabled ? 'On' : 'Off'}
      </span>
    </div>
  );
};

export default SentimentToggle;
