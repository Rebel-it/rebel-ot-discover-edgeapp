import styles from './RichTextRenderer.module.css';

const SPLIT_REGEX = /(\[[^\]]+\]\(https?:\/\/[^)]+\)|https?:\/\/[^\s]+)/g;

type Props = {
  text: string;
}

function renderPart(part: string, key: number) {
  const markdownMatch = part.match(/^\[([^\]]+)\]\((https?:\/\/[^)]+)\)$/);
  if (markdownMatch) {
    return <a className={styles.link} key={key} href={markdownMatch[2]} target="_blank" rel="noreferrer">{markdownMatch[1]}</a>;
  }
  if (/^https?:\/\//.test(part)) {
    return <a className={styles.link} key={key} href={part} target="_blank" rel="noreferrer">{part}</a>;
  }
  return part;
}

export default function RichTextRenderer({ text }: Readonly<Props>) {
  return (
    <p className={styles.text}>
      {text.split(SPLIT_REGEX).map((part, i) => renderPart(part, i))}
    </p>
  );
}