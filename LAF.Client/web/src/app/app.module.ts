import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MsalModule, MsalService, MsalGuard, MsalBroadcastService, MSAL_INSTANCE } from '@azure/msal-angular';
import { PublicClientApplication, Configuration } from '@azure/msal-browser';
import { environment } from '../environments/environment';

export function MSALInstanceFactory(): PublicClientApplication {
  const msalConfig: Configuration = {
    auth: {
      clientId: environment.auth.clientId,
      authority: environment.auth.authority,
      redirectUri: environment.auth.redirectUri,
      postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
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
              console.error(message);
              return;
            case 1:
              console.warn(message);
              return;
            case 2:
              console.info(message);
              return;
            case 3:
              console.debug(message);
              return;
          }
        },
        piiLoggingEnabled: false,
      },
    },
  };

  return new PublicClientApplication(msalConfig);
}

@NgModule({
  imports: [
    MsalModule,
  ],
  providers: [
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ],
})
export class AppModule { }