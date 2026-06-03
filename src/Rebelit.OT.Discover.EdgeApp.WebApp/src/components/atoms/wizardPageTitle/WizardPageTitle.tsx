import style from "./WizardPageTitle.module.css";

type Props = {
  title: string;
} 

export default function WizardPageTitle({ title }: Readonly<Props>) {
  return (
    <h1 className={style.wizardPageTitle}>{title}</h1>
  )
}