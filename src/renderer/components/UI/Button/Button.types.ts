/**
 * Button Component Types
 */

import { ButtonHTMLAttributes, ReactNode } from 'react';

export type ButtonVariant = 
  | 'primary'
  | 'secondary' 
  | 'success'
  | 'warning'
  | 'error'
  | 'ghost'
  | 'link';

export type ButtonSize = 'small' | 'medium' | 'large';

export interface ButtonProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'size'> {
  /** Button variant */
  variant?: ButtonVariant;
  
  /** Button size */
  size?: ButtonSize;
  
  /** Whether button is loading */
  loading?: boolean;
  
  /** Icon to display before text */
  startIcon?: ReactNode;
  
  /** Icon to display after text */
  endIcon?: ReactNode;
  
  /** Whether button should take full width */
  fullWidth?: boolean;
  
  /** Button content */
  children: ReactNode;
}