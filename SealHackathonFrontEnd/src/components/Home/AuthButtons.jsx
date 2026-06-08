import React from 'react';

const btnBase =
  'px-5 py-2 text-sm font-semibold tracking-wide rounded-lg transition-all duration-250 flex items-center justify-center';

export function AuthButtons({
  onLoginClick,
  onRegisterClick,
  className = '',
  registerClassName = '',
  loginClassName = '',
  fullWidth = false,
}) {
  const width = fullWidth ? 'w-full' : '';

  return (
    <div className={`flex items-center gap-3.5 ${className}`}>
      <button
        type="button"
        onClick={onLoginClick}
        className={`${btnBase} ${width} border border-white/10 text-slate-300 hover:text-white hover:bg-white/5 hover:border-white/20 active:scale-[0.98] ${loginClassName}`}
      >
        Login
      </button>
      <button
        type="button"
        onClick={onRegisterClick}
        className={`${btnBase} ${width} bg-[#F26F21] text-white hover:bg-[#e05811] shadow-[0_1px_2px_rgba(0,0,0,0.05)] hover:shadow-[0_4px_12px_rgba(242,111,33,0.15)] active:scale-[0.98] ${registerClassName}`}
      >
        Register
      </button>
    </div>
  );
}
