import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

function Dashboard() {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    // Get token from URL
    const urlParams = new URLSearchParams(window.location.search);
    const tokenFromUrl = urlParams.get('token');
    
    if (tokenFromUrl) {
      // Store token in localStorage
      localStorage.setItem('jwtToken', tokenFromUrl);
      // Clear token from URL
      window.history.replaceState({}, document.title, window.location.pathname);
    }
    
    fetchUserProfile();
  }, []);

  const fetchUserProfile = async () => {
    const storedToken = localStorage.getItem('jwtToken');
    
    if (!storedToken) {
      setError('No authentication token found. Please login again.');
      setLoading(false);
      return;
    }
    
    try {
      const response = await fetch('/api/user/profile', {
        headers: {
          'Authorization': `Bearer ${storedToken}`
        }
      });
      
      if (response.ok) {
        const userData = await response.json();
        setUser(userData);
        setLoading(false);
      } else {
        setError('Failed to load user profile. Please try logging in again.');
        setLoading(false);
      }
    } catch (error) {
      setError('Error loading profile: ' + error.message);
      setLoading(false);
    }
  };

  const logout = async () => {
    try {
      await fetch('/api/auth/logout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
        }
      });
    } catch (error) {
      console.error('Logout error:', error);
    }
    
    localStorage.removeItem('jwtToken');
    navigate('/');
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString();
  };

  return (
    <div className="container">
      <div className="success-icon">
        <svg viewBox="0 0 24 24">
          <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
        </svg>
      </div>
      
      <h1>Authentication Successful!</h1>
      <p>You are now logged in to OPTCG Tracker</p>
      
      {loading && (
        <div className="loading">
          <div className="spinner"></div>
          <span>Loading your profile...</span>
        </div>
      )}
      
      {user && (
        <div className="user-info">
          <h3>User Profile</h3>
          <p><strong>Username:</strong> {user.username}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>OAuth Provider:</strong> {user.oAuthProvider}</p>
          <p><strong>Member Since:</strong> {formatDate(user.createdDate)}</p>
        </div>
      )}
      
      {error && (
        <div className="error">{error}</div>
      )}
      
      <button className="logout-btn" onClick={logout}>Logout</button>
    </div>
  );
}

export default Dashboard;
