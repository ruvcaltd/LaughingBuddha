import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useMsal, useAccount } from '@azure/msal-react'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { loginRequest, apiConfig } from '@shared/config/auth.config'

interface AuthContextType {
  user: any | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  loginWithMicrosoft: () => Promise<void>
  logout: () => void
  getAccessToken: () => Promise<string | null>
  isMsalAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const { instance, accounts } = useMsal()
  const account = useAccount(accounts[0] || {})
  const [user, setUser] = useState<any | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [jwtUser, setJwtUser] = useState<any | null>(null)

  useEffect(() => {
    if (account) {
      setUser(account)
    } else if (jwtUser) {
      setUser(jwtUser)
    } else {
      setUser(null)
    }
    setIsLoading(false)
  }, [account, jwtUser])

  const login = async (email: string, password: string) => {
    // JWT login implementation
    try {
      setIsLoading(true)
      // Call your JWT login API here
      const response = await fetch(`${apiConfig.baseUrl}/api/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      
      if (!response.ok) {
        throw new Error('Login failed')
      }
      
      const data = await response.json()
      setJwtUser(data.user)
      
      // Store JWT token if provided
      if (data.token) {
        localStorage.setItem('jwtToken', data.token)
      }
    } catch (error) {
      console.error('Login error:', error)
      throw error
    } finally {
      setIsLoading(false)
    }
  }

  const loginWithMicrosoft = async () => {
    try {
      await instance.loginPopup(loginRequest)
      // MSAL will handle setting the user via the account effect
    } catch (error) {
      console.error('Microsoft login error:', error)
      throw error
    }
  }

  const logout = () => {
    instance.logoutPopup()
    setUser(null)
    setJwtUser(null)
    localStorage.removeItem('jwtToken')
  }

  const getAccessToken = async (): Promise<string | null> => {
    // If we have a JWT user, return a mock token for now
    if (jwtUser) {
      return localStorage.getItem('jwtToken') || 'jwt-mock-token'
    }
    
    if (!account) return null
    
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      })
      return response.accessToken
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await instance.acquireTokenPopup(loginRequest)
        return response.accessToken
      }
      console.error('Token acquisition error:', error)
      return null
    }
  }

  const value = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    loginWithMicrosoft,
    logout,
    getAccessToken,
    isMsalAuthenticated: !!account,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}