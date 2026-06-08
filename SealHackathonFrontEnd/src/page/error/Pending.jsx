import { Clock, Zap, ArrowLeft } from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function Pending() {
  const navigate = useNavigate();

  return (
    <div className="grid-bg relative flex min-h-screen flex-col items-center justify-center px-6 py-10">
      {/* Subtle ambient glow */}
      <div
        className="pointer-events-none absolute left-1/2 top-1/3 -translate-x-1/2 -translate-y-1/2"
        style={{
          width: 480,
          height: 480,
          borderRadius: "50%",
          background:
            "radial-gradient(circle, rgba(242,111,33,0.06) 0%, transparent 70%)",
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
          {/* Status icon */}
          <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-2xl bg-amber-500/10 ring-1 ring-amber-500/20">
            <Clock className="h-8 w-8 text-amber-400" />
          </div>

          {/* Title */}
          <h1
            className="mb-3 text-2xl font-bold text-white sm:text-3xl"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            Account Pending Approval
          </h1>

          {/* Description */}
          <p className="mx-auto mb-2 max-w-sm text-sm leading-relaxed text-slate-400">
            Your registration has been received and is currently under review by
            our coordination team.
          </p>

          <p className="mx-auto mb-8 max-w-sm text-sm leading-relaxed text-slate-500">
            This process typically takes 1–2 business days. You'll receive an
            email notification once your account has been approved.
          </p>

          {/* Status indicator */}
          <div className="mx-auto mb-8 flex max-w-xs items-center gap-3 rounded-xl border border-amber-500/15 bg-amber-500/5 px-4 py-3">
            <div className="relative flex h-2.5 w-2.5 flex-shrink-0">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-amber-400 opacity-50" />
              <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-amber-400" />
            </div>
            <span className="text-xs font-medium text-amber-300/90">
              Review in progress — please check back later
            </span>
          </div>

          {/* Action */}
          <button
            onClick={() => navigate("/")}
            className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-[#F26F21] px-6 py-3 text-sm font-semibold text-white transition-all duration-200 hover:bg-[#d9610e] hover:shadow-lg hover:shadow-orange-500/20 active:scale-[0.98]"
          >
            <ArrowLeft className="h-4 w-4" />
            Return to Home
          </button>
        </div>

        {/* Footer note */}
        <p className="mt-8 text-xs text-slate-600">
          Need help?{" "}
          <a
            href="mailto:support@fpt.edu.vn"
            className="text-[#F26F21]/70 transition-colors hover:text-[#F26F21]"
          >
            Contact support
          </a>
        </p>
      </div>
    </div>
  );
}
