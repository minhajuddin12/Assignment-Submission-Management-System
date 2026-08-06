import { ReactNode } from "react";
import { LucideIcon } from "lucide-react";

interface CardProps {
  children: ReactNode;
  className?: string;
  title?: string;
  eyebrow?: string;
  icon?: LucideIcon;
}

export function Card({ children, className = "", title, eyebrow, icon: Icon }: CardProps) {
  return (
    <div className={`bg-paper-raised border border-slate-light rounded-xl p-6 card-shadow ${className}`}>
      {(title || eyebrow) && (
        <div className="flex items-center gap-3 mb-5">
          {Icon && (
            <div className="w-9 h-9 rounded-lg bg-indigo-soft flex items-center justify-center shrink-0">
              <Icon size={18} className="text-indigo" />
            </div>
          )}
          <div>
            {eyebrow && <p className="eyebrow">{eyebrow}</p>}
            {title && <h2 className="font-semibold text-ink text-base">{title}</h2>}
          </div>
        </div>
      )}
      {children}
    </div>
  );
}