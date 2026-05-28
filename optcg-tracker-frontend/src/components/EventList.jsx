import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

function EventList() {
  const navigate = useNavigate();
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('all'); // all, finalized, active
  const [sortBy, setSortBy] = useState('date'); // date, result
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  });

  useEffect(() => {
    fetchEvents();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, sortBy, pagination.page]);

  const fetchEvents = async () => {
    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch(`/api/event?page=${pagination.page}&pageSize=${pagination.pageSize}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        let filteredEvents = data.events;

        // Apply filter
        if (filter === 'finalized') {
          filteredEvents = filteredEvents.filter(e => e.isFinalized);
        } else if (filter === 'active') {
          filteredEvents = filteredEvents.filter(e => !e.isFinalized);
        }

        // Apply sort
        if (sortBy === 'date') {
          filteredEvents.sort((a, b) => new Date(b.date) - new Date(a.date));
        } else if (sortBy === 'result') {
          filteredEvents.sort((a, b) => {
            if (!a.finalResult) return 1;
            if (!b.finalResult) return -1;
            return a.finalResult.localeCompare(b.finalResult);
          });
        }

        setEvents(filteredEvents);
        setPagination(data.pagination);
      } else {
        setError('Failed to load events');
      }
    } catch (err) {
      setError('Error loading events');
    } finally {
      setLoading(false);
    }
  };

  const handleEventClick = (eventId) => {
    navigate(`/event/${eventId}`);
  };

  const handleDeleteEvent = async (eventId, e) => {
    e.stopPropagation();
    if (!window.confirm('Are you sure you want to delete this event? This action cannot be undone.')) {
      return;
    }

    try {
      const token = localStorage.getItem('jwtToken');
      const response = await fetch(`/api/event/${eventId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        // Refresh the event list
        fetchEvents();
      } else {
        setError('Failed to delete event');
      }
    } catch (err) {
      setError('Error deleting event');
    }
  };

  const handlePageChange = (newPage) => {
    setPagination(prev => ({ ...prev, page: newPage }));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen px-4 pt-20">
        <div className="flex items-center gap-3 text-gray-600 dark:text-gray-300">
          <div className="w-5 h-5 border-3 border-gray-300 border-t-purple-500 rounded-full animate-spin"></div>
          <span>Loading events...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 pt-20 pb-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-white mb-8">Event History</h1>

        {error && (
          <div className="mb-6 p-4 bg-red-100 dark:bg-red-900/30 border border-red-400 dark:border-red-700 text-red-700 dark:text-red-300 rounded-lg">
            {error}
          </div>
        )}

        {/* Filters and Sort */}
        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-6 mb-6">
          <div className="flex flex-wrap gap-4">
            <div className="flex-1 min-w-[200px]">
              <label htmlFor="filter" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Filter
              </label>
              <select
                id="filter"
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              >
                <option value="all">All Events</option>
                <option value="finalized">Finalized</option>
                <option value="active">Active</option>
              </select>
            </div>
            <div className="flex-1 min-w-[200px]">
              <label htmlFor="sortBy" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Sort By
              </label>
              <select
                id="sortBy"
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              >
                <option value="date">Date (Newest First)</option>
                <option value="result">Result</option>
              </select>
            </div>
          </div>
        </div>

        {/* Event List */}
        {events.length === 0 ? (
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8 text-center">
            <p className="text-gray-600 dark:text-gray-300 mb-4">No events found.</p>
            <button
              onClick={() => navigate('/event/create')}
              className="bg-purple-600 hover:bg-purple-700 text-white py-2 px-6 rounded-lg font-semibold transition-all hover:scale-105"
            >
              Create Your First Event
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {events.map(event => (
              <div
                key={event.id}
                onClick={() => handleEventClick(event.id)}
                className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 hover:shadow-xl transition-all cursor-pointer hover:scale-[1.02]"
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                      {event.name}
                    </h3>
                    <div className="flex flex-wrap gap-4 text-sm text-gray-600 dark:text-gray-400">
                      <span>
                        <span className="font-medium">Date:</span> {new Date(event.date).toLocaleDateString()}
                      </span>
                      <span>
                        <span className="font-medium">Rounds:</span> {event.roundCount}
                      </span>
                      {event.isFinalized && (
                        <span className="text-green-600 dark:text-green-400 font-medium">
                          Result: {event.finalResult}
                        </span>
                      )}
                      {!event.isFinalized && (
                        <span className="text-yellow-600 dark:text-yellow-400 font-medium">
                          In Progress
                        </span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    <button
                      onClick={(e) => handleDeleteEvent(event.id, e)}
                      className="p-2 text-red-500 hover:text-red-700 hover:bg-red-100 dark:hover:bg-red-900/30 rounded-lg transition-all"
                      title="Delete event"
                    >
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z"/>
                      </svg>
                    </button>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" className="text-gray-400">
                      <path d="M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z"/>
                    </svg>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Pagination */}
        {pagination.totalPages > 1 && (
          <div className="flex items-center justify-center gap-2 mt-6">
            <button
              onClick={() => handlePageChange(pagination.page - 1)}
              disabled={pagination.page === 1}
              className="px-4 py-2 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            >
              Previous
            </button>
            <span className="px-4 py-2 text-gray-700 dark:text-gray-300">
              Page {pagination.page} of {pagination.totalPages}
            </span>
            <button
              onClick={() => handlePageChange(pagination.page + 1)}
              disabled={pagination.page === pagination.totalPages}
              className="px-4 py-2 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            >
              Next
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default EventList;
