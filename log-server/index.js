const express = require('express');
const fs = require('fs');
const path = require('path');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const LOG_FILE = path.join(__dirname, 'logs.json');

// Ensure log file exists
if (!fs.existsSync(LOG_FILE)) {
  fs.writeFileSync(LOG_FILE, JSON.stringify([]));
}

// Endpoint to append a log
app.post('/log', (req, res) => {
  const { message } = req.body;
  const log = { message, timestamp: new Date().toISOString() };
  const logs = JSON.parse(fs.readFileSync(LOG_FILE));
  logs.push(log);
  fs.writeFileSync(LOG_FILE, JSON.stringify(logs));
  res.status(200).json({ success: true });
});

// Endpoint to get last 10 logs
app.get('/logs', (req, res) => {
  const logs = JSON.parse(fs.readFileSync(LOG_FILE));
  const lastLogs = logs.slice(-10);
  res.json(lastLogs);
});

app.listen(3001, () => {
  console.log('Log server running on port 3001');
});
