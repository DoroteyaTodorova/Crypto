export async function fetchLastLogs() {
  try {
    const res = await fetch('http://localhost:3001/logs');
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`);
    }
    return await res.json();
  } catch (error) {
    console.warn('fetchLastLogs failed:', error.message);
    return [];
  }
}
  
  export async function saveLog(message) {
    try {
      const res = await fetch('http://localhost:3001/log', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message }),
      });
      if (!res.ok) {
        throw new Error(`HTTP error! status: ${res.status}`);
      }
    } catch (error) {
      console.warn('saveLog failed:', error.message);
    }
  }
  