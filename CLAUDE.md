# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Gideon is a comprehensive native Windows desktop application for EVE Online players that provides advanced ship fitting, character planning, and market analysis tools. The application is built using .NET 9.0 + WPF with Windows 11 integration, designed to be highly accurate with calculations matching EVE Online's server-side formulas within 0.01%.

## Current Architecture & Technology Stack

**Platform:** .NET 9.0 + WPF (Windows-only)  
**Language:** C# 13 with latest language features  
**Project Status:** Phase 1 - Foundation & Project Setup (in progress)

### Core Technologies
- **.NET 9.0** - Latest .NET with native AOT support
- **WPF** - Windows Presentation Foundation for rich native UI
- **CommunityToolkit.Mvvm** - Modern MVVM framework with source generators
- **HelixToolkit.Wpf** - 3D visualization for ship viewer
- **MaterialDesignThemes** - Modern Material Design UI components
- **Entity Framework Core** - Data persistence with SQLite
- **Refit** - Type-safe HTTP client for ESI API
- **Polly** - Resilience patterns for API calls

## Development Commands

### Build and Run
```bash
# Development
dotnet run --project Gideon.WPF    # Start application
dotnet watch run --project Gideon.WPF  # Start with hot reload

# Building
dotnet build                      # Debug build
dotnet build -c Release           # Release build
dotnet publish                    # Self-contained publish

# Testing
dotnet test                       # Run all tests
dotnet test --collect:"XPlat Code Coverage"  # With coverage

# Package Management
dotnet restore                    # Restore NuGet packages
dotnet clean                      # Clean build artifacts
```

### Project Structure
```
Gideon.WPF/
├── Core/
│   ├── Application/       # CQRS commands, queries, services
│   ├── Domain/           # Domain entities, value objects, interfaces
│   └── Infrastructure/   # Data access, external services
├── Infrastructure/       # Concrete implementations
├── Presentation/         # ViewModels, Views, converters
archive-electron/         # Legacy Electron reference implementation
```

## Architecture Overview

### Clean Architecture Pattern
- **Core/Domain**: Business entities (Ship, Character, Module, ShipFitting)
- **Core/Application**: Use cases, DTOs, service interfaces
- **Core/Infrastructure**: Configuration and common infrastructure
- **Infrastructure**: Data access, ESI API clients, external services
- **Presentation**: WPF Views, ViewModels, user controls

### Key Services
- **IAuthenticationService**: ESI OAuth2 integration with Windows Credential Manager
- **IShipFittingService**: Ship fitting calculations and validation
- **GideonDbContext**: Entity Framework Core with SQLite
- **Repository Pattern**: Generic repository with Unit of Work

### MVVM Implementation
- **CommunityToolkit.Mvvm**: Source generators for ViewModels
- **BaseViewModel**: Common functionality for all ViewModels
- **RelayCommand**: Command pattern implementation
- **ObservableProperty**: Auto-generated property change notifications

## Development Workflow

### Task Management
**CRITICAL:** Always check `development-plan.md` before starting work. This file contains the complete 163-task development plan with current progress tracking.

#### Current Status (as of document)
- **Phase:** Phase 1 - Foundation & Project Setup  
- **Progress:** ~25% complete (4/8 infrastructure tasks done)
- **Next Tasks:** Configure Git workflow, analyzers, build targets

#### Workflow Process
1. Check `development-plan.md` for current active task
2. Update task status to "in_progress" when starting
3. Mark tasks as "completed" when finished
4. Commit with task reference (e.g., "TASK-005: Configure Git workflow")
5. Move to next priority task

### Code Standards
- **C# 13** with latest language features and nullable reference types
- **Clean Architecture** with CQRS pattern
- **MVVM** using CommunityToolkit.Mvvm source generators
- **Async/await** throughout for responsive UI
- **Dependency Injection** with Microsoft.Extensions.DependencyInjection
- **EditorConfig** enforced formatting (.editorconfig in root)
- **Code Analysis** with StyleCop and .NET analyzers

## Package Management

### NuGet Central Package Management
**CRITICAL:** This project uses Central Package Management (CPM) via `Directory.Packages.props`

#### Package Version Control
- All package versions are centrally managed in `Directory.Packages.props`
- Individual projects reference packages WITHOUT version numbers
- To add a package: Add version to Directory.Packages.props, then reference in .csproj
- To update: Change version in Directory.Packages.props only

#### Key Dependencies
- **.NET 9.0** - Target framework with C# 13
- **MaterialDesignThemes 5.2.1** - UI component library
- **CommunityToolkit.Mvvm 8.3.2** - MVVM framework
- **Entity Framework Core 9.0.6** - Data access
- **Refit 8.0.0** - HTTP client for ESI API
- **HelixToolkit.Wpf** - 3D visualization

### Legacy Electron Reference
The `archive-electron/` directory contains a complete, working Electron implementation with:
- ✅ OAuth2 PKCE authentication system
- ✅ Multi-character support with secure token storage  
- ✅ 3D ship visualization with Babylon.js
- ✅ Ship fitting calculation engines
- ✅ Comprehensive React component library

Use this as reference for WPF implementation patterns and proven business logic.

## Performance Requirements

- **Startup Time**: <2 seconds (vs 3-5s Electron)
- **Memory Usage**: <100MB normal operation (vs ~200MB Electron)
- **UI Response**: <100ms for all interactions
- **Calculation Accuracy**: Within 0.01% of EVE Online values
- **Bundle Size**: ~30MB (vs ~150MB Electron)

## ESI API Integration

### Authentication
- OAuth2 PKCE flow with native Windows integration
- Windows Credential Manager for secure token storage
- Multi-character support with automatic token refresh
- Reference implementation available in `archive-electron/src/renderer/services/auth/`

### ESI Services
- Type-safe HTTP client using Refit
- Polly for resilience and retry policies
- Rate limiting and error handling
- Character, skills, and market data integration