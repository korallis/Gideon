# Gideon Styling System

A comprehensive CSS utility system inspired by Tailwind CSS, built specifically for the EVE Online aesthetic.

## Overview

The styling system provides:
- **Utility Classes**: Tailwind-inspired utility classes for rapid development
- **Component Classes**: Pre-built component styles for common patterns
- **Theme Configuration**: Centralized theme tokens for consistent styling
- **CSS Custom Properties**: CSS variables for dynamic theming

## Quick Start

### Using Utility Classes

```tsx
// Flex layout with spacing
<div className="flex items-center justify-between p-4 bg-secondary">
  <h1 className="text-xl font-semibold text-primary">Title</h1>
  <button className="btn btn-primary">Action</button>
</div>

// Card layout
<div className="card">
  <div className="card-header">
    <h2 className="card-title">Ship Fitting</h2>
  </div>
  <div className="card-body">
    <p className="text-secondary">Content goes here</p>
  </div>
</div>

// Grid layout
<div className="grid grid-cols-3 gap-4">
  <div className="bg-glass p-4 rounded-lg">Item 1</div>
  <div className="bg-glass p-4 rounded-lg">Item 2</div>
  <div className="bg-glass p-4 rounded-lg">Item 3</div>
</div>
```

### Using Component Classes

```tsx
// Buttons
<button className="btn btn-primary">Primary Action</button>
<button className="btn btn-secondary">Secondary</button>
<button className="btn btn-ghost">Ghost</button>

// Inputs
<input className="input" placeholder="Enter text..." />
<input className="input input-lg" placeholder="Large input" />

// Cards with glass effect
<div className="card card-glass">
  <div className="card-body">
    <h3 className="card-title">Glass Card</h3>
    <p className="card-subtitle">With backdrop blur</p>
  </div>
</div>

// Badges and alerts
<span className="badge badge-success">Online</span>
<div className="alert alert-info">Information message</div>
```

## Color System

### EVE Online Inspired Colors

```css
/* Primary backgrounds */
.bg-primary      /* Deep space black (#0a0e1a) */
.bg-secondary    /* Station interior (#1a1f2e) */

/* Faction colors */
.text-accent-blue    /* Caldari blue (#00d4ff) */
.text-accent-orange  /* Amarr gold (#ff8c00) */
.text-success        /* Online/success (#00ff88) */
.text-warning        /* Warning systems (#ffcc00) */
.text-error          /* Hull damage (#ff4444) */

/* Ship system colors */
.text-shield     /* Shield HP (#00d4ff) */
.text-armor      /* Armor HP (#ff8c00) */
.text-hull       /* Hull HP (#ff4444) */
.text-capacitor  /* Capacitor (#ffcc00) */
```

## Spacing System

```css
/* Consistent spacing scale */
.p-1   /* 4px padding */
.p-2   /* 8px padding */
.p-4   /* 16px padding */
.p-6   /* 24px padding */
.p-8   /* 32px padding */

.m-auto    /* Auto margin */
.mx-auto   /* Horizontal auto margin */
.gap-4     /* 16px gap in flex/grid */
```

## Typography

```css
/* Font sizes */
.text-xs    /* 12px */
.text-sm    /* 14px */
.text-base  /* 16px */
.text-lg    /* 18px */
.text-xl    /* 20px */
.text-2xl   /* 24px */

/* Font weights */
.font-normal    /* 400 */
.font-medium    /* 500 */
.font-semibold  /* 600 */
.font-bold      /* 700 */

/* Font families */
.font-sans  /* Inter, system fonts */
.font-mono  /* JetBrains Mono, monospace */
```

## Layout

```css
/* Display */
.flex, .grid, .block, .inline-block, .hidden

/* Flexbox */
.flex-col, .flex-row
.items-center, .items-start, .items-end
.justify-center, .justify-between, .justify-start

/* Grid */
.grid-cols-1, .grid-cols-2, .grid-cols-3, .grid-cols-4
.grid-cols-6, .grid-cols-12

/* Sizing */
.w-full, .w-1/2, .w-1/3, .w-1/4
.h-full, .h-screen, .min-h-screen
```

## Responsive Design

```css
/* Breakpoint prefixes */
.sm:flex      /* 640px+ */
.md:grid      /* 768px+ */
.lg:w-1/2     /* 1024px+ */

/* Example: Mobile-first responsive design */
<div className="w-full md:w-1/2 lg:w-1/3">
  Responsive width
</div>
```

## Interactive States

```css
/* Hover states */
.hover:bg-gray-500:hover
.hover:scale-105:hover
.hover:opacity-75:hover

/* Focus states */
.focus:outline-none:focus
.focus:ring-2:focus

/* Example */
<button className="btn btn-primary hover:scale-105 transition-transform">
  Hover to scale
</button>
```

## Special Effects

```css
/* Glass morphism */
.bg-glass           /* Backdrop blur effect */
.card-glass         /* Glass card variant */

/* Glows */
.border-glow        /* Blue glow border */
.border-glow-orange /* Orange glow border */
.text-glow          /* Text glow effect */

/* Animations */
.animate-pulse      /* Pulsing animation */
.animate-spin       /* Spinning animation */
.transition-all     /* Smooth transitions */
```

## Using with CSS-in-JS

```tsx
import { cssTheme, commonStyles } from '../styles';

const StyledComponent = styled.div`
  ${commonStyles.flexCenter}
  background: ${cssTheme.colors.secondaryBg};
  padding: ${cssTheme.spacing.md};
  border-radius: ${cssTheme.radius.lg};
  
  &:hover {
    ${commonStyles.glowBlue}
  }
`;
```

## Theme Configuration

Access theme tokens in TypeScript:

```tsx
import { theme, getColor, getSpacing } from '../styles';

// Direct access
const primaryColor = theme.colors.primary.bg;
const mediumSpacing = theme.spacing.md;

// Helper functions
const accentBlue = getColor('accent.blue');
const largePadding = getSpacing('lg');
```

## Best Practices

1. **Use utility classes for rapid prototyping**
2. **Use component classes for consistent patterns**
3. **Combine utilities for custom designs**
4. **Use responsive prefixes for mobile-first design**
5. **Leverage hover/focus states for interactivity**
6. **Use the theme system for consistent spacing and colors**

## EVE Online Theming

The color palette is specifically designed for EVE Online:
- **Dark backgrounds** mimic the vacuum of space
- **Blue accents** represent Caldari technology and shields
- **Orange accents** represent Amarr empire and armor
- **Green** indicates online systems and success states
- **Red** represents danger, hull damage, and alerts
- **Yellow** represents warnings and capacitor

This creates an authentic EVE Online feel while maintaining excellent readability and accessibility.