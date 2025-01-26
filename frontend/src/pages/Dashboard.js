import React from 'react';
import Metrics from '../components/Metrics';
import DailyNetCumulativeChart from '../components/Charts/DailyNetCumulativeChart';
import NetDailyPLChart from '../components/Charts/NetDailyPLChart';
import RecentTrades from '../components/RecentTrades';
import Calendar from '../components/Calendar';

function Dashboard() {
    const metricsData = [
        { title: 'Net P&L', value: '-$47.85', icon: '📉' },
        { title: 'Trade Win %', value: '0.00%', icon: '📈' },
        { title: 'Profit Factor', value: '--', icon: 'ℹ️' },
        { title: 'Day Win %', value: '0.00%', icon: '📊' },
        { title: 'Avg Win/Loss Trade', value: '--', subText: '-$9.57', icon: '⚖️' },
    ];

    const tradesData = [
        { date: '04/02/2024', symbol: 'SPY', netPL: '-$6.10' },
        { date: '04/02/2024', symbol: 'SPY', netPL: '-$4.19' },
        { date: '04/02/2024', symbol: 'SPY', netPL: '-$3.20' },
        { date: '04/02/2024', symbol: 'SPY', netPL: '-$6.54' },
        { date: '04/02/2024', symbol: 'SPY', netPL: '-$27.84' },
    ];

    return (
        <div className="p-6 bg-gray-100 min-h-screen">
            {/* Header Section */}
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-2xl font-bold">Dashboard</h1>
                <div className="flex items-center space-x-4">
                    <p className="text-sm text-gray-500">Last import: Apr 11, 2024 04:42 PM</p>
                    <button className="text-blue-500 underline">Resync</button>
                </div>
            </div>

            <div className="flex items-center justify-between mb-6">
                {/* Metrics Dropdown */}
                <select className="border rounded-md p-2">
                    <option value="dollar">Dollar</option>
                    <option value="percentage">Percentage</option>
                    <option value="r-multiple">R-Multiple</option>
                    <option value="ticks">Ticks</option>
                    <option value="pips">Pips</option>
                    <option value="points">Points</option>
                </select>

                {/* Filters and Date Range */}
                <div className="flex items-center space-x-4">
                    <button className="border rounded-md px-4 py-2 bg-gray-100 hover:bg-gray-200">
                        Filters
                    </button>
                    <button className="border rounded-md px-4 py-2 bg-gray-100 hover:bg-gray-200">
                        Apr 02, 2024 - Apr 02, 2024
                    </button>
                </div>

                {/* Account Selector */}
                <select className="border rounded-md p-2">
                    <option value="all">All Accounts</option>
                    <option value="account1">Account 1</option>
                </select>
            </div>

            {/* Metrics Section */}
            <Metrics data={metricsData} />

            {/* Charts Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
                <div className="bg-white shadow-lg rounded-lg p-6">
                    <h2 className="text-lg font-semibold mb-4">Daily Net Cumulative P&L</h2>
                    <DailyNetCumulativeChart />
                </div>
                <div className="bg-white shadow-lg rounded-lg p-6">
                    <h2 className="text-lg font-semibold mb-4">Net Daily P&L</h2>
                    <NetDailyPLChart />
                </div>
            </div>

            {/* Bottom Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
                <div className="bg-white shadow-lg rounded-lg p-6">
                    <h2 className="text-lg font-semibold mb-4">Recent Trades</h2>
                    <RecentTrades trades={tradesData} />
                </div>
                <div className="bg-white shadow-lg rounded-lg p-6">
                    <h2 className="text-lg font-semibold mb-4">Calendar</h2>
                    <Calendar />
                </div>
            </div>
        </div>
    );
}

export default Dashboard;
