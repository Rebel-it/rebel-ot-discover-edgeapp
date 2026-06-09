import styles from './RichTextRenderer.module.css';

const URL_REGEX = /(https?:\/\/[^\s]+)/g;

type Props = {
  text: string;
}

export default function RichTextRenderer({ text }: Readonly<Props>) {
  return (
    <p className={styles.text}>
      {text.split(URL_REGEX).map((part, i) =>
        URL_REGEX.test(part)
          ? <a className={styles.link} key={i} href={part} target="_blank" rel="noreferrer">{part}</a>
          : part
      )}
    </p>
  );
}