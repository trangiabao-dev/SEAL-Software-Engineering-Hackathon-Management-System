import { Zap, ArrowLeft, Search } from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <div className="grid-bg relative flex min-h-screen flex-col items-center justify-center px-6 py-16">
      {/* Subtle ambient glow */}
      <div
        className="pointer-events-none absolute left-1/2 top-1/3 -translate-x-1/2 -translate-y-1/2"
        style={{
          width: 480,
          height: 480,
          borderRadius: "50%",
          background:
            "radial-gradient(circle, rgba(242,111,33,0.05) 0%, transparent 70%)",
        }}
      />

      <div className="fade-in-up relative z-10 flex w-full max-w-md flex-col items-center text-center">
        {/* Logo */}
        <a href="/" className="mb-10 flex items-center gap-2.5 group">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-[#F26F21] transition-transform duration-300 group-hover:scale-105">
            <Zap className="h-4 w-4 text-white" />
          </div>
          <span
            className="text-lg font-extrabold tracking-wider text-white"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            FPTU <span className="text-[#F26F21]">Hackathon</span>
          </span>
        </a>

        {/* Glass card */}
        <div className="glass w-full rounded-2xl px-8 py-10 sm:px-10">
          {/* 404 indicator */}
          <div className="mb-6">
            <p
              className="text-7xl font-extrabold tracking-tight text-white/[0.08] sm:text-8xl"
              style={{ fontFamily: "Montserrat, sans-serif" }}
            >
              404
            </p>
          </div>

          {/* Icon */}
          <div className="mx-auto mb-6 flex h-14 w-14 items-center justify-center rounded-2xl bg-[#F26F21]/10 ring-1 ring-[#F26F21]/20">
            <Search className="h-7 w-7 text-[#F26F21]" />
          </div>

          {/* Title */}
          <h1
            className="mb-3 text-2xl font-bold text-white sm:text-3xl"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            Page Not Found
          </h1>

          {/* Description */}
          <p className="mx-auto mb-8 max-w-sm text-sm leading-relaxed text-slate-400">
            The page you're looking for doesn't exist or has been moved. Please
            check the URL or navigate back to a known page.
          </p>

          {/* Actions */}
          <div className="flex flex-col gap-3 sm:flex-row">
            <button
              onClick={() => navigate(-1)}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-xl border border-white/[0.08] bg-white/[0.03] px-6 py-3 text-sm font-semibold text-slate-300 transition-all duration-200 hover:border-white/[0.15] hover:bg-white/[0.06] hover:text-white active:scale-[0.98]"
            >
              <ArrowLeft className="h-4 w-4" />
              Go Back
            </button>
            <button
              onClick={() => navigate("/")}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-xl bg-[#F26F21] px-6 py-3 text-sm font-semibold text-white transition-all duration-200 hover:bg-[#d9610e] hover:shadow-lg hover:shadow-orange-500/20 active:scale-[0.98]"
            >
              Return Home
            </button>
          </div>
        </div>

        {/* Footer note */}
        <p className="mt-8 text-xs text-slate-600">
          SEAL Hackathon Management System
        </p>
      </div>
    </div>
  );
}
