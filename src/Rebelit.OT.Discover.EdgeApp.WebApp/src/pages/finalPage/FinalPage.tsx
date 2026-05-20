import styles from '../loginPage/LoginPage.module.css'
import { useNavigate } from 'react-router-dom'

function FinalPage() {
  const navigate = useNavigate();

  return (
    <div className={styles.wrapper}>
      <div>
        <h1>Completed!</h1>
        <p>You have successfully completed the setup. If you wish, you can run the wizard again
          to make changes to your configuration.
          Finally, you can run the provided <code>remove_discover_edgeapp.sh</code> script to uninstall the DiscoverEdgeApp on your Secure Edge Pro.
        </p>
      </div>

      <button type="button" className={styles.nextButton} onClick={() => navigate('/')}>
        Finish
      </button>
    </div>
  )
}

export default FinalPage;