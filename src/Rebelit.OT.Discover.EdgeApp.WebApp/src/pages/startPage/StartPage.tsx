import { useNavigate } from 'react-router-dom'
import styles from './StartPage.module.css'
import WizardPage from '../wizardPage/WizardPage'
import { WizardStep } from '../../models/WizardStep'

function StartPage() {
  const navigate = useNavigate()

  return (
    <WizardPage 
      wizardStep={WizardStep.Introduction}
      continueButtonText="Start"
      onContinue={() => navigate('/login')}
    >
      <section className={styles.page}>
          <h1 className={styles.title}>EdgeApp</h1>
          <p className={styles.description}>
            Rebel:it EdgeApp helps you connect, configure, and manage edge devices so operators can
            quickly start discovery workflows and keep the environment under control.
          </p>
      </section>
    </WizardPage>
  )
}

export default StartPage
