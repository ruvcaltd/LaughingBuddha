import { Injectable } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import {
  AccountInfo,
  AuthenticationResult,
  EventMessage,
  EventType,
  InteractionStatus,
  PopupRequest,
  RedirectRequest,
} from '@azure/msal-browser';
import { Observable, Subject, filter, from, map, takeUntil } from 'rxjs';
import { AuthConfigService } from './auth-config.service';
import { MsalInitService } from './msal-init.service';

@Injectable({
  providedIn: 'root',
})
export class EntraAuthService {
  private readonly _destroying$ = new Subject<void>();
  private isInitialized = false;
  private initializationPromise: Promise<void> | null = null;

  constructor(
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    private authConfigService: AuthConfigService,
    private msalInitService: MsalInitService,
  ) {
    this.initialize();
  }

  private initialize(): void {
    console.log('Initializing EntraAuthService...');

    // Initialize MSAL through the service
    this.msalInitService.isReady(); // This will trigger initialization if not done

    this.msalBroadcastService.msalSubject$
      .pipe(
        filter(
          (msg: EventMessage) =>
            msg.eventType === EventType.LOGIN_SUCCESS ||
            msg.eventType === EventType.ACQUIRE_TOKEN_SUCCESS,
        ),
        takeUntil(this._destroying$),
      )
      .subscribe((result: EventMessage) => {
        const payload = result.payload as AuthenticationResult;
        this.msalService.instance.setActiveAccount(payload.account);
      });
  }

  private async initializeMsal(): Promise<void> {
    try {
      console.log('Starting MSAL initialization...');

      // Wait for MSAL to be ready using the initialization service
      if (!this.msalInitService.isReady()) {
        // Wait a bit for the async initialization to complete
        await new Promise((resolve) => setTimeout(resolve, 100));

        if (!this.msalInitService.isReady()) {
          throw new Error('MSAL initialization service is not ready');
        }
      }

      console.log('MSAL initialization completed successfully');
      this.isInitialized = true;
    } catch (error) {
      console.error('Error during MSAL initialization:', error);
      throw error;
    }
  }

  login(): Observable<AuthenticationResult> {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      throw new Error('Entra authentication is not enabled');
    }

