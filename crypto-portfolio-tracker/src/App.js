import React, { useState, useEffect } from 'react';

import './App.css';

import Header from './shared/Header/Header';

import FileUpload from './components/FileUpload/FileUpload';
import PortfolioTable from './components/PortfolioTable/PortfolioTable';
import PortfolioTableSkeleton from './components/PortfolioTableSkeleton/PortfolioTableSkeleton';
import RefreshButton from './components/RefreshButton/RefreshButton';
import Logs from './components/Logs/Logs';
import SentimentToggle from './components/SentimentToggle/SentimentToggle';

import useSentimentAnalysis from './hooks/useSentimentAnalysis';

import { fetchNewsForCoin } from './utils/fetchNewsForCoin';
import { saveLog } from './utils/logService';

function App() {
  const [parsedData, setParsedData] = useState([]);
  const [portfolio, setPortfolio] = useState([]);
  const [refreshInterval, setRefreshInterval] = useState(300000);
  const [logs, setLogs] = React.useState([]); 
  const [enableSentiment, setEnableSentiment] = useState(false);
  const analyzeSentiment = useSentimentAnalysis();

  const newsCache = {};

  const addLog = async (message) => {
    const newLog = { message, timestamp: new Date().toISOString() };
    await saveLog(message);
    
    setLogs(prevLogs => [
      ...prevLogs,
      newLog
    ]);
  };


  async function getNewsWithCache(coin) {
    if (newsCache[coin]) return newsCache[coin];
    const news = await fetchNewsForCoin(coin);
    newsCache[coin] = news;
    return news;
  };

  const fetchCoinPrices = async (coins) => {
    addLog(`Fetching coin prices for ${coins.length} coins...`);

    try {
      const response = await fetch('https://api.coinlore.net/api/tickers/');
      if (!response.ok) throw new Error(`API error: ${response.status}`);

      const json = await response.json();
      const prices = json.data;

      const result = [];

      for (const entry of coins) {
        const coinSymbol = entry.coin.toUpperCase();
        const match = prices.find(c => c.symbol.toUpperCase() === coinSymbol);
        
        if (!match) {
          console.warn(`Skipping unknown coin: ${coinSymbol}`);
          continue;
        }      
        const currentPrice = match ? parseFloat(match.price_usd) : null;
        const change = currentPrice
          ? ((currentPrice - entry.buyPrice) / entry.buyPrice) * 100
          : null;
      
        let sentiment = 'N/A';

        if (enableSentiment) {
          try {
            const news = await getNewsWithCache(entry.coin);
            if (news) {
              const score = await analyzeSentiment(news);
              if (score >= 0.4 && score <= 0.6) {
                sentiment = 'neutral';
              } else {
                sentiment = score;
              }
            }
          } catch (e) {
            console.warn(`Failed to process ${entry.coin}`, e);
          }
        }
      
        result.push({
          ...entry,
          currentPrice,
          change,
          sentiment,
        });
      }

      setPortfolio(result);
      addLog(`Portfolio updated with latest prices and sentiment.`);
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
          setPortfolio([]);
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
      <SentimentToggle
        enabled={enableSentiment}
        onToggle={(checked) => {
          setEnableSentiment(checked);
          addLog(`Sentiment analysis ${checked ? 'enabled' : 'disabled'}.`);
        }}
      />
      </div>
      {parsedData.length > 0 && portfolio.length === 0 && <PortfolioTableSkeleton />}
      <PortfolioTable portfolio={portfolio} />
      <Logs logs={logs} />
    </div>
  );
}

export default App;
