"use client";

import { useEffect, useState } from "react";
import { DashboardLayout } from "@/components/DashboardLayout";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { StatCard } from "@/components/ui/StatCard";
import { api } from "@/lib/api";
import type { ClassRoom, Subject, AdminUser } from "@/types";
import { LayoutGrid, School, BookOpen, Users, Plus, UserPlus, Link2 } from "lucide-react";

const navItems = [{ label: "Overview", href: "/admin", icon: LayoutGrid }];

export default function AdminPage() {
  const [classRooms, setClassRooms] = useState<ClassRoom[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);

  const [newClassName, setNewClassName] = useState("");
  const [newSubjectName, setNewSubjectName] = useState("");
  const [newSubjectClassId, setNewSubjectClassId] = useState("");

  const [newUserName, setNewUserName] = useState("");
  const [newUserEmail, setNewUserEmail] = useState("");
  const [newUserPassword, setNewUserPassword] = useState("");
  const [newUserRole, setNewUserRole] = useState("Student");
  const [userError, setUserError] = useState("");
  const [creatingUser, setCreatingUser] = useState(false);

  const [assignTeacherId, setAssignTeacherId] = useState("");
  const [assignSubjectId, setAssignSubjectId] = useState("");
  const [assignStudentId, setAssignStudentId] = useState("");
  const [assignClassId, setAssignClassId] = useState("");

  async function loadAll() {
    const [classData, subjectData, userData] = await Promise.all([
      api.get<ClassRoom[]>("/Admin/classrooms"),
      api.get<Subject[]>("/Admin/subjects"),
      api.get<AdminUser[]>("/Admin/users"),
    ]);
    setClassRooms(classData);
    setSubjects(subjectData);
    setUsers(userData);
    setLoading(false);
  }

  useEffect(() => {
    loadAll();
  }, []);

  async function handleCreateClass(e: React.FormEvent) {
    e.preventDefault();
    if (!newClassName.trim()) return;
    await api.post("/Admin/classrooms", { name: newClassName });
    setNewClassName("");
    loadAll();
  }

  async function handleCreateSubject(e: React.FormEvent) {
    e.preventDefault();
    if (!newSubjectName.trim() || !newSubjectClassId) return;
    await api.post("/Admin/subjects", { name: newSubjectName, classRoomId: Number(newSubjectClassId) });
    setNewSubjectName("");
    setNewSubjectClassId("");
    loadAll();
  }

  async function handleCreateUser(e: React.FormEvent) {
    e.preventDefault();
    setUserError("");
    if (!newUserName.trim() || !newUserEmail.trim() || !newUserPassword.trim()) {
      setUserError("Fill in every field.");
      return;
    }
    setCreatingUser(true);
    try {
      await api.post("/Auth/register", {
        fullName: newUserName,
        email: newUserEmail,
        password: newUserPassword,
        role: newUserRole,
        classId: null,
      });
      setNewUserName("");
      setNewUserEmail("");
      setNewUserPassword("");
      setNewUserRole("Student");
      loadAll();
    } catch {
      setUserError("Couldn't create the user. That email may already be registered.");
    } finally {
      setCreatingUser(false);
    }
  }

  async function handleAssignTeacher(e: React.FormEvent) {
    e.preventDefault();
    if (!assignTeacherId || !assignSubjectId) return;
    await api.post("/Admin/assign-teacher", {
      teacherId: Number(assignTeacherId),
      subjectId: Number(assignSubjectId),
    });
    setAssignTeacherId("");
    setAssignSubjectId("");
    loadAll();
  }

  async function handleAssignStudent(e: React.FormEvent) {
    e.preventDefault();
    if (!assignStudentId || !assignClassId) return;
    await api.put("/Admin/assign-student-class", {
      studentId: Number(assignStudentId),
      classRoomId: Number(assignClassId),
    });
    setAssignStudentId("");
    setAssignClassId("");
    loadAll();
  }

  const teachers = users.filter((u) => u.role === "Teacher");
  const students = users.filter((u) => u.role === "Student");

  return (
    <DashboardLayout requiredRole="Admin" navItems={navItems}>
      <div className="mb-8">
        <p className="eyebrow mb-1">Dashboard</p>
        <h1 className="text-3xl font-semibold text-ink">Admin overview</h1>
      </div>

      {loading ? (
        <p className="text-slate font-mono-data text-sm">Loading…</p>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
            <StatCard label="Classrooms" value={classRooms.length} icon={School} />
            <StatCard label="Subjects" value={subjects.length} icon={BookOpen} />
            <StatCard label="Users" value={users.length} icon={Users} />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card title="Classrooms" eyebrow="Manage" icon={School}>
              <ul className="flex flex-col mb-4">
                {classRooms.map((c) => (
                  <li key={c.id} className="text-sm text-ink-soft py-2.5 border-b border-slate-light last:border-0">{c.name}</li>
                ))}
                {classRooms.length === 0 && <li className="text-sm text-slate py-2">No classrooms yet.</li>}
              </ul>
              <form onSubmit={handleCreateClass} className="flex gap-2">
                <Input placeholder="e.g. Grade 10 - Section A" value={newClassName} onChange={(e) => setNewClassName(e.target.value)} className="flex-1" />
                <Button type="submit" className="flex items-center gap-1.5 shrink-0"><Plus size={16} /> Add</Button>
              </form>
            </Card>

            <Card title="Subjects" eyebrow="Manage" icon={BookOpen}>
              <ul className="flex flex-col mb-4">
                {subjects.map((s) => (
                  <li key={s.id} className="text-sm text-ink-soft py-2.5 border-b border-slate-light last:border-0">
                    <span className="text-ink">{s.name}</span> — {classRooms.find((c) => c.id === s.classRoomId)?.name || "Unknown"}
                  </li>
                ))}
                {subjects.length === 0 && <li className="text-sm text-slate py-2">No subjects yet.</li>}
              </ul>
              <form onSubmit={handleCreateSubject} className="flex flex-col gap-2">
                <Input placeholder="e.g. Mathematics" value={newSubjectName} onChange={(e) => setNewSubjectName(e.target.value)} />
                <div className="flex gap-2">
                  <select value={newSubjectClassId} onChange={(e) => setNewSubjectClassId(e.target.value)} className="flex-1 px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                    <option value="">Select classroom</option>
                    {classRooms.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                  <Button type="submit" className="flex items-center gap-1.5 shrink-0"><Plus size={16} /> Add</Button>
                </div>
              </form>
            </Card>

            <Card title="Create user" eyebrow="Add" icon={UserPlus}>
              <form onSubmit={handleCreateUser} className="flex flex-col gap-2">
                <Input placeholder="Full name" value={newUserName} onChange={(e) => setNewUserName(e.target.value)} />
                <Input type="email" placeholder="Email" value={newUserEmail} onChange={(e) => setNewUserEmail(e.target.value)} />
                <Input type="password" placeholder="Password" value={newUserPassword} onChange={(e) => setNewUserPassword(e.target.value)} />
                <select value={newUserRole} onChange={(e) => setNewUserRole(e.target.value)} className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                  <option value="Student">Student</option>
                  <option value="Teacher">Teacher</option>
                  <option value="Admin">Admin</option>
                </select>
                {userError && <div className="bg-red-soft text-red text-sm rounded-md px-3 py-2">{userError}</div>}
                <Button type="submit" disabled={creatingUser} className="flex items-center justify-center gap-1.5">
                  <UserPlus size={16} /> {creatingUser ? "Creating…" : "Create user"}
                </Button>
              </form>
            </Card>

            <Card title="Assignments" eyebrow="Link" icon={Link2}>
              <div className="flex flex-col gap-5">
                <div>
                  <p className="text-xs font-medium text-ink-soft mb-2">Assign teacher to subject</p>
                  <form onSubmit={handleAssignTeacher} className="flex flex-col gap-2">
                    <select value={assignTeacherId} onChange={(e) => setAssignTeacherId(e.target.value)} className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                      <option value="">Select teacher</option>
                      {teachers.map((t) => <option key={t.id} value={t.id}>{t.fullName}</option>)}
                    </select>
                    <div className="flex gap-2">
                      <select value={assignSubjectId} onChange={(e) => setAssignSubjectId(e.target.value)} className="flex-1 px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                        <option value="">Select subject</option>
                        {subjects.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
                      </select>
                      <Button type="submit" className="shrink-0">Assign</Button>
                    </div>
                  </form>
                </div>

                <div>
                  <p className="text-xs font-medium text-ink-soft mb-2">Assign student to classroom</p>
                  <form onSubmit={handleAssignStudent} className="flex flex-col gap-2">
                    <select value={assignStudentId} onChange={(e) => setAssignStudentId(e.target.value)} className="px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                      <option value="">Select student</option>
                      {students.map((s) => <option key={s.id} value={s.id}>{s.fullName}</option>)}
                    </select>
                    <div className="flex gap-2">
                      <select value={assignClassId} onChange={(e) => setAssignClassId(e.target.value)} className="flex-1 px-3 py-2 rounded-md border border-slate-light bg-white text-ink text-sm">
                        <option value="">Select classroom</option>
                        {classRooms.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                      </select>
                      <Button type="submit" className="shrink-0">Assign</Button>
                    </div>
                  </form>
                </div>
              </div>
            </Card>

            <Card title="Users" eyebrow="Directory" icon={Users} className="lg:col-span-2">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-left border-b border-slate-light">
                    <th className="pb-2 font-medium eyebrow">Name</th>
                    <th className="pb-2 font-medium eyebrow">Email</th>
                    <th className="pb-2 font-medium eyebrow">Role</th>
                    <th className="pb-2 font-medium eyebrow">Class</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((u) => (
                    <tr key={u.id} className="border-b border-slate-light last:border-0">
                      <td className="py-2.5 text-ink">{u.fullName}</td>
                      <td className="py-2.5 text-ink-soft">{u.email}</td>
                      <td className="py-2.5 text-indigo font-mono-data text-xs">{u.role}</td>
                      <td className="py-2.5 text-ink-soft text-xs">{classRooms.find((c) => c.id === u.classId)?.name || "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>
          </div>
        </>
      )}
    </DashboardLayout>
  );
}