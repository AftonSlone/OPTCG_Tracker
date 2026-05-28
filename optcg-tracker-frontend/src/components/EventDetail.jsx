import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import RoundForm from './RoundForm';

function EventDetail() {
  const { eventId } = useParams();
  const navigate = useNavigate();
  const [event, setEvent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingRound, setEditingRound] = useState(null);
  const [showRoundForm, setShowRoundForm] = useState(false);

  useEffect(() => {
    fetchEvent();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventId]);

  const fetchEvent = async () => {
    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch(`/api/event/${eventId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setEvent(data);
      } else if (response.status === 404) {
        setError('Event not found');
      } else {
        setError('Failed to load event');
      }
    } catch (err) {
      setError('Error loading event');
    } finally {
      setLoading(false);
    }
  };

  const handleAddRound = () => {
    setEditingRound(null);
    setShowRoundForm(true);
  };

  const handleEditRound = (round) => {
    setEditingRound(round);
    setShowRoundForm(true);
  };

  const handleDeleteRound = async (roundId) => {
    if (!window.confirm('Are you sure you want to delete this round?')) {
      return;
    }

    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch(`/api/event/${eventId}/round/${roundId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        fetchEvent();
      } else {
        setError('Failed to delete round');
      }
    } catch (err) {
      setError('Error deleting round');
    }
  };

  const handleFinalizeEvent = async () => {
    if (!window.confirm('Are you sure you want to finalize this event? You cannot make changes after finalizing.')) {
      return;
    }

    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch(`/api/event/${eventId}/finalize`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        fetchEvent();
      } else {
        const data = await response.json();
        setError(data.message || 'Failed to finalize event');
      }
    } catch (err) {
      setError('Error finalizing event');
    }
  };

  const handleRoundFormClose = () => {
    setShowRoundForm(false);
    setEditingRound(null);
  };

  const handleRoundFormSubmit = () => {
    setShowRoundForm(false);
    setEditingRound(null);
    fetchEvent();
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen px-4 pt-20">
        <div className="flex items-center gap-3 text-gray-600 dark:text-gray-300">
          <div className="w-5 h-5 border-3 border-gray-300 border-t-purple-500 rounded-full animate-spin"></div>
          <span>Loading event...</span>
        </div>
      </div>
    );
  }

  if (error || !event) {
    return (
      <div className="min-h-screen px-4 pt-20 pb-8">
        <div className="max-w-4xl mx-auto">
          <div className="bg-red-100 dark:bg-red-900/30 border border-red-400 dark:border-red-700 text-red-700 dark:text-red-300 rounded-lg p-6">
            {error || 'Event not found'}
          </div>
          <button
            onClick={() => navigate(-1)}
            className="mt-4 px-6 py-3 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-medium rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-all"
          >
            Go Back
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 pt-20 pb-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold text-white">{event.name}</h1>
          <button
            onClick={() => navigate(-1)}
            className="px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 font-medium rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600 transition-all"
          >
            Back
          </button>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8 mb-6">
          <div className="grid grid-cols-2 gap-4 mb-6">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">Date</p>
              <p className="text-lg font-medium text-gray-900 dark:text-white">
                {new Date(event.date).toLocaleDateString()}
              </p>
            </div>
            {event.isFinalized && (
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Final Result</p>
                <p className="text-lg font-bold text-green-600 dark:text-green-400">
                  {event.finalResult}
                </p>
              </div>
            )}
          </div>

          {!event.isFinalized && (
            <button
              onClick={handleFinalizeEvent}
              className="w-full px-6 py-3 bg-gradient-to-r from-green-600 to-emerald-600 text-white font-medium rounded-lg hover:from-green-700 hover:to-emerald-700 transition-all hover:scale-105 shadow-lg"
            >
              Finalize Event
            </button>
          )}
        </div>

        {showRoundForm && (
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8 mb-6">
            <RoundForm
              eventId={eventId}
              editingRound={editingRound}
              onClose={handleRoundFormClose}
              onSubmit={handleRoundFormSubmit}
            />
          </div>
        )}

        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Rounds</h2>
            {!event.isFinalized && (
              <button
                onClick={handleAddRound}
                className="px-4 py-2 bg-gradient-to-r from-purple-600 to-indigo-600 text-white font-medium rounded-lg hover:from-purple-700 hover:to-indigo-700 transition-all hover:scale-105 shadow-lg"
              >
                Add Round
              </button>
            )}
          </div>

          {event.rounds && event.rounds.length > 0 ? (
            <div className="space-y-4">
              {event.rounds.map((round) => (
                <div
                  key={round.id}
                  className={`p-4 rounded-lg border-2 transition-all ${
                    round.isWin
                      ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                      : 'border-red-500 bg-red-50 dark:bg-red-900/20'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <span className="text-lg font-bold text-gray-900 dark:text-white">
                          Round {round.roundNumber}
                        </span>
                        <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                          round.isWin
                            ? 'bg-green-500 text-white'
                            : 'bg-red-500 text-white'
                        }`}>
                          {round.isWin ? 'Win' : 'Loss'}
                        </span>
                      </div>
                      {round.opponentLeader && (
                        <p className="text-gray-700 dark:text-gray-300">
                          Opponent: {round.opponentLeader}
                        </p>
                      )}
                      {round.diceRollResult && (
                        <p className="text-gray-700 dark:text-gray-300">
                          Dice Roll: {round.diceRollResult}
                        </p>
                      )}
                      {round.wentFirst && (
                        <p className="text-gray-700 dark:text-gray-300">
                          Went First
                        </p>
                      )}
                    </div>
                    {!event.isFinalized && (
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleEditRound(round)}
                          className="px-3 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-all"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDeleteRound(round.id)}
                          className="px-3 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-all"
                        >
                          Delete
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-center text-gray-600 dark:text-gray-400 py-8">
              No rounds recorded yet
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

export default EventDetail;
