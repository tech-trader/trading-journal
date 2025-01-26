import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import axios from 'axios';
import Dashboard from './pages/Dashboard';

function App() {
    const [dashboardData, setDashboardData] = useState(null);

    useEffect(() => {
      const backendURL = process.env.REACT_APP_BACKEND_URL || 'http://localhost:5001';
  
      console.log('Fetching data from:', `${backendURL}/api/dashboard`);
  
      axios.get(`${backendURL}/api/dashboard`)
          .then((response) => {
              console.log('Dashboard data fetched:', response.data);
              setDashboardData(response.data);
          })
          .catch((error) => {
              console.error('Error fetching dashboard data:', error);
          });
  }, []);
  
  

    return (
        <Router>
            <Routes>
                <Route path="/" element={<Dashboard data={dashboardData} />} />
            </Routes>
        </Router>
    );
}

export default App;
