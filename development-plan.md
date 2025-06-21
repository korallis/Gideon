# Gideon Development Master Plan

> **Status:** WPF Native Development | **Last Updated:** June 21, 2025  
> **Tech Stack:** .NET 9.0 + WPF + CommunityToolkit.Mvvm + HelixToolkit + Windows 11 Integration
> **Target:** High-performance native Windows desktop application for EVE Online

c## 🎯 Project Overview

Building Gideon (EVE Online's AI Copilot) - a high-performance native Windows desktop application for EVE Online players featuring ship fitting, character planning, and market analysis with Windows 11 Fluent Design integration.

### 🚀 **Technology Stack**
- **.NET 9.0** - Latest .NET with C# 13 and native AOT support
- **WPF** - Windows Presentation Foundation for rich native UI
- **CommunityToolkit.Mvvm** - Modern MVVM framework with source generators
- **HelixToolkit.Wpf** - 3D visualization for ship viewer
- **MaterialDesignInXamlToolkit** - Modern Material Design UI components
- **Entity Framework Core** - Data persistence with SQLite
- **Refit** - Type-safe HTTP client for ESI API
- **Polly** - Resilience patterns for API calls
- **Windows 11 Features** - Native notifications, system tray, Fluent Design

---

## 📋 Development Phases

### Phase 0: Project Cleanup & Documentation (Week 0) 🎯 **CRITICAL**
**Goal:** Clean up legacy Electron artifacts and update project documentation

#### 0.1 Legacy Cleanup
- [x] **TASK-000** Clean up Electron artifacts and legacy files ✅
- [x] **TASK-000-B** Update README.md to reflect WPF-only development approach ✅
- [x] **TASK-000-C** Archive/organize existing codebase for reference ✅

### Phase 1: Foundation & Project Setup (Weeks 1-2)
**Goal:** Establish modern .NET 9 WPF foundation with Windows 11 integration

#### 1.1 Project Infrastructure
- [x] **TASK-001** Create .NET 9 WPF project with Windows 11 targeting ✅
- [x] **TASK-002** Configure project structure with Clean Architecture patterns ✅
- [x] **TASK-003** Set up NuGet package management with Central Package Management ✅
- [x] **TASK-004** Configure Cursor IDE with .NET 9 and WPF development extensions ✅
- [x] **TASK-005** Set up Git integration and development workflow ✅
- [x] **TASK-006** Configure EditorConfig and .NET analyzers for code quality ✅
- [ ] **TASK-007** Set up MSBuild targets for automated builds
- [ ] **TASK-008** Configure application manifest for Windows 11 features

#### 1.2 Modern WPF Stack Setup
- [ ] **TASK-009** Install CommunityToolkit.Mvvm 8.3+ for MVVM framework
- [ ] **TASK-010** Set up Microsoft.Extensions.DependencyInjection for IoC
- [ ] **TASK-011** Configure Microsoft.Extensions.Hosting for application lifecycle
- [ ] **TASK-012** Install MaterialDesignInXamlToolkit 5.1+ for modern UI
- [ ] **TASK-013** Set up HelixToolkit.Wpf 2024.6+ for 3D ship visualization
- [ ] **TASK-014** Configure Serilog 8.0+ for structured logging
- [ ] **TASK-015** Install Microsoft.Toolkit.Win32.UI.Controls for modern controls
- [ ] **TASK-016** Set up FluentValidation 11.9+ for form validation

#### 1.3 Windows Integration Foundation
- [ ] **TASK-017** Configure Windows 11 Fluent Design System integration
- [ ] **TASK-018** Set up Windows.ApplicationModel for modern app features
- [ ] **TASK-019** Configure Windows Credential Manager integration
- [ ] **TASK-020** Set up Windows notification system integration
- [ ] **TASK-021** Configure system tray and jump list functionality
- [ ] **TASK-022** Implement Windows 11 theme awareness and accent colors
- [ ] **TASK-023** Set up Windows App SDK integration for enhanced features
- [ ] **TASK-024** Configure high DPI awareness and scaling

### Phase 2: Core Architecture & MVVM (Weeks 3-4)
**Goal:** Establish MVVM architecture and core services

#### 2.1 MVVM Architecture Setup
- [ ] **TASK-025** Design ViewModels hierarchy with CommunityToolkit.Mvvm
- [ ] **TASK-026** Create base ViewModel classes with property notification
- [ ] **TASK-027** Implement RelayCommand and AsyncRelayCommand patterns
- [ ] **TASK-028** Set up Messenger for decoupled communication
- [ ] **TASK-029** Create service interfaces for dependency injection
- [ ] **TASK-030** Implement repository pattern for data access
- [ ] **TASK-031** Set up AutoMapper 13.0+ for object mapping
- [ ] **TASK-032** Create navigation service for view management

#### 2.2 Data Layer Foundation
- [ ] **TASK-033** Create EVE data models as C# record types
- [ ] **TASK-034** Set up Entity Framework Core 8.0+ for data persistence
- [ ] **TASK-035** Configure SQLite provider for local data storage
- [ ] **TASK-036** Design ship and module data structures
- [ ] **TASK-037** Implement data seeding for EVE universe data
- [ ] **TASK-038** Create data access layer with async patterns
- [ ] **TASK-039** Set up data caching with Microsoft.Extensions.Caching
- [ ] **TASK-040** Implement data validation and integrity checks

#### 2.3 Configuration and Settings
- [ ] **TASK-041** Set up .NET configuration system with JSON providers
- [ ] **TASK-042** Create user settings management system
- [ ] **TASK-043** Implement theme and appearance settings
- [ ] **TASK-044** Configure application-level settings management
- [ ] **TASK-045** Set up settings backup and restore functionality
- [ ] **TASK-046** Implement settings validation and migration
- [ ] **TASK-047** Create settings UI with modern WPF controls
- [ ] **TASK-048** Set up settings change notification system

### Phase 3: Authentication System (Weeks 5-6)
**Goal:** Implement OAuth2 authentication with native Windows integration

#### 3.1 OAuth2 PKCE Implementation
- [ ] **TASK-049** Create OAuth2 service with System.Net.Http.HttpClient
- [ ] **TASK-050** Implement PKCE flow with System.Security.Cryptography
- [ ] **TASK-051** Set up embedded browser control for EVE login
- [ ] **TASK-052** Configure callback URL handling with localhost server
- [ ] **TASK-053** Implement state parameter validation and security
- [ ] **TASK-054** Create token management service
- [ ] **TASK-055** Set up automatic token refresh with hosted services
- [ ] **TASK-056** Implement secure token storage with Windows Credential Manager

#### 3.2 Multi-Character Support
- [ ] **TASK-057** Design character management ViewModels
- [ ] **TASK-058** Create character switching UI with modern WPF
- [ ] **TASK-059** Implement character data synchronization
- [ ] **TASK-060** Set up character-specific settings and preferences
- [ ] **TASK-061** Create character authentication status monitoring
- [ ] **TASK-062** Implement character logout and cleanup
- [ ] **TASK-063** Set up character data caching and persistence
- [ ] **TASK-064** Create character management navigation

#### 3.3 ESI Integration Layer
- [ ] **TASK-065** Create typed ESI client with Refit 7.1+
- [ ] **TASK-066** Implement ESI rate limiting with Polly 8.4+
- [ ] **TASK-067** Set up ESI response caching strategies
- [ ] **TASK-068** Create ESI error handling and retry policies
- [ ] **TASK-069** Implement ESI scope management
- [ ] **TASK-070** Set up ESI data validation and serialization
- [ ] **TASK-071** Create ESI monitoring and diagnostics
- [ ] **TASK-072** Implement offline mode with cached data

### Phase 4: Main UI & Navigation (Weeks 7-8)
**Goal:** Create modern Windows 11 UI foundation

#### 4.1 Main Window and Layout
- [ ] **TASK-073** Design main window with Windows 11 aesthetics
- [ ] **TASK-074** Implement navigation system with modern patterns
- [ ] **TASK-075** Create responsive layout with grid and panels
- [ ] **TASK-076** Set up window state management and persistence
- [ ] **TASK-077** Implement title bar customization
- [ ] **TASK-078** Create status bar with real-time information
- [ ] **TASK-079** Set up keyboard shortcuts and accessibility
- [ ] **TASK-080** Implement window management (minimize to tray, etc.)

#### 4.2 Authentication UI
- [ ] **TASK-081** Create modern login interface
- [ ] **TASK-082** Design character selection and management UI
- [ ] **TASK-083** Implement character switching interface
- [ ] **TASK-084** Create authentication status indicators
- [ ] **TASK-085** Design OAuth flow user experience
- [ ] **TASK-086** Implement authentication error handling UI
- [ ] **TASK-087** Create character portrait and information display
- [ ] **TASK-088** Set up authentication progress indicators

### Phase 5: Ship Fitting System (Weeks 9-10)
**Goal:** Core ship fitting functionality with native WPF interface

#### 5.1 Ship Fitting Interface
- [ ] **TASK-089** Design drag-and-drop fitting interface
- [ ] **TASK-090** Create slot management with modern controls
- [ ] **TASK-091** Implement module browser with advanced search
- [ ] **TASK-092** Design fitting comparison interface
- [ ] **TASK-093** Create performance calculation displays
- [ ] **TASK-094** Implement fitting templates and presets UI
- [ ] **TASK-095** Design constraint validation indicators
- [ ] **TASK-096** Create fitting export/import interface

#### 5.2 Ship Fitting Engine
- [ ] **TASK-097** Implement DPS calculations with precision
- [ ] **TASK-098** Create tank calculations (shield/armor/hull)
- [ ] **TASK-099** Implement capacitor stability analysis
- [ ] **TASK-100** Create speed and agility calculations
- [ ] **TASK-101** Implement targeting system calculations
- [ ] **TASK-102** Create stacking penalty system
- [ ] **TASK-103** Implement skill-based calculation adjustments
- [ ] **TASK-104** Set up ammunition and charge effects

### Phase 6: 3D Ship Visualization (Weeks 11-12)
**Goal:** 3D ship viewer with hardware acceleration

#### 6.1 3D Visualization System
- [ ] **TASK-105** Integrate HelixToolkit for 3D ship rendering
- [ ] **TASK-106** Implement ship model loading and caching
- [ ] **TASK-107** Create camera controls and navigation
- [ ] **TASK-108** Set up module highlighting and selection
- [ ] **TASK-109** Implement material and lighting systems
- [ ] **TASK-110** Create particle effects for visual enhancement
- [ ] **TASK-111** Set up 3D performance optimization
- [ ] **TASK-112** Implement fallback 2D mode for low-end systems

### Phase 7: Market Analysis & Character Planning (Weeks 13-14)
**Goal:** Advanced features for market analysis and character development

#### 7.1 Market Analysis System
- [ ] **TASK-113** Create market data retrieval services
- [ ] **TASK-114** Implement price history tracking
- [ ] **TASK-115** Set up market order monitoring
- [ ] **TASK-116** Create price alert system
- [ ] **TASK-117** Implement market trend analysis
- [ ] **TASK-118** Create cost calculation engines
- [ ] **TASK-119** Set up profit/loss analysis
- [ ] **TASK-120** Implement market data visualization

#### 7.2 Character Planning System
- [ ] **TASK-121** Create skill tree visualization
- [ ] **TASK-122** Implement skill queue optimization
- [ ] **TASK-123** Set up training time calculations
- [ ] **TASK-124** Create character goal setting
- [ ] **TASK-125** Implement skill comparison tools
- [ ] **TASK-126** Set up attribute remapping analysis
- [ ] **TASK-127** Create implant effect calculations
- [ ] **TASK-128** Implement skill plan export/import

### Phase 8: Testing & Quality Assurance (Weeks 15)
**Goal:** Comprehensive testing and quality validation

#### 8.1 Testing Infrastructure
- [ ] **TASK-129** Set up xUnit testing framework
- [ ] **TASK-130** Create unit tests for core services
- [ ] **TASK-131** Implement integration tests for ESI services
- [ ] **TASK-132** Set up UI automation tests with WinAppDriver
- [ ] **TASK-133** Create performance and load testing
- [ ] **TASK-134** Implement memory leak detection tests
- [ ] **TASK-135** Set up continuous testing in CI/CD
- [ ] **TASK-136** Create test data management

#### 8.2 Quality Assurance
- [ ] **TASK-137** Validate calculation accuracy against EVE Online
- [ ] **TASK-138** Test Windows integration features
- [ ] **TASK-139** Perform security testing and validation
- [ ] **TASK-140** Conduct accessibility testing
- [ ] **TASK-141** Test high DPI and scaling scenarios
- [ ] **TASK-142** Validate performance benchmarks
- [ ] **TASK-143** Conduct user acceptance testing
- [ ] **TASK-144** Test application stability and reliability

### Phase 9: Deployment & Distribution (Weeks 16)
**Goal:** Windows application packaging and distribution

#### 9.1 Application Packaging
- [ ] **TASK-145** Configure MSIX packaging for modern deployment
- [ ] **TASK-146** Set up Windows application signing
- [ ] **TASK-147** Create installer with WiX Toolset v5
- [ ] **TASK-148** Configure auto-updater with Squirrel.Windows
- [ ] **TASK-149** Set up crash reporting with Sentry
- [ ] **TASK-150** Create application telemetry and analytics
- [ ] **TASK-151** Configure Windows Store packaging
- [ ] **TASK-152** Set up distribution channels

#### 9.2 Production Deployment
- [ ] **TASK-153** Configure production build optimization
- [ ] **TASK-154** Set up release automation pipeline
- [ ] **TASK-155** Create deployment verification tests
- [ ] **TASK-156** Set up monitoring and diagnostics
- [ ] **TASK-157** Configure backup and recovery procedures
- [ ] **TASK-158** Create user documentation and help system
- [ ] **TASK-159** Set up customer support infrastructure
- [ ] **TASK-160** Plan go-to-market strategy

---

## 📊 Progress Tracking

### **Overall Project Status**
| Phase | Tasks | Completed | In Progress | Pending | Progress | Timeline |
|-------|--------|-----------|-------------|---------|----------|----------|
| 0: Cleanup & Docs | 3 | 3 | 0 | 0 | 100% | Week 0 |
| 1: Foundation | 24 | 0 | 0 | 24 | 0% | Weeks 1-2 |
| 2: Architecture | 24 | 0 | 0 | 24 | 0% | Weeks 3-4 |
| 3: Authentication | 24 | 0 | 0 | 24 | 0% | Weeks 5-6 |
| 4: UI Foundation | 16 | 0 | 0 | 16 | 0% | Weeks 7-8 |
| 5: Ship Fitting | 16 | 0 | 0 | 16 | 0% | Weeks 9-10 |
| 6: 3D Visualization | 8 | 0 | 0 | 8 | 0% | Weeks 11-12 |
| 7: Advanced Features | 16 | 0 | 0 | 16 | 0% | Weeks 13-14 |
| 8: Testing & QA | 16 | 0 | 0 | 16 | 0% | Week 15 |
| 9: Deployment | 16 | 0 | 0 | 16 | 0% | Week 16 |
| **TOTAL** | **163** | **3** | **0** | **160** | **2%** | **17 Weeks** |

### **Current Status**
- **Active Phase:** Phase 1 - Foundation & Project Setup
- **Next Task:** TASK-001 - Create .NET 9 WPF project with Windows 11 targeting
- **Overall Progress:** 2% (3/163 tasks completed)
- **Estimated Timeline:** 17 weeks to production release (includes cleanup week)

---

## 🎯 Success Criteria

### **Technical Requirements**
- **Performance:** Application launches in under 2 seconds
- **Memory Usage:** Under 100MB during normal operation
- **Calculation Accuracy:** Within 0.01% of EVE Online server values
- **Windows Integration:** Full Windows 11 native features and theming
- **3D Performance:** 60+ FPS for ship visualization on mid-range hardware

### **Quality Standards**
- **Bundle Size:** Under 50MB installed footprint
- **Startup Time:** 1-2 seconds cold start
- **UI Responsiveness:** Sub-100ms response to all user interactions
- **Crash Rate:** Less than 0.1% during normal operation
- **Test Coverage:** 80%+ code coverage with automated tests

### **User Experience Goals**
- **Native Windows Feel:** Fully integrated with Windows 11 design system
- **Professional Quality:** Matches or exceeds commercial software standards
- **EVE Integration:** Seamless workflow with EVE Online game client
- **Multi-Character Support:** Efficient management of multiple EVE characters
- **Offline Capability:** Core features work without internet connection

---

## 🛠️ Development Workflow

### **Task Management**
1. **Sequential Development:** Complete phases in order to ensure proper foundation
2. **Task Tracking:** Update progress after each completed task
3. **Quality Gates:** Complete testing before moving to next phase
4. **Documentation:** Maintain architectural decisions and patterns

### **Quality Assurance**
- All features must meet EVE Online calculation accuracy standards
- Performance benchmarks must show improvement over existing tools
- Windows integration must provide value beyond web-based alternatives
- User experience must be superior to current EVE tools

### **Success Metrics**
- **Native Performance:** 75%+ improvement in startup and memory vs Electron
- **Feature Completeness:** 100% of planned features implemented and tested
- **Windows Integration:** Advanced features like system tray, notifications, file associations
- **User Satisfaction:** Production-ready software worthy of EVE Online community

---

*This plan delivers Gideon as a premier native Windows application that sets the standard for EVE Online desktop tools.*