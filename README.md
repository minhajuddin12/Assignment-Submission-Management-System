# Assignment & Submission Management System - live url: https://assignment-submission-management-sy-psi.vercel.app/login

A role-based full-stack web application for schools/colleges where teachers create and grade
assignments, and students submit work and track their results.

## Overview

Admins manage users, classrooms, and subjects, and link teachers to a class + subject pair.
Teachers create assignments (draft or published) scoped to a subject/classroom, review student
submissions, grade them, and can manually change a submission's status (e.g. return work for
revision). Students see only assignments for their own classroom, submit answers before the
deadline, and can update their submission until it's graded or the deadline passes.

## Features

**Admin**
- Create/manage classrooms and subjects
- Create, edit, and delete users (with guards against deleting teachers/students who have
  linked assignments or submissions)
- Assign a teacher to a specific classroom + subject together
- Assign students to a classroom

**Teacher**
- Create, update, delete assignments (title, description, deadline, max marks)
- Publish immediately or save as a draft
- View all submissions for an assignment
- Grade a submission (marks + written feedback)
- Manually change a submission's status (e.g. mark as returned for revision)

**Student**
- View only published assignments for their own classroom
- View assignment details and deadline
- Submit an answer; update it up until the deadline (locked once graded or past due)
- View submission status, marks, and teacher feedback

##Tech stack

* Frontend: Next.js (App Router), React, TypeScript, Tailwind CSS
* Backend: ASP.NET Core Web API (.NET 10), C#, Entity Framework Core
* Database: PostgreSQL — chosen for the relational structure of the data
  (Users, ClassRooms, Subjects, Assignments, Submissions all have clear
  foreign-key relationships)
* Auth: JWT bearer tokens, role-based authorization (`Admin`, `Teacher`, `Student`)
* Testing: xUnit, Moq
* Docs: OpenAPI via Scalar (`/scalar` when running)

## Project structure

assignment-submission-system/
│
├── backend/
│   │
│   ├── AssignmentSystem.Api/                 # ASP.NET Core Web API
│   │   ├── bin/
│   │   ├── Controllers/                      # API controllers
│   │   ├── Data/                             # EF Core DbContext
│   │   ├── DTOs/                             # Request/Response DTOs
│   │   ├── Migrations/                       # EF Core migrations
│   │   ├── Models/                           # Entity models
│   │   ├── obj/
│   │   ├── Properties/
│   │   ├── Services/                         # JWT & current-user helper
│   │   ├── .env.example
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Dockerfile
│   │   └── Program.cs
│   │
│   └── AssignmentSystem.Tests/               # xUnit test project
│
├── frontend/
│   │
│   ├── .next/
│   ├── node_modules/
│   ├── public/
│   ├── src/
│   │   ├── app/                             # Next.js App Router
│   │   │   ├── admin/
│   │   │   ├── teacher/
│   │   │   ├── student/
│   │   │   └── login/
│   │   ├── components/                      # Reusable UI components
│   │   ├── context/                         # Authentication context
│   │   ├── lib/                             # API client & utilities
│   │   └── types/                           # TypeScript types/interfaces
│   │
│   ├── .env.example
│   ├── .env.local
│   └── .gitignore
│
└── README.md

## Setup instructions

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- A PostgreSQL database (local, Docker, or a hosted instance like Neon/Supabase)

## Database Setup

This project uses PostgreSQL (Neon).

1. Configure the connection string in appsettings.json:

ConnectionStrings:
  DefaultConnection=<Host=ep-dawn-frost-azxftn71-pooler.c-3.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_pCGB0NXvTVa3;SSL Mode=Require;Trust Server Certificate=true>

2. Apply migrations:

dotnet ef database update

3. Seed initial data:

dotnet run

### 2. Backend setup
```bash
cd backend/AssignmentSystem.Api
```
Copy `.env.example` to your local config and fill in real values (see that file for the exact
variable names). At minimum you need:
- `ConnectionStrings__DefaultConnection` — your PostgreSQL connection string
- `Jwt__Key` — any random string, 32+ characters

You can either export these as environment variables, or put them directly into
`appsettings.Development.json` (already gitignored, safe to edit locally).

Then run:
```bash
dotnet restore
dotnet run
```
The API starts on `http://localhost:5029` by default and auto-applies migrations + seeds demo
users on first launch. Swagger/OpenAPI docs are available at `/scalar/v1`.

### 3. Frontend setup
```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```
The app runs at `http://localhost:3000`. `.env.local` should point `NEXT_PUBLIC_API_URL` at your
running backend (defaults to `http://localhost:5029/api`).

### 4. Running tests
```bash
cd backend/AssignmentSystem.Tests
dotnet test
```

## Demo credentials

Seeded automatically on first backend run:

| Role    | Email               | Password     |
|---------|----------------------|--------------|
| Admin   | admin@school.com    | Admin@123    |
| Teacher | teacher@school.com  | Teacher@123  |
| Student | student@school.com  | Student@123  |

(The demo student has no classroom assigned by default — assign one from the Admin dashboard to
see published assignments appear for them.)

## Data model / relationships

- `ClassRoom` has many `Subject`s and many `User`s (students)
- `Subject` belongs to one `ClassRoom`
- `TeacherSubjectAssignment` links a `User` (Teacher) to a `Subject` — since every `Subject`
  belongs to exactly one classroom, assigning a teacher to a subject also scopes them to that
  subject's classroom; there is no separate teacher-to-classroom table
- `Assignment` belongs to one `Subject` (and therefore one classroom) and one creating Teacher
- `Submission` belongs to one `Assignment` and one Student, with a unique constraint on
  (AssignmentId, StudentId) — one submission per student per assignment, updated in place rather
  than duplicated

## Assumptions

- A subject belongs to exactly one classroom (e.g. "Math" for Grade 10 Section A and "Math" for
  Grade 10 Section B are two separate `Subject` records)
- A student belongs to exactly one classroom at a time
- Assigning a teacher to a subject is treated as assigning them to that subject's classroom —
  there's no separate class-only assignment without a subject
- "Published" vs a student's submission status are intentionally separate fields: an assignment
  stays "Published" once live; each student's own progress (Submitted/Late/Graded/Returned for
  revision) is tracked per-submission, not on the assignment itself

## Known limitations

- Classroom names are not enforced unique at the database or API level; creating two classrooms
  with the same name is possible and can cause confusion in dropdowns (they'll have different
  IDs internally, so admins should avoid duplicate names)
- The teacher's manual "change submission status" action does not populate marks/feedback when
  set to "Graded" directly — grading with marks should be done through the dedicated Grade action
- Deleting a teacher or student with existing assignments/submissions is blocked rather than
  cascaded, to avoid silent data loss; there's currently no bulk-reassign flow, so an admin must
  resolve those links manually first if a deletion is needed
- No pagination or search/filtering on the admin Users/Classrooms/Subjects lists
- No automated frontend tests; test coverage is backend-only (xUnit)

## Optional additions included

- Dockerfile for the backend (containerized deploy, used for the live Render deployment)
- Global exception-handling middleware with structured logging of unhandled errors
- live url: https://assignment-submission-management-sy-psi.vercel.app/login
