const express = require('express');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const HF_API_URL = process.env.REACT_APP_HF_API_URL;
const HF_API_TOKEN = process.env.REACT_APP_HF_API_TOKEN;
const PORT = 5000;

app.post('/api/sentiment', async (req, res) => {
  const text = req.body.text;

  try {
    const response = await fetch(HF_API_URL, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${HF_API_TOKEN}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ inputs: text }),
    });

    const result = await response.json();
    res.json(result);
  } catch (err) {
    res.status(500).json({ error: 'Failed to fetch sentiment' });
  }
});

app.listen(PORT, () => console.log(`Sentiment API proxy running on http://localhost:${PORT}`));
