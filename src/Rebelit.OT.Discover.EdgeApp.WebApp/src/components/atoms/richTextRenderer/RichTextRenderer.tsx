import styles from './RichTextRenderer.module.css';

const INLINE_SPLIT_REGEX = /(\*\*[^*]+\*\*|\[[^\]]+\]\(https?:\/\/[^)]+\)|https?:\/\/[^\s]+)/g;

type Props = {
  text: string;
}

function renderInlinePart(part: string, key: number) {
  if (part.startsWith('**') && part.endsWith('**')) {
    return <strong key={key}>{part.slice(2, -2)}</strong>;
  }
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
    <>
      {text.split('\n\n').map((paragraph, pi) => (
        <p key={pi} className={styles.text}>
          {paragraph.split('\n').map((line, li, lines) => (
            <span key={li}>
              {line.split(INLINE_SPLIT_REGEX).map((part, i) => renderInlinePart(part, i))}
              {li < lines.length - 1 && <br />}
            </span>
          ))}
        </p>
      ))}
    </>
  );
}