import React, { useState, useEffect, useCallback } from 'react';

function Leaderboard() {
  const [leaderboard, setLeaderboard] = useState([]);
  const [loading, setLoading] = useState(true);
  const [timePeriod, setTimePeriod] = useState('7d');

  const timePeriodOptions = [
    { value: '7d', label: 'Last 7 Days' },
    { value: '30d', label: 'Last 30 Days' },
    { value: 'ytd', label: 'Year to Date' },
    { value: 'year', label: 'This Year' },
    { value: 'all', label: 'All Time' }
  ];

  const fetchLeaderboard = useCallback(async () => {
    setLoading(true);
    try {
      console.log('Fetching leaderboard for timePeriod:', timePeriod);
      const response = await fetch(`http://localhost:5126/api/leaderboard?timePeriod=${timePeriod}`);
      console.log('Response status:', response.status);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      console.log('Leaderboard data received:', data);
      console.log('Data length:', data.length);
      if (data.length > 0) {
        console.log('First entry:', data[0]);
        console.log('First entry thumbnailUrl:', data[0].thumbnailUrl);
      }
      setLeaderboard(data);
    } catch (error) {
      console.error('Error fetching leaderboard:', error);
      setLeaderboard([]);
    } finally {
      setLoading(false);
    }
  }, [timePeriod]);

  useEffect(() => {
    fetchLeaderboard();
  }, [fetchLeaderboard]);

  return (
    <div className="min-h-screen pt-20 px-4">
      <div className="max-w-6xl mx-auto">
        <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
              Leaderboard
            </h1>
            <div className="flex flex-wrap gap-2">
              {timePeriodOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => setTimePeriod(option.value)}
                  className={`px-4 py-2 rounded-lg font-medium transition-all ${
                    timePeriod === option.value
                      ? 'bg-purple-600 text-white'
                      : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600'
                  }`}
                >
                  {option.label}
                </button>
              ))}
            </div>
          </div>

          {loading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div>
            </div>
          ) : leaderboard.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-xl text-gray-600 dark:text-gray-300">No data available</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-gray-700">
                    <th className="text-left py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Rank</th>
                    <th className="text-left py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Leader</th>
                    <th className="text-right py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Play Rate</th>
                    <th className="text-right py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Win Rate</th>
                    <th className="text-right py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Events</th>
                    <th className="text-right py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Rounds</th>
                    <th className="text-right py-3 px-4 text-gray-700 dark:text-gray-300 font-semibold">Wins</th>
                  </tr>
                </thead>
                <tbody>
                  {leaderboard.map((entry, index) => (
                    <tr
                      key={entry.leaderId}
                      className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                    >
                      <td className="py-3 px-4">
                        <span className={`inline-flex items-center justify-center w-8 h-8 rounded-full font-bold ${
                          index === 0 ? 'bg-yellow-500 text-white' :
                          index === 1 ? 'bg-gray-400 text-white' :
                          index === 2 ? 'bg-orange-600 text-white' :
                          'bg-gray-200 dark:bg-gray-600 text-gray-700 dark:text-gray-300'
                        }`}>
                          {index + 1}
                        </span>
                      </td>
                      <td className="py-3 px-4">
                        <div className="flex items-center gap-3">
                          {entry.thumbnailUrl ? (
                            <img
                              src={entry.thumbnailUrl}
                              alt={entry.leaderName}
                              className="w-12 h-16 object-cover rounded shadow-md"
                              onError={(e) => {
                                console.error('Image load error:', e);
                                e.target.style.display = 'none';
                              }}
                              onLoad={() => {
                                console.log('Image loaded:', entry.thumbnailUrl);
                              }}
                            />
                          ) : (
                            <div className="w-12 h-16 bg-gray-200 dark:bg-gray-700 rounded flex items-center justify-center text-xs text-gray-500">
                              No img
                            </div>
                          )}
                          <span className="font-medium text-gray-900 dark:text-white">
                            {entry.leaderName}
                          </span>
                        </div>
                      </td>
                      <td className="py-3 px-4 text-right">
                        <span className="font-semibold text-purple-600 dark:text-purple-400">
                          {entry.playRate.toFixed(1)}%
                        </span>
                      </td>
                      <td className="py-3 px-4 text-right">
                        <span className={`font-semibold ${
                          entry.winRate >= 50 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'
                        }`}>
                          {entry.winRate.toFixed(1)}%
                        </span>
                      </td>
                      <td className="py-3 px-4 text-right text-gray-600 dark:text-gray-300">
                        {entry.totalEvents}
                      </td>
                      <td className="py-3 px-4 text-right text-gray-600 dark:text-gray-300">
                        {entry.totalRounds}
                      </td>
                      <td className="py-3 px-4 text-right text-gray-600 dark:text-gray-300">
                        {entry.wins}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Leaderboard;
