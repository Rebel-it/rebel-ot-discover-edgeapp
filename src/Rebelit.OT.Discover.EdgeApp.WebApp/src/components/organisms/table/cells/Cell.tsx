import style from "./Cell.module.css";

export interface CellProps {
    children: React.ReactNode;
    className?: string;
    selected?: boolean;
}

export default function Cell({ children, className, selected }: Readonly<CellProps>) {
    return (
        <div className={[style.cell, selected ? style.selected : '', className].filter(Boolean).join(' ')}>
            {children}
        </div>
    );
}