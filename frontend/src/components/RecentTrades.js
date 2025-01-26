import React from 'react';

function RecentTrades({ trades }) {
    return (
        <div>
            <h2 className="text-lg font-semibold mb-4">Recent Trades</h2>
            <ul>
                {trades.map((trade, index) => (
                    <li key={index}>
                        {trade.id}: {trade.result > 0 ? '+' : ''}
                        {trade.result} {trade.currency}
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default RecentTrades;
