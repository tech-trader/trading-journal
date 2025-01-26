const express = require('express');
const router = express.Router();

// Dashboard API
router.get('/', (req,res) => {
    const dashboardData = {
        totalTrades: 10,
        totalPL: 200,
        winRate: 60,
    };
    res.json(dashboardData);
});

module.exports = router;