// import { Injectable } from '@angular/core';
// import { AuthService } from './auth.service';
// import { AuthClient } from '../api/client';

// @Injectable({
//   providedIn: 'root'
// })
// export class ApiClientService extends AuthClient {
//   constructor(private authService: AuthService) {
//     // Use the correct backend URL - make sure this matches your backend port
//     const baseUrl = 'https://localhost:7202';
    
//     // Create a properly configured fetch function for CORS
//     const httpClient = {
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
          
//           console.log('Making API request:', { url, init: modifiedInit });
//           const response = await fetch(url, modifiedInit);
//           console.log('API response received:', { status: response.status, statusText: response.statusText });
          
//           if (!response.ok) {
//             console.error('API request failed:', { status: response.status, statusText: response.statusText });
//           }
          
//           return response;
//         } catch (error) {
//           console.error('API Client fetch error:', error);
//           throw error;
//         }
//       }
//     };
    
//     super(baseUrl, httpClient);
//   }
// }