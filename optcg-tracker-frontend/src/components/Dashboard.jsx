import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

function Dashboard() {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingDisplayName, setEditingDisplayName] = useState(false);
  const [displayName, setDisplayName] = useState('');
  const [updating, setUpdating] = useState(false);

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

  const startEditingDisplayName = () => {
    setDisplayName(user.displayName || user.username);
    setEditingDisplayName(true);
  };

  const cancelEditingDisplayName = () => {
    setEditingDisplayName(false);
    setDisplayName('');
  };

  const saveDisplayName = async () => {
    setUpdating(true);
    try {
      const response = await fetch('/api/user/profile', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
        },
        body: JSON.stringify({ displayName: displayName })
      });
      
      if (response.ok) {
        const updatedUser = await response.json();
        setUser(updatedUser);
        setEditingDisplayName(false);
      } else {
        setError('Failed to update display name');
      }
    } catch (error) {
      setError('Error updating display name: ' + error.message);
    } finally {
      setUpdating(false);
    }
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
          <p><strong>Display Name:</strong> {user.displayName || user.username}</p>
          <p><strong>Username:</strong> {user.username}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>OAuth Provider:</strong> {user.oAuthProvider}</p>
          <p><strong>Member Since:</strong> {formatDate(user.createdDate)}</p>
          {user.lastLoginDate && (
            <p><strong>Last Login:</strong> {formatDate(user.lastLoginDate)}</p>
          )}
          
          <div className="display-name-edit">
            {editingDisplayName ? (
              <div className="edit-form">
                <input
                  type="text"
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  placeholder="Enter display name"
                  maxLength="100"
                />
                <button 
                  className="save-btn" 
                  onClick={saveDisplayName}
                  disabled={updating}
                >
                  {updating ? 'Saving...' : 'Save'}
                </button>
                <button 
                  className="cancel-btn" 
                  onClick={cancelEditingDisplayName}
                  disabled={updating}
                >
                  Cancel
                </button>
              </div>
            ) : (
              <button className="edit-btn" onClick={startEditingDisplayName}>
                Edit Display Name
              </button>
            )}
          </div>
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
