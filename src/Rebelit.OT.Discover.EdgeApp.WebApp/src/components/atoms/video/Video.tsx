import styles from './Video.module.css';
import rebelVideo from '../../../assets/rebelVideo.mp4';

export default function Video() {
  return (
    <div className={styles.videoWrapper}>
      <video className={styles.video} autoPlay loop muted>
        <source src={rebelVideo} type="video/mp4" />
        Your browser does not support the video tag.
      </video>
    </div>
  );
}