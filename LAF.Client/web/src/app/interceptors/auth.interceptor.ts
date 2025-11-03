import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, from, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { EntraAuthService } from '../services/entra-auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private entraAuthService: EntraAuthService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authMethod = this.authService.getAuthMethod();
    
    if (!authMethod) {
      return next.handle(req);
    }

    if (authMethod === 'entra') {
      return from(this.entraAuthService.getAccessToken()).pipe(
        switchMap(token => {
          if (token) {
            const authReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            });
            return next.handle(authReq);
          }
          return next.handle(req);
        }),
        catchError(error => {
          console.error('Error getting Entra access token:', error);
          return next.handle(req);
        })
      );
    } else {
      const token = this.authService.getToken();
      if (token) {
        const authReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next.handle(authReq);
      }
      return next.handle(req);
    }
  }
}