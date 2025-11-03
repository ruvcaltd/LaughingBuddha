import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { IPublicClientApplication } from '@azure/msal-browser';

@Injectable({
  providedIn: 'root'
})
export class MsalInitService {
  private msalInstance: IPublicClientApplication | null = null;
  private isInitialized = false;

  constructor(private msalService: MsalService) {
    this.initializeMsalAsync();
  }

  private async initializeMsalAsync(): Promise<void> {
    try {
      console.log('MsalInitService: Starting MSAL initialization...');
      console.log('MsalService.instance exists:', !!this.msalService.instance);
      
      if (this.msalService.instance) {
        console.log('MsalInitService: Calling MSAL initialize()...');
        console.log('MSAL instance before initialize:', this.msalService.instance);
        
        try {
          await this.msalService.instance.initialize();
          console.log('MsalInitService: MSAL initialize() completed');
        } catch (initError) {
          console.error('MsalInitService: MSAL initialize() failed:', initError);
          throw initError;
        }
        
        this.msalInstance = this.msalService.instance;
        this.isInitialized = true;
        console.log('MsalInitService: MSAL initialized successfully');
      } else {
        console.error('MsalInitService: MSAL instance not available');
      }
    } catch (error) {
      console.error('MsalInitService: Error initializing MSAL:', error);
    }
  }

  getInstance(): IPublicClientApplication | null {
    return this.msalInstance;
  }

  isReady(): boolean {
    const ready = this.isInitialized && this.msalInstance !== null;
    console.log('MsalInitService.isReady() called, returning:', ready);
    console.log('isInitialized:', this.isInitialized);
    console.log('msalInstance exists:', this.msalInstance !== null);
    return ready;
  }

  async ensureReady(): Promise<void> {
    if (!this.isInitialized) {
      throw new Error('MSAL is not initialized');
    }
    
    if (!this.msalInstance) {
      throw new Error('MSAL instance is not available');
    }
  }
}