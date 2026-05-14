import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

function Dashboard() {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingDisplayName, setEditingDisplayName] = useState(false);
  const [displayName, setDisplayName] = useState('');
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const tokenFromUrl = urlParams.get('token');
    
    if (tokenFromUrl) {
      localStorage.setItem('jwtToken', tokenFromUrl);
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
    <div className="flex items-center justify-center min-h-screen px-4 pt-20">
      <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 max-w-lg w-full">
        <div className="w-20 h-20 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-6">
          <svg width="50" height="50" viewBox="0 0 24 24" fill="white">
            <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
          </svg>
        </div>
        
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2 text-center">
          Welcome Back!
        </h1>
        <p className="text-gray-600 dark:text-gray-300 text-center mb-8">
          You are now logged in to OPTCG Tracker
        </p>
        
        {loading && (
          <div className="flex items-center justify-center gap-3 text-gray-600 dark:text-gray-300">
            <div className="w-5 h-5 border-3 border-gray-300 border-t-green-500 rounded-full animate-spin"></div>
            <span>Loading your profile...</span>
          </div>
        )}
        
        {user && (
          <div className="bg-gray-50 dark:bg-gray-700 rounded-xl p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">User Profile</h2>
            <div className="space-y-3">
              <p className="text-gray-700 dark:text-gray-300">
                <span className="font-semibold">Display Name:</span> {user.displayName || user.username}
              </p>
              <p className="text-gray-700 dark:text-gray-300">
                <span className="font-semibold">Username:</span> {user.username}
              </p>
              <p className="text-gray-700 dark:text-gray-300">
                <span className="font-semibold">Email:</span> {user.email}
              </p>
              <p className="text-gray-700 dark:text-gray-300">
                <span className="font-semibold">OAuth Provider:</span> {user.oAuthProvider}
              </p>
              <p className="text-gray-700 dark:text-gray-300">
                <span className="font-semibold">Member Since:</span> {formatDate(user.createdDate)}
              </p>
              {user.lastLoginDate && (
                <p className="text-gray-700 dark:text-gray-300">
                  <span className="font-semibold">Last Login:</span> {formatDate(user.lastLoginDate)}
                </p>
              )}
            </div>
            
            <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-600">
              {editingDisplayName ? (
                <div className="flex flex-col gap-3">
                  <input
                    type="text"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    placeholder="Enter display name"
                    maxLength="100"
                    className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-purple-500"
                  />
                  <div className="flex gap-3">
                    <button 
                      onClick={saveDisplayName}
                      disabled={updating}
                      className="flex-1 bg-green-500 hover:bg-green-600 text-white py-2 px-4 rounded-lg font-semibold transition-all hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {updating ? 'Saving...' : 'Save'}
                    </button>
                    <button 
                      onClick={cancelEditingDisplayName}
                      disabled={updating}
                      className="flex-1 bg-red-500 hover:bg-red-600 text-white py-2 px-4 rounded-lg font-semibold transition-all hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <button 
                  onClick={startEditingDisplayName}
                  className="w-full bg-purple-600 hover:bg-purple-700 text-white py-2 px-4 rounded-lg font-semibold transition-all hover:scale-105"
                >
                  Edit Display Name
                </button>
              )}
            </div>
          </div>
        )}
        
        {error && (
          <div className="bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 p-4 rounded-xl mb-6">
            {error}
          </div>
        )}
        
        <button 
          onClick={logout}
          className="w-full bg-red-500 hover:bg-red-600 text-white py-3 px-6 rounded-xl font-semibold transition-all hover:scale-105 shadow-md"
        >
          Logout
        </button>
      </div>
    </div>
  );
}

export default Dashboard;
