import { DataGrid, Column } from 'devextreme-react/data-grid'

const mockRepoRates = [
  { id: 1, currency: 'USD', tenor: '1W', rate: 5.25, date: '2024-01-15' },
  { id: 2, currency: 'EUR', tenor: '1M', rate: 4.15, date: '2024-01-15' },
  { id: 3, currency: 'GBP', tenor: '3M', rate: 5.75, date: '2024-01-15' },
]

export const RepoRatesPage = () => {
  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Repo Rates</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">Current repurchase agreement rates</p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-6">
          <DataGrid
            dataSource={mockRepoRates}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
          >
            <Column dataField="currency" caption="Currency" />
            <Column dataField="tenor" caption="Tenor" />
            <Column dataField="rate" caption="Rate (%)" format="percent" />
            <Column dataField="date" caption="Date" dataType="date" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}