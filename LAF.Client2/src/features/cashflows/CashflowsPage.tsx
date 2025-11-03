import { DataGrid, Column } from 'devextreme-react/data-grid'

const mockCashflows = [
  { id: 1, date: '2024-01-15', description: 'Dividend AAPL', amount: 500, type: 'Income', currency: 'USD' },
  { id: 2, date: '2024-01-14', description: 'Interest Payment', amount: -250, type: 'Expense', currency: 'USD' },
  { id: 3, date: '2024-01-13', description: 'Dividend MSFT', amount: 350, type: 'Income', currency: 'USD' },
]

export const CashflowsPage = () => {
  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Cashflows</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">Track income and expenses</p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-6">
          <DataGrid
            dataSource={mockCashflows}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
          >
            <Column dataField="date" caption="Date" dataType="date" />
            <Column dataField="description" caption="Description" />
            <Column dataField="amount" caption="Amount" format="currency" />
            <Column dataField="type" caption="Type" />
            <Column dataField="currency" caption="Currency" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}