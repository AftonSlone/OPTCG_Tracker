import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Menu from './Menu';

describe('Menu Component', () => {
  const mockOnClose = jest.fn();
  const mockToggleDarkMode = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('does not render when isOpen is false', () => {
    render(
      <Menu 
        isOpen={false} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    expect(screen.queryByText('Menu')).not.toBeInTheDocument();
  });

  test('renders when isOpen is true', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    expect(screen.getByText('Menu')).toBeInTheDocument();
  });

  test('renders navigation items', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('My Decks')).toBeInTheDocument();
    expect(screen.getByText('Match History')).toBeInTheDocument();
    expect(screen.getByText('Leaderboard')).toBeInTheDocument();
    expect(screen.getByText('Card Database')).toBeInTheDocument();
    expect(screen.getByText('Tournaments')).toBeInTheDocument();
    expect(screen.getByText('Settings')).toBeInTheDocument();
    expect(screen.getByText('Help & Support')).toBeInTheDocument();
  });

  test('calls onClose when overlay is clicked', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    const overlay = screen.getByText('Menu').parentElement?.parentElement;
    if (overlay) {
      fireEvent.click(overlay);
      expect(mockOnClose).toHaveBeenCalled();
    }
  });

  test('calls onClose when close button is clicked', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    const closeButton = screen.getByRole('button', { name: /close/i }) || 
                        screen.getByText('Menu').parentElement?.querySelector('button');
    if (closeButton) {
      fireEvent.click(closeButton);
      expect(mockOnClose).toHaveBeenCalled();
    }
  });

  test('renders dark mode toggle button', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    expect(screen.getByText(/Switch to Dark Mode/i)).toBeInTheDocument();
  });

  test('renders light mode toggle when dark mode is active', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={true} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    expect(screen.getByText(/Switch to Light Mode/i)).toBeInTheDocument();
  });

  test('calls toggleDarkMode when dark mode button is clicked', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    const darkModeButton = screen.getByText(/Switch to Dark Mode/i);
    fireEvent.click(darkModeButton);

    expect(mockToggleDarkMode).toHaveBeenCalled();
  });

  test('renders dashboard link with correct href', () => {
    render(
      <Menu 
        isOpen={true} 
        onClose={mockOnClose} 
        darkMode={false} 
        toggleDarkMode={mockToggleDarkMode} 
      />
    );

    const dashboardLink = screen.getByText('Dashboard').closest('a');
    expect(dashboardLink).toHaveAttribute('href', '/dashboard');
  });
});
