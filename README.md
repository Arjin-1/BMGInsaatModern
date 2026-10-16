
# BMG İnşaat Modern

A full-stack corporate website and content management platform built with Angular, ASP.NET Core, PostgreSQL, and Azure AI Translator.

> **Project Status:** Under active development

## Overview

BMG İnşaat Modern is a bilingual corporate website and custom content management system designed for a construction company operating in infrastructure and superstructure projects.

The project focuses on maintainable architecture, security, testability, accessibility, and a modern user experience.

Rather than relying on a generic website template, the platform is being developed with a modular frontend, layered backend, and custom administrative workflows.

## Tech Stack

### Frontend
- Angular 22
- TypeScript
- SCSS
- Angular Router
- Vitest

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- PostgreSQL

### Integrations
- Azure AI Translator (Turkish to English)

### Development Tools
- Git
- Visual Studio Code
- PowerShell

## Architecture

The solution follows a layered backend architecture and a component-based frontend architecture.

```text
BMGInsaatModern/
├── backend/
│   ├── BMGInsaat.Domain/
│   ├── BMGInsaat.Application/
│   ├── BMGInsaat.Infrastructure/
│   └── BMGInsaat.API/
│
├── frontend/
│   └── src/
│       └── app/
│           ├── pages/
│           ├── shared/
│           ├── app.routes.ts
│           └── app.ts
│
├── .editorconfig
├── .gitattributes
└── README.md
```

### Engineering Principles

- Separation of Concerns
- Dependency Injection
- SOLID principles where appropriate
- Reusable Angular components
- Strongly typed application contracts
- Secure authentication and authorization
- Automated testing
- Incremental and maintainable development

## Implemented Features

### Backend
- Layered .NET architecture
- PostgreSQL integration with EF Core
- Project creation, updating, and deletion
- Published project listing
- SuperAdmin authentication using ASP.NET Core Identity
- Role-based authorization
- HttpOnly authentication cookies
- CSRF protection for administrative operations
- Azure-powered Turkish-to-English project translation

### Frontend
- Angular standalone components
- Lazy-loaded home page
- Reusable navigation header
- Responsive landing page
- Centralized design tokens
- Initial automated component tests

## Planned Features

- Complete corporate website pages
- Dynamic project galleries
- Project videos and certificates
- Custom admin dashboard
- Editable website content
- Bilingual content management
- Secure media uploads
- Contact and location information
- Expanded automated testing
- CI workflows and deployment configuration

## Local Development

### Backend

```bash
cd backend
dotnet restore BMGInsaat.sln
dotnet build BMGInsaat.sln
dotnet run --project BMGInsaat.API
```

The backend requires a configured PostgreSQL database and local development secrets.

### Frontend

```bash
cd frontend
npm ci
npm start
```

### Frontend Tests

```bash
cd frontend
npm test -- --watch=false
```

### Production Build

```bash
cd frontend
npm run build
```

## Security Notes

Secrets and credentials must never be committed to the repository.

Local development credentials are managed outside source control using appropriate configuration mechanisms, including .NET User Secrets.

Production deployment security and configuration will be documented as deployment functionality is implemented.

## Development Workflow

- `main`: stable integration branch
- `feature/*`: feature development
- `fix/*`: bug fixes
- `docs/*`: documentation updates

Changes are intended to be validated through builds, tests, and code review before integration.

## Project Roadmap

This repository is actively evolving from an initial full-stack foundation into a complete, maintainable corporate CMS platform.

Documentation will be updated as new features are implemented.
```
