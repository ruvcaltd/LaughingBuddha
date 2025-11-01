import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { environment } from './../../environments';

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
import { AuthService } from './services/auth.service';
import { SignalRService } from './services/signalr.service';
import { ThemeService } from './services/theme.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    // Attach auth token to all outgoing HTTP requests
    provideHttpClient(
      withInterceptors([
        (req, next) => {
          const authService = inject(AuthService);
          const token = authService.getToken();
          const headers = token ? req.headers.set('Authorization', `Bearer ${token}`) : req.headers;
          const cloned = req.clone({ headers, withCredentials: true });
          return next(cloned);
        },
      ]),
    ),
    ThemeService,
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
