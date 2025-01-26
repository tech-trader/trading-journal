require('dotenv').config(); // Import dotenv at the top

const express = require('express');
const cors = require ('cors');
const dashboardRoutes = require('./routes/dashboard');

const app = express();

// Use environment variables
const PORT = process.env.PORT || 5001;

//Middleware
app.use(cors());
app.use(express.json());

// Root route
app.get('/', (req, res) => {
    res.send('Welcome to the Trading Journal Backend!');
});

//Routes
app.use('/api/dashboard', dashboardRoutes)

//Start Server
app.listen(PORT, () => {
    console.log('Server is running on http://localhost:${PORT}');
});