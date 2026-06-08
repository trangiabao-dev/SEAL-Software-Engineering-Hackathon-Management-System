import { useState, useRef } from "react";
import { CloudUpload, Link, CheckCircle, X, Github } from "lucide-react";

export function SubmissionZone() {
  const [tab, setTab] = useState("file");
  const [dragging, setDragging] = useState(false);
  const [files, setFiles] = useState([]);
  const [repoUrl, setRepoUrl] = useState("");
  const [submitted, setSubmitted] = useState(false);
  const fileRef = useRef();

  const handleDrop = (e) => {
    e.preventDefault();
    setDragging(false);
    const dropped = Array.from(e.dataTransfer.files);
    setFiles((prev) => [...prev, ...dropped]);
  };

  const handleSubmit = () => {
    if (
      (tab === "file" && files.length > 0) ||
      (tab === "repo" && repoUrl.trim())
    ) {
      setSubmitted(true);
      setTimeout(() => setSubmitted(false), 3000);
    }
    console.log("Submitted:", { files, repoUrl });
  };

  return (
    <div
      className="rounded-2xl p-6 transition-all duration-300"
      style={{
        background: "#FFFFFF",
        border: "1px solid #E5E7EB",
        boxShadow: "0 10px 30px rgba(0,0,0,0.02)",
      }}
    >
      <div className="flex items-center gap-3 mb-5">
        <div
          className="w-8 h-8 rounded-lg flex items-center justify-center"
          style={{
            background: "rgba(242,111,33,0.1)",
            border: "1px solid rgba(242,111,33,0.2)",
          }}
        >
          <CloudUpload className="w-4 h-4" style={{ color: "#F26F21" }} />
        </div>
        <div>
          <h3 className="text-sm font-bold text-[#111827]">Quick Submission</h3>
          <p className="text-[11px] text-slate-500">
            Upload files or link your repository
          </p>
        </div>
      </div>

      {/* Tabs */}
      <div
        className="flex gap-1 mb-4 p-1 rounded-xl"
        style={{ background: "#F3F4F6" }}
      >
        {[
          ["file", "File Upload"],
          ["repo", "Repository"],
        ].map(([id, label]) => (
          <button
            key={id}
            onClick={() => setTab(id)}
            className="flex-1 py-2 rounded-lg text-xs font-semibold transition-all duration-200"
            style={{
              background: tab === id ? "#F26F21" : "transparent",
              color: tab === id ? "#FFFFFF" : "#6b7280",
              boxShadow:
                tab === id ? "0 2px 8px rgba(242,111,33,0.15)" : "none",
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === "file" ? (
        <div>
          <div
            className="rounded-xl p-6 text-center cursor-pointer transition-all duration-200 mb-3"
            style={{
              border: `2px dashed ${dragging ? "#F26F21" : "#D1D5DB"}`,
              background: dragging ? "#FFF6F0" : "#FAFAFA",
            }}
            onDragOver={(e) => {
              e.preventDefault();
              setDragging(true);
            }}
            onDragLeave={() => setDragging(false)}
            onDrop={handleDrop}
            onClick={() => fileRef.current?.click()}
          >
            <input
              ref={fileRef}
              type="file"
              multiple
              className="hidden"
              onChange={(e) =>
                setFiles((prev) => [
                  ...prev,
                  ...Array.from(e.target.files || []),
                ])
              }
            />
            <CloudUpload
              className="w-8 h-8 mx-auto mb-2"
              style={{ color: dragging ? "#F26F21" : "#6b7280" }}
            />
            <p className="text-xs font-semibold text-[#111827]">
              {dragging ? "Drop files here" : "Drag & drop or click to upload"}
            </p>
            <p className="text-[10px] text-slate-500 mt-1">
              ZIP, PDF, or any project files
            </p>
          </div>
          {files.length > 0 && (
            <div className="space-y-1.5 mb-3">
              {files.map((f, i) => (
                <div
                  key={i}
                  className="flex items-center gap-2 px-3 py-2 rounded-lg text-xs"
                  style={{ background: "#F9FAFB", border: "1px solid #E5E7EB" }}
                >
                  <CheckCircle className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0" />
                  <span className="text-slate-700 truncate flex-1">
                    {f.name}
                  </span>
                  <button
                    onClick={() =>
                      setFiles((prev) => prev.filter((_, j) => j !== i))
                    }
                  >
                    <X className="w-3 h-3 text-slate-400 hover:text-red-500 transition-colors" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      ) : (
        <div className="mb-3">
          <div
            className="flex items-center gap-2 px-3 py-2.5 rounded-xl mb-2 border border-slate-300 hover:border-slate-400 focus-within:border-[#F26F21] focus-within:ring-2 focus-within:ring-[#F26F21]/18 transition-all duration-200"
            style={{ background: "#FFFFFF" }}
          >
            <Github className="w-4 h-4 text-slate-500 flex-shrink-0" />
            <input
              type="text"
              value={repoUrl}
              onChange={(e) => setRepoUrl(e.target.value)}
              placeholder="https://github.com/team-alpha/project"
              className="flex-1 bg-transparent text-sm text-[#0F172A] placeholder-slate-500 outline-none"
            />
          </div>
          <div
            className="flex items-center gap-2 px-3 py-2.5 rounded-xl border border-slate-300 hover:border-slate-400 focus-within:border-[#F26F21] focus-within:ring-2 focus-within:ring-[#F26F21]/18 transition-all duration-200"
            style={{ background: "#FFFFFF" }}
          >
            <Link className="w-4 h-4 text-slate-500 flex-shrink-0" />
            <input
              type="text"
              placeholder="Demo URL (optional)"
              className="flex-1 bg-transparent text-sm text-[#0F172A] placeholder-slate-500 outline-none"
            />
          </div>
        </div>
      )}

      <button
        onClick={handleSubmit}
        className="w-full py-3 rounded-xl text-sm font-bold transition-all duration-200"
        style={{
          background: submitted
            ? "#D1FAE5"
            : "linear-gradient(135deg, #F26F21, #c9520e)",
          color: submitted ? "#065F46" : "#fff",
          border: submitted ? "1px solid #A7F3D0" : "none",
          boxShadow: submitted ? "none" : "0 4px 14px rgba(242,111,33,0.2)",
        }}
        onMouseEnter={(e) => {
          if (!submitted)
            e.currentTarget.style.boxShadow =
              "0 6px 20px rgba(242,111,33,0.35)";
        }}
        onMouseLeave={(e) => {
          if (!submitted)
            e.currentTarget.style.boxShadow = "0 4px 14px rgba(242,111,33,0.2)";
        }}
      >
        {submitted ? (
          <span className="flex items-center justify-center gap-2">
            <CheckCircle className="w-4 h-4" /> Submitted Successfully!
          </span>
        ) : (
          "Submit Project"
        )}
      </button>
    </div>
  );
}
