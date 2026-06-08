import React, { useState, useEffect } from 'react';
import { Menu, X, Zap } from 'lucide-react';
import { AuthButtons } from './AuthButtons';

export default function Navbar({ onLoginClick, onRegisterClick }) {
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 40);
    window.addEventListener('scroll', onScroll);
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  const links = ['Home', 'Timeline', 'Prizes', 'FAQ'];

  const handleLogin = () => {
    setMenuOpen(false);
    onLoginClick?.();
  };

  const handleRegister = () => {
    setMenuOpen(false);
    onRegisterClick?.();
  };

  return (
    <nav
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        scrolled
          ? 'bg-[#080A0F]/80 backdrop-blur-md border-b border-white/[0.06] shadow-[0_4px_30px_rgba(0,0,0,0.4)]'
          : 'bg-transparent border-b border-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
        {/* Logo */}
        <a href="#home" className="flex items-center gap-2.5 group">
          <div className="w-8 h-8 rounded-lg bg-[#F26F21] flex items-center justify-center transition-transform duration-300 group-hover:scale-105">
            <Zap className="w-4 h-4 text-white" />
          </div>
          <span
            className="text-white font-extrabold text-lg tracking-wider"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            FPTU <span className="text-[#F26F21]">Hackathon</span>
          </span>
        </a>

        {/* Desktop links */}
        <div className="hidden md:flex items-center gap-8">
          {links.map((link) => (
            <a
              key={link}
              href={`#${link.toLowerCase()}`}
              className="text-slate-300 hover:text-white text-[13px] font-medium tracking-wide uppercase transition-colors duration-200 relative group"
            >
              {link}
              <span className="absolute -bottom-1.5 left-0 w-0 h-[1.5px] bg-[#F26F21] group-hover:w-full transition-all duration-200" />
            </a>
          ))}
          <AuthButtons
            onLoginClick={handleLogin}
            onRegisterClick={handleRegister}
          />
        </div>

        {/* Mobile toggle */}
        <button
          className="md:hidden text-slate-300 hover:text-white p-1"
          onClick={() => setMenuOpen(!menuOpen)}
          aria-label="Toggle menu"
        >
          {menuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
        </button>
      </div>

      {/* Mobile menu */}
      {menuOpen && (
        <div className="md:hidden bg-[#0A0D14]/98 backdrop-blur-lg border-t border-white/[0.06] px-6 py-6 flex flex-col gap-5 shadow-2xl fade-in-up">
          {links.map((link) => (
            <a
              key={link}
              href={`#${link.toLowerCase()}`}
              onClick={() => setMenuOpen(false)}
              className="text-slate-300 hover:text-white text-sm font-semibold tracking-wide uppercase transition-colors py-1"
            >
              {link}
            </a>
          ))}
          <div className="h-px bg-white/[0.06] my-2" />
          <AuthButtons
            onLoginClick={handleLogin}
            onRegisterClick={handleRegister}
            fullWidth
            className="flex-col"
            loginClassName="text-center"
            registerClassName="text-center"
          />
        </div>
      )}
    </nav>
  );
}
