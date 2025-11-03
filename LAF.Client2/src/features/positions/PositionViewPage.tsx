import { DataGrid, Column } from 'devextreme-react/data-grid'

const mockPositions = [
  { id: 1, security: 'AAPL', quantity: 1000, price: 150.25, value: 150250, pnl: 2500 },
  { id: 2, security: 'GOOGL', quantity: 500, price: 2750.50, value: 1375250, pnl: -1250 },
  { id: 3, security: 'MSFT', quantity: 750, price: 305.75, value: 229312.5, pnl: 312.5 },
]

export const PositionViewPage = () => {
  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Position View</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">Detailed view of all positions</p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-6">
          <DataGrid
            dataSource={mockPositions}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
          >
            <Column dataField="security" caption="Security" />
            <Column dataField="quantity" caption="Quantity" />
            <Column dataField="price" caption="Price" format="currency" />
            <Column dataField="value" caption="Market Value" format="currency" />
            <Column dataField="pnl" caption="P&L" format="currency" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}