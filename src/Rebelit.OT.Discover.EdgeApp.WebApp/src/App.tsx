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
import { WizardProvider } from './context/WizardContext'
import { Pages } from './models/Pages'


function App() {
  return (
    <WizardProvider>
      <BrowserRouter>
        <Routes>
          <Route path={Pages.start} element={<StartPage />} />
          <Route path={Pages.login} element={<LoginPage />} />
          <Route path={Pages.final} element={<FinalPage />} />
          {/* <Route element={<ProtectedRoute />}> */}
          <Route path={Pages.plcConnect} element={<PlcConnect />} />
          <Route path={Pages.source} element={<SourcePage />} />
          <Route path={Pages.variables} element={<VariablesPage />} />
          <Route path={Pages.tags} element={<TagPage />} />
          <Route path={Pages.deviceConfig} element={<DeviceConfigPage />} />
          {/* </Route> */}
        </Routes>
      </BrowserRouter>
    </WizardProvider>
  )
}

export default App