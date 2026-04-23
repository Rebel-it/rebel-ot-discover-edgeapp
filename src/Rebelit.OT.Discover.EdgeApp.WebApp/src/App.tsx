import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StartPage from './components/startPage/StartPage'
import LoginPage from './components/loginPage/LoginPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StartPage />} />
        <Route path="/login" element={<LoginPage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
