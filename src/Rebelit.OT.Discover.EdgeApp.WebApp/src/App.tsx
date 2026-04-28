import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './components/startPage/StartPage'
import LoginPage from './components/loginPage/LoginPage'
import PlcConnect from './components/plcConnect/PlcConnect'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StartPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/plc" element={<PlcConnect />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
