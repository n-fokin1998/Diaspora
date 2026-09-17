import { useNavigate } from 'react-router-dom'
import { useSession } from '../../entities/session'

function HomePage() {
  const { logout } = useSession()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <main className="home">
      <h1>Welcome</h1>
      <button type="button" onClick={handleLogout}>
        Log out
      </button>
    </main>
  )
}

export default HomePage
