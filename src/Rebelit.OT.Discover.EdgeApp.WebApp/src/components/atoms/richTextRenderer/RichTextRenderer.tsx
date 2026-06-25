import Markdown from 'react-markdown';
import type { Components } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import styles from './RichTextRenderer.module.css';

type Props = {
  text: string;
}

const components: Components = {
  p: ({ children }) => <p className={styles.text}>{children}</p>,
  a: ({ href, children }) => (
    <a className={styles.link} href={href} target="_blank" rel="noreferrer">{children}</a>
  ),
};

export default function RichTextRenderer({ text }: Readonly<Props>) {
  return <Markdown components={components} remarkPlugins={[remarkGfm]} allowedElements={['p', 'strong', 'em', 'a', 'br']} unwrapDisallowed>{text}</Markdown>;
}