import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthClient, AuthenticationRequest } from '../api/client';

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
  providedIn: 'root',
})
export class AuthService {
  private tokenKey = 'laf_auth_token';
  private emailKey = 'laf_user_email';
  private displayNameKey = 'laf_user_display_name';
  private userIdKey = 'laf_user_id';
  private currentToken: string | null = null;
  private currentEmail: string | null = null;
  private currentDisplayName: string | null = null;
  private currentUserId: number | null = null;

  constructor(
    private httpClient: AuthClient,
    private router: Router,
  ) {
    this.currentToken = localStorage.getItem(this.tokenKey);
    this.currentEmail = localStorage.getItem(this.emailKey);
    this.currentDisplayName = localStorage.getItem(this.displayNameKey);
    this.currentUserId = +(localStorage.getItem(this.userIdKey) || '0');
  }

  async login(email: string, password: string): Promise<LoginResult> {
    try {
      const credentials = new AuthenticationRequest();
      credentials.email = email;
      credentials.password = password;

      const response = await firstValueFrom(this.httpClient.login(credentials));

      if (response.token) {
        this.setToken(response.token);
        if (response.email) {
          this.setEmail(response.email);
        }
        if (response.displayName) {
          this.setDisplayName(response.displayName);
        }
        if (response.userId) {
          this.setUserId(response.userId);
        }
        return { success: true, token: response.token };
      } else {
        return { success: false, error: 'Invalid credentials' };
      }
    } catch (error: any) {
      console.error('Login error:', error);
      return {
        success: false,
        error: error.message || 'Login failed. Please try again.',
      };
    }
  }

  logout(): void {
    this.clearToken();
    this.clearEmail();
    this.clearDisplayName();
    this.clearUserId();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.currentToken;
  }

  getEmail(): string | null {
    return this.currentEmail;
  }

  getDisplayName(): string | null {
    return this.currentDisplayName;
  }

  getUserId(): number | null {
    return this.currentUserId;
  }

  isAuthenticated(): boolean {
    return !!this.currentToken;
  }

  setToken(token: string): void {
    this.currentToken = token;
    localStorage.setItem(this.tokenKey, token);
  }

  setEmail(email: string): void {
    this.currentEmail = email;
    localStorage.setItem(this.emailKey, email);
  }

  setDisplayName(displayName: string): void {
    this.currentDisplayName = displayName;
    localStorage.setItem(this.displayNameKey, displayName);
  }

  setUserId(userId: number): void {
    this.currentUserId = userId;
    localStorage.setItem(this.userIdKey, userId.toString());
  }

  clearToken(): void {
    this.currentToken = null;
    localStorage.removeItem(this.tokenKey);
  }

  clearEmail(): void {
    this.currentEmail = null;
    localStorage.removeItem(this.emailKey);
  }

  clearDisplayName(): void {
    this.currentDisplayName = null;
    localStorage.removeItem(this.displayNameKey);
  }

  clearUserId(): void {
    this.currentUserId = null;
    localStorage.removeItem(this.userIdKey);
  }

  getAuthHeaders(): Record<string, string> {
    const token = this.getToken();
    if (token) {
      return {
        Authorization: `Bearer ${token}`,
      };
    }
    return {};
  }
}
