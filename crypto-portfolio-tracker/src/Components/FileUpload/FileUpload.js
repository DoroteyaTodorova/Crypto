import React, { useRef } from 'react';
import './FileUpload.css';

function FileUpload({ onFileParsed  }) {
    const fileInputRef = useRef();

    const handleFileChange = (e) => {
        const file = e.target.files[0];
        if (!file) return;
      
        const reader = new FileReader();
        reader.onload = async (event) => {
          const lines = event.target.result.trim().split('\n');
          const parsed = [];
          const errors = [];
      
          lines.forEach((line, index) => {
            const parts = line.split('|');
            if (parts.length !== 3) {
              errors.push(`Invalid format on line ${index + 1}: expected 3 values separated by "|"`);
              return; // skip this line
            }
            
            let [amount, coin, buyPrice] = parts;
            amount = parseFloat(amount);
            buyPrice = parseFloat(buyPrice);
            coin = coin ? coin.trim().toUpperCase() : '';
      
            if (isNaN(amount) || !coin || isNaN(buyPrice)) {
              errors.push(`Invalid data on line ${index + 1}: check amount, coin symbol, and buy price.`);
              return; // skip this line
            }
      
            parsed.push({ amount, coin, buyPrice });
          });
      
          if (errors.length > 0) {
            // You can do something with errors here, like logging or showing user feedback
            console.warn('File parsing warnings:', errors);
          }
      
          onFileParsed(parsed);
        };
      
        reader.readAsText(file);
      };

    const handleButtonClick = () => {
        fileInputRef.current.click();
    };

    return (
        <div className="file-upload-container">
            <button className="upload-button" onClick={handleButtonClick}>
                Upload Crypto Portfolio File
            </button>
            <input
                type="file"
                accept=".txt,.csv"
                ref={fileInputRef}
                onChange={handleFileChange}
                className="hidden-file-input"
            />
        </div>
    );
}

export default FileUpload;
