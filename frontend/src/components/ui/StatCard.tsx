import { LucideIcon } from "lucide-react";

export function StatCard({
  label,
  value,
  icon: Icon,
}: {
  label: string;
  value: number | string;
  icon: LucideIcon;
}) {
  return (
    <div className="bg-paper-raised border border-slate-light rounded-xl p-5 card-shadow flex items-center gap-4">
      <div className="w-11 h-11 rounded-lg bg-indigo-soft flex items-center justify-center shrink-0">
        <Icon size={20} className="text-indigo" />
      </div>
      <div>
        <p className="text-2xl font-semibold text-ink font-mono-data leading-none">{value}</p>
        <p className="text-xs text-slate mt-1">{label}</p>
      </div>
    </div>
  );
}