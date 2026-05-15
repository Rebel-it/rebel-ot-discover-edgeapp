import { Button } from "@rebel/design";
import styles from './CleanupPage.module.css'
import { useNavigate } from "react-router-dom";

export default function CleanupPage() {
  const navigate = useNavigate()

  function onCancelClick() {
    navigate('/');
  }

  function onCleanupClick() {

  }

  return (
    <div className={styles['cleanup-page-container']}>

      <div className={styles['cleanup-message-wrapper']}>
        <p>
          Do you wish to clean up the created service account and docker containers?
          This will ensure that no resources are left running after you have completed the tutorial.
        </p>

        <div className={styles['cleanup-decision-wrapper']}>
          <Button
            color="Orange"
            label="Annuleren"
            onClick={onCancelClick}
            shade={500}
            size="medium"
          />

          <Button
            color="Purple"
            label="Opruimen"
            onClick={onCleanupClick}
            shade={500}
            size="medium"
          />
        </div>
      </div>


    </div>
  );
}