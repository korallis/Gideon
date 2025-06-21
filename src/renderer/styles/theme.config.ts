/**
 * Theme Configuration
 * Centralized theme management for consistent styling
 */

// Color palette based on EVE Online's dark aesthetic
export const colors = {
  // Primary backgrounds
  primary: {
    bg: '#0a0e1a',
    surface: '#1a1f2e',
    elevated: '#242937',
  },
  
  // Accent colors inspired by EVE Online
  accent: {
    blue: '#00d4ff',      // Caldari blue
    orange: '#ff8c00',    // Amarr gold/orange
    green: '#00ff88',     // Success/online
    yellow: '#ffcc00',    // Warning
    red: '#ff4444',       // Danger/structure damage
  },
  
  // Text colors
  text: {
    primary: '#ffffff',
    secondary: '#a0a8b8',
    tertiary: '#6b7280',
    disabled: '#4b5563',
  },
  
  // Border colors
  border: {
    primary: '#2a3441',
    accent: '#3a4552',
    focus: '#00d4ff',
  },
  
  // Status colors
  status: {
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6',
  },
  
  // Shield/Armor/Hull colors (EVE specific)
  eve: {
    shield: '#00d4ff',    // Blue for shields
    armor: '#ff8c00',     // Orange for armor
    hull: '#ff4444',      // Red for hull
    capacitor: '#ffcc00', // Yellow for capacitor
  },
} as const;

// Spacing scale
export const spacing = {
  xs: '4px',
  sm: '8px',
  md: '16px',
  lg: '24px',
  xl: '32px',
  xxl: '48px',
  xxxl: '64px',
} as const;

// Border radius scale
export const radius = {
  none: '0',
  sm: '4px',
  md: '8px',
  lg: '12px',
  xl: '16px',
  full: '9999px',
} as const;

// Typography scale
export const typography = {
  fontSize: {
    xs: '0.75rem',      // 12px
    sm: '0.875rem',     // 14px
    base: '1rem',       // 16px
    lg: '1.125rem',     // 18px
    xl: '1.25rem',      // 20px
    '2xl': '1.5rem',    // 24px
    '3xl': '1.875rem',  // 30px
    '4xl': '2.25rem',   // 36px
  },
  
  fontWeight: {
    thin: 100,
    light: 300,
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
    black: 900,
  },
  
  lineHeight: {
    tight: 1.25,
    normal: 1.5,
    relaxed: 1.75,
  },
  
  fontFamily: {
    sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
    mono: ['JetBrains Mono', 'Consolas', 'Monaco', 'monospace'],
  },
} as const;

// Animation durations
export const duration = {
  fast: '150ms',
  normal: '300ms',
  slow: '500ms',
  verySlow: '1000ms',
} as const;

// Shadows
export const shadows = {
  sm: '0 1px 2px 0 rgba(0, 0, 0, 0.3)',
  md: '0 4px 6px -1px rgba(0, 0, 0, 0.4)',
  lg: '0 10px 15px -3px rgba(0, 0, 0, 0.5)',
  xl: '0 20px 25px -5px rgba(0, 0, 0, 0.6)',
  glow: '0 0 20px rgba(0, 212, 255, 0.3)',
  glowOrange: '0 0 20px rgba(255, 140, 0, 0.3)',
} as const;

// Z-index scale
export const zIndex = {
  auto: 'auto',
  base: 0,
  dropdown: 100,
  overlay: 1000,
  modal: 1001,
  notification: 1002,
  tooltip: 1003,
} as const;

// Breakpoints for responsive design
export const breakpoints = {
  sm: '640px',
  md: '768px',
  lg: '1024px',
  xl: '1280px',
  '2xl': '1536px',
} as const;

// Component-specific theme tokens
export const components = {
  button: {
    height: {
      sm: '32px',
      md: '40px',
      lg: '48px',
    },
    padding: {
      sm: `${spacing.xs} ${spacing.sm}`,
      md: `${spacing.sm} ${spacing.md}`,
      lg: `${spacing.md} ${spacing.lg}`,
    },
  },
  
  input: {
    height: {
      sm: '32px',
      md: '40px',
      lg: '48px',
    },
  },
  
  sidebar: {
    width: '280px',
    collapsedWidth: '60px',
  },
  
  header: {
    height: '64px',
  },
  
  card: {
    padding: spacing.lg,
    radius: radius.lg,
  },
} as const;

// Complete theme object
export const theme = {
  colors,
  spacing,
  radius,
  typography,
  duration,
  shadows,
  zIndex,
  breakpoints,
  components,
} as const;

// CSS custom properties generator
export const generateCSSVariables = (themeObj = theme) => {
  const flattenObject = (obj: any, prefix = ''): Record<string, string> => {
    const result: Record<string, string> = {};
    
    Object.entries(obj).forEach(([key, value]) => {
      const newKey = prefix ? `${prefix}-${key}` : key;
      
      if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
        Object.assign(result, flattenObject(value, newKey));
      } else {
        result[`--${newKey}`] = String(value);
      }
    });
    
    return result;
  };
  
  return flattenObject(themeObj);
};

// Utility functions for theme usage
export const getColor = (colorPath: string) => {
  const path = colorPath.split('.');
  let current: any = colors;
  
  for (const segment of path) {
    current = current[segment];
    if (current === undefined) return undefined;
  }
  
  return current;
};

export const getSpacing = (size: keyof typeof spacing) => spacing[size];
export const getRadius = (size: keyof typeof radius) => radius[size];
export const getShadow = (size: keyof typeof shadows) => shadows[size];

// Theme variants for different contexts
export const themeVariants = {
  // Dark mode (default)
  dark: theme,
  
  // High contrast mode
  highContrast: {
    ...theme,
    colors: {
      ...theme.colors,
      text: {
        primary: '#ffffff',
        secondary: '#cccccc',
        tertiary: '#999999',
        disabled: '#666666',
      },
      border: {
        primary: '#555555',
        accent: '#777777',
        focus: '#00ffff',
      },
    },
  },
  
  // Compact mode (smaller spacing)
  compact: {
    ...theme,
    spacing: {
      xs: '2px',
      sm: '4px',
      md: '8px',
      lg: '12px',
      xl: '16px',
      xxl: '24px',
      xxxl: '32px',
    },
    components: {
      ...theme.components,
      button: {
        height: {
          sm: '28px',
          md: '32px',
          lg: '40px',
        },
        padding: {
          sm: '2px 4px',
          md: '4px 8px',
          lg: '8px 12px',
        },
      },
    },
  },
} as const;

export type Theme = typeof theme;
export type ThemeColors = typeof colors;
export type ThemeSpacing = typeof spacing;

export default theme;