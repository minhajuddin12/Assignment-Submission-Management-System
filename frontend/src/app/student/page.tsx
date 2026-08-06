"use client";

import { useEffect, useState } from "react";
import { DashboardLayout } from "@/components/DashboardLayout";
import { Card } from "@/components/ui/Card";
import { StatCard } from "@/components/ui/StatCard";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { api } from "@/lib/api";
import { LayoutGrid, ClipboardList, CheckCircle2, Clock } from "lucide-react";
import type { Assignment, Submission } from "@/types";

const navItems = [{ label: "Overview", href: "/student", icon: LayoutGrid }];

export default function StudentPage() {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [submissionStatuses, setSubmissionStatuses] = useState<Record<number, string>>({}); // NEW
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<Assignment[]>("/Assignments").then(async (data) => {
      setAssignments(data);

      const entries = await Promise.all(
        data.map(async (a) => {
          try {
            const sub = await api.get<Submission>(`/assignments/${a.id}/Submissions/mine`);
            return [a.id, sub.status] as const;
          } catch {
            return null;
          }
        })
      );
      const statusMap: Record<number, string> = {};
      entries.forEach((e) => { if (e) statusMap[e[0]] = e[1]; });
      setSubmissionStatuses(statusMap);

      setLoading(false);
    });
  }, []);

  const now = new Date();
  const upcoming = assignments.filter((a) => new Date(a.deadline) > now).length;
  const overdue = assignments.filter((a) => new Date(a.deadline) <= now).length;

  return (
    <DashboardLayout requiredRole="Student" navItems={navItems}>
      <div className="mb-8">
        <p className="eyebrow mb-1">Dashboard</p>
        <h1 className="text-3xl font-semibold text-ink">Your assignments</h1>
      </div>

      {loading ? (
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
            <StatCard label="Total assignments" value={assignments.length} icon={ClipboardList} />
            <StatCard label="Upcoming" value={upcoming} icon={Clock} />
            <StatCard label="Past deadline" value={overdue} icon={CheckCircle2} />
          </div>

          <Card title="Assignments" eyebrow="All" icon={ClipboardList}>
            <div className="flex flex-col">
              {assignments.map((a) => (
                <a key={a.id} href={`/student/assignments/${a.id}`} className="flex items-center justify-between py-3 border-b border-slate-light last:border-0 hover:bg-paper -mx-2 px-2 rounded-md transition-colors">
                  <div>
                    <p className="text-sm font-medium text-ink">{a.title}</p>
                    <p className="text-xs text-slate mt-0.5">{a.subjectName} · Due {new Date(a.deadline).toLocaleString()}</p>
                  </div>
                  <StatusBadge status={submissionStatuses[a.id] ?? a.status} />
                </a>
              ))}
              {assignments.length === 0 && (
                <p className="text-sm text-slate py-4">No assignments yet. Check back once your teacher publishes one.</p>
              )}
            </div>
          </Card>
        </>
      )}
    </DashboardLayout>
  );
}