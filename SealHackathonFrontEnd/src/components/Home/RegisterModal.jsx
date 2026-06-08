import { useState } from "react";
import Modal from "./Modal";
import PasswordInput from "./PasswordInput";
import authService from "../../services/authService";

const inputClass =
  "w-full px-4 py-2 rounded-lg bg-[#0F121E] border border-white/[0.18] text-white text-sm placeholder:text-slate-400 focus:outline-none focus:border-[#F26F21] focus:ring-2 focus:ring-[#F26F21]/30 hover:border-white/[0.3] transition-all";

const labelClass =
  "block text-[10px] font-bold text-slate-300 uppercase tracking-widest mb-1.5";

const initialState = () => ({
  leaderName: "",
  leaderEmail: "",
  leaderPassword: "",
});

export default function RegisterModal({ open, onClose }) {
  const [form, setForm] = useState(initialState);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const resetAndClose = () => {
    setForm(initialState());
    onClose();
  };

  const updateLeader = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      await authService.register({
        username: form.leaderName,
        email: form.leaderEmail,
        password: form.leaderPassword,
      });
      setSuccess(true);
    } catch (err) {
      const status = err.response?.status;
      const msg = err.response?.data?.message;

      if (status === 409)
        setError(msg || "Email hoặc username đã được sử dụng.");
      else setError("Đã có lỗi xảy ra. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      open={open}
      onClose={resetAndClose}
      title="Register"
      subtitle="Register your team for FPT Hackathon 2026"
      maxWidth="max-w-xl"
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Team leader */}
        <section className="space-y-4">
          <h3
            className="text-xs font-bold text-[#F26F21] uppercase tracking-widest pb-1.5 border-b border-[#F26F21]/10"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            Team Leader
          </h3>
          <div className="space-y-3.5">
            <div>
              <label htmlFor="leader-name" className={labelClass}>
                Full Name
              </label>
              <input
                id="leader-name"
                type="text"
                required
                placeholder="John Doe"
                value={form.leaderName}
                onChange={(e) => updateLeader("leaderName", e.target.value)}
                className={inputClass}
              />
            </div>
            <div>
              <label htmlFor="leader-email" className={labelClass}>
                Gmail
              </label>
              <input
                id="leader-email"
                type="email"
                required
                autoComplete="email"
                placeholder="example@gmail.com"
                value={form.leaderEmail}
                onChange={(e) => updateLeader("leaderEmail", e.target.value)}
                className={inputClass}
              />
            </div>
            <PasswordInput
              id="leader-password"
              label="Password"
              value={form.leaderPassword}
              onChange={(e) => updateLeader("leaderPassword", e.target.value)}
              autoComplete="new-password"
            />
          </div>
        </section>

        {error && <p className="text-red-400 text-xs">{error}</p>}
        {success ? (
          <div className="w-full px-5 py-3 rounded-lg bg-green-500/10 border border-green-500/30 text-green-400 text-sm text-center">
            Đăng ký thành công! Vui lòng mở Gmail để xác thực tài khoản.
          </div>
        ) : (
          <button
            type="submit"
            disabled={loading}
            className="w-full px-5 py-3 bg-[#F26F21] text-white text-sm font-bold tracking-wider uppercase rounded-lg glow-orange hover:bg-[#e05a10] transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? "Đang đăng ký..." : "Register"}
          </button>
        )}
      </form>
    </Modal>
  );
}
