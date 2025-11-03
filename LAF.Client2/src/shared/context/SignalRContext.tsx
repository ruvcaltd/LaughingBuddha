import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import * as signalR from '@microsoft/signalr'
import { useAuth } from '@features/auth/context/AuthContext'
import { apiConfig } from '@shared/config/auth.config'

interface SignalRContextType {
  connection: signalR.HubConnection | null
  isConnected: boolean
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined)

interface SignalRProviderProps {
  children: ReactNode
}

export const SignalRProvider = ({ children }: SignalRProviderProps) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const { getAccessToken, isAuthenticated } = useAuth()

  useEffect(() => {
    if (!isAuthenticated) return

    const createConnection = async () => {
      try {
        const token = await getAccessToken()
        if (!token) return

        const newConnection = new signalR.HubConnectionBuilder()
          .withUrl(`${apiConfig.baseUrl}/hub/notification`, {
            accessTokenFactory: () => token,
          })
          .withAutomaticReconnect()
          .build()

        newConnection.on('ReceiveMessage', (message) => {
          console.log('Received message:', message)
          // Handle real-time messages here
        })

        await newConnection.start()
        setConnection(newConnection)
        setIsConnected(true)
      } catch (error) {
        console.error('SignalR connection error:', error)
      }
    }

    createConnection()

    return () => {
      if (connection) {
        connection.stop()
      }
    }
  }, [isAuthenticated, getAccessToken])

  const value = {
    connection,
    isConnected,
  }

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  )
}

export const useSignalR = () => {
  const context = useContext(SignalRContext)
  if (!context) {
    throw new Error('useSignalR must be used within a SignalRProvider')
  }
  return context
}