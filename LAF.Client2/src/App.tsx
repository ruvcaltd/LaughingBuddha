import { Routes, Route } from 'react-router-dom'
import { ProtectedRoute } from '@shared/components/ProtectedRoute'
import { Navbar } from '@shared/components/Navbar'
import { LoginPage } from '@features/auth/LoginPage'
import { DashboardPage } from '@features/dashboard/DashboardPage'
import { PositionViewPage } from '@features/positions/PositionViewPage'
import { RepoRatesPage } from '@features/repo-rates/RepoRatesPage'
import { SubmittedTradesPage } from '@features/trades/SubmittedTradesPage'
import { OrderBasketPage } from '@features/orders/OrderBasketPage'
import { CashflowsPage } from '@features/cashflows/CashflowsPage'

function App() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        
        {/* Protected Routes */}
        <Route path="/" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <DashboardPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
        
        <Route path="/position-view" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <PositionViewPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
        
        <Route path="/repo-rates" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <RepoRatesPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
        
        <Route path="/submitted-trades" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <SubmittedTradesPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
        
        <Route path="/order-basket" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <OrderBasketPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
        
        <Route path="/cashflows" element={
          <ProtectedRoute>
            <div className="flex flex-col h-screen">
              <Navbar />
              <div className="flex-1 overflow-hidden">
                <CashflowsPage />
              </div>
            </div>
          </ProtectedRoute>
        } />
      </Routes>
    </div>
  )
}

export default App