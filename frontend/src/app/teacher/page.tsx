"use client";

import { useEffect, useState } from "react";
import { DashboardLayout } from "@/components/DashboardLayout";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { StatCard } from "@/components/ui/StatCard";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { api } from "@/lib/api";
import { LayoutGrid, ClipboardList, CheckCircle2, FileClock, Plus } from "lucide-react";
import type { Assignment, Submission } from "@/types";

interface TeacherSubject {
  id: number;
  name: string;
  classRoomId: number;
}

const navItems = [{ label: "Overview", href: "/teacher", icon: LayoutGrid }];

export default function TeacherPage() {
  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [subjects, setSubjects] = useState<TeacherSubject[]>([]);
  const [submissionSummaries, setSubmissionSummaries] = useState<Record<number, string>>({}); // NEW
  const [loading, setLoading] = useState(true);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [deadline, setDeadline] = useState("");
  const [maxMarks, setMaxMarks] = useState("100");
  const [subjectId, setSubjectId] = useState("");
  const [publishNow, setPublishNow] = useState(true);
  const [creating, setCreating] = useState(false);
  const [formError, setFormError] = useState("");

  async function loadAll() {
    const [assignmentData, subjectData] = await Promise.all([
      api.get<Assignment[]>("/Assignments"),
      api.get<TeacherSubject[]>("/Teacher/subjects"),
    ]);
    setAssignments(assignmentData);
    setSubjects(subjectData);

    // NEW: fetch submission progress for each assignment
    const entries = await Promise.all(
      assignmentData.map(async (a) => {
        try {
          const subs = await api.get<Submission[]>(`/assignments/${a.id}/Submissions`);
          const graded = subs.filter((s) => s.status === "Graded").length;
          return [a.id, `${subs.length} submitted · ${graded} graded`] as const;
        } catch {
          return null;
        }
      })
    );
    const summaryMap: Record<number, string> = {};
    entries.forEach((e) => { if (e) summaryMap[e[0]] = e[1]; });
    setSubmissionSummaries(summaryMap);

    setLoading(false);
  }

  useEffect(() => {
    loadAll();
  }, []);

  async function handleCreateAssignment(e: React.FormEvent) {
    e.preventDefault();
    setFormError("");

    if (!title.trim() || !description.trim() || !deadline || !subjectId) {
      setFormError("Fill in every field before creating the assignment.");
      return;
    }

    setCreating(true);
    try {
      await api.post("/Assignments", {
        title,
        description,
        deadline: new Date(deadline).toISOString(),
        maxMarks: Number(maxMarks),
        subjectId: Number(subjectId),
        publishNow,
      });
      setTitle("");
      setDescription("");
      setDeadline("");
      setMaxMarks("100");
      setSubjectId("");
      setPublishNow(true);
      loadAll();
    } catch {
      setFormError("Couldn't create the assignment. Check your details and try again.");
    } finally {
      setCreating(false);
    }
  }

  const publishedCount = assignments.filter((a) => a.status === "Published").length;
  const draftCount = assignments.filter((a) => a.status === "Draft").length;

  return (
    <DashboardLayout requiredRole="Teacher" navItems={navItems}>
      <div className="mb-8">
        <p className="eyebrow mb-1">Dashboard</p>
        <h1 className="text-3xl font-semibold text-ink">Teacher overview</h1>
      </div>

      {loading ? (
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
            <StatCard label="Total assignments" value={assignments.length} icon={ClipboardList} />
            <StatCard label="Published" value={publishedCount} icon={CheckCircle2} />
            <StatCard label="Drafts" value={draftCount} icon={FileClock} />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
            <Card title="New assignment" eyebrow="Create" icon={Plus} className="lg:col-span-2 h-fit">
              <form onSubmit={handleCreateAssignment} className="flex flex-col gap-3">
                <Input
                  placeholder="Assignment title"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
                <textarea
                  placeholder="Description / instructions"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={3}
                  className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm placeholder:text-slate focus-visible:border-indigo resize-none"
                />
                <select
                  value={subjectId}
                  onChange={(e) => setSubjectId(e.target.value)}
                  className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm"
                >
                  <option value="">Select subject</option>
                  {subjects.map((s) => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
                <div className="flex gap-2">
                  <Input
                    type="datetime-local"
                    label="Deadline"
                    value={deadline}
                    onChange={(e) => setDeadline(e.target.value)}
                    className="flex-1"
                  />
                  <Input
                    type="number"
                    label="Max marks"
                    value={maxMarks}
                    onChange={(e) => setMaxMarks(e.target.value)}
                    className="w-24"
                  />
                </div>
                <label className="flex items-center gap-2 text-sm text-ink-soft">
                  <input
                    type="checkbox"
                    checked={publishNow}
                    onChange={(e) => setPublishNow(e.target.checked)}
                  />
                  Publish immediately (uncheck to save as draft)
                </label>

                {formError && (
                  <div className="bg-red-soft text-red text-sm rounded-md px-3 py-2">
                    {formError}
                  </div>
                )}

                <Button type="submit" disabled={creating} className="flex items-center justify-center gap-1.5">
                  <Plus size={16} /> {creating ? "Creating…" : "Create assignment"}
                </Button>

                {subjects.length === 0 && (
                  <p className="text-xs text-slate">
                    You have no subjects assigned yet — ask an Admin to assign you to a subject before creating assignments.
                  </p>
                )}
              </form>
            </Card>

            <Card title="Your assignments" eyebrow="All" icon={ClipboardList} className="lg:col-span-3">
              <div className="flex flex-col">
                {assignments.map((a) => (
                  <a key={a.id} href={`/teacher/assignments/${a.id}`} className="flex items-center justify-between py-3 border-b border-slate-light last:border-0 hover:bg-paper -mx-2 px-2 rounded-md transition-colors">
                    <div>
                      <p className="text-sm font-medium text-ink">{a.title}</p>
                      <p className="text-xs text-slate mt-0.5">
                        {a.subjectName} · Due {new Date(a.deadline).toLocaleDateString()}
                        {submissionSummaries[a.id] && ` · ${submissionSummaries[a.id]}`}
                      </p>
                    </div>
                    <StatusBadge status={a.status} />
                  </a>
                ))}
                {assignments.length === 0 && (
                  <p className="text-sm text-slate py-4">You haven&apos;t created any assignments yet.</p>
                )}
              </div>
            </Card>
          </div>
        </>
      )}
    </DashboardLayout>
  );
}