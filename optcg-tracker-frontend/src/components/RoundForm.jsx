import React, { useState, useEffect } from 'react';

function RoundForm({ eventId, editingRound, onClose, onSubmit }) {
  const [formData, setFormData] = useState({
    opponentLeader: '',
    diceRollResult: '',
    wentFirst: false,
    isWin: false
  });
  const [error, setError] = useState('');

  useEffect(() => {
    if (editingRound) {
      setFormData({
        opponentLeader: editingRound.opponentLeader || '',
        diceRollResult: editingRound.diceRollResult || '',
        wentFirst: editingRound.wentFirst || false,
        isWin: editingRound.isWin || false
      });
    }
  }, [editingRound]);

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

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(formData)
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
            placeholder="e.g., Luffy, Zoro, Nami"
            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
          />
        </div>

        <div>
          <label htmlFor="diceRollResult" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Dice Roll Result
          </label>
          <input
            id="diceRollResult"
            type="text"
            name="diceRollResult"
            value={formData.diceRollResult}
            onChange={handleChange}
            placeholder="e.g., 6, 1-6"
            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white transition-all"
          />
        </div>

        <div className="flex items-center gap-6">
          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              name="wentFirst"
              checked={formData.wentFirst}
              onChange={handleChange}
              className="w-5 h-5 text-purple-600 border-gray-300 rounded focus:ring-purple-500"
            />
            <span className="text-gray-700 dark:text-gray-300 font-medium">Went First</span>
          </label>

          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              name="isWin"
              checked={formData.isWin}
              onChange={handleChange}
              className="w-5 h-5 text-purple-600 border-gray-300 rounded focus:ring-purple-500"
            />
            <span className="text-gray-700 dark:text-gray-300 font-medium">Win</span>
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
