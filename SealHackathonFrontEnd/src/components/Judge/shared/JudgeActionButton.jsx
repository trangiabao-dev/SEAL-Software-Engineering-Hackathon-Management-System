export function JudgeActionButton({ variant = "secondary", disabled = false, children, icon: Icon, className = "", ...props }) {
  const variants = {
    primary: "border-orange-600 bg-gradient-to-r from-[#F26F21] to-[#c9520e] text-white hover:shadow-lg",
    secondary: "border-slate-300 bg-white text-slate-700 hover:border-orange-400 hover:bg-orange-50 hover:text-orange-700",
    ghost: "border-transparent bg-transparent text-slate-600 hover:bg-slate-100",
    danger: "border-red-300 bg-red-50 text-red-700 hover:bg-red-100",
  };

  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-xl border px-3.5 py-2 text-sm font-semibold transition-all disabled:cursor-not-allowed disabled:opacity-50 ${variants[variant] || variants.secondary} ${className}`}
      disabled={disabled}
      {...props}
    >
      {Icon && <Icon className="h-4 w-4" />}
      {children}
    </button>
  );
}
