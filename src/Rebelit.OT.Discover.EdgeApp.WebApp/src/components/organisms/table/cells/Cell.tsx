import style from "./Cell.module.css";

export interface CellProps {
    children: React.ReactNode;
    className?: string;
}

export default function Cell({ children, className }: Readonly<CellProps>) {
    return (
        <div className={[style.cell, className].filter(Boolean).join(' ')}>
            {children}
        </div>
    );
}