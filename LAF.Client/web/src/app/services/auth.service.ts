import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthClient, AuthenticationRequest } from '../api/client';
import { AuthConfigService } from './auth-config.service';
import { EntraAuthService } from './entra-auth.service';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResult {
  success: boolean;
  token?: string;
  error?: string;
  authMethod?: 'jwt' | 'entra';
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private tokenKey = 'laf_auth_token';
  private emailKey = 'laf_user_email';
  private displayNameKey = 'laf_user_display_name';
  private userIdKey = 'laf_user_id';
  private authMethodKey = 'laf_auth_method';
  private currentToken: string | null = null;
  private currentEmail: string | null = null;
  private currentDisplayName: string | null = null;
  private currentUserId: number | null = null;
  private currentAuthMethod: 'jwt' | 'entra' | null = null;

  constructor(
    private httpClient: AuthClient,
    private router: Router,
    private entraAuthService: EntraAuthService,
    private authConfigService: AuthConfigService,
  ) {
    this.currentToken = localStorage.getItem(this.tokenKey);
    this.currentEmail = localStorage.getItem(this.emailKey);
    this.currentDisplayName = localStorage.getItem(this.displayNameKey);
    this.currentUserId = +(localStorage.getItem(this.userIdKey) || '0');
    const authMethod = localStorage.getItem(this.authMethodKey);
    this.currentAuthMethod = authMethod as 'jwt' | 'entra' | null;
  }

  async login(email: string, password: string): Promise<LoginResult> {
    try {
      const credentials = new AuthenticationRequest();
      credentials.email = email;
      credentials.password = password;

      const response = await firstValueFrom(this.httpClient.login(credentials));

      if (response.token) {
        this.setToken(response.token);
        this.setAuthMethod('jwt');
        if (response.email) {
          this.setEmail(response.email);
        }
        if (response.displayName) {
          this.setDisplayName(response.displayName);
        }
        if (response.userId) {
          this.setUserId(response.userId);
        }
        return { success: true, token: response.token, authMethod: 'jwt' };
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

  async loginWithEntra(): Promise<LoginResult> {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      return {
        success: false,
        error: 'Microsoft Entra authentication is not enabled',
      };
    }

    try {
      const result = await this.entraAuthService.loginPopup().toPromise();
      if (result) {
        const accessToken = result.accessToken;
        this.setToken(accessToken);
        this.setAuthMethod('entra');

        const account = this.entraAuthService.getCurrentUser();
        if (account) {
          this.setEmail(account.username);
          this.setDisplayName(account.name || '');
        }

        return { success: true, token: accessToken, authMethod: 'entra' };
      }
      return { success: false, error: 'Entra login failed' };
    } catch (error: any) {
      console.error('Entra login error:', error);

      // If Entra login fails and JWT fallback is enabled, suggest fallback
      if (this.authConfigService.isJwtFallbackEnabled()) {
        return {
          success: false,
          error: 'Microsoft login failed. Please use email/password login instead.',
        };
      }

      return {
        success: false,
        error: error.message || 'Entra login failed. Please try again.',
      };
    }
  }

  async loginWithEntraRedirect() {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      throw new Error('Microsoft Entra authentication is not enabled');
    }

    console.log('AuthService: Starting Entra redirect login...');
    await this.entraAuthService.loginRedirect();
  }

  logout(): void {
    if (this.currentAuthMethod === 'entra') {
      this.entraAuthService.logout();
    }
    this.clearToken();
    this.clearEmail();
    this.clearDisplayName();
    this.clearUserId();
    this.clearAuthMethod();
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

  getAuthMethod(): 'jwt' | 'entra' | null {
    return this.currentAuthMethod;
  }

  setAuthMethod(method: 'jwt' | 'entra'): void {
    this.currentAuthMethod = method;
    localStorage.setItem(this.authMethodKey, method);
  }

  clearAuthMethod(): void {
    this.currentAuthMethod = null;
    localStorage.removeItem(this.authMethodKey);
  }

  isEntraAuthenticated(): boolean {
    return this.currentAuthMethod === 'entra' && this.isAuthenticated();
  }

  isJwtAuthenticated(): boolean {
    return this.currentAuthMethod === 'jwt' && this.isAuthenticated();
  }
}
