import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

function DeckList() {
  const navigate = useNavigate();
  const [decks, setDecks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingDeck, setEditingDeck] = useState(null);
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  useEffect(() => {
    fetchDecks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchDecks = async () => {
    const token = localStorage.getItem('jwtToken');
    if (!token) {
      navigate('/');
      return;
    }

    try {
      const response = await fetch('/api/deck', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setDecks(data);
        setLoading(false);
      } else {
        setError('Failed to load decks');
        setLoading(false);
      }
    } catch (error) {
      setError('Error loading decks: ' + error.message);
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingDeck(null);
    setShowForm(true);
  };

  const handleEdit = (deck) => {
    setEditingDeck(deck);
    setShowForm(true);
  };

  const handleDelete = (deck) => {
    setDeleteConfirm(deck);
  };

  const confirmDelete = async () => {
    if (!deleteConfirm) return;

    const token = localStorage.getItem('jwtToken');
    try {
      const response = await fetch(`/api/deck/${deleteConfirm.id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        setDecks(decks.filter(d => d.id !== deleteConfirm.id));
        setDeleteConfirm(null);
      } else {
        setError('Failed to delete deck');
      }
    } catch (error) {
      setError('Error deleting deck: ' + error.message);
    }
  };

  const handleFormSubmit = (deckData) => {
    if (editingDeck) {
      setDecks(decks.map(d => d.id === editingDeck.id ? { ...deckData, id: editingDeck.id } : d));
    } else {
      setDecks([...decks, deckData]);
    }
    setShowForm(false);
    setEditingDeck(null);
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingDeck(null);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen px-4 pt-20">
        <div className="flex items-center gap-3 text-gray-600 dark:text-gray-300">
          <div className="w-5 h-5 border-3 border-gray-300 border-t-purple-500 rounded-full animate-spin"></div>
          <span>Loading decks...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 pt-20 pb-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold text-white">My Decks</h1>
          <button
            onClick={handleCreate}
            className="bg-green-500 hover:bg-green-600 text-white py-2 px-6 rounded-lg font-semibold transition-all hover:scale-105 shadow-md"
          >
            + Create Deck
          </button>
        </div>

        {error && (
          <div className="bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 p-4 rounded-xl mb-6">
            {error}
          </div>
        )}

        {showForm && (
          <DeckForm
            deck={editingDeck}
            onSubmit={handleFormSubmit}
            onCancel={handleFormCancel}
          />
        )}

        {decks.length === 0 && !showForm && (
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 text-center">
            <p className="text-gray-600 dark:text-gray-300 mb-4">You don't have any decks yet.</p>
            <button
              onClick={handleCreate}
              className="bg-purple-600 hover:bg-purple-700 text-white py-2 px-6 rounded-lg font-semibold transition-all hover:scale-105"
            >
              Create Your First Deck
            </button>
          </div>
        )}

        <div className="grid gap-4">
          {decks.map(deck => (
            <div
              key={deck.id}
              className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 hover:shadow-xl transition-shadow"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                    {deck.name}
                  </h3>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Created: {new Date(deck.createdDate).toLocaleDateString()}
                  </p>
                  {deck.lastModified && (
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      Last Modified: {new Date(deck.lastModified).toLocaleDateString()}
                    </p>
                  )}
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleEdit(deck)}
                    className="p-2 rounded-lg bg-blue-500 hover:bg-blue-600 text-white transition-all hover:scale-105"
                    title="Edit deck"
                  >
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/>
                    </svg>
                  </button>
                  <button
                    onClick={() => handleDelete(deck)}
                    className="p-2 rounded-lg bg-red-500 hover:bg-red-600 text-white transition-all hover:scale-105"
                    title="Delete deck"
                  >
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {deleteConfirm && (
          <div className="fixed inset-0 bg-black/50 z-[1000] flex items-center justify-center p-4">
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 max-w-md w-full">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                Delete Deck
              </h2>
              <p className="text-gray-600 dark:text-gray-300 mb-6">
                Are you sure you want to delete "{deleteConfirm.name}"? This action cannot be undone.
              </p>
              <div className="flex gap-3">
                <button
                  onClick={confirmDelete}
                  className="flex-1 bg-red-500 hover:bg-red-600 text-white py-2 px-4 rounded-lg font-semibold transition-all hover:scale-105"
                >
                  Delete
                </button>
                <button
                  onClick={() => setDeleteConfirm(null)}
                  className="flex-1 bg-gray-300 hover:bg-gray-400 text-gray-900 py-2 px-4 rounded-lg font-semibold transition-all hover:scale-105"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function DeckForm({ deck, onSubmit, onCancel }) {
  const [name, setName] = useState(deck?.name || '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [leaders, setLeaders] = useState([]);
  const [loadingLeaders, setLoadingLeaders] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showDropdown, setShowDropdown] = useState(false);
  const [selectedLeaderId, setSelectedLeaderId] = useState(deck?.leaderId || null);
  const dropdownRef = useRef(null);

  useEffect(() => {
    fetchLeaders();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (deck?.leaderId) {
      setSelectedLeaderId(deck.leaderId);
      const leader = leaders.find(l => l.id === deck.leaderId);
      if (leader) {
        setSearchTerm(`${leader.name} (${leader.cardNumber}) - ${leader.color2 ? `${leader.color1}/${leader.color2}` : leader.color1}`);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deck?.leaderId, leaders]);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setShowDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const fetchLeaders = async () => {
    try {
      const response = await fetch('/api/leader');
      if (response.ok) {
        const data = await response.json();
        setLeaders(data);
      }
    } catch (err) {
      console.error('Failed to fetch leaders:', err);
    } finally {
      setLoadingLeaders(false);
    }
  };

  const handleSearchChange = (e) => {
    setSearchTerm(e.target.value);
    setShowDropdown(true);
  };

  const handleLeaderSelect = (leader) => {
    const selectedValue = `${leader.name} (${leader.cardNumber}) - ${leader.color2 ? `${leader.color1}/${leader.color2}` : leader.color1}`;
    setSelectedLeaderId(leader.id);
    setSearchTerm(selectedValue);
    setShowDropdown(false);
  };

  const filteredLeaders = leaders.filter(leader =>
    leader.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    leader.cardNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    leader.color1?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    leader.color2?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('Deck name is required');
      return;
    }

    setSaving(true);
    setError('');
    const token = localStorage.getItem('jwtToken');

    try {
      const url = deck ? `/api/deck/${deck.id}` : '/api/deck';
      const method = deck ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ 
          name: name.trim(),
          leaderId: selectedLeaderId
        })
      });

      if (response.ok) {
        const data = await response.json();
        onSubmit(data);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save deck');
      }
    } catch (error) {
      setError('Error saving deck: ' + error.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 mb-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
        {deck ? 'Edit Deck' : 'Create New Deck'}
      </h2>

      {error && (
        <div className="bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 p-4 rounded-xl mb-6">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="mb-6">
          <label className="block text-gray-700 dark:text-gray-300 font-semibold mb-2">
            Deck Name
          </label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Enter deck name"
            maxLength="100"
            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-purple-500"
          />
        </div>

        <div ref={dropdownRef} className="mb-6 relative">
          <label className="block text-gray-700 dark:text-gray-300 font-semibold mb-2">
            Leader
          </label>
          <input
            type="text"
            value={searchTerm}
            onChange={handleSearchChange}
            onFocus={() => setShowDropdown(true)}
            placeholder="Search or select a leader"
            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-purple-500"
          />
          {showDropdown && (
            <div className="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg max-h-60 overflow-y-auto">
              {loadingLeaders ? (
                <div className="px-4 py-3 text-gray-500 dark:text-gray-400">Loading leaders...</div>
              ) : filteredLeaders.length === 0 ? (
                <div className="px-4 py-3 text-gray-500 dark:text-gray-400">No leaders found</div>
              ) : (
                filteredLeaders.map(leader => (
                  <div
                    key={leader.id}
                    onClick={() => handleLeaderSelect(leader)}
                    className="px-4 py-3 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-900 dark:text-white transition-colors"
                  >
                    {leader.name} ({leader.cardNumber}) - {leader.color2 ? `${leader.color1}/${leader.color2}` : leader.color1}
                  </div>
                ))
              )}
            </div>
          )}
        </div>

        <div className="flex gap-3">
          <button
            type="submit"
            disabled={saving}
            className="flex-1 bg-green-500 hover:bg-green-600 text-white py-3 px-6 rounded-lg font-semibold transition-all hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? 'Saving...' : (deck ? 'Update Deck' : 'Create Deck')}
          </button>
          <button
            type="button"
            onClick={onCancel}
            disabled={saving}
            className="flex-1 bg-gray-300 hover:bg-gray-400 text-gray-900 py-3 px-6 rounded-lg font-semibold transition-all hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

export default DeckList;
