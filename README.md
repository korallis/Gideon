# Gideon - EVE Online AI Copilot

> **Status:** 🚧 In Development - Alpha Phase  
> **Version:** 0.1.0  
> **Tech Stack:** Electron + TypeScript + React + Babylon.js

A comprehensive desktop application for EVE Online players featuring ship fitting, character planning, and market analysis with a game-like sci-fi UI.

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- npm 8+
- Git

### Installation

```bash
# Clone the repository
git clone https://github.com/your-org/gideon.git
cd gideon

# Install dependencies
npm install

# Start development server
npm run dev
```

### Development Commands

```bash
# Development
npm run dev                 # Start dev server with hot reload
npm run dev:vite           # Start Vite dev server only
npm run dev:electron       # Start Electron only (requires Vite running)

# Building
npm run build              # Build renderer and main processes
npm run build:renderer     # Build React frontend only
npm run build:main         # Build Electron main process only
npm run build:all          # Build and package for distribution

# Testing
npm test                   # Run unit tests
npm run test:ui            # Run tests with UI
npm run test:coverage      # Run tests with coverage
npm run test:e2e           # Run E2E tests

# Code Quality
npm run lint               # Run ESLint
npm run lint:fix           # Fix ESLint issues
npm run format             # Format code with Prettier
npm run format:check       # Check code formatting
npm run typecheck          # Check TypeScript types
```

## 📋 Development Plan

This project follows a comprehensive 144-task development plan organized into 7 phases:

1. **Phase 1: Foundation** - Project setup, tooling, and core architecture
2. **Phase 2: Ship Fitting** - 3D visualization and fitting interface  
3. **Phase 3: Character Management** - ESI integration and skill planning
4. **Phase 4: Market Analysis** - Real-time market data and analysis
5. **Phase 5: Advanced Features** - AI optimization and polish
6. **Phase 6: Testing & QA** - Comprehensive testing and quality assurance
7. **Phase 7: Deployment** - Build, packaging, and distribution

**Current Progress:** Phase 1 - Foundation (0% complete)  
**Active Task:** TASK-001 - Initialize Node.js project

See [development-plan.md](./development-plan.md) for the complete task breakdown and progress tracking.

## 🏗️ Architecture

### Tech Stack
- **Frontend:** React 19.1.0 + TypeScript 5.8.3
- **Desktop:** Electron 34.0.0
- **Build:** Vite 6.3.5 with hot reload
- **3D Graphics:** Babylon.js 8.13.0
- **Styling:** Emotion CSS-in-JS + EVE-inspired design system
- **State:** Zustand + React Query
- **Testing:** Vitest + Playwright + React Testing Library

### Project Structure
```
src/
├── main/              # Electron main process
│   ├── main.ts        # App entry point
│   └── preload.ts     # Secure IPC bridge
├── renderer/          # React frontend
│   ├── components/    # UI components
│   ├── services/      # API services
│   ├── stores/        # State management
│   ├── styles/        # Styling and themes
│   └── utils/         # Utilities
├── shared/            # Shared code
│   ├── types/         # TypeScript definitions
│   ├── constants/     # App constants
│   └── utils/         # Shared utilities
├── workers/           # Web Workers
└── test/              # Test utilities
```

## 🎯 Key Features

### Core Modules
- **Ship Fitting:** Drag-and-drop interface with 3D visualization
- **Character Management:** ESI authentication and skill planning
- **Market Analysis:** Real-time pricing and trading tools
- **Settings:** Customizable preferences and performance tuning

### Technical Highlights
- **Accuracy:** Ship calculations within 0.1% of EVE Online values
- **Performance:** <5s startup, <100ms UI response, <500MB memory
- **Security:** OAuth2 PKCE with secure credential storage
- **Offline Mode:** Local data storage with background sync

## 🔧 Configuration

### Environment Variables
```bash
NODE_ENV=development          # development | production | test
VITE_ESI_CLIENT_ID=your_id   # ESI application client ID (optional)
VITE_API_BASE_URL=...        # Custom API base URL (optional)
```

### ESI Integration
The application includes pre-configured ESI credentials for seamless user experience. Users can authenticate directly without creating developer accounts.

## 🧪 Testing

### Unit Tests
```bash
npm test                      # Run all tests
npm run test:ui              # Visual test runner
npm run test:coverage        # Coverage report
```

### E2E Tests
```bash
npm run test:e2e             # Run Playwright tests
npx playwright show-report   # View test report
```

### Performance Testing
- Memory usage monitoring
- Startup time benchmarks
- UI response time validation
- 3D rendering performance

## 📦 Building & Distribution

### Development Build
```bash
npm run build               # Build for development
```

### Production Build
```bash
npm run build:all          # Build and package
```

### Supported Platforms
- **Windows:** NSIS installer, Microsoft Store
- **macOS:** DMG, Mac App Store  
- **Linux:** AppImage, Snap, Flatpak

## 🤝 Contributing

This project is in active development. See [development-plan.md](./development-plan.md) for current tasks and progress.

### Development Workflow
1. Check current task in development plan
2. Create feature branch: `git checkout -b feature/task-xxx`
3. Implement changes following existing patterns
4. Run tests: `npm test` and `npm run test:e2e`
5. Check code quality: `npm run lint` and `npm run typecheck`
6. Update development plan with progress
7. Submit pull request

### Code Standards
- TypeScript strict mode
- ESLint + Prettier formatting
- 100% test coverage for critical paths
- Performance budget compliance
- Security audit compliance

## 📄 License

MIT License - see [LICENSE](./LICENSE) for details.

## 🔗 Links

- [Product Requirements](./prd.md) - Complete product specification
- [Technology Stack](./tech.md) - Detailed technical architecture  
- [Development Plan](./development-plan.md) - Task breakdown and progress
- [EVE Online ESI](https://esi.evetech.net/) - EVE API documentation

---

**Note:** This is an alpha release under active development. Features and APIs may change significantly.