    return new Observable((observer) => {
      if (this.msalService.instance.getAllAccounts().length > 0) {
        this.msalService.instance.setActiveAccount(this.msalService.instance.getAllAccounts()[0]);
        observer.complete();
        return;
      }

      const config = this.authConfigService.getAzureConfig();
      const request: RedirectRequest = {
        scopes: config.scopes,
        redirectUri: config.redirectUri,
      };

      this.msalService.loginRedirect(request);
      observer.complete();
    });
  }

  loginPopup(): Observable<AuthenticationResult> {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      throw new Error('Entra authentication is not enabled');
    }

    return from(this.performLoginPopup());
  }

  async loginRedirect() {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      throw new Error('Entra authentication is not enabled');
    }

    return await this.performLoginRedirectSync();
  }

  private async performLoginPopup(): Promise<AuthenticationResult> {
    try {
      // Ensure MSAL is initialized before proceeding
      await this.ensureInitialized();

      const config = this.authConfigService.getAzureConfig();
      const request: PopupRequest = {
        scopes: config.scopes,
        redirectUri: config.redirectUri,
      };

      console.log('Attempting MSAL login popup with request:', request);

      // Use the MSAL instance directly for login
      const result = await this.msalService.loginPopup(request).toPromise();
      if (!result) {
        throw new Error('No response from loginPopup');
      }
      console.log('MSAL login popup successful:', result);
      return result;
    } catch (error) {
      console.error('MSAL login popup failed:', error);
      throw error;
    }
  }

  private async performLoginRedirectSync(): Promise<void> {
    try {
      console.log('Starting performLoginRedirect...');

      // Ensure MSAL is initialized before proceeding
      console.log('Ensuring MSAL is initialized...');
      this.ensureInitialized(); // This is already synchronous
      console.log('MSAL initialization confirmed');

      const config = this.authConfigService.getAzureConfig();
      console.log('Got Azure config:', config);
      console.log('Current window location:', window.location.href);
      console.log('Redirect URI from config:', config.redirectUri);
      console.log('Authority from config:', config.authority);
      console.log('Scopes from config:', config.scopes);

      // Fix redirect URI to match current location (without query params for security)
      const cleanRedirectUri = window.location.origin + window.location.pathname;
      console.log('Clean redirect URI (without query params):', cleanRedirectUri);

      const request: RedirectRequest = {
        scopes: config.scopes,
        redirectUri: cleanRedirectUri, // Use clean URL to avoid mismatch
        redirectStartPage: window.location.href, // Store current page for redirect back
      };

      console.log('Complete redirect request:', request);

      console.log('Attempting MSAL login redirect with request:', request);
      console.log('MSAL instance available:', !!this.msalService.instance);
      console.log(
        'MSAL service loginRedirect method exists:',
        typeof this.msalService.loginRedirect,
      );
      console.log(
        'MSAL instance loginRedirect method exists:',
        typeof this.msalService.instance.loginRedirect,
      );

      // Check if we can get accounts (to verify MSAL is working)
      try {
        const accounts = this.msalService.instance.getAllAccounts();
        console.log('Current MSAL accounts before redirect:', accounts);
      } catch (accountError) {
        console.log('Could not get accounts:', accountError);
      }

      // Use redirect login - this doesn't return a promise, it redirects the browser
      console.log('About to call this.msalService.loginRedirect(request)...');

      // Try calling loginRedirect using the MSAL service method
      console.log('Calling loginRedirect using MSAL service...');

      try {
        console.log('About to call loginRedirect...');

        // Try the service method first - this should trigger the redirect
        if (typeof this.msalService.loginRedirect === 'function') {
          console.log('Using MSAL service loginRedirect method');

          // Call loginRedirect - this should immediately redirect the browser
          console.log('Calling loginRedirect now...');

          // Try calling it directly on the instance instead of through the service
          console.log('Trying MSAL instance loginRedirect directly...');

          // loginRedirect returns a Promise - we need to await it
          console.log('Awaiting loginRedirect Promise...');
          const result = await this.msalService.instance.loginRedirect(request);

          console.log('loginRedirect Promise resolved with:', result);
          console.log(
            'MSAL instance loginRedirect completed - this should not happen if redirect worked!',
          );

          // If we reach this point, the redirect didn't happen
          console.warn('WARNING: loginRedirect returned without redirecting!');
          console.warn('This might indicate:');
          console.warn('1. Browser blocked the redirect');
          console.warn('2. Invalid configuration');
          console.warn('3. MSAL decided not to redirect');
        } else if (typeof this.msalService.instance.loginRedirect === 'function') {
          console.log('Using MSAL instance loginRedirect method');
          await this.msalService.instance.loginRedirect(request);
        } else {
          console.error('No loginRedirect method found on MSAL service or instance');
          console.error('MSAL service methods:', Object.getOwnPropertyNames(this.msalService));
          console.error(
            'MSAL instance methods:',
            Object.getOwnPropertyNames(this.msalService.instance),
          );
        }
      } catch (redirectError: any) {
        console.error('MSAL loginRedirect threw error:', redirectError);
        console.error('Error name:', redirectError.name);
        console.error('Error message:', redirectError.message);
        console.error('Error stack:', redirectError.stack);
        throw redirectError;
      }

      console.log('MSAL login redirect initiated'); // This won't be reached
    } catch (error) {
      console.error('MSAL login redirect failed:', error);
      throw error;
    }
  }

  logout(): void {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      return;
    }

    const config = this.authConfigService.getAzureConfig();
    this.msalService.logoutRedirect({
      postLogoutRedirectUri: config.postLogoutRedirectUri,
    });
  }

  getAccessToken(): Observable<string> {
    if (!this.authConfigService.isEntraAuthEnabled()) {
      throw new Error('Entra authentication is not enabled');
    }

    return from(this.performGetAccessToken());
  }

  private async performGetAccessToken(): Promise<string> {
    try {
      // Ensure MSAL is initialized before proceeding
      await this.ensureInitialized();

      const account = this.msalService.instance.getActiveAccount();
      if (!account) {
        throw new Error('No active account');
      }

      const config = this.authConfigService.getAzureConfig();
      const request = {
        scopes: config.scopes,
        account: account,
      };

      try {
        const response = await this.msalService.acquireTokenSilent(request).toPromise();
        if (!response) {
          throw new Error('No response from acquireTokenSilent');
        }
        return response.accessToken;
      } catch (error: any) {
        if (error.name === 'InteractionRequiredAuthError') {
          const response = await this.msalService.acquireTokenPopup(request).toPromise();
          if (!response) {
            throw new Error('No response from acquireTokenPopup');
          }
          return response.accessToken;
        } else {
          throw error;
        }
      }
    } catch (error) {
      console.error('Error getting access token:', error);
      throw error;
    }
  }

  getCurrentUser(): AccountInfo | null {
    return this.msalService.instance.getActiveAccount();
  }

  isLoggedIn(): boolean {
    return this.msalService.instance.getAllAccounts().length > 0;
  }

  getUserProfile(): Observable<any> {
    return this.getAccessToken().pipe(
      map((token) => {
        const account = this.getCurrentUser();
        return {
          displayName: account?.name || '',
          email: account?.username || '',
          id: account?.localAccountId || '',
          accessToken: token,
        };
      }),
    );
  }

  isInteractionRequired(): Observable<boolean> {
    return this.msalBroadcastService.inProgress$.pipe(
      map((status: InteractionStatus) => status !== InteractionStatus.None),
    );
  }

  private ensureInitialized(): void {
    try {
      console.log('Checking MSAL initialization status...');
      console.log('MsalInitService isReady():', this.msalInitService.isReady());

      // Check if MSAL is ready through the initialization service
      if (this.msalInitService.isReady()) {
        this.isInitialized = true;
        console.log('MSAL instance initialized successfully through MsalInitService');
      } else {
        console.warn('MSAL instance not ready through MsalInitService');
        throw new Error('MSAL is not properly initialized');
      }
    } catch (error) {
      console.error('Error ensuring MSAL initialization:', error);
      throw error;
    }
  }

  ngOnDestroy(): void {
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }
}
