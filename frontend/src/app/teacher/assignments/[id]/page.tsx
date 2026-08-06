"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { DashboardLayout } from "@/components/DashboardLayout";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { StatusBadge } from "@/components/ui/StatusBadge";
import { api } from "@/lib/api";
import type { Assignment, Submission } from "@/types";
import { LayoutGrid, ClipboardList, ArrowLeft } from "lucide-react";

const navItems = [{ label: "Overview", href: "/teacher", icon: LayoutGrid }];

export default function TeacherAssignmentDetailPage() {
  const params = useParams();
  const assignmentId = params.id as string;

  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);

  const [gradingId, setGradingId] = useState<number | null>(null);
  const [marksInput, setMarksInput] = useState("");
  const [feedbackInput, setFeedbackInput] = useState("");
  const [saving, setSaving] = useState(false);

  async function loadAll() {
    const [assignmentData, submissionData] = await Promise.all([
      api.get<Assignment>(`/Assignments/${assignmentId}`),
      api.get<Submission[]>(`/assignments/${assignmentId}/Submissions`),
    ]);
    setAssignment(assignmentData);
    setSubmissions(submissionData);
    setLoading(false);
  }

  useEffect(() => {
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [assignmentId]);

  function startGrading(s: Submission) {
    setGradingId(s.id);
    setMarksInput(s.marksAwarded?.toString() || "");
    setFeedbackInput(s.teacherFeedback || "");
  }

  async function submitGrade(submissionId: number) {
    setSaving(true);
    try {
      await api.put(`/assignments/${assignmentId}/Submissions/${submissionId}/grade`, {
        marksAwarded: Number(marksInput),
        feedback: feedbackInput,
      });
      setGradingId(null);
      loadAll();
    } finally {
      setSaving(false);
    }
  }

  return (
    <DashboardLayout requiredRole="Teacher" navItems={navItems}>
      <a href="/teacher" className="inline-flex items-center gap-1.5 text-sm text-slate hover:text-ink mb-4 transition-colors">
        <ArrowLeft size={15} /> Back to overview
      </a>

      {loading || !assignment ? (
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      ) : (
        <>
          <div className="mb-8">
            <p className="eyebrow mb-1">{assignment.subjectName}</p>
            <h1 className="text-3xl font-semibold text-ink">{assignment.title}</h1>
            <p className="text-sm text-ink-soft mt-2 max-w-2xl">{assignment.description}</p>
            <div className="flex items-center gap-4 mt-3">
              <StatusBadge status={assignment.status} />
              <span className="text-xs text-slate font-mono-data">
                Due {new Date(assignment.deadline).toLocaleString()}
              </span>
              <span className="text-xs text-slate font-mono-data">Max marks: {assignment.maxMarks}</span>
            </div>
          </div>

          <Card title="Submissions" eyebrow={`${submissions.length} total`} icon={ClipboardList}>
            <div className="flex flex-col">
              {submissions.map((s) => (
                <div key={s.id} className="py-4 border-b border-slate-light last:border-0">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <p className="text-sm font-medium text-ink">{s.studentName}</p>
                        <StatusBadge status={s.status} />
                      </div>
                      <p className="text-sm text-ink-soft whitespace-pre-wrap">{s.answerText}</p>
                      <p className="text-xs text-slate font-mono-data mt-2">
                        Submitted {new Date(s.submittedAt).toLocaleString()}
                        {s.updatedAt && ` · Updated ${new Date(s.updatedAt).toLocaleString()}`}
                      </p>

                      {s.marksAwarded !== null && gradingId !== s.id && (
                        <div className="mt-3 flex items-start gap-3">
                          <span className="stamp inline-block px-3 py-1.5 text-green font-mono-data text-sm">
                            {s.marksAwarded} / {assignment.maxMarks}
                          </span>
                          {s.teacherFeedback && (
                            <p className="text-sm text-ink-soft italic pt-1">&quot;{s.teacherFeedback}&quot;</p>
                          )}
                        </div>
                      )}
                    </div>

                    {gradingId !== s.id && (
                      <Button variant="secondary" onClick={() => startGrading(s)}>
                        {s.marksAwarded !== null ? "Edit grade" : "Grade"}
                      </Button>
                    )}
                  </div>

                  {gradingId === s.id && (
                    <div className="mt-4 bg-paper rounded-lg p-4 flex flex-col gap-3">
                      <div className="flex items-center gap-2">
                        <input
                          type="number"
                          min={0}
                          max={assignment.maxMarks}
                          value={marksInput}
                          onChange={(e) => setMarksInput(e.target.value)}
                          placeholder="Marks"
                          className="w-24 px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm"
                        />
                        <span className="text-sm text-slate">/ {assignment.maxMarks}</span>
                      </div>
                      <textarea
                        placeholder="Feedback for the student"
                        value={feedbackInput}
                        onChange={(e) => setFeedbackInput(e.target.value)}
                        rows={2}
                        className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm resize-none"
                      />
                      <div className="flex gap-2">
                        <Button onClick={() => submitGrade(s.id)} disabled={saving || marksInput === ""}>
                          {saving ? "Saving…" : "Save grade"}
                        </Button>
                        <Button variant="secondary" onClick={() => setGradingId(null)}>
                          Cancel
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              ))}
              {submissions.length === 0 && (
                <p className="text-sm text-slate py-4">No submissions yet.</p>
              )}
            </div>
          </Card>
        </>
      )}
    </DashboardLayout>
  );
}