import React, { useState, useEffect } from 'react';
import './App.css';
import Header from './Shared/Header/Header';
import FileUpload from './Components/FileUpload/FileUpload';
import PortfolioTable from './Components/PortfolioTable/PortfolioTable';
import RefreshButton from './Components/RefreshButton/RefreshButton';
import Logs from './Components/Logs/Logs'
import PortfolioTableSkeleton from './Components/PortfolioTableSkeleton/PortfolioTableSkeleton';
function App() {
  const [parsedData, setParsedData] = useState([]);
  const [portfolio, setPortfolio] = useState([]);
  const [refreshInterval, setRefreshInterval] = useState(300000);
  const [logs, setLogs] = useState([]);

  const addLog = (message) => {
    setLogs((prevLogs) => [...prevLogs, { message, timestamp: Date.now() }]);
  };

  const fetchCoinPrices = async (coins) => {
    addLog(`Fetching coin prices for ${coins.length} coins...`);
  
    try {
      const response = await fetch('https://api.coinlore.net/api/tickers/');
      if (!response.ok) throw new Error(`API error: ${response.status}`);
  
      const json = await response.json();
      const prices = json.data;
  
      const result = coins.map(entry => {
        const match = prices.find(c => c.symbol.toUpperCase() === entry.coin);
        const currentPrice = match ? parseFloat(match.price_usd) : null;
        const change = currentPrice
            ? ((currentPrice - entry.buyPrice) / entry.buyPrice) * 100
            : null;
  
        return {
            ...entry,
            currentPrice,
            change
        };
      });
  
      setPortfolio(result);
      addLog(`Portfolio updated with latest prices.`);
    } catch (error) {
      addLog(`Error fetching coin prices: ${error.message}`);
    }
  };

  useEffect(() => {
    if (parsedData.length > 0) {
        fetchCoinPrices(parsedData);
    }
  }, [parsedData]);
  
  return (
    <div className="App">
      <Header />
      <div className='Table-btns'>
      <FileUpload onFileParsed={(parsed) => {
        setParsedData(parsed);
        if (parsed.length === 0) {
          setPortfolio([]);  // clear previous portfolio data if no valid entries
          addLog('Uploaded file contains no valid entries. Portfolio cleared.');
        } else {
          addLog(`File parsed successfully with ${parsed.length} entries.`);
        }
      }} />
      <RefreshButton
        onClick={() => {
          addLog('Manual refresh triggered');
          fetchCoinPrices(parsedData);
        }}
        refreshInterval={refreshInterval}
        setRefreshInterval={setRefreshInterval}
      />
      </div>
      {parsedData.length > 0 && portfolio.length === 0 && <PortfolioTableSkeleton />}
      {/* <PortfolioTableSkeleton /> */}
      <PortfolioTable portfolio={portfolio} />
      <Logs logs={logs} />
    </div>
  );
}

export default App;
