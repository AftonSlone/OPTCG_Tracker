import React from 'react';
import { render, screen } from '@testing-library/react';
import Login from './Login';

describe('Login Component', () => {
  test('renders login heading', () => {
    render(<Login />);
    const heading = screen.getByText(/Welcome to OPTCG Tracker/i);
    expect(heading).toBeInTheDocument();
  });

  test('renders sign in message', () => {
    render(<Login />);
    const message = screen.getByText(/Sign in with your preferred account/i);
    expect(message).toBeInTheDocument();
  });

  test('renders Google login button', () => {
    render(<Login />);
    const googleButton = screen.getByText(/Login with Google/i);
    expect(googleButton).toBeInTheDocument();
  });

  test('renders Microsoft login button', () => {
    render(<Login />);
    const microsoftButton = screen.getByText(/Login with Microsoft/i);
    expect(microsoftButton).toBeInTheDocument();
  });

  test('renders Discord login button', () => {
    render(<Login />);
    const discordButton = screen.getByText(/Login with Discord/i);
    expect(discordButton).toBeInTheDocument();
  });

  test('renders Twitch login button', () => {
    render(<Login />);
    const twitchButton = screen.getByText(/Login with Twitch/i);
    expect(twitchButton).toBeInTheDocument();
  });

  test('renders OAuth login links with correct hrefs', () => {
    render(<Login />);
    
    const googleLink = screen.getByText(/Login with Google/i).closest('a');
    expect(googleLink).toHaveAttribute('href', '/api/auth/login/Google');

    const microsoftLink = screen.getByText(/Login with Microsoft/i).closest('a');
    expect(microsoftLink).toHaveAttribute('href', '/api/auth/login/Microsoft');

    const discordLink = screen.getByText(/Login with Discord/i).closest('a');
    expect(discordLink).toHaveAttribute('href', '/api/auth/login/Discord');

    const twitchLink = screen.getByText(/Login with Twitch/i).closest('a');
    expect(twitchLink).toHaveAttribute('href', '/api/auth/login/Twitch');
  });
});
