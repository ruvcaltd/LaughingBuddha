import { useState } from 'react'
import { DataGrid, Column, Editing } from 'devextreme-react/data-grid'
import { Button } from 'devextreme-react/button'

const mockOrders = [
  { id: 1, security: 'AAPL', quantity: 100, price: 150.25, side: 'Buy', orderType: 'Market', status: 'New' },
  { id: 2, security: 'GOOGL', quantity: 50, price: 2750.50, side: 'Sell', orderType: 'Limit', status: 'New' },
]

export const OrderBasketPage = () => {
  const [orders, setOrders] = useState(mockOrders)

  const handleSubmitOrders = () => {
    // Submit orders logic here
    console.log('Submitting orders:', orders)
  }

  const handleRowUpdated = (e: any) => {
    const updatedOrders = orders.map(order => 
      order.id === e.key ? { ...order, ...e.data } : order
    )
    setOrders(updatedOrders)
  }

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Order Basket</h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">Create and manage trading orders</p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-6">
          <div className="mb-4 flex justify-end">
            <Button
              text="Submit Orders"
              type="default"
              stylingMode="contained"
              onClick={handleSubmitOrders}
            />
          </div>
          
          <DataGrid
            dataSource={orders}
            showBorders={true}
            rowAlternationEnabled={true}
            allowColumnReordering={true}
            allowColumnResizing={true}
            columnAutoWidth={true}
            onRowUpdated={handleRowUpdated}
          >
            <Editing
              mode="cell"
              allowUpdating={true}
              allowAdding={true}
              allowDeleting={true}
            />
            <Column dataField="security" caption="Security" />
            <Column dataField="quantity" caption="Quantity" />
            <Column dataField="price" caption="Price" format="currency" />
            <Column dataField="side" caption="Side" />
            <Column dataField="orderType" caption="Order Type" />
            <Column dataField="status" caption="Status" />
          </DataGrid>
        </div>
      </div>
    </div>
  )
}