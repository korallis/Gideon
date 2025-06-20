/**
 * Button Component
 * EVE Online styled button with multiple variants
 */

import React from 'react';
import { css } from '@emotion/react';
import { ButtonProps } from './Button.types';
import { useTheme } from '@emotion/react';

const buttonStyles = css`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: none;
  border-radius: 4px;
  font-family: inherit;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
  outline: none;
  user-select: none;
  position: relative;
  overflow: hidden;

  &:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    pointer-events: none;
  }

  /* Loading state */
  &[data-loading="true"] {
    cursor: progress;
    
    .button-content {
      opacity: 0.7;
    }
  }
`;

const getVariantStyles = (variant: string, theme: any) => {
  const variants = {
    primary: css`
      background: linear-gradient(135deg, #4A9EFF 0%, #0066CC 100%);
      color: white;
      box-shadow: 0 2px 8px rgba(74, 158, 255, 0.3);

      &:hover:not(:disabled) {
        background: linear-gradient(135deg, #5AA8FF 0%, #0077DD 100%);
        box-shadow: 0 4px 12px rgba(74, 158, 255, 0.4);
        transform: translateY(-1px);
      }

      &:active {
        transform: translateY(0);
        box-shadow: 0 1px 4px rgba(74, 158, 255, 0.4);
      }
    `,
    secondary: css`
      background: linear-gradient(135deg, #2A2A2A 0%, #1A1A1A 100%);
      color: #E0E0E0;
      border: 1px solid #404040;

      &:hover:not(:disabled) {
        background: linear-gradient(135deg, #3A3A3A 0%, #2A2A2A 100%);
        border-color: #505050;
        transform: translateY(-1px);
      }

      &:active {
        transform: translateY(0);
        background: linear-gradient(135deg, #1A1A1A 0%, #0A0A0A 100%);
      }
    `,
    success: css`
      background: linear-gradient(135deg, #4CAF50 0%, #2E7D32 100%);
      color: white;
      box-shadow: 0 2px 8px rgba(76, 175, 80, 0.3);

      &:hover:not(:disabled) {
        background: linear-gradient(135deg, #5CBF60 0%, #3E8D42 100%);
        transform: translateY(-1px);
      }
    `,
    warning: css`
      background: linear-gradient(135deg, #FFC107 0%, #F57C00 100%);
      color: #1A1A1A;
      box-shadow: 0 2px 8px rgba(255, 193, 7, 0.3);

      &:hover:not(:disabled) {
        background: linear-gradient(135deg, #FFD117 0%, #FF8C00 100%);
        transform: translateY(-1px);
      }
    `,
    error: css`
      background: linear-gradient(135deg, #F44336 0%, #C62828 100%);
      color: white;
      box-shadow: 0 2px 8px rgba(244, 67, 54, 0.3);

      &:hover:not(:disabled) {
        background: linear-gradient(135deg, #F55346 0%, #D63838 100%);
        transform: translateY(-1px);
      }
    `,
    ghost: css`
      background: transparent;
      color: #E0E0E0;
      border: 1px solid transparent;

      &:hover:not(:disabled) {
        background: rgba(255, 255, 255, 0.1);
        border-color: #404040;
      }

      &:active {
        background: rgba(255, 255, 255, 0.05);
      }
    `,
    link: css`
      background: transparent;
      color: #4A9EFF;
      border: none;
      text-decoration: underline;
      padding: 0;

      &:hover:not(:disabled) {
        color: #5AA8FF;
        text-decoration: none;
      }
    `,
  };

  return variants[variant as keyof typeof variants] || variants.primary;
};

const getSizeStyles = (size: string) => {
  const sizes = {
    small: css`
      padding: 6px 12px;
      font-size: 12px;
      min-height: 28px;
    `,
    medium: css`
      padding: 10px 16px;
      font-size: 14px;
      min-height: 36px;
    `,
    large: css`
      padding: 14px 20px;
      font-size: 16px;
      min-height: 44px;
    `,
  };

  return sizes[size as keyof typeof sizes] || sizes.medium;
};

const fullWidthStyles = css`
  width: 100%;
`;

export const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  size = 'medium',
  loading = false,
  startIcon,
  endIcon,
  fullWidth = false,
  children,
  className,
  ...props
}) => {
  const theme = useTheme();

  return (
    <button
      css={[
        buttonStyles,
        getVariantStyles(variant, theme),
        getSizeStyles(size),
        fullWidth && fullWidthStyles,
      ]}
      className={className}
      data-loading={loading}
      {...props}
    >
      <span className="button-content">
        {startIcon && <span className="start-icon">{startIcon}</span>}
        {children}
        {endIcon && <span className="end-icon">{endIcon}</span>}
      </span>
      
      {loading && (
        <span
          css={css`
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 16px;
            height: 16px;
            border: 2px solid rgba(255, 255, 255, 0.3);
            border-top: 2px solid white;
            border-radius: 50%;
            animation: spin 1s linear infinite;

            @keyframes spin {
              0% { transform: translate(-50%, -50%) rotate(0deg); }
              100% { transform: translate(-50%, -50%) rotate(360deg); }
            }
          `}
        />
      )}
    </button>
  );
};