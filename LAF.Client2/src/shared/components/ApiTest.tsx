import { useState, useEffect } from 'react'
import { useAuth } from '@features/auth/context/AuthContext'

export const ApiTest = () => {
  const { getAccessToken } = useAuth()
  const [testResult, setTestResult] = useState<string>('Testing API connection...')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const testApiConnection = async () => {
      try {
        const token = await getAccessToken()
        if (!token) {
          setTestResult('No access token available - please log in')
          setIsLoading(false)
          return
        }

        // Test with AuthClient since it has a simple login endpoint
        // const client = createApiClient(AuthClient, { accessToken: token })
        
        // For now, just verify we have a token
        setTestResult(`✅ Access token available: ${token.substring(0, 20)}...`)
        
      } catch (error) {
        console.error('API test failed:', error)
        setTestResult(`❌ API test failed: ${error instanceof Error ? error.message : 'Unknown error'}`)
      } finally {
        setIsLoading(false)
      }
    }

    testApiConnection()
  }, [getAccessToken])

  return (
    <div className="p-4 bg-gray-100 dark:bg-gray-800 rounded-lg">
      <h3 className="text-lg font-semibold mb-2">API Connection Test</h3>
      <div className={`p-2 rounded ${isLoading ? 'bg-yellow-100 dark:bg-yellow-900' : testResult.includes('✅') ? 'bg-green-100 dark:bg-green-900' : 'bg-red-100 dark:bg-red-900'}`}>
        <p className="text-sm">{testResult}</p>
      </div>
    </div>
  )
}