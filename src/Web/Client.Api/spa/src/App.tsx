import diasporaLogo from './assets/diaspora-logo.svg'
import './App.css'

function App() {
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
        <button type="button" className="welcome-action welcome-action-primary">
          Sign Up
        </button>
        <button type="button" className="welcome-action welcome-action-secondary">
          Sign In
        </button>
      </section>
    </main>
  )
}

export default App
