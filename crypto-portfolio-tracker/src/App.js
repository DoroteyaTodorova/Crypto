import React, { useState, useEffect } from 'react';

import './App.css';

import Header from './shared/Header/Header';

import FileUpload from './components/FileUpload/FileUpload';
import PortfolioTable from './components/PortfolioTable/PortfolioTable';
import PortfolioTableSkeleton from './components/PortfolioTableSkeleton/PortfolioTableSkeleton';
import RefreshButton from './components/RefreshButton/RefreshButton';
import SentimentToggle from './components/SentimentToggle/SentimentToggle';

function App() {
  const [parsedData, setParsedData] = useState([]);
  const [portfolio, setPortfolio] = useState([]);
  const [refreshInterval, setRefreshInterval] = useState(300000);
  const [enableSentiment, setEnableSentiment] = useState(false);

  const fetchCoinPrices = async (validPortfolio) => {

    try {
      const res = await fetch('https://localhost:7164/api/Portfolio/calculate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          portfolio: validPortfolio,
          includeSentiment: enableSentiment,
        }),
      });

      if (!res.ok) throw new Error(`Backend error: ${res.status}`);
      const updatedPortfolio = await res.json();
      setPortfolio(updatedPortfolio);
    } catch (error) {
      console.error('Failed to fetch portfolio data:', error);
      alert('There was a problem fetching the portfolio data. Please try again later.');
    }
  };

  useEffect(() => {
    if (parsedData.length > 0) {
        fetchCoinPrices(parsedData);
    }
  }, [parsedData]);
  
  useEffect(() => {
    if (parsedData.length > 0) {
      fetchCoinPrices(parsedData);
    }
  }, [enableSentiment]);

  return (
    <div className="App">
      <Header />
      <div className='Table-btns'>
      <FileUpload onFileParsed={(parsed) => {
        setParsedData(parsed);
        if (parsed.length === 0) {
          setPortfolio([]);
        }
      }} />
      <RefreshButton
        onClick={() => {
          fetchCoinPrices(parsedData);
        }}
        refreshInterval={refreshInterval}
        setRefreshInterval={setRefreshInterval}
      />
      <SentimentToggle
        enabled={enableSentiment}
        onToggle={(checked) => {
          setEnableSentiment(checked);
        }}
      />
      </div>
      {parsedData.length > 0 && portfolio.length === 0 && <PortfolioTableSkeleton />}
      <PortfolioTable portfolio={portfolio} />
    </div>
  );
}

export default App;
