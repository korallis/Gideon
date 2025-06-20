import { describe, it, expect } from 'vitest';

describe('Basic Test Suite', () => {
  it('should run basic tests', () => {
    expect(1 + 1).toBe(2);
  });

  it('should work with TypeScript', () => {
    const message: string = 'Hello Vitest';
    expect(message).toBe('Hello Vitest');
  });

  it('should test async operations', async () => {
    const result = await Promise.resolve('async test');
    expect(result).toBe('async test');
  });
});