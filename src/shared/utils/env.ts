/**
 * Environment utilities for both main and renderer processes
 */

export const isDev = process.env.NODE_ENV === 'development';
export const isProd = process.env.NODE_ENV === 'production';
export const isTest = process.env.NODE_ENV === 'test';

export const getEnvVar = (name: string, defaultValue?: string): string => {
  return process.env[name] || defaultValue || '';
};

export const requireEnvVar = (name: string): string => {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Required environment variable ${name} is not set`);
  }
  return value;
};