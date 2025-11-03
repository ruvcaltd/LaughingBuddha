import { apiConfig } from '@shared/config/auth.config'

export interface ApiClientOptions {
  accessToken?: string
}

export function createApiClient<T>(
  ClientClass: new (baseUrl: string, http?: { fetch: (url: RequestInfo, init?: RequestInit) => Promise<Response> }) => T,
  options?: ApiClientOptions
): T {
  const http = {
    fetch: (url: RequestInfo, init?: RequestInit) => {
      const headers: Record<string, string> = {}
      
      // Copy existing headers
      if (init?.headers) {
        if (init.headers instanceof Headers) {
          init.headers.forEach((value, key) => {
            headers[key] = value
          })
        } else if (Array.isArray(init.headers)) {
          init.headers.forEach(([key, value]) => {
            headers[key] = value
          })
        } else {
          Object.assign(headers, init.headers)
        }
      }

      if (options?.accessToken) {
        headers['Authorization'] = `Bearer ${options.accessToken}`
      }

      // Ensure the URL is absolute by prepending the base URL if needed
      const fullUrl = typeof url === 'string' && !url.startsWith('http') 
        ? `${apiConfig.baseUrl}${url}`
        : url

      return fetch(fullUrl, {
        ...init,
        headers,
      })
    }
  }

  return new ClientClass(apiConfig.baseUrl, http)
}