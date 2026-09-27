import { Link } from 'react-router-dom'
import diasporaLogo from '../../assets/diaspora-logo.svg'
import './WelcomePage.css'

function WelcomePage() {
  return (
    <main className="welcome">
      <section className="welcome-brand" aria-labelledby="welcome-heading">
        <img className="welcome-logo" src={diasporaLogo} alt="Diaspora logo" />
        <h1 id="welcome-heading">Diaspora</h1>
        <p className="welcome-description">
          Diaspora helps you find friends and join activities in your new city or country.
        </p>
      </section>
      <section className="welcome-actions" aria-label="Account actions">
        <Link to="/register" className="welcome-action welcome-action-primary">
          Sign Up
        </Link>
        <Link to="/login" className="welcome-action welcome-action-secondary">
          Sign In
        </Link>
      </section>
    </main>
  )
}

export default WelcomePage
