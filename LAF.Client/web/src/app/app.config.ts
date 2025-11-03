import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { MsalBroadcastService, MsalGuard, MsalService, MSAL_INSTANCE } from '@azure/msal-angular';
import { PublicClientApplication, Configuration } from '@azure/msal-browser';
import { environment } from '../environments/environment';
import {
  API_BASE_URL,
  AuthClient,
  CashflowClient, // Added
  CollateralTypesClient,
  CounterpartiesClient,
  FundsClient,
  PositionsClient,
  RepoRatesClient,
  RepoTradesClient,
} from './api/client';
import { routes } from './app.routes';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { SignalRService } from './services/signalr.service';
import { ThemeService } from './services/theme.service';
import { MsalInitService } from './services/msal-init.service';

export function MSALInstanceFactory(): PublicClientApplication {
  console.log('Creating MSAL instance with config:', environment.auth);
  
  const msalConfig: Configuration = {
    auth: {
      clientId: environment.auth.clientId,
      authority: environment.auth.authority,
      redirectUri: environment.auth.redirectUri,
      postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: false,
    },
    system: {
      loggerOptions: {
        loggerCallback: (level, message, containsPii) => {
          if (containsPii) {
            return;
          }
          switch (level) {
            case 0:
              console.error('[MSAL]', message);
              return;
            case 1:
              console.warn('[MSAL]', message);
              return;
            case 2:
              console.info('[MSAL]', message);
              return;
            case 3:
              console.debug('[MSAL]', message);
              return;
          }
        },
        piiLoggingEnabled: false,
        logLevel: 2, // Info level
      },
    },
  };

  try {
    const instance = new PublicClientApplication(msalConfig);
    console.log('MSAL instance created successfully');
    return instance;
  } catch (error) {
    console.error('Error creating MSAL instance:', error);
    throw error;
  }
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    // Attach auth token to all outgoing HTTP requests
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
    MsalInitService,
    ThemeService,
    AuthInterceptor,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    // NSwag API clients (marked @Injectable without providedIn) must be provided here
    AuthClient,
    CashflowClient, // Added
    RepoRatesClient,
    PositionsClient,
    FundsClient,
    RepoTradesClient,
    CounterpartiesClient,
    CollateralTypesClient,
    SignalRService,
    {
      provide: API_BASE_URL,
      useFactory: () => environment.apiUrl,
    },
  ],
};
