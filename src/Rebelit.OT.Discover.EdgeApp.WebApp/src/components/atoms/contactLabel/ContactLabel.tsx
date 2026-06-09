import styles from "./ContactLabel.module.css";

type Props = {
  label: string;
  children: React.ReactNode;
  type: "email" | "phone";
}

export default function ContactLabel({ label, children, type }: Props) {
  return (
    <div className={styles.contactLabel}>
      {children}
      {type === "email" ? (
        <a href={`mailto:${label}`} className={styles.label}>{label}</a>
      ) : (
        <a href={`tel:${label}`} className={styles.label}>{label}</a>
      )}
    </div>
  );
}