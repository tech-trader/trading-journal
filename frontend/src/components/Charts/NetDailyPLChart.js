import React from 'react';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';

// Register the required components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

function NetDailyPLChart() {
    const data = {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        datasets: [
            {
                label: 'Net P&L',
                data: [100, 300, -200, 400, 500],
                backgroundColor: [
                    '#36a2eb',
                    '#ff6384',
                    '#ffcd56',
                    '#4bc0c0',
                    '#9966ff',
                ],
            },
        ],
    };

    return <Bar data={data} />;
}

export default NetDailyPLChart;
