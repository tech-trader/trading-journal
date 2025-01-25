const express = require('express');
const cors = require ('cors');

const app = express();
const PORT = 5000;

//Middleware
app.use(cors());
app.use(express.json());

//Routes
app.get('/', (req,res) => {
    res.send('Backend is running!');
});

//Start Server
app.listen(PORT, () => {
    console.log('Server is running on http://localhost:${PORT}');
});