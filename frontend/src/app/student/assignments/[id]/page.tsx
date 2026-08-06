"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { DashboardLayout } from "@/components/DashboardLayout";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { api } from "@/lib/api";
import type { Assignment, Submission } from "@/types";
import { LayoutGrid, ArrowLeft, FileText } from "lucide-react";

const navItems = [{ label: "Overview", href: "/student", icon: LayoutGrid }];

export default function StudentAssignmentDetailPage() {
  const params = useParams();
  const assignmentId = params.id as string;

  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [submission, setSubmission] = useState<Submission | null>(null);
  const [answerText, setAnswerText] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  async function loadAll() {
    const assignmentData = await api.get<Assignment>(`/Assignments/${assignmentId}`);
    setAssignment(assignmentData);

    try {
      const submissionData = await api.get<Submission>(`/assignments/${assignmentId}/Submissions/mine`);
      setSubmission(submissionData);
      setAnswerText(submissionData.answerText);
    } catch {
      setSubmission(null);
    }

    setLoading(false);
  }

  useEffect(() => {
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [assignmentId]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    if (!answerText.trim()) return;

    setSaving(true);
    try {
      await api.post(`/assignments/${assignmentId}/Submissions`, { answerText });
      loadAll();
    } catch {
      setError("Couldn't submit your answer. The deadline may have passed.");
    } finally {
      setSaving(false);
    }
  }

  const isPastDeadline = assignment ? new Date() > new Date(assignment.deadline) : false;
  const isGraded = submission?.status === "Graded";
  const canEdit = !isPastDeadline && !isGraded;

  return (
    <DashboardLayout requiredRole="Student" navItems={navItems}>
      <a href="/student" className="inline-flex items-center gap-1.5 text-sm text-slate hover:text-ink mb-4 transition-colors"><ArrowLeft size={15} /> Back to overview</a>

      {loading || !assignment ? (
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      ) : (
        <>
          <div className="mb-8">
            <p className="eyebrow mb-1">{assignment.subjectName}</p>
            <h1 className="text-3xl font-semibold text-ink">{assignment.title}</h1>
            <p className="text-sm text-ink-soft mt-2 max-w-2xl">{assignment.description}</p>
            <div className="flex items-center gap-4 mt-3">
              {submission && <StatusBadge status={submission.status} />}
              <span className="text-xs text-slate font-mono-data">Due {new Date(assignment.deadline).toLocaleString()}</span>
              <span className="text-xs text-slate font-mono-data">Max marks: {assignment.maxMarks}</span>
            </div>
          </div>

          {isGraded && submission && (
            <Card title="Your grade" eyebrow="Graded" icon={FileText} className="mb-6">
              <div className="flex items-start gap-4">
                <span className="stamp inline-block px-4 py-2 text-green font-mono-data text-lg">
                  {submission.marksAwarded} / {assignment.maxMarks}
                </span>
                {submission.teacherFeedback && (
                  <p className="text-sm text-ink-soft italic pt-2">&quot;{submission.teacherFeedback}&quot;</p>
                )}
              </div>
            </Card>
          )}

          <Card title={submission ? "Your submission" : "Submit your answer"} eyebrow={canEdit ? "Editable" : "Locked"} icon={FileText}>
            <form onSubmit={handleSubmit} className="flex flex-col gap-3">
              <textarea
                value={answerText}
                onChange={(e) => setAnswerText(e.target.value)}
                disabled={!canEdit}
                rows={8}
                placeholder="Write your answer here…"
                className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm placeholder:text-slate focus-visible:border-indigo resize-none disabled:bg-paper disabled:text-ink-soft"
              />

              {error && <div className="bg-red-soft text-red text-sm rounded-md px-3 py-2">{error}</div>}

              {isPastDeadline && !submission && (
                <p className="text-sm text-red">The deadline has passed. You can no longer submit.</p>
              )}
              {isPastDeadline && submission && !isGraded && (
                <p className="text-sm text-amber">The deadline has passed — your submission is locked from further edits.</p>
              )}
              {isGraded && (
                <p className="text-sm text-slate">This submission has been graded and can no longer be changed.</p>
              )}

              {canEdit && (
                <Button type="submit" disabled={saving} className="self-start">
                  {saving ? "Saving…" : submission ? "Update submission" : "Submit"}
                </Button>
              )}
            </form>
          </Card>
        </>
      )}
    </DashboardLayout>
  );
}