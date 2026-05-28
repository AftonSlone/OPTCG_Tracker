import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import EventForm from './EventForm';

// Mock fetch
global.fetch = jest.fn();

describe('EventForm Component', () => {
  beforeEach(() => {
    localStorage.clear();
    fetch.mockClear();
  });

  test('renders Create New Event heading', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Create New Event/i)).toBeInTheDocument();
    });
  });

  test('renders loading state initially', () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    expect(screen.getByText(/Loading decks.../i)).toBeInTheDocument();
  });

  test('renders form fields when decks loaded', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Red Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve(mockDecks)
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByLabelText(/Event Name/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Date/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/Deck/i)).toBeInTheDocument();
    });
  });

  test('renders deck options in selector', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Red Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: null },
      { id: 2, name: 'Blue Deck', createdDate: '2024-01-02T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve(mockDecks)
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Red Deck/i)).toBeInTheDocument();
      expect(screen.getByText(/Blue Deck/i)).toBeInTheDocument();
      expect(screen.getByText(/\+ Create New Deck/i)).toBeInTheDocument();
    });
  });

  test('shows error when event name is empty', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Test Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve(mockDecks)
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      const submitButton = screen.getByText(/Create Event/i);
      fireEvent.click(submitButton);
    });

    await waitFor(() => {
      expect(screen.getByText(/Event name is required/i)).toBeInTheDocument();
    });
  });

  test('shows error when deck is not selected', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Test Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve(mockDecks)
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      const nameInput = screen.getByLabelText(/Event Name/i);
      fireEvent.change(nameInput, { target: { value: 'Test Event' } });
    });

    await waitFor(() => {
      const submitButton = screen.getByText(/Create Event/i);
      fireEvent.click(submitButton);
    });

    await waitFor(() => {
      expect(screen.getByText(/Please select a deck/i)).toBeInTheDocument();
    });
  });

  test('submits form successfully', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Test Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation((url) => {
      if (url === '/api/deck') {
        return Promise.resolve({
          ok: true,
          json: () => Promise.resolve(mockDecks)
        });
      }
      if (url === '/api/event') {
        return Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ id: 123 })
        });
      }
      return Promise.resolve({ ok: false });
    });

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      const nameInput = screen.getByLabelText(/Event Name/i);
      fireEvent.change(nameInput, { target: { value: 'Test Event' } });
    });

    await waitFor(() => {
      const deckSelect = screen.getByLabelText(/Deck/i);
      fireEvent.change(deckSelect, { target: { value: '1' } });
    });

    await waitFor(() => {
      const submitButton = screen.getByText(/Create Event/i);
      fireEvent.click(submitButton);
    });

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledWith('/api/event', expect.objectContaining({
        method: 'POST',
        body: expect.stringContaining('Test Event')
      }));
    });
  });

  test('renders cancel button', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <EventForm />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Cancel/i)).toBeInTheDocument();
    });
  });
});
