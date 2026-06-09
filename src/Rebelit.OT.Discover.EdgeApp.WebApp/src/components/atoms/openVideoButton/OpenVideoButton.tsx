import styles from "./OpenVideoButton.module.css";
import openVideoIcon from "../../../assets/openvideoicon.svg";

type Props = {
  onClick?: () => void;
}

export default function OpenVideoButton({ onClick }: Readonly<Props>) {
  return (
    <>
      {onClick ? (
        <button className={styles.openVideoButton} onClick={onClick} aria-label="Close video">
          <img src={openVideoIcon} alt="Close Icon" className={styles.playIcon} />
        </button>
      ) : (
        <div className={styles.openVideoButton}>
          <img src={openVideoIcon} alt="Play Icon" className={styles.playIcon} />
        </div>
      )}
    </>
  );
}