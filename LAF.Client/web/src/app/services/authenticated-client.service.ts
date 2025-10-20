// import { Injectable } from '@angular/core';
// import { AuthClient } from '../api/client';
// import { AuthService } from './auth.service';

// @Injectable({
//   providedIn: 'root'
// })
// export class AuthenticatedClientService extends AuthClient {
//   constructor(    
//     private authService: AuthService
//   ) {
//     // Create a fetch wrapper that adds authentication headers
//     const authenticatedHttpClient = {
//       fetch: async (url: RequestInfo, init?: RequestInit): Promise<Response> => {
//         try {
//           // Create headers object
//           const headers = new Headers(init?.headers || {});
          
//           // Add authentication headers if available
//           const authHeaders = this.authService.getAuthHeaders();
//           Object.entries(authHeaders).forEach(([key, value]) => {
//             headers.set(key, value);
//           });
          
//           // Ensure content type is set for JSON requests
//           if (!headers.has('Content-Type') && init?.body) {
//             headers.set('Content-Type', 'application/json');
//           }
          
//           // Create properly configured request init
//           const modifiedInit: RequestInit = {
//             ...init,
//             headers: headers,
//             mode: 'cors', // Explicitly set CORS mode
//             credentials: 'include', // Include cookies for authentication
//             redirect: 'follow'
//           };
          
//           console.log('Making authenticated API request:', { url, init: modifiedInit });
//           const response = await fetch(url, modifiedInit);
//           console.log('Authenticated API response received:', { status: response.status, statusText: response.statusText });
          
//           if (!response.ok) {
//             console.error('Authenticated API request failed:', { status: response.status, statusText: response.statusText });
//           }
          
//           return response;
//         } catch (error) {
//           console.error('Authenticated API Client fetch error:', error);
//           throw error;
//         }
//       }
//     };
    
//     // Use the same base URL as the HTTP client
//     const baseUrl = 'https://localhost:7202';
//     super(authenticatedHttpClient);
//   }
// }