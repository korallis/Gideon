# Gideon - EVE Online Toolkit

> **Status:** 🚧 In Development - Foundation Phase  
> **Version:** 2.0.0-alpha  
> **Tech Stack:** .NET 9.0 + WPF + Windows 11 Integration

A high-performance native Windows desktop application for EVE Online players featuring ship fitting, character planning, and market analysis with Windows 11 Fluent Design integration.

## 🚀 Quick Start

### Prerequisites
- **Windows 10 version 1903+** or **Windows 11** (recommended)
- **.NET 9.0 Runtime** (will be bundled with installer)
- **Visual Studio 2022 17.11+** (for development)
- **Git** for version control

### Development Setup

```bash
# Clone the repository
git clone https://github.com/your-username/Gideon.git
cd Gideon

# The WPF project will be created in Phase 1
# Legacy Electron code is archived in archive-electron/
```

### Build Commands (Coming in Phase 1)

```bash
# Development
dotnet run                    # Start application in development mode
dotnet watch run              # Start with hot reload

# Building
dotnet build                  # Debug build
dotnet build -c Release       # Release build
dotnet publish               # Publish self-contained

# Testing
dotnet test                   # Run unit tests
dotnet test --collect:"XPlat Code Coverage"  # With coverage
```

## 📋 Development Plan

This project follows a comprehensive **163-task development plan** organized into **10 phases**:

### **Phase 0: Project Cleanup & Documentation** ✅ **ACTIVE**
- Clean up legacy Electron artifacts
- Update project documentation
- Archive existing codebase for reference

### **Phase 1: Foundation & Project Setup** (Weeks 1-2)
- .NET 9 WPF project with Windows 11 targeting
- Modern WPF stack (CommunityToolkit.Mvvm, MaterialDesign, HelixToolkit)
- Windows integration foundation

### **Phase 2: Core Architecture & MVVM** (Weeks 3-4)
- MVVM architecture with CommunityToolkit.Mvvm
- Data layer with Entity Framework Core
- Configuration and settings management

### **Phase 3: Authentication System** (Weeks 5-6)
- OAuth2 PKCE implementation with native Windows integration
- Multi-character support
- ESI integration layer

### **Phase 4: Main UI & Navigation** (Weeks 7-8)
- Windows 11 Fluent Design main window
- Modern navigation system
- Authentication UI

### **Phase 5: Ship Fitting System** (Weeks 9-10)
- Native WPF drag-and-drop fitting interface
- High-precision calculation engine

### **Phase 6: 3D Ship Visualization** (Weeks 11-12)
- HelixToolkit 3D ship rendering
- Hardware-accelerated graphics

### **Phase 7: Market Analysis & Character Planning** (Weeks 13-14)
- Real-time market data integration
- Skill tree visualization and optimization

### **Phase 8: Testing & Quality Assurance** (Week 15)
- Comprehensive testing with xUnit
- Performance and security validation

### **Phase 9: Deployment & Distribution** (Week 16)
- MSIX packaging and Windows Store preparation
- Auto-updater and crash reporting

**Current Progress:** Phase 0 - Project Cleanup (67% complete)  
**Active Task:** TASK-000-B - Update README.md  
**Next Phase:** Phase 1 - Foundation & Project Setup

See [development-plan.md](./development-plan.md) for complete task breakdown and progress tracking.

## 🏗️ Architecture

### Technology Stack
- **.NET 9.0** - Latest .NET with C# 13 and native AOT support
- **WPF** - Windows Presentation Foundation for rich native UI
- **CommunityToolkit.Mvvm** - Modern MVVM framework with source generators
- **HelixToolkit.Wpf** - 3D visualization for ship viewer
- **MaterialDesignInXamlToolkit** - Modern Material Design UI components
- **Entity Framework Core** - Data persistence with SQLite
- **Refit** - Type-safe HTTP client for ESI API
- **Polly** - Resilience patterns for API calls
- **Windows 11 Features** - Native notifications, system tray, Fluent Design

### Project Structure (Planned)
```
Gideon.WPF/
├── Models/              # EVE data models and entities
├── ViewModels/          # MVVM ViewModels with CommunityToolkit
├── Views/               # XAML user interface
├── Services/            # Business logic and data services
│   ├── Authentication/ # ESI OAuth2 and token management
│   ├── Data/           # EVE data access and caching
│   ├── Fitting/        # Ship fitting calculations
│   └── Market/         # Market analysis services
├── Resources/           # Assets, styles, and resources
├── Utilities/           # Helper classes and extensions
└── Tests/              # Unit and integration tests
```

