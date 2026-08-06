export type UserRole = "Admin" | "Teacher" | "Student";

export interface AuthUser {
  fullName: string;
  email: string;
  role: UserRole;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface Assignment {
  id: number;
  title: string;
  description: string;
  deadline: string;
  maxMarks: number;
  status: "Draft" | "Published";
  subjectName: string;
  createdAt: string;
}

export interface Submission {
  id: number;
  answerText: string;
  submittedAt: string;
  updatedAt: string | null;
  status: "Submitted" | "Late" | "Graded" | "ReturnedForRevision";
  marksAwarded: number | null;
  teacherFeedback: string | null;
  studentName: string;
  assignmentTitle: string;
}

export interface ClassRoom {
  id: number;
  name: string;
}

export interface Subject {
  id: number;
  name: string;
  classRoomId: number;
}

export interface AdminUser {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  classId: number | null;
}