import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

export interface AuthConfig {
  enableEntraAuth: boolean;
  enableJwtFallback: boolean;
  azureClientId: string;
  azureTenantId: string;
  azureAuthority: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthConfigService {
  private config: AuthConfig;

  constructor() {
    this.config = this.loadConfig();
  }

  private loadConfig(): AuthConfig {
    // Extract tenant ID from authority URL if available
    const authority = environment.auth.authority;
    let tenantId = '';
    if (authority && authority.includes('login.microsoftonline.com')) {
      const parts = authority.split('/');
      tenantId = parts[parts.length - 1] || '';
    }

    return {
      enableEntraAuth: this.getEnvVar('ENABLE_ENTRA_AUTH', 'true') === 'true',
      enableJwtFallback: this.getEnvVar('ENABLE_JWT_FALLBACK', 'true') === 'true',
      azureClientId: this.getEnvVar('AZURE_CLIENT_ID', environment.auth.clientId),
      azureTenantId: this.getEnvVar('AZURE_TENANT_ID', tenantId),
      azureAuthority: this.getEnvVar('AZURE_AUTHORITY', environment.auth.authority)
    };
  }

  private getEnvVar(key: string, defaultValue: string): string {
    // In development, check for environment variables
    // Note: In browser environment, process is not available
    // Use window or globalThis for browser environment variables
    if (typeof (globalThis as any).process !== 'undefined' && (globalThis as any).process?.env) {
      return (globalThis as any).process.env[key] || defaultValue;
    }
    // For browser environment, you might want to use a different approach
    // such as reading from meta tags or a config endpoint
    return defaultValue;
  }

  getConfig(): AuthConfig {
    return this.config;
  }

  isEntraAuthEnabled(): boolean {
    const enabled = this.config.enableEntraAuth && this.hasValidAzureConfig();
    console.log('Entra Auth Debug:', {
      enableEntraAuth: this.config.enableEntraAuth,
      hasValidConfig: this.hasValidAzureConfig(),
      clientId: this.config.azureClientId,
      tenantId: this.config.azureTenantId,
      authority: this.config.azureAuthority,
      isEnabled: enabled
    });
    return enabled;
  }

  isJwtFallbackEnabled(): boolean {
    return this.config.enableJwtFallback;
  }

  private hasValidAzureConfig(): boolean {
    // Check if we have valid Azure AD configuration
    const hasClientId = !!(this.config.azureClientId && 
                          this.config.azureClientId.length > 5 && // Allow shorter client IDs for now
                          this.config.azureClientId !== 'your-client-id-here');
    
    const hasTenantId = !!(this.config.azureTenantId && 
                          this.config.azureTenantId.length > 5 && // Allow shorter tenant IDs for now
                          this.config.azureTenantId !== 'your-tenant-id-here');
    
    const hasValidAuthority = !!(this.config.azureAuthority && 
                                this.config.azureAuthority.includes('login.microsoftonline.com'));
    
    return hasClientId && hasTenantId && hasValidAuthority;
  }

  getAzureConfig() {
    return {
      clientId: this.config.azureClientId,
      authority: this.config.azureAuthority,
      redirectUri: environment.auth.redirectUri,
      postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
      scopes: environment.auth.scopes
    };
  }
}