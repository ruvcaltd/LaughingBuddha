import { environment } from './../../environments';
import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { ThemeService } from './services/theme.service';
import {
  API_BASE_URL,
  AuthClient,
  RepoRatesClient,
  FundsClient,
  RepoTradesClient,
  CounterpartiesClient,
  CollateralTypesClient
} from './api/client';
import { AuthService } from './services/auth.service';

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
        }
      ])
    ),
    ThemeService,    
    // NSwag API clients (marked @Injectable without providedIn) must be provided here
    AuthClient,
    RepoRatesClient,
    FundsClient,
    RepoTradesClient,
    CounterpartiesClient,
    CollateralTypesClient,
    {
      provide: API_BASE_URL,
      useFactory: () => environment.apiUrl
    }
  ]
};
