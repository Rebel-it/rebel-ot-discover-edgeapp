import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './components/startPage/StartPage'
import LoginPage from './components/loginPage/LoginPage'
import PlcConnect from './components/plcConnect/PlcConnect'
import SourcePage from './components/sourcePage/SourcePage'
import TagPage from './components/tagPage/TagPage'
import VariablesPage from './components/VariablesPage/VariablesPage'
import ProtectedRoute from './components/shared/ProtectedRoute'

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
