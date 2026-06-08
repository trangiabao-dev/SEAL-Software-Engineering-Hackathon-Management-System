import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import authService from "../services/authService";

export default function VerifyEmail() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState("loading");
  const [message, setMessage] = useState("Đang xác thực email...");

  useEffect(() => {
    const token = searchParams.get("token");
    if (!token) {
      setStatus("error");
      setMessage("Thiếu token xác nhận.");
      return;
    }

    authService
      .verifyEmail(token)
      .then((res) => {
        const payload = res?.data;
        if (payload?.success) {
          setStatus("success");
          setMessage(
            payload?.message ||
              "Xác nhận email thành công. Bạn có thể đăng nhập ngay.",
          );
          return;
        }
        setStatus("error");
        setMessage(payload?.message || "Xác nhận email thất bại.");
      })
      .catch(() => {
        setStatus("error");
        setMessage("Không kết nối được server. Kiểm tra BE đang chạy.");
      });
  }, [navigate, searchParams]);

  return (
    <div className="min-h-screen bg-[#080A0F] text-white flex items-center justify-center px-6">
      <div className="w-full max-w-md rounded-2xl border border-white/10 bg-white/[0.04] p-8 text-center shadow-[0_30px_80px_rgba(0,0,0,0.55)]">
        <h1 className="text-xl font-semibold">Xác nhận email</h1>
        <p className="mt-4 text-sm text-slate-300">{message}</p>
        {status !== "loading" && (
          <div className="mt-6">
            <button
              type="button"
              onClick={() => navigate("/login")}
              className="w-full px-5 py-3 bg-[#F26F21] text-white text-sm font-bold tracking-wider uppercase rounded-lg glow-orange hover:bg-[#e05a10] transition-all duration-200"
            >
              {status === "success" ? "Đăng nhập ngay" : "Quay lại trang chủ"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
