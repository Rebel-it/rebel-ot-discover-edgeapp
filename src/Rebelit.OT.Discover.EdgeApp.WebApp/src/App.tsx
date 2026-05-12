import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './components/startPage/StartPage'
import LoginPage from './components/loginPage/LoginPage'
import PlcConnect from './components/plcConnect/PlcConnect'
import SourcePage from './components/sourcePage/SourcePage'
import VariablesPage from './components/variablesPage/VariablesPage'
import TagPage from './Pages/tagPage/TagPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StartPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/plc" element={<PlcConnect />} />
        <Route path="/source" element={<SourcePage />} />
        <Route path="/variables" element={<VariablesPage />} />
        <Route path="/tags" element={<TagPage />} />

      </Routes>
    </BrowserRouter>
  )
}

export default App
