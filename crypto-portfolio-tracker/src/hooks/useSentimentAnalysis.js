import { useCallback } from 'react';

const API_URL = 'http://localhost:5000/api/sentiment';

export default function useSentimentAnalysis() {
  const analyzeSentiment = useCallback(async (text) => {
    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('Sentiment API error response:', errorText);
        throw new Error(`Sentiment API error: ${response.status}`);
      }

      const result = await response.json();

      if (Array.isArray(result) && result.length > 0 && Array.isArray(result[0])) {
        return result[0][0].label.toLowerCase();
      } else if (result[0]?.label) {
        return result[0].label.toLowerCase();
      }

      return 'neutral';
    } catch (error) {
      console.error('Error analyzing sentiment:', error);
      return 'neutral';
    }
  }, []);

  return analyzeSentiment;
}
