const CRYPTOPANIC_API_KEY = process.env.REACT_APP_CRYPTOPANIC_API_KEY;

export async function fetchNewsForCoin(coinSymbol) {
  const url = `https://cryptopanic.com/api/developer/v2/posts/?auth_token=${CRYPTOPANIC_API_KEY}&currencies=${coinSymbol.toLowerCase()}&public=true`;

  try {
    const response = await fetch(url);
    const data = await response.json();
    const headlines = data.results.map(item => item.title);
    return headlines.length ? headlines[0] : null;
  } catch (error) {
    console.error(`Failed to fetch news for ${coinSymbol}:`, error);
    return null;
  }
}
