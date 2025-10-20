import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthClient, AuthenticationRequest } from '../api/client';
import { firstValueFrom } from 'rxjs';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResult {
  success: boolean;
  token?: string;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenKey = 'laf_auth_token';
  private currentToken: string | null = null;

  constructor(
    private httpClient: AuthClient,
    private router: Router
  ) {
    this.currentToken = localStorage.getItem(this.tokenKey);
  }

  async login(email: string, password: string): Promise<LoginResult> {
    try {
      const credentials = new AuthenticationRequest();
      credentials.email = email;
      credentials.password = password;

      const response = await firstValueFrom(this.httpClient.login(credentials));

      if (response.token) {
        this.setToken(response.token);
        return { success: true, token: response.token };
      } else {
        return { success: false, error: 'Invalid credentials' };
      }
    } catch (error: any) {
      console.error('Login error:', error);
      return {
        success: false,
        error: error.message || 'Login failed. Please try again.'
      };
    }
  }

  logout(): void {
    this.clearToken();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.currentToken;
  }

  isAuthenticated(): boolean {
    return !!this.currentToken;
  }

  setToken(token: string): void {
    this.currentToken = token;
    localStorage.setItem(this.tokenKey, token);
  }

  clearToken(): void {
    this.currentToken = null;
    localStorage.removeItem(this.tokenKey);
  }

  getAuthHeaders(): Record<string, string> {
    const token = this.getToken();
    if (token) {
      return {
        'Authorization': `Bearer ${token}`
      };
    }
    return {};
  }
}
