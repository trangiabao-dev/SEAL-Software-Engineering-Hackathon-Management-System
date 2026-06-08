export function CoordinatorPageTitle({ title, sub }) {
  return (
    <div
      className="border-b bg-white px-4 py-6 sm:px-8"
      style={{ borderColor: "#E5E7EB" }}
    >
      <div className="flex items-start gap-4">
        <div
          className="mt-1 h-12 w-1 rounded-full"
          style={{ background: "#F26F21" }}
        />
        <div>
          <h2
            className="text-2xl font-bold tracking-tight text-slate-900"
            style={{ fontFamily: "'Montserrat', 'Inter', sans-serif" }}
          >
            {title}
          </h2>
          <p className="mt-1 text-sm text-slate-500">{sub}</p>
        </div>
      </div>
    </div>
  );
}
