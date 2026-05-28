import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import DeckList from './DeckList';

// Mock fetch
global.fetch = jest.fn();

describe('DeckList Component', () => {
  beforeEach(() => {
    localStorage.clear();
    fetch.mockClear();
  });

  test('renders My Decks heading', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/My Decks/i)).toBeInTheDocument();
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
        <DeckList />
      </BrowserRouter>
    );

    expect(screen.getByText(/Loading decks.../i)).toBeInTheDocument();
  });

  test('renders empty state when no decks', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/You don't have any decks yet/i)).toBeInTheDocument();
    });
  });

  test('renders deck list when decks loaded', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    const mockDecks = [
      { id: 1, name: 'Red Deck', createdDate: '2024-01-01T00:00:00Z', lastModified: '2024-01-02T00:00:00Z' },
      { id: 2, name: 'Blue Deck', createdDate: '2024-01-03T00:00:00Z', lastModified: null }
    ];
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve(mockDecks)
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Red Deck/i)).toBeInTheDocument();
      expect(screen.getByText(/Blue Deck/i)).toBeInTheDocument();
    });
  });

  test('renders create deck button', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/\+ Create Deck/i)).toBeInTheDocument();
    });
  });

  test('opens form when create button clicked', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    const createButton = await screen.findByText(/\+ Create Deck/i);
    fireEvent.click(createButton);

    await waitFor(() => {
      expect(screen.getByText(/Create New Deck/i)).toBeInTheDocument();
    });
  });

  test('renders edit and delete buttons for each deck', async () => {
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
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      const editButtons = screen.getAllByTitle(/Edit deck/i);
      const deleteButtons = screen.getAllByTitle(/Delete deck/i);
      expect(editButtons.length).toBe(1);
      expect(deleteButtons.length).toBe(1);
    });
  });

  test('opens edit form when edit button clicked', async () => {
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
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      const editButton = screen.getByTitle(/Edit deck/i);
      fireEvent.click(editButton);
    });

    await waitFor(() => {
      expect(screen.getByText(/Edit Deck/i)).toBeInTheDocument();
    });
  });

  test('opens delete confirmation when delete button clicked', async () => {
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
        <DeckList />
      </BrowserRouter>
    );

    await waitFor(() => {
      const deleteButton = screen.getByTitle(/Delete deck/i);
      fireEvent.click(deleteButton);
    });

    await waitFor(() => {
      expect(screen.getByText(/Delete Deck/i)).toBeInTheDocument();
      expect(screen.getByText(/Are you sure you want to delete/i)).toBeInTheDocument();
    });
  });

  test('closes form when cancel button clicked', async () => {
    localStorage.setItem('jwtToken', 'test-token');
    fetch.mockImplementation(() => Promise.resolve({
      ok: true,
      json: () => Promise.resolve([])
    }));

    render(
      <BrowserRouter>
        <DeckList />
      </BrowserRouter>
    );

    const createButton = await screen.findByText(/\+ Create Deck/i);
    fireEvent.click(createButton);

    await waitFor(() => {
      expect(screen.getByText(/Create New Deck/i)).toBeInTheDocument();
    });

    const cancelButton = screen.getByText(/Cancel/i);
    fireEvent.click(cancelButton);

    await waitFor(() => {
      expect(screen.queryByText(/Create New Deck/i)).not.toBeInTheDocument();
    });
  });
});
