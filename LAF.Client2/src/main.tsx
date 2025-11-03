import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { MsalProvider } from '@azure/msal-react'
import { PublicClientApplication } from '@azure/msal-browser'
import { msalConfig } from './shared/config/auth.config'
import { AuthProvider } from './features/auth/context/AuthContext'
import { ThemeProvider } from './shared/context/ThemeContext'
import { SignalRProvider } from './shared/context/SignalRContext'
import App from './App'
import './index.css'

const msalInstance = new PublicClientApplication(msalConfig)

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <BrowserRouter>
        <ThemeProvider>
          <AuthProvider>
            <SignalRProvider>
              <App />
            </SignalRProvider>
          </AuthProvider>
        </ThemeProvider>
      </BrowserRouter>
    </MsalProvider>
  </StrictMode>,
)