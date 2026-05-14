import React from 'react';
import './Menu.css';

function Menu({ isOpen, onClose }) {
  return (
    <div className={`menu-overlay ${isOpen ? 'open' : ''}`} onClick={onClose}>
      <div className={`menu-panel ${isOpen ? 'open' : ''}`} onClick={(e) => e.stopPropagation()}>
        <div className="menu-header">
          <h2>Menu</h2>
          <button className="close-btn" onClick={onClose}>×</button>
        </div>
        <nav className="menu-nav">
          <a href="/dashboard" className="menu-link">Dashboard</a>
          <button className="menu-link">My Decks</button>
          <button className="menu-link">Match History</button>
          <button className="menu-link">Leaderboard</button>
          <button className="menu-link">Card Database</button>
          <button className="menu-link">Tournaments</button>
          <button className="menu-link">Settings</button>
          <button className="menu-link">Help & Support</button>
        </nav>
      </div>
    </div>
  );
}

export default Menu;
