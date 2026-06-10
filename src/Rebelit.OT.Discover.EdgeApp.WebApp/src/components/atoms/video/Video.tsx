import styles from "./Video.module.css";
import serviceAccountTutorial from "../../../assets/serviceaccounttutorial.mp4";
import OpenVideoButton from "../openVideoButton/OpenVideoButton";

type Props = {
  onClose: () => void;
}

export default function Video({ onClose }: Readonly<Props>) {
  return (
    <div className={styles.videoWrapper}>
      <video className={styles.video} autoPlay loop muted>
        <source src={serviceAccountTutorial} type="video/mp4" />
        Your browser does not support the video tag.
      </video>
      <OpenVideoButton onClick={onClose} />
    </div>
  );
}