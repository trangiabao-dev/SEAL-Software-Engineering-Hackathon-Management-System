import { XCircle, Zap, ArrowLeft, Mail } from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function Decline() {
  const navigate = useNavigate();

  // In a real implementation, rejection reason could come from
  // Redux store or route state. Keeping a placeholder for now.
  const rejectionReason = null;

  return (
    <div className="grid-bg relative flex min-h-screen flex-col items-center justify-center px-6 py-12">
      {/* Subtle ambient glow */}
      <div
        className="pointer-events-none absolute left-1/2 top-1/3 -translate-x-1/2 -translate-y-1/2"
        style={{
          width: 480,
          height: 480,
          borderRadius: "50%",
          background:
            "radial-gradient(circle, rgba(220,38,38,0.04) 0%, transparent 70%)",
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
          <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-2xl bg-red-500/10 ring-1 ring-red-500/20">
            <XCircle className="h-8 w-8 text-red-400" />
          </div>

          {/* Title */}
          <h1
            className="mb-3 text-2xl font-bold text-white sm:text-3xl"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            Registration Declined
          </h1>

          {/* Description */}
          <p className="mx-auto mb-6 max-w-sm text-sm leading-relaxed text-slate-400">
            Unfortunately, your account registration has not been approved by
            our coordination team at this time.
          </p>

          {/* Rejection reason (if provided) */}
          {rejectionReason && (
            <div className="mx-auto mb-6 max-w-sm rounded-xl border border-red-500/15 bg-red-500/5 px-5 py-4 text-left">
              <p className="mb-1.5 text-xs font-semibold uppercase tracking-wider text-red-400/80">
                Reason
              </p>
              <p className="text-sm leading-relaxed text-slate-300">
                {rejectionReason}
              </p>
            </div>
          )}

          {/* Info box */}
          <div className="mx-auto mb-8 max-w-sm rounded-xl border border-white/[0.06] bg-white/[0.02] px-5 py-4 text-left">
            <p className="text-xs leading-relaxed text-slate-500">
              If you believe this was a mistake, or if you'd like further
              clarification, please reach out to the support team. We're happy
              to assist you with the registration process.
            </p>
          </div>

          {/* Actions */}
          <div className="flex flex-col gap-3">
            <button
              onClick={() => navigate("/")}
              className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-[#F26F21] px-6 py-3 text-sm font-semibold text-white transition-all duration-200 hover:bg-[#d9610e] hover:shadow-lg hover:shadow-orange-500/20 active:scale-[0.98]"
            >
              <ArrowLeft className="h-4 w-4" />
              Return to Home
            </button>
            <a
              href="mailto:support@fpt.edu.vn"
              className="inline-flex w-full items-center justify-center gap-2 rounded-xl border border-white/[0.08] bg-white/[0.03] px-6 py-3 text-sm font-semibold text-slate-300 transition-all duration-200 hover:border-white/[0.15] hover:bg-white/[0.06] hover:text-white active:scale-[0.98]"
            >
              <Mail className="h-4 w-4" />
              Contact Support
            </a>
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
