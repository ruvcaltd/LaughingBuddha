import { DataGrid, Column } from 'devextreme-react/data-grid'

const mockTrades = [
  { id: 1, tradeId: 'TRD001', security: 'AAPL', quantity: 100, price: 150.25, side: 'Buy', status: 'Filled', date: '2024-01-15' },
  { id: 2, tradeId: 'TRD002', security: 'GOOGL', quantity: 50, price: 2750.50, side: 'Sell', status: 'Filled', date: '2024-01-14' },
  { id: 3, tradeId: 'TRD003', security: 'MSFT', quantity: 75, price: 305.75, side: 'Buy', status: 'Pending', date: '2024-01-15' },
]

export const SubmittedTradesPage = () => {
  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Submitted Trades</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">History of submitted trades</p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-6">
          <DataGrid
            dataSource={mockTrades}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
          >
            <Column dataField="tradeId" caption="Trade ID" />
            <Column dataField="security" caption="Security" />
            <Column dataField="quantity" caption="Quantity" />
            <Column dataField="price" caption="Price" format="currency" />
            <Column dataField="side" caption="Side" />
            <Column dataField="status" caption="Status" />
            <Column dataField="date" caption="Date" dataType="date" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}