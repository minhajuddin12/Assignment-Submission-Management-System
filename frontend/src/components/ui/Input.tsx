import { InputHTMLAttributes, forwardRef } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = "", id, ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={id} className="text-sm font-medium text-ink-soft">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={id}
          className={`px-3 py-2 rounded-md border border-slate-light bg-white text-ink placeholder:text-slate focus-visible:border-indigo ${className}`}
          {...props}
        />
        {error && <span className="text-sm text-red">{error}</span>}
      </div>
    );
  }
);
Input.displayName = "Input";