## 🎯 Key Features

### Core Modules
- **Ship Fitting:** Native drag-and-drop interface with 3D visualization
- **Character Management:** ESI authentication with Windows Credential Manager
- **Market Analysis:** Real-time pricing with native charts
- **Settings:** Windows-native preferences and theming

### Technical Excellence
- **Performance:** <2s startup, <100ms UI response, <100MB memory
- **Accuracy:** Ship calculations within 0.01% of EVE Online values
- **Security:** OAuth2 PKCE with Windows Credential Manager integration
- **Integration:** Full Windows 11 features (notifications, system tray, themes)

### Windows-Native Benefits
- **80% smaller** footprint than Electron (~30MB vs ~150MB)
- **75% faster** startup time (1-2s vs 3-5s)
- **Native performance** with hardware acceleration
- **Windows integration** - system tray, notifications, file associations
- **Professional feel** matching Windows 11 design standards

## 🔧 Configuration

### Application Settings (Planned)
```json
{
  "Authentication": {
    "ESIClientId": "your-esi-client-id",
    "RedirectUri": "http://localhost:3000/auth/callback",
    "Scopes": ["esi-skills.read_skills.v1", "..."]
  },
  "UI": {
    "Theme": "Windows11",
    "AccentColor": "Auto",
    "StartMinimized": false
  },
  "Performance": {
    "Enable3D": true,
    "MaxMemoryMB": 500,
    "CacheRetentionDays": 7
  }
}
```

### ESI Integration
Native Windows OAuth2 flow with secure credential storage using Windows Credential Manager. No need for separate developer accounts - streamlined user experience.

## 📦 Legacy Archive

The original Electron implementation has been archived to `archive-electron/` for reference. This archive contains:

- ✅ **Complete OAuth2 PKCE authentication system**
- ✅ **Multi-character support with secure token storage**
- ✅ **3D ship visualization with Babylon.js**
- ✅ **Ship fitting calculation engines**
- ✅ **Comprehensive React component library**

These proven patterns serve as reference for the WPF implementation while we build a superior native Windows experience.

## 🧪 Testing (Planned)

### Testing Strategy
```bash
# Unit Tests
dotnet test

# Integration Tests
dotnet test --filter Category=Integration

# UI Tests
# WinAppDriver automation tests

# Performance Tests
# Memory usage and startup time validation
```

### Quality Metrics
- **Calculation Accuracy:** Within 0.01% of EVE Online
- **Performance:** Startup <2s, Memory <100MB
- **Test Coverage:** 80%+ with comprehensive scenarios
- **Security:** Regular security audits and penetration testing

## 🚀 Migration Benefits

### Electron → WPF Improvements
| Metric | Electron | WPF Native | Improvement |
|--------|----------|------------|-------------|
| **Bundle Size** | ~150MB | ~30MB | **80% smaller** |
| **Memory Usage** | ~200MB | ~50MB | **75% reduction** |
| **Startup Time** | 3-5 seconds | 1-2 seconds | **75% faster** |
| **Windows Integration** | Limited | Excellent | **Native features** |
| **Performance** | Good | Excellent | **Hardware acceleration** |

## 🤝 Development Workflow

### Current Phase Process
1. **Check Active Task:** Review [development-plan.md](./development-plan.md)
2. **Update Progress:** Mark tasks as in_progress → completed
3. **Sequential Development:** Complete phases in order for proper foundation
4. **Quality Gates:** Test thoroughly before advancing phases
5. **Documentation:** Update progress and patterns

### Code Standards (Planned)
- **C# 13** with latest language features
- **MVVM pattern** with CommunityToolkit.Mvvm
- **Async/await** throughout for responsive UI
- **Dependency injection** with Microsoft.Extensions.DependencyInjection
- **Clean Architecture** principles
- **Windows 11 design guidelines**

## 📄 License

MIT License - see [LICENSE](./LICENSE) for details.

## 🔗 Links

- [Product Requirements](./prd.md) - Complete product specification
- [Development Plan](./development-plan.md) - Task breakdown and progress
- [Architecture Decision](./ARCHITECTURE_DECISION.md) - Technology choice rationale
- [Electron Archive](./archive-electron/) - Reference implementation
- [EVE Online ESI](https://esi.evetech.net/) - EVE API documentation

---

**Building the future of EVE Online desktop applications with native Windows performance and integration.**