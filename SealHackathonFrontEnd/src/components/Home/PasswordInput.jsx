import { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

const inputClass =
  "w-full px-4 py-2 rounded-lg bg-[#0F121E] border border-white/[0.18] text-white text-sm placeholder:text-slate-400 focus:outline-none focus:border-[#F26F21] focus:ring-2 focus:ring-[#F26F21]/30 hover:border-white/[0.3] transition-all";

export const authLabelClass =
  "block text-[10px] font-bold text-slate-300 uppercase tracking-widest mb-1.5";

export default function PasswordInput({
  id,
  label,
  value,
  onChange,
  placeholder = '••••••••',
  autoComplete = 'current-password',
  required = true,
}) {
  const [visible, setVisible] = useState(false);

  return (
    <div>
      <label htmlFor={id} className={authLabelClass}>
        {label}
      </label>
      <div className="relative">
        <input
          id={id}
          type={visible ? 'text' : 'password'}
          required={required}
          autoComplete={autoComplete}
          placeholder={placeholder}
          value={value}
          onChange={onChange}
          className={inputClass}
        />
        <button
          type="button"
          onClick={() => setVisible((v) => !v)}
          className="absolute right-3 top-1/2 -translate-y-1/2 p-1 text-slate-500 hover:text-slate-300 transition-colors"
          aria-label={visible ? 'Hide password' : 'Show password'}
        >
          {visible ? (
            <EyeOff className="w-4 h-4" />
          ) : (
            <Eye className="w-4 h-4" />
          )}
        </button>
      </div>
    </div>
  );
}
