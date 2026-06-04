import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './pages/startPage/StartPage'
import PlcConnect from './pages/plcConnectPage/PlcConnect'
import LoginPage from './pages/loginPage/LoginPage'
import TagPage from './pages/tagPage/TagPage'
import SourcePage from './pages/sourcePage/SourcePage'
import DeviceConfigPage from './pages/deviceConfigPage/DeviceConfigPage'
import VariablesPage from './pages/variablesPage/VariablesPage'
import FinalPage from './pages/finalPage/FinalPage'
import { WizardProvider } from './context/WizardContext'
import { Pages } from './models/Pages'
import WizardStepGuard from './components/shared/WizardStepGuard'
import { TagsProvider } from './context/TagContext'

function App() {
  return (
    <WizardProvider>
      <TagsProvider>
        <BrowserRouter>
          <Routes>
            <Route path={Pages.start} element={<StartPage />} />
            <Route path={Pages.login} element={<LoginPage />} />

            <Route element={<WizardStepGuard requiredStep="login" />}>
              <Route path={Pages.plcConnect} element={<PlcConnect />} />
            </Route>

            <Route element={<WizardStepGuard requiredStep="plcConnect" />}>
              <Route path={Pages.source} element={<SourcePage />} />
            </Route>

            <Route element={<WizardStepGuard requiredStep="source" />}>
              <Route path={Pages.variables} element={<VariablesPage />} />
            </Route>

            <Route element={<WizardStepGuard requiredStep="variables" />}>
              <Route path={Pages.tags} element={<TagPage />} />
            </Route>

            <Route element={<WizardStepGuard requiredStep="tags" />}>
              <Route path={Pages.deviceConfig} element={<DeviceConfigPage />} />
            </Route>

            <Route element={<WizardStepGuard requiredStep="deviceConfig" />}>
              <Route path={Pages.final} element={<FinalPage />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </TagsProvider>
    </WizardProvider>
  )
}

export default App;