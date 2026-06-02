import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './pages/startPage/StartPage'
import PlcConnect from './pages/plcConnectPage/PlcConnect'
import ProtectedRoute from './components/shared/ProtectedRoute.'
import LoginPage from './pages/loginPage/LoginPage'
import TagPage from './pages/tagPage/TagPage'
import SourcePage from './pages/sourcePage/SourcePage'
import DeviceConfigPage from './pages/deviceConfigPage/DeviceConfigPage'
import VariablesPage from './pages/variablesPage/VariablesPage'
import FinalPage from './pages/finalPage/FinalPage'


function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StartPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/final" element={<FinalPage />} />
        {/* <Route element={<ProtectedRoute />}> */}
        <Route path="/plc" element={<PlcConnect />} />
        <Route path="/source" element={<SourcePage />} />
        <Route path="/variables" element={<VariablesPage />} />
        <Route path="/tags" element={<TagPage />} />
        <Route path="/deviceconfig" element={<DeviceConfigPage />} />
        {/* </Route> */}
      </Routes>
    </BrowserRouter>
  )
}

export default App