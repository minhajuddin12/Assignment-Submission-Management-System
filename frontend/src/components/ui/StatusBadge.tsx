const styles: Record<string, string> = {
  Draft: "bg-slate-light text-ink-soft",
  Published: "bg-indigo-soft text-indigo",
  Submitted: "bg-indigo-soft text-indigo",
  Late: "bg-amber-soft text-amber",
  Graded: "bg-green-soft text-green",
  ReturnedForRevision: "bg-red-soft text-red",
};

export function StatusBadge({ status }: { status: string }) {
  return (
    <span
      className={`inline-block px-2.5 py-1 rounded-full text-xs font-medium font-mono-data ${
        styles[status] || "bg-slate-light text-ink-soft"
      }`}
    >
      {status}
    </span>
  );
}