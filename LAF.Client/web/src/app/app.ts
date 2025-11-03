import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { MsalModule, MsalService, MsalBroadcastService, MSAL_GUARD_CONFIG, MsalGuardConfiguration } from '@azure/msal-angular';
import { EventMessage, EventType, AuthenticationResult } from '@azure/msal-browser';
import { SignalRService } from './services/signalr.service';
import { ThemeService } from './services/theme.service';
import { Navbar } from './shared/navbar/navbar';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, MsalModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('web');
  private themeService = inject(ThemeService);
  private signalRService = inject(SignalRService);
  private msalService = inject(MsalService);
  private router = inject(Router);
  private msalBroadcastService = inject(MsalBroadcastService);

  async ngOnInit(): Promise<void> {
    // Initialize theme service
    this.themeService;

    // Initialize MSAL properly
    try {
      console.log('App Component: Initializing MSAL...');
      console.log('App Component: MSAL instance exists:', !!this.msalService.instance);
      
      if (this.msalService.instance) {
        console.log('App Component: MSAL instance found');
        
        // Check if MSAL is already initialized
        try {
          const accounts = this.msalService.instance.getAllAccounts();
          console.log('App Component: MSAL accounts found:', accounts.length);
          console.log('App Component: MSAL accounts details:', accounts);
        } catch (accountError) {
          console.log('App Component: Could not get accounts (may not be initialized yet):', accountError);
        }
        
        console.log('App Component: MSAL initialization completed');
      } else {
        console.warn('App Component: MSAL instance not available');
      }
    } catch (error) {
      console.error('App Component: Error initializing MSAL:', error);
    }

    // Handle MSAL redirect responses - this must be done before other checks
    this.handleRedirectResponse();
    
    // Give MSAL time to process any redirect that just occurred
    setTimeout(() => {
      this.checkExistingAuthentication();
    }, 100);

    // Connect to SignalR hub
    try {
      await this.signalRService.connect();
    } catch (error) {
      console.error('Failed to connect to SignalR hub:', error);
    }
  }

  private handleRedirectResponse(): void {
    console.log('Setting up MSAL redirect response handler...');
    
    // Check if we're returning from a Microsoft redirect
    this.msalBroadcastService.msalSubject$
      .pipe(
        filter((msg: EventMessage) => {
          const isRelevant = msg.eventType === EventType.LOGIN_SUCCESS || 
                           msg.eventType === EventType.LOGIN_FAILURE ||
                           msg.eventType === EventType.ACQUIRE_TOKEN_SUCCESS ||
                           msg.eventType === EventType.ACQUIRE_TOKEN_FAILURE;
          
          if (isRelevant) {
            console.log('MSAL event detected:', msg.eventType);
          }
          return isRelevant;
        })
      )
      .subscribe((result: EventMessage) => {
        console.log('MSAL redirect response received:', result);
        
        if (result.eventType === EventType.LOGIN_SUCCESS || result.eventType === EventType.ACQUIRE_TOKEN_SUCCESS) {
          const payload = result.payload as AuthenticationResult;
          console.log('Authentication successful, account:', payload.account);
          
          // Set the active account
          if (payload.account) {
            this.msalService.instance.setActiveAccount(payload.account);
          }
          
          // Handle successful authentication
          console.log('Navigating to repo-rates...');
          this.router.navigate(['/repo-rates']).then(success => {
            console.log('Navigation result:', success);
          }).catch(error => {
            console.error('Navigation error:', error);
          });
        } else if (result.eventType === EventType.LOGIN_FAILURE || result.eventType === EventType.ACQUIRE_TOKEN_FAILURE) {
          console.error('Authentication failed:', result);
          // Handle authentication failure - could redirect to login with error
          this.router.navigate(['/login'], { queryParams: { error: 'authentication_failed' } });
        }
      });
  }

  async ngOnDestroy(): Promise<void> {
    await this.signalRService.disconnect();
  }

  private checkExistingAuthentication(): void {
    console.log('Checking for existing authentication...');
    
    // Check if we have an active account
    const accounts = this.msalService.instance.getAllAccounts();
    console.log('MSAL accounts found:', accounts);
    
    if (accounts.length > 0) {
      console.log('Setting active account from existing accounts');
      this.msalService.instance.setActiveAccount(accounts[0]);
      
      // If we have an active account and we're on the login page, redirect
      // But don't redirect if we're returning from a failed authentication
      if (this.router.url.startsWith('/login') && !this.router.url.includes('error=authentication_failed')) {
        console.log('User is authenticated but on login page, redirecting to repo-rates');
        this.router.navigate(['/repo-rates']);
      }
    }
  }
}
