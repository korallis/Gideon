# Technology Stack for Gideon - EVE Online AI Copilot

## Executive Summary

This document outlines the technology stack for building Gideon, a game-like desktop application for EVE Online players. The stack is designed to deliver a sci-fi animated UI experience similar to EVE Online's Photon UI while meeting all performance and functionality requirements from the PRD.

## Primary Tech Stack - Electron + TypeScript + React + Babylon.js

### Core Framework
- **Electron 34.0.0** - Cross-platform desktop framework
- **TypeScript 5.8.3** - Type safety and developer experience  
- **React 19.1.0** - Component-based UI framework
- **Vite 6.3.5** - Fast build tool and dev server

### 3D Graphics & Animation
- **Babylon.js 8.13.0** - Complete 3D engine for game-like features
- **Framer Motion 11.x** - Advanced React animations
- **Lottie React** - Complex animations from After Effects
- **CSS-in-JS (Emotion)** - Dynamic styling with theming

### State Management & Data
- **Zustand** - Lightweight state management
- **React Query (TanStack Query)** - Server state management and caching
- **Dexie.js** - IndexedDB wrapper for local storage
- **Axios** - HTTP client for ESI API

### UI Components
- **Radix UI** - Unstyled, accessible components
- **React Hook Form** - Form management
- **React DnD** - Drag and drop functionality
- **React Virtual** - Virtualization for large lists

### Performance & Optimization
- **Web Workers** - Background processing for calculations
- **Service Workers** - Offline functionality and caching
- **React.memo & useMemo** - Performance optimization
- **Bundle analysis** - Code splitting and lazy loading

## 3D Graphics & Animation Strategy

### Babylon.js 8.13.0 Implementation

**Why Babylon.js:**
- Complete 3D engine with built-in physics, animations, and GUI
- Microsoft-backed with active development
- Better suited for game-like experiences than Three.js
- Built-in PBR rendering pipeline
- Native gamepad support
- New in 8.0: Area Lights, Node Render Graph, Enhanced IBL

**3D Features:**
```typescript
// Ship visualization pipeline
- GLTF/GLB model loading for EVE ships
- Real-time lighting and shadows
- Particle systems for effects
- Post-processing pipeline
- VR/AR ready architecture
```

**Animation Systems:**
- **UI Animations:** Framer Motion for React components
- **3D Animations:** Babylon.js animation system
- **Particle Effects:** Babylon.js particle system
- **CSS Animations:** Hardware-accelerated transforms

### Sci-Fi UI Design System

**EVE Online Photon UI Inspiration:**
- Dark theme with neon accents
- Glass morphism effects
- Glowing borders and highlights
- Holographic elements
- Military/technical typography
- Subtle particle backgrounds

**Color Palette:**
```css
--primary-bg: #0a0e1a
--secondary-bg: #1a1f2e
--accent-blue: #00d4ff
--accent-orange: #ff8c00
--success-green: #00ff88
--warning-yellow: #ffcc00
--error-red: #ff4444
--text-primary: #ffffff
--text-secondary: #a0a8b8
```

**Animation Principles:**
- Light-based effects (glows, flickers)
- Smooth 60fps transitions
- Parallax depth effects
- Interactive hover states
- Loading animations with progress
- Micro-interactions for feedback

## ESI API Integration Architecture

### Authentication Layer
```typescript
// OAuth2 PKCE flow implementation
interface ESIAuth {
  clientId: string
  redirectUri: string
  scopes: string[]
  codeChallenge: string
  state: string
}
```

**Pre-configured ESI Application Credentials:**
- **Client ID:** `53781295f2e644268c846a070cb5845d`
- **Client Secret:** `mjlNGicbibaoMMYshkpeP8JYM68W7Kg3B1YYm6XN`
- **Callback URL:** `eveauth-eva://callback`

**Note:** These credentials are pre-configured in the application, eliminating the need for end users to create their own EVE Developer accounts. Users can simply download and run the application without any additional setup.

**Libraries:**
- **oauth-pkce** - PKCE OAuth2 implementation
- **jose** - JWT token handling
- **keytar** - Secure credential storage

### API Client Design
```typescript
// ESI client with rate limiting and caching
class ESIClient {
  private rateLimiter: RateLimiter
  private cache: Cache
  private retryPolicy: RetryPolicy
}
```

**Features:**
- Automatic rate limiting (300 req/min where applicable)
- Intelligent caching with TTL
- Retry logic for transient failures
- Error handling and logging
- Background data synchronization

### Data Synchronization Strategy

