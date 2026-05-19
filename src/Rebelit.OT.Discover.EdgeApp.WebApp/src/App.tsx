import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './pages/startPage/StartPage'
import PlcConnect from './components/plcConnect/PlcConnect'
import ProtectedRoute from './components/shared/ProtectedRoute.'
import LoginPage from './pages/loginPage/LoginPage'
import TagPage from './pages/tagPage/TagPage'
import SourcePage from './pages/sourcePage/SourcePage'
import VariablesPage from './pages/VariablesPage/VariablesPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StartPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/plc" element={<PlcConnect />} />
          <Route path="/source" element={<SourcePage />} />
          <Route path="/variables" element={<VariablesPage />} />
          <Route path="/tags" element={<TagPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App