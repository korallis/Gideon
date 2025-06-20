/**
 * ESI API Client
 * Axios-based HTTP client for EVE Online API with automatic token management
 */

import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { ESIResponse, ESIErrorResponse, ESIError } from './types';

// ESI API Configuration
const ESI_BASE_URL = 'https://esi.evetech.net/latest';
const ESI_OAUTH_URL = 'https://login.eveonline.com/v2/oauth';

export class ESIClientError extends Error {
  public readonly status: number;
  public readonly esiError?: ESIError;

  constructor(message: string, status: number, esiError?: ESIError) {
    super(message);
    this.name = 'ESIClientError';
    this.status = status;
    this.esiError = esiError;
  }
}

export class ESIClient {
  private client: AxiosInstance;
  private accessToken?: string;

  constructor() {
    this.client = axios.create({
      baseURL: ESI_BASE_URL,
      timeout: 30000,
      headers: {
        'User-Agent': 'Gideon/0.1.0 (EVE Online Desktop App)',
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  /**
   * Set access token for authenticated requests
   */
  public setAccessToken(token: string): void {
    this.accessToken = token;
  }

  /**
   * Clear access token
   */
  public clearAccessToken(): void {
    this.accessToken = undefined;
  }

  /**
   * Generic GET request
   */
  public async get<T>(
    endpoint: string,
    config?: AxiosRequestConfig
  ): Promise<ESIResponse<T>> {
    try {
      const response = await this.client.get<T>(endpoint, {
        ...config,
        headers: {
          ...config?.headers,
          ...(this.accessToken && { Authorization: `Bearer ${this.accessToken}` }),
        },
      });

      return {
        data: response.data,
        headers: response.headers as Record<string, string>,
        status: response.status,
      };
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Generic POST request
   */
  public async post<T>(
    endpoint: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<ESIResponse<T>> {
    try {
      const response = await this.client.post<T>(endpoint, data, {
        ...config,
        headers: {
          ...config?.headers,
          ...(this.accessToken && { Authorization: `Bearer ${this.accessToken}` }),
        },
      });

      return {
        data: response.data,
        headers: response.headers as Record<string, string>,
        status: response.status,
      };
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Generic PUT request
   */
  public async put<T>(
    endpoint: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<ESIResponse<T>> {
    try {
      const response = await this.client.put<T>(endpoint, data, {
        ...config,
        headers: {
          ...config?.headers,
          ...(this.accessToken && { Authorization: `Bearer ${this.accessToken}` }),
        },
      });

      return {
        data: response.data,
        headers: response.headers as Record<string, string>,
        status: response.status,
      };
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Generic DELETE request
   */
  public async delete<T>(
    endpoint: string,
    config?: AxiosRequestConfig
  ): Promise<ESIResponse<T>> {
    try {
      const response = await this.client.delete<T>(endpoint, {
        ...config,
        headers: {
          ...config?.headers,
          ...(this.accessToken && { Authorization: `Bearer ${this.accessToken}` }),
        },
      });

      return {
        data: response.data,
        headers: response.headers as Record<string, string>,
        status: response.status,
      };
    } catch (error) {
      throw this.handleError(error);
    }
  }

  /**
   * Setup request/response interceptors
   */
  private setupInterceptors(): void {
    // Request interceptor for rate limiting
    this.client.interceptors.request.use(
      (config) => {
        // Add timestamp for rate limiting
        config.metadata = { startTime: Date.now() };
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling and rate limiting
    this.client.interceptors.response.use(
      (response: AxiosResponse) => {
        // Log rate limit headers for monitoring
        const remaining = response.headers['x-esi-error-limit-remain'];
        const reset = response.headers['x-esi-error-limit-reset'];
        
        if (remaining && parseInt(remaining) < 10) {
          console.warn(`ESI rate limit warning: ${remaining} requests remaining`);
        }

        return response;
      },
      (error) => {
        return Promise.reject(error);
      }
    );
  }

  /**
   * Handle and transform errors
   */
  private handleError(error: any): ESIClientError {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status || 0;
      const esiError = error.response?.data as ESIError;
      
      if (status === 429) {
        return new ESIClientError(
          'ESI rate limit exceeded. Please wait before making more requests.',
          status,
          esiError
        );
      }
      
      if (status === 401) {
        return new ESIClientError(
          'Authentication failed. Please re-authenticate.',
          status,
          esiError
        );
      }
      
      if (status === 403) {
        return new ESIClientError(
          'Access forbidden. Check required scopes.',
          status,
          esiError
        );
      }
      
      if (status >= 500) {
        return new ESIClientError(
          'ESI server error. Please try again later.',
          status,
          esiError
        );
      }

      return new ESIClientError(
        esiError?.error_description || error.message,
        status,
        esiError
      );
    }

    return new ESIClientError(
      error.message || 'Unknown network error',
      0
    );
  }
}

// Export singleton instance
export const esiClient = new ESIClient();