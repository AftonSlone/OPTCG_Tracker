import React, { useState, useEffect } from 'react';

function RoundForm({ eventId, editingRound, onClose, onSubmit }) {
  const [formData, setFormData] = useState({
    opponentLeader: '',
    wonDiceRoll: false,
    wentFirst: false,
    isWin: false
  });
  const [error, setError] = useState('');
  const [leaders, setLeaders] = useState([]);
  const [loadingLeaders, setLoadingLeaders] = useState(true);

  useEffect(() => {
    fetchLeaders();
  }, []);

  useEffect(() => {
    if (editingRound) {
      setFormData({
        opponentLeader: editingRound.opponentLeader || '',
        wonDiceRoll: editingRound.diceRollResult === 'won',
        wentFirst: editingRound.wentFirst || false,
        isWin: editingRound.isWin || false
      });
    }
  }, [editingRound]);

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

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const token = localStorage.getItem('jwtToken');
      const url = editingRound
        ? `/api/event/${eventId}/round/${editingRound.id}`
        : `/api/event/${eventId}/round`;
      const method = editingRound ? 'PUT' : 'POST';

      const submissionData = {
        opponentLeader: formData.opponentLeader,
        diceRollResult: formData.wonDiceRoll ? 'won' : 'lost',
        wentFirst: formData.wentFirst,
        isWin: formData.isWin
      };

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(submissionData)
      });

      if (response.ok) {
        onSubmit();
      } else {
        const data = await response.json();
        setError(data.message || 'Failed to save round');
      }
    } catch (err) {
      setError('Error saving round');
    }
  };

  return (
    <div>
      <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-6">
        {editingRound ? `Edit Round ${editingRound.roundNumber}` : 'Add New Round'}
      </h3>

      {error && (
        <div className="mb-6 p-4 bg-red-100 dark:bg-red-900/30 border border-red-400 dark:border-red-700 text-red-700 dark:text-red-300 rounded-lg">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="opponentLeader" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Opponent Leader
          </label>
          <input
            id="opponentLeader"
            type="text"
            name="opponentLeader"
            value={formData.opponentLeader}
            onChange={handleChange}
            placeholder="Select or type leader name"
            list="leaderOptions"
            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
          />
          <datalist id="leaderOptions">
            {loadingLeaders ? (
              <option value="" disabled>Loading leaders...</option>
            ) : (
              leaders.map(leader => (
                <option key={leader.id} value={leader.name}>
                  ({leader.cardNumber}) - {leader.color2 ? `${leader.color1}/${leader.color2}` : leader.color1}
                </option>
              ))
            )}
          </datalist>
        </div>

        <div>
          <label className="flex items-center gap-3 cursor-pointer">
            <div className="relative">
              <input
                type="checkbox"
                name="wonDiceRoll"
                checked={formData.wonDiceRoll}
                onChange={handleChange}
                className="sr-only peer"
              />
              <div className="w-11 h-6 bg-red-300 peer-focus:ring-2 peer-focus:ring-green-500 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
            </div>
            <span className="text-gray-700 dark:text-gray-300 font-medium">Won Dice Roll</span>
          </label>
        </div>

        <div className="flex items-center gap-6">
          <label className="flex items-center gap-3 cursor-pointer">
            <div className="relative">
              <input
                type="checkbox"
                name="wentFirst"
                checked={formData.wentFirst}
                onChange={handleChange}
                className="sr-only peer"
              />
              <div className="w-11 h-6 bg-red-300 peer-focus:ring-2 peer-focus:ring-green-500 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
            </div>
            <span className="text-gray-700 dark:text-gray-300 font-medium">Went First</span>
          </label>

          <label className="flex items-center gap-3 cursor-pointer">
            <div className="relative">
              <input
                type="checkbox"
                name="isWin"
                checked={formData.isWin}
                onChange={handleChange}
                className="sr-only peer"
              />
              <div className="w-11 h-6 bg-red-300 peer-focus:ring-2 peer-focus:ring-green-500 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-600"></div>
            </div>
            <span className="text-gray-700 dark:text-gray-300 font-medium">Won Match</span>
          </label>
        </div>

        <div className="flex gap-4">
          <button
            type="submit"
            className="flex-1 px-6 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white font-medium rounded-lg hover:from-purple-700 hover:to-indigo-700 transition-all hover:scale-105 shadow-lg"
          >
            {editingRound ? 'Update Round' : 'Add Round'}
          </button>
          <button
            type="button"
            onClick={onClose}
            className="px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-medium rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-all"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

export default RoundForm;
