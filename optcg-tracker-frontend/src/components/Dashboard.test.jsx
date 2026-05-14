import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Dashboard from './Dashboard';

// Mock fetch
global.fetch = jest.fn();

describe('Dashboard Component', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    // Reset fetch mock
    fetch.mockClear();
  });

  test('renders welcome heading', () => {
    localStorage.setItem('jwtToken', 'test-token');
    
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    const heading = screen.getByText(/Welcome Back!/i);
    expect(heading).toBeInTheDocument();
  });

  test('renders loading state initially', () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve({
        id: 1,
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00Z',
        lastModified: '2024-01-01T00:00:00Z',
        lastLoginDate: '2024-01-01T00:00:00Z',
        oAuthProvider: 'Google'
      })
    }));

    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    expect(screen.getByText(/Loading your profile.../i)).toBeInTheDocument();
  });

  test('renders error when no token found', async () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/No authentication token found/i)).toBeInTheDocument();
    });
  });

  test('renders user profile when data loaded successfully', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve({
        id: 1,
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00Z',
        lastModified: '2024-01-01T00:00:00Z',
        lastLoginDate: '2024-01-01T00:00:00Z',
        oAuthProvider: 'Google'
      })
    }));

    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/User Profile/i)).toBeInTheDocument();
      expect(screen.getByText(/Test User/i)).toBeInTheDocument();
      expect(screen.getByText(/testuser/i)).toBeInTheDocument();
      expect(screen.getByText(/test@example.com/i)).toBeInTheDocument();
    });
  });

  test('renders edit display name button', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve({
        id: 1,
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00Z',
        lastModified: '2024-01-01T00:00:00Z',
        lastLoginDate: '2024-01-01T00:00:00Z',
        oAuthProvider: 'Google'
      })
    }));

    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      const editButton = screen.getByText(/Edit Display Name/i);
      expect(editButton).toBeInTheDocument();
    });
  });

  test('opens edit form when edit button clicked', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve({
        id: 1,
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00Z',
        lastModified: '2024-01-01T00:00:00Z',
        lastLoginDate: '2024-01-01T00:00:00Z',
        oAuthProvider: 'Google'
      })
    }));

    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      const editButton = screen.getByText(/Edit Display Name/i);
      fireEvent.click(editButton);
    });

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/Enter display name/i)).toBeInTheDocument();
      expect(screen.getByText(/Save/i)).toBeInTheDocument();
      expect(screen.getByText(/Cancel/i)).toBeInTheDocument();
    });
  });

  test('renders logout button', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve({
        id: 1,
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        createdDate: '2024-01-01T00:00:00Z',
        lastModified: '2024-01-01T00:00:00Z',
        lastLoginDate: '2024-01-01T00:00:00Z',
        oAuthProvider: 'Google'
      })
    }));

    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    await waitFor(() => {
      const logoutButton = screen.getByText(/Logout/i);
      expect(logoutButton).toBeInTheDocument();
    });
  });
});
