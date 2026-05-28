import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

function EventForm() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: '',
    date: new Date().toISOString().split('T')[0],
    deckId: ''
  });
  const [decks, setDecks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchDecks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchDecks = async () => {
    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch('/api/deck', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setDecks(data);
      } else {
        setError('Failed to load decks');
      }
    } catch (err) {
      setError('Error loading decks');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!formData.name.trim()) {
      setError('Event name is required');
      return;
    }

    if (!formData.deckId) {
      setError('Please select a deck');
      return;
    }

    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch('/api/event', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          name: formData.name,
          date: formData.date,
          deckId: parseInt(formData.deckId)
        })
      });

      if (response.ok) {
        const data = await response.json();
        navigate(`/event/${data.id}`);
      } else {
        setError('Failed to create event');
      }
    } catch (err) {
      setError('Error creating event');
    }
  };

  const handleCreateDeck = () => {
    navigate('/decks');
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
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold text-white mb-8">Create New Event</h1>

        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
          {error && (
            <div className="mb-6 p-4 bg-red-100 dark:bg-red-900/30 border border-red-400 dark:border-red-700 text-red-700 dark:text-red-300 rounded-lg">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Event Name
              </label>
              <input
                id="name"
                type="text"
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Enter event name"
                className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
                required
              />
            </div>

            <div>
              <label htmlFor="date" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Date
              </label>
              <input
                id="date"
                type="date"
                name="date"
                value={formData.date}
                onChange={handleChange}
                className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
                required
              />
            </div>

            <div>
              <label htmlFor="deckId" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Deck
              </label>
              <select
                id="deckId"
                name="deckId"
                value={formData.deckId}
                onChange={(e) => {
                  if (e.target.value === 'create-new') {
                    handleCreateDeck();
                  } else {
                    handleChange(e);
                  }
                }}
                className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
                required
              >
                <option value="">Select a deck</option>
                {decks.map(deck => (
                  <option key={deck.id} value={deck.id}>
                    {deck.name}
                  </option>
                ))}
                <option value="create-new" className="text-purple-600 dark:text-purple-400 font-medium">
                  + Create New Deck
                </option>
              </select>
            </div>

            <div className="flex gap-4">
              <button
                type="submit"
                className="flex-1 px-6 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white font-medium rounded-lg hover:from-purple-700 hover:to-indigo-700 transition-all hover:scale-105 shadow-lg"
              >
                Create Event
              </button>
              <button
                type="button"
                onClick={() => navigate(-1)}
                className="px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-medium rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-all"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default EventForm;
