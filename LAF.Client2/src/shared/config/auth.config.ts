export const apiConfig = {
  baseUrl: 'https://localhost:7202',
}

export const loginRequest = {
  scopes: ['User.Read'],
}

import { Configuration } from '@azure/msal-browser'

export const msalConfig: Configuration = {
  auth: {
    clientId: 'your-client-id',
    authority: 'https://login.microsoftonline.com/your-tenant-id',
    redirectUri: 'http://localhost:3000',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: any, message: string, containsPii: boolean) => {
        if (containsPii) {
          return
        }
        switch (level) {
          case 3: // LogLevel.Error
            console.error(message)
            return
          case 2: // LogLevel.Info
            console.info(message)
            return
          case 0: // LogLevel.Verbose
            console.debug(message)
            return
          case 1: // LogLevel.Warning
            console.warn(message)
            return
        }
      },
    },
  },
}