import styles from "./VideoPreview.module.css";
import serviceAccountTutorial from "../../../assets/serviceaccounttutorial.mp4";
import OpenVideoButton from "../openVideoButton/OpenVideoButton";

type Props = {
  onClick: () => void;
}

export default function VideoPreview({ onClick }: Readonly<Props>) {
  return (
    <button className={styles.preview} onClick={onClick} aria-label="Open video">
      <video className={styles.video} autoPlay loop muted>
        <source src={serviceAccountTutorial} type="video/mp4" />
      </video>
      <div className={styles.overlay}>
        <OpenVideoButton />
      </div>
    </button>
  );
}