**Real-time Updates:**
- Character skills and training queue
- Market data (15-minute intervals)
- Character assets and location
- Corporation information

**Offline Capability:**
- Local SQLite database with encrypted storage
- Background sync when online
- Conflict resolution for data updates
- Export/import functionality

## Performance Optimization

### Memory Management
```typescript
// Resource cleanup patterns
useEffect(() => {
  const cleanup = () => {
    // Dispose of Babylon.js resources
    scene.dispose()
    engine.dispose()
  }
  return cleanup
}, [])
```

### Bundle Optimization
- **Code Splitting:** Route-based and component-based
- **Tree Shaking:** Remove unused code
- **Asset Optimization:** Compressed textures and models
- **Lazy Loading:** Load components on demand

### Rendering Performance
- **React.memo:** Prevent unnecessary re-renders
- **Canvas Optimization:** Frame rate limiting
- **WebGL Context:** Efficient resource management
- **Background Processing:** Web Workers for calculations

## Development Workflow

### Build Pipeline
```json
{
  "scripts": {
    "dev": "concurrently \"vite\" \"electron .\"",
    "build": "tsc && vite build && electron-builder",
    "test": "vitest",
    "lint": "eslint . --ext .ts,.tsx",
    "format": "prettier --write ."
  }
}
```

### Testing Strategy
- **Unit Tests:** Vitest for logic testing
- **Component Tests:** React Testing Library
- **E2E Tests:** Playwright for desktop app testing
- **Visual Tests:** Chromatic for UI regression
- **Performance Tests:** Lighthouse CI

### Code Quality
- **ESLint + Prettier** - Code formatting and linting
- **Husky** - Git hooks for quality gates
- **TypeScript Strict Mode** - Type safety
- **SonarQube** - Code quality analysis

## Deployment & Distribution

### Build Targets
- **Windows:** NSIS installer, Microsoft Store
- **macOS:** DMG, Mac App Store
- **Linux:** AppImage, Snap, Flatpak

### Auto-Updates
- **electron-updater** - Automatic application updates
- **Progressive updates** - Download in background
- **Rollback capability** - Safe update mechanism

### Security
- **Code Signing:** Windows Authenticode, macOS notarization
- **CSP Headers:** Content Security Policy
- **Sandboxing:** Electron security features
- **Encrypted Storage:** User data protection

## File Structure

```
gideon/
├── src/
│   ├── main/              # Electron main process
│   ├── renderer/          # React frontend
│   │   ├── components/    # UI components
│   │   ├── services/      # ESI API services
│   │   ├── stores/        # State management
│   │   ├── utils/         # Utility functions
│   │   └── assets/        # Static assets
│   ├── shared/            # Shared types and constants
│   └── workers/           # Web Workers
├── public/                # Static files
├── tests/                 # Test files
├── docs/                  # Documentation
└── scripts/               # Build scripts
```

## Hardware Requirements

### Minimum System Requirements
- **OS:** Windows 10 1909+, macOS 10.15+, Ubuntu 18.04+
- **RAM:** 4GB (Application uses <500MB)
- **GPU:** DirectX 11 compatible
- **Storage:** 500MB free space
- **Network:** Internet connection for ESI API

### Recommended Specifications
- **RAM:** 8GB for optimal performance
- **GPU:** Dedicated graphics card for 3D rendering
- **CPU:** Multi-core processor for background calculations
- **Storage:** SSD for faster startup times

## Implementation Phases

### Phase 1: Core Infrastructure (Months 1-2)
- Set up Electron 34.0.0 + React 19.1.0 + TypeScript 5.8.3 foundation
- Implement basic ESI authentication
- Create core UI components with Radix UI
- Set up build and deployment pipeline with Vite 6.3.5

### Phase 2: 3D Integration (Months 2-3)
- Integrate Babylon.js 8.13.0 for 3D ship visualization
- Implement fitting interface with drag-and-drop
- Add performance calculations engine
- Create animation system with Framer Motion

### Phase 3: Advanced Features (Months 3-4)
- Market data integration and analysis
- Character planning and skill queue optimization
- Advanced fitting recommendations
- Polish UI animations and effects

## Key Package Versions Summary

- **Electron:** 34.0.0
- **TypeScript:** 5.8.3
- **React:** 19.1.0
- **Babylon.js:** 8.13.0
- **Vite:** 6.3.5

This technology stack provides a robust foundation for building Gideon with a game-like, animated UI that meets all PRD requirements. The combination of these modern, stable versions ensures optimal performance, developer experience, and visual fidelity for creating an EVE Online application that feels native to the game's aesthetic.