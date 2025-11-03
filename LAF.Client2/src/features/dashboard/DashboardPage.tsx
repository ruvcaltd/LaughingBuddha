import { useEffect, useState } from 'react'
import { DataGrid, Column } from 'devextreme-react/data-grid'
import { useAuth } from '@features/auth/context/AuthContext'
import { PositionsClient, PositionDto } from '@api/client'
import { createApiClient } from '@shared/utils/api-client'
import { ApiTest } from '@shared/components/ApiTest'

export const DashboardPage = () => {
  const { getAccessToken } = useAuth()
  const [positions, setPositions] = useState<PositionDto[]>([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const loadPositions = async () => {
      try {
        const token = await getAccessToken()
        if (!token) return

        const client = createApiClient(PositionsClient, { accessToken: token })

        const data = await client.day(new Date())
        setPositions(data)
      } catch (error) {
        console.error('Failed to load positions:', error)
      } finally {
        setIsLoading(false)
      }
    }

    loadPositions()
  }, [getAccessToken])

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary-500"></div>
      </div>
    )
  }

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">Overview of your trading positions</p>
      </div>

      <ApiTest />

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-medium text-gray-900 dark:text-white">Current Positions</h2>
        </div>
        <div className="p-6">
          <DataGrid
            dataSource={positions}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
          >
            <Column dataField="securityName" caption="Security" />
            <Column dataField="collateralTypeName" caption="Collateral Type" />
            <Column dataField="counterpartyName" caption="Counterparty" />
            <Column dataField="rate" caption="Rate" format="percent" />
            <Column dataField="variance" caption="Variance" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}