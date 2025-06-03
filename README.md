# Crypto Portfolio Analyzer
A web application for uploading cryptocurrency portfolios, retrieving real-time prices, and analyzing sentiment from live news using AI.
---

## Features
- Upload a `|`-delimited portfolio file (`COIN|AMOUNT|BUY_PRICE`) .txt or .csv
- Get current crypto prices via the CoinLore API
- Analyze news sentiment per coin using Hugging Face AI and Crypto Panic API
- Automatic data refresh every 5 minutes (Customizable)
- Full backend logging and error reporting
- View live-updating portfolio changes
- Responsive frontend built with React and Custom css
---

## Tech Stack

### Backend – ASP.NET Core (.NET 8)
- REST API with controllers
- `HttpClientFactory` for external API calls
- `ILogger<T>` for detailed logging
- JSON handling via `System.Text.Json`
- Sentiment analysis via Hugging Face endpoint
- Dependency injection with clean service architecture
---

### Frontend – React
- File upload and validation
- Real-time portfolio value updates
- Fetch APIs with error handling
- Sentiment visualization
- Styled with custom css

## How to Run
### Backend (.NET)

1. Navigate to the `CryptoBackend` folder.
2. Add your Hugging Face API Key (https://huggingface.co/settings/tokens) and News API Key (https://cryptopanic.com/developers/api/about) in `appsettings.json`
3. Run the app

### Frontend (React)
1. Navigate to the frontend project directory (crypto-portfolio-tracker).
2. Install dependencies:
npm install
3. Run the app:
npm start

### Usage Instructions
Once both backend and frontend are running:

1. Click the "Upload Crypto Portfolio File" button on the frontend to upload your data file.
2. A sample portfolio file is available in the Examples/ folder.
3. You can toggle Sentiment Analysis on or off.
- Note: The free tier of CryptoPanic limits you to 100 requests/month. Each coin triggers one request.

## APIs Used
1. CoinLore Public API
2. Hugging Face-hosted NLP model for sentiment - distilbert-base-uncased-finetuned-sst-2-english
3. Crypto Panic API

## Sentiment Logic
1. Fetches news articles for each coin
2. Aggregates titles into a single text block
3. Sends the text to a Hugging Face model (distilbert, bert-base-uncased, etc.)
4. Interprets the label and score: positive/negative
5. Converts uncertain scores (0.4–0.6) to "Neutral"

## Tests
1. SentimentService is tested with mocked HTTP responses.
2. PortfolioService tested with fake coin price and sentiment results.
3. Validation for malformed or missing input is included.