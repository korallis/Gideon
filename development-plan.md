# Gideon Development Master Plan

> **Status:** WPF Native Development | **Last Updated:** June 21, 2025  
> **Tech Stack:** .NET 9.0 + WPF + CommunityToolkit.Mvvm + HelixToolkit + Windows 11 Integration
> **Target:** High-performance native Windows desktop application for EVE Online

## 🎯 Project Overview

Building Gideon (EVE Online's AI Copilot) - a high-performance native Windows desktop application for EVE Online players featuring ship fitting, character planning, and market analysis with Windows 11 Fluent Design integration and Westworld-EVE fusion holographic UI.

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
- **Custom Shaders** - HLSL shaders for holographic effects
- **Particle Systems** - Real-time data visualization effects

---

## 🎨 UI Design Concept: Westworld-EVE Fusion

### **Core Design Principles**
- **Holographic Foundation:** Layered semi-transparent panels with depth effects
- **EVE's Military Palette:** Dark blues/blacks with electric blue accents and gold highlights
- **Animated Data Visualization:** Particle effects for data streams and 3D holographic projections

### **Key UI Components**
| Element | Westworld Inspiration | EVE Integration |
|---------|----------------------|-----------------|
| Main Panel | Glassmorphism with depth layers | EVE's angular panel borders with corporation insignia |
| Data Displays | Animated particle streams | EVE's tactical HUD elements and radar visuals |
| Icons | Glowing minimalist glyphs | EVE's redesigned iconography for modules/stations |
| Ship Viewer | 3D holographic projections | Interactive ship models with EVE's material textures |

### **Implementation Features**
- **Holographic Material System:** Glassmorphism panels with blur effects
- **Animated Data Streams:** Particle systems for market data visualization
- **3D Ship Visualization:** HelixToolkit integration with holographic materials
- **Character Authentication:** Animated 3D avatar with fluid movements
- **Market Analysis:** Particle streams representing price fluctuations
- **Ship Fitting Interface:** Module slots with "digital DNA" connection effects

---

## 📋 Development Phases - UI-First Approach

### Phase 0: Project Cleanup & Documentation (Week 0) 🎯 **COMPLETED**
**Goal:** Clean up legacy Electron artifacts and update project documentation

#### 0.1 Legacy Cleanup
- [x] **TASK-000** Clean up Electron artifacts and legacy files ✅
- [x] **TASK-000-B** Update README.md to reflect WPF-only development approach ✅
- [x] **TASK-000-C** Archive/organize existing codebase for reference ✅

### Phase 1: Foundation & Project Setup (Weeks 1-2) 🎯 **COMPLETED**
**Goal:** Establish modern .NET 9 WPF foundation with Windows 11 integration

#### 1.1 Project Infrastructure
- [x] **TASK-001** Create .NET 9 WPF project with Windows 11 targeting ✅
- [x] **TASK-002** Configure project structure with Clean Architecture patterns ✅
- [x] **TASK-003** Set up NuGet package management with Central Package Management ✅
- [x] **TASK-004** Configure Cursor IDE with .NET 9 and WPF development extensions ✅
- [x] **TASK-005** Set up Git integration and development workflow ✅
- [x] **TASK-006** Configure EditorConfig and .NET analyzers for code quality ✅
- [x] **TASK-007** Set up MSBuild targets for automated builds ✅
- [x] **TASK-008** Configure application manifest for Windows 11 features ✅

#### 1.2 Modern WPF Stack Setup
- [x] **TASK-009** Install CommunityToolkit.Mvvm 8.3+ for MVVM framework ✅
- [x] **TASK-010** Set up Microsoft.Extensions.DependencyInjection for IoC ✅
- [x] **TASK-011** Configure Microsoft.Extensions.Hosting for application lifecycle ✅
- [x] **TASK-012** Install MaterialDesignInXamlToolkit 5.1+ for modern UI ✅
- [x] **TASK-013** Set up HelixToolkit.Wpf for 3D ship visualization ✅
- [x] **TASK-014** Configure Serilog 8.0+ for structured logging ✅
- [x] **TASK-015** Install Microsoft.Toolkit.Win32.UI.Controls for modern controls ✅
- [x] **TASK-016** Set up FluentValidation 11.9+ for form validation ✅

#### 1.3 Windows Integration Foundation
- [x] **TASK-017** Configure Windows 11 Fluent Design System integration ✅
- [x] **TASK-018** Set up Windows.ApplicationModel for modern app features ✅
- [x] **TASK-019** Configure Windows Credential Manager integration ✅
- [x] **TASK-020** Set up Windows notification system integration ✅
- [x] **TASK-021** Configure system tray and jump list functionality ✅
- [x] **TASK-022** Implement Windows 11 theme awareness and accent colors ✅
- [x] **TASK-023** Set up Windows App SDK integration for enhanced features ✅
- [x] **TASK-024** Configure high DPI awareness and scaling ✅

### Phase 2: Holographic UI Foundation (Weeks 3-4) 🎨 **UI-FIRST**
**Goal:** Build complete Westworld-EVE fusion holographic UI system

#### 2.1 Holographic Material System
- [x] **TASK-025** Create glassmorphism panel templates with depth effects ✅
- [x] **TASK-026** Implement custom HLSL shaders for holographic rendering ✅
- [x] **TASK-027** Set up layered composition system (background, mid-layer, foreground) ✅
- [x] **TASK-028** Create EVE military color palette with electric blue and gold accents ✅
- [x] **TASK-029** Implement blur effects and transparency for glass panels ✅
- [x] **TASK-030** Set up angular panel borders with corporation insignia support ✅
- [ ] **TASK-031** Create glowing minimalist glyph icon system
- [ ] **TASK-032** Implement depth-based layering for holographic depth perception

#### 2.2 Animated Data Visualization
- [ ] **TASK-033** Create particle system for market data streams
- [ ] **TASK-034** Implement animated particle effects for price fluctuations
- [ ] **TASK-035** Set up 3D holographic graphs with EVE color scheme
- [ ] **TASK-036** Create tactical HUD elements and radar visuals
- [ ] **TASK-037** Implement data stream animations for real-time information
- [ ] **TASK-038** Set up performance-optimized particle rendering
- [ ] **TASK-039** Create fallback 2D visualization for low-end systems
- [ ] **TASK-040** Implement configurable animation intensity settings

#### 2.3 Core Holographic Controls
- [ ] **TASK-041** Create HoloPanel custom control with glassmorphism
- [ ] **TASK-042** Implement HoloButton with glowing effects and animations
- [ ] **TASK-043** Create HoloCard for data display with depth effects
- [ ] **TASK-044** Set up HoloProgressBar with particle stream effects
- [ ] **TASK-045** Implement HoloDataGrid with animated row highlighting
- [ ] **TASK-046** Create HoloNavigation with holographic transitions
- [ ] **TASK-047** Set up HoloStatusBar with real-time data streams
- [ ] **TASK-048** Implement HoloTooltip with contextual information display

### Phase 3: Main UI Shell & Navigation (Weeks 5-6) 🎨 **UI-FIRST**
**Goal:** Build complete application shell with holographic navigation

#### 3.1 Main Window & Shell
- [ ] **TASK-049** Design holographic main window with Windows 11 aesthetics
- [ ] **TASK-050** Implement holographic navigation system with particle transitions
- [ ] **TASK-051** Create responsive holographic layout with adaptive panels
- [ ] **TASK-052** Set up holographic window chrome with EVE styling
- [ ] **TASK-053** Implement holographic title bar with character info display
- [ ] **TASK-054** Create holographic status bar with real-time data streams
- [ ] **TASK-055** Set up holographic sidebar navigation with module icons
- [ ] **TASK-056** Implement holographic breadcrumb navigation

#### 3.2 Authentication UI Shell
- [ ] **TASK-057** Create holographic login interface with Westworld aesthetics
- [ ] **TASK-058** Design character selection with 3D holographic projections
- [ ] **TASK-059** Implement character switching with fluid particle animations
- [ ] **TASK-060** Create holographic authentication status indicators
- [ ] **TASK-061** Design OAuth flow with holographic progress indicators
- [ ] **TASK-062** Implement holographic error handling with animated feedback
- [ ] **TASK-063** Create holographic character portrait display system
- [ ] **TASK-064** Set up holographic corporation insignia animations

#### 3.3 Module Navigation Shell
- [ ] **TASK-065** Create holographic ship fitting module interface
- [ ] **TASK-066** Design holographic market analysis module shell
- [ ] **TASK-067** Implement holographic character planning module UI
- [ ] **TASK-068** Set up holographic settings and preferences interface
- [ ] **TASK-069** Create holographic help and documentation system
- [ ] **TASK-070** Implement holographic search interface with particle effects
- [ ] **TASK-071** Set up holographic notification center
- [ ] **TASK-072** Create holographic quick actions interface

### Phase 4: Ship Fitting Holographic Interface (Weeks 7-8) 🎨 **UI-FIRST**
**Goal:** Complete holographic ship fitting interface

#### 4.1 Holographic Ship Viewer
- [ ] **TASK-073** Create 3D holographic ship projection system
- [ ] **TASK-074** Implement holographic ship materials with EVE textures
- [ ] **TASK-075** Set up interactive holographic ship rotation and zoom
- [ ] **TASK-076** Create holographic module slot highlighting system
- [ ] **TASK-077** Implement "digital DNA" connection effects for modules
- [ ] **TASK-078** Set up holographic ship statistics overlay
- [ ] **TASK-079** Create holographic damage visualization system
- [ ] **TASK-080** Implement holographic ship comparison interface

#### 4.2 Holographic Fitting Interface
- [ ] **TASK-081** Design holographic drag-and-drop module interface
- [ ] **TASK-082** Create holographic module browser with animated search
- [ ] **TASK-083** Implement holographic fitting slot management
- [ ] **TASK-084** Set up holographic module compatibility indicators
- [ ] **TASK-085** Create holographic performance metrics display
- [ ] **TASK-086** Implement holographic fitting templates interface
- [ ] **TASK-087** Set up holographic constraint validation system
- [ ] **TASK-088** Create holographic export/import interface

#### 4.3 Holographic Data Visualization
- [ ] **TASK-089** Create real-time holographic DPS visualization
- [ ] **TASK-090** Implement holographic capacitor flow animation
- [ ] **TASK-091** Set up holographic tank effectiveness display
- [ ] **TASK-092** Create holographic speed and agility indicators
- [ ] **TASK-093** Implement holographic targeting system visualization
- [ ] **TASK-094** Set up holographic stacking penalty indicators
- [ ] **TASK-095** Create holographic skill requirement display
- [ ] **TASK-096** Implement holographic ammunition effects visualization

### Phase 5: Market Analysis Holographic Interface (Weeks 9-10) 🎨 **UI-FIRST**
**Goal:** Complete holographic market analysis interface

#### 5.1 Holographic Market Data Visualization
- [ ] **TASK-097** Create particle streams for price fluctuation visualization
- [ ] **TASK-098** Implement holographic 3D market graphs with EVE colors
- [ ] **TASK-099** Set up holographic price history timeline
- [ ] **TASK-100** Create holographic market order book display
- [ ] **TASK-101** Implement holographic profit/loss indicators
- [ ] **TASK-102** Set up holographic market alerts with animated notifications
- [ ] **TASK-103** Create holographic market trend analysis display
- [ ] **TASK-104** Implement holographic regional market comparison

#### 5.2 Holographic Trading Interface
- [ ] **TASK-105** Design holographic item search with particle effects
- [ ] **TASK-106** Create holographic trading calculator interface
- [ ] **TASK-107** Implement holographic portfolio tracking display
- [ ] **TASK-108** Set up holographic market watchlist interface
- [ ] **TASK-109** Create holographic trading history visualization
- [ ] **TASK-110** Implement holographic market predictions display
- [ ] **TASK-111** Set up holographic arbitrage opportunity indicators
- [ ] **TASK-112** Create holographic market news and events display

### Phase 6: Character Planning Holographic Interface (Weeks 11-12) 🎨 **UI-FIRST**
**Goal:** Complete holographic character planning interface

#### 6.1 Holographic Skill Visualization
- [ ] **TASK-113** Create holographic skill tree with animated connections
- [ ] **TASK-114** Implement holographic character progression timeline
- [ ] **TASK-115** Set up holographic skill queue visualization
- [ ] **TASK-116** Create holographic skill comparison interface
- [ ] **TASK-117** Implement holographic training time predictions
- [ ] **TASK-118** Set up holographic attribute remapping analysis
- [ ] **TASK-119** Create holographic implant effect visualization
- [ ] **TASK-120** Implement holographic skill plan export/import

#### 6.2 Holographic Character Management
- [ ] **TASK-121** Design holographic character overview dashboard
- [ ] **TASK-122** Create holographic character asset visualization
- [ ] **TASK-123** Implement holographic character location tracking
- [ ] **TASK-124** Set up holographic character goal setting interface
- [ ] **TASK-125** Create holographic character statistics display
- [ ] **TASK-126** Implement holographic character comparison tools
- [ ] **TASK-127** Set up holographic character backup interface
- [ ] **TASK-128** Create holographic character sharing system

### Phase 7: Backend Integration & Data Services (Weeks 13-14) ⚙️ **FUNCTIONALITY**
**Goal:** Connect holographic UI to functional backend services

#### 7.1 MVVM Architecture Implementation
- [ ] **TASK-129** Implement ViewModels for all holographic interfaces
- [ ] **TASK-130** Create data binding for holographic controls
- [ ] **TASK-131** Set up command routing for holographic interactions
- [ ] **TASK-132** Implement property change notifications
- [ ] **TASK-133** Create validation systems for holographic forms
- [ ] **TASK-134** Set up messaging patterns between holographic modules
- [ ] **TASK-135** Implement state management for holographic UI
- [ ] **TASK-136** Create navigation service for holographic transitions

#### 7.2 Data Layer Integration
- [ ] **TASK-137** Connect Entity Framework to holographic interfaces
- [ ] **TASK-138** Implement data repositories for ship fitting
- [ ] **TASK-139** Set up market data services
- [ ] **TASK-140** Create character data management
- [ ] **TASK-141** Implement data caching strategies
- [ ] **TASK-142** Set up data synchronization services
- [ ] **TASK-143** Create data validation and integrity checks
- [ ] **TASK-144** Implement offline data support

### Phase 8: EVE Online Integration (Weeks 15-16) 🚀 **FUNCTIONALITY**
**Goal:** Connect to EVE Online services and data

#### 8.1 ESI API Integration
- [ ] **TASK-145** Implement OAuth2 PKCE authentication flow
- [ ] **TASK-146** Create ESI client with rate limiting
- [ ] **TASK-147** Set up character data synchronization
- [ ] **TASK-148** Implement market data retrieval
- [ ] **TASK-149** Create skill queue monitoring
- [ ] **TASK-150** Set up asset tracking
- [ ] **TASK-151** Implement notification system
- [ ] **TASK-152** Create real-time data updates

#### 8.2 Ship Fitting Engine
- [ ] **TASK-153** Implement DPS calculations with EVE accuracy
- [ ] **TASK-154** Create tank calculations (shield/armor/hull)
- [ ] **TASK-155** Implement capacitor stability analysis
- [ ] **TASK-156** Create speed and agility calculations
- [ ] **TASK-157** Implement targeting system calculations
- [ ] **TASK-158** Create stacking penalty system
- [ ] **TASK-159** Implement skill-based calculation adjustments
- [ ] **TASK-160** Set up ammunition and charge effects

### Phase 9: Performance Optimization & Polish (Week 17) ⚡ **OPTIMIZATION**
**Goal:** Optimize holographic effects and overall performance

#### 9.1 Performance Optimization
- [ ] **TASK-161** Optimize particle system performance
- [ ] **TASK-162** Implement LOD system for holographic effects
- [ ] **TASK-163** Optimize shader performance
- [ ] **TASK-164** Implement memory management for animations
- [ ] **TASK-165** Optimize 3D rendering performance
- [ ] **TASK-166** Implement adaptive quality settings
- [ ] **TASK-167** Create performance monitoring system
- [ ] **TASK-168** Optimize startup time and memory usage

#### 9.2 Testing & Quality Assurance
- [ ] **TASK-169** Test holographic effects on different hardware
- [ ] **TASK-170** Validate calculation accuracy against EVE
- [ ] **TASK-171** Test Windows integration features
- [ ] **TASK-172** Conduct accessibility testing
- [ ] **TASK-173** Test high DPI and scaling scenarios
- [ ] **TASK-174** Validate holographic UI responsiveness
- [ ] **TASK-175** Conduct user experience testing
- [ ] **TASK-176** Test application stability and reliability

### Phase 10: Deployment & Distribution (Week 18) 📦 **RELEASE**
**Goal:** Package and distribute the application

#### 10.1 Application Packaging
- [ ] **TASK-177** Configure MSIX packaging for modern deployment
- [ ] **TASK-178** Set up Windows application signing
- [ ] **TASK-179** Create installer with WiX Toolset v5
- [ ] **TASK-180** Configure auto-updater system
- [ ] **TASK-181** Set up crash reporting and telemetry
- [ ] **TASK-182** Create user documentation
- [ ] **TASK-183** Set up distribution channels
- [ ] **TASK-184** Plan release and marketing strategy

---

## 📊 Progress Tracking

### **Overall Project Status**
| Phase | Tasks | Completed | In Progress | Pending | Progress | Timeline |
|-------|--------|-----------|-------------|---------|----------|----------|
| 0: Cleanup & Docs | 3 | 3 | 0 | 0 | 100% | Week 0 |
| 1: Foundation | 24 | 24 | 0 | 0 | 100% | Weeks 1-2 |
| 2: Holographic UI | 24 | 6 | 0 | 18 | 25% | Weeks 3-4 |
| 3: Main UI Shell | 24 | 0 | 0 | 24 | 0% | Weeks 5-6 |
| 4: Ship Fitting UI | 24 | 0 | 0 | 24 | 0% | Weeks 7-8 |
| 5: Market Analysis UI | 16 | 0 | 0 | 16 | 0% | Weeks 9-10 |
| 6: Character Planning UI | 16 | 0 | 0 | 16 | 0% | Weeks 11-12 |
| 7: Backend Integration | 16 | 0 | 0 | 16 | 0% | Weeks 13-14 |
| 8: EVE Integration | 16 | 0 | 0 | 16 | 0% | Weeks 15-16 |
| 9: Optimization | 16 | 0 | 0 | 16 | 0% | Week 17 |
| 10: Deployment | 8 | 0 | 0 | 8 | 0% | Week 18 |
| **TOTAL** | **191** | **33** | **0** | **158** | **17%** | **18 Weeks** |

### **Current Status**
- **Active Phase:** Phase 2 - Holographic UI Foundation (In Progress!)
- **Next Task:** TASK-031 - Create glowing minimalist glyph icon system
- **Previous Task:** TASK-030 - Set up angular panel borders with corporation insignia support ✅ **COMPLETED**
- **Overall Progress:** 17% (33/191 tasks completed)
- **Estimated Timeline:** 18 weeks to production release

---

## 🎯 Success Criteria

### **Technical Requirements**
- **Performance:** Application launches in under 2 seconds
- **Memory Usage:** Under 150MB during normal operation (increased for holographic effects)
- **Calculation Accuracy:** Within 0.01% of EVE Online server values
- **Windows Integration:** Full Windows 11 native features and theming
- **Holographic Performance:** Smooth 30+ FPS for particle systems and animations
- **3D Performance:** 60+ FPS for ship visualization on mid-range hardware

### **Quality Standards**
- **Bundle Size:** Under 75MB installed footprint (increased for shaders)
- **Startup Time:** 1-2 seconds cold start
- **UI Responsiveness:** Sub-100ms response to all interactions
- **Holographic Effects:** Consistent frame rates across different hardware
- **Crash Rate:** Less than 0.1% during normal operation
- **Test Coverage:** 80%+ code coverage with automated tests

### **User Experience Goals**
- **Westworld-EVE Aesthetic:** Immersive holographic UI that feels like EVE's future
- **Professional Quality:** Exceeds commercial software standards
- **EVE Integration:** Seamless workflow with EVE Online game client
- **Multi-Character Support:** Efficient management of multiple EVE characters
- **Offline Capability:** Core features work without internet connection
- **Accessibility:** Full screen reader and accessibility support

---

## 🛠️ Development Workflow

### **UI-First Development Strategy**
1. **Complete Visual Foundation:** Build all holographic UI components first
2. **Progressive Enhancement:** Add functionality to existing beautiful interfaces  
3. **Parallel Development:** UI and backend teams can work simultaneously
4. **User Testing:** Early feedback on holographic design and usability
5. **Performance Optimization:** Optimize holographic effects throughout development

### **Quality Assurance**
- All holographic effects must enhance usability without compromising performance
- EVE Online calculation accuracy standards must be met
- Windows integration must provide value beyond web-based alternatives
- Holographic UI must be accessible and performant across hardware configurations

### **Success Metrics**
- **Visual Excellence:** UI that sets new standards for desktop EVE tools
- **Native Performance:** 75%+ improvement in startup and memory vs Electron
- **Feature Completeness:** 100% of planned features implemented and tested
- **User Satisfaction:** Production-ready software worthy of EVE Online community
- **Innovation:** Pioneering holographic desktop application design

---

*This plan delivers Gideon as a premier native Windows application with Westworld-EVE fusion holographic UI that revolutionizes EVE Online desktop tools.*