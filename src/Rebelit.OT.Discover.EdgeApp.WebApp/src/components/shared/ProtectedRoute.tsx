import { Navigate, Outlet } from 'react-router-dom'
import { loadIxonAuthenticationHeaders } from '../../services/sessionStorageService'

function ProtectedRoute() {
  const isAuthenticated = loadIxonAuthenticationHeaders() !== null

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />
}

export default ProtectedRoute
