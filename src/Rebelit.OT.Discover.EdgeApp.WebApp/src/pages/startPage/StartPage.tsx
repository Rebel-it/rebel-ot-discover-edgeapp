import { useNavigate } from 'react-router-dom'
import styles from './StartPage.module.css'

function StartPage() {
  const navigate = useNavigate()

  return (
    <section className={styles.page}>
      <div className={styles.content}>
        <h1 className={styles.title}>EdgeApp</h1>
        <p className={styles.description}>
          Rebel:it EdgeApp helps you connect, configure, and manage edge devices so operators can
          quickly start discovery workflows and keep the environment under control.
        </p>
      </div>

      <button type="button" className={styles.startButton} onClick={() => navigate('/login')}>
        Start
      </button>
    </section>
  )
}

export default StartPage
