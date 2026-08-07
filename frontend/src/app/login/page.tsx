"use client";

import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Card } from "@/components/ui/Card";

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login(email, password);
    } catch {
      setError("Incorrect email or password. Check your details and try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-paper flex items-center justify-center px-4 relative overflow-hidden">
      {/* illustrated background scene */}
      <svg
        className="absolute inset-0 w-full h-full opacity-[0.09] pointer-events-none"
        viewBox="0 0 1200 800"
        preserveAspectRatio="xMidYMid slice"
        fill="none"
        stroke="#33415C"
        strokeWidth="1.5"
      >
        {/* graduation cap */}
        <g transform="translate(90,110)">
          <path d="M0 22 L55 4 L110 22 L55 40 Z" strokeLinejoin="round" />
          <path d="M27 30 L27 55 Q55 68 83 55 L83 30" strokeLinejoin="round" />
          <line x1="110" y1="22" x2="110" y2="52" />
          <circle cx="110" cy="56" r="3.5" fill="#33415C" stroke="none" />
        </g>

        {/* open book */}
        <g transform="translate(940,120)">
          <path d="M0 12 Q45 -10 90 12 L90 100 Q45 78 0 100 Z" />
          <path d="M90 12 Q135 -10 180 12 L180 100 Q135 78 90 100 Z" />
          <line x1="90" y1="12" x2="90" y2="100" />
        </g>

        {/* pencil */}
        <g transform="translate(120,600) rotate(-30)">
          <rect x="0" y="0" width="150" height="20" rx="3" />
          <path d="M150 0 L175 10 L150 20 Z" strokeLinejoin="round" />
        </g>

        {/* checkmark badge */}
        <g transform="translate(1020,560)">
          <circle cx="0" cy="0" r="30" />
          <path d="M-13 0 L-3 10 L15 -12" strokeLinecap="round" strokeLinejoin="round" />
        </g>

        {/* clipboard */}
        <g transform="translate(560,60)">
          <rect x="0" y="10" width="70" height="90" rx="6" />
          <rect x="20" y="0" width="30" height="16" rx="4" />
          <line x1="14" y1="40" x2="56" y2="40" />
          <line x1="14" y1="58" x2="56" y2="58" />
          <line x1="14" y1="76" x2="40" y2="76" />
        </g>

        {/* sparkle accents */}
        <g strokeWidth="1.2">
          <path d="M780 640 L780 660 M770 650 L790 650" strokeLinecap="round" />
          <path d="M260 260 L260 276 M252 268 L268 268" strokeLinecap="round" />
        </g>

        {/* faint constellation lines */}
        <path d="M200 150 Q450 300 590 130" strokeOpacity="0.5" strokeDasharray="3 6" />
        <path d="M980 200 Q950 400 1010 560" strokeOpacity="0.5" strokeDasharray="3 6" />
        <path d="M270 640 Q450 550 560 560" strokeOpacity="0.5" strokeDasharray="3 6" />
      </svg>

      {/* soft color glows for depth */}
      <div className="absolute -top-32 -left-24 w-96 h-96 rounded-full bg-indigo/10 blur-3xl pointer-events-none" />
      <div className="absolute -bottom-24 -right-16 w-96 h-96 rounded-full bg-green/10 blur-3xl pointer-events-none" />

      <div className="w-full max-w-sm relative z-10">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-semibold text-ink">Assignment System</h1>
          <p className="text-slate text-sm mt-1 font-mono-data">Sign in to continue</p>
        </div>

        <Card>
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <Input
              id="email"
              type="email"
              label="Email"
              placeholder="you@school.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <Input
              id="password"
              type="password"
              label="Password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />

            {error && (
              <div className="bg-red-soft text-red text-sm rounded-md px-3 py-2">
                {error}
              </div>
            )}

            <Button type="submit" disabled={loading} className="w-full mt-2">
              {loading ? "Signing in…" : "Sign in"}
            </Button>
          </form>
        </Card>
      </div>
    </main>
  );
}