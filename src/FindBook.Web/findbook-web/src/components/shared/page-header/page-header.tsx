import type { ReactNode } from "react";

type PageHeaderProps = {
  title: string;
  description: string;
  action?: ReactNode;
};

export function PageHeader({ title, description, action }: PageHeaderProps) {
  return (
    <div className="page-hero">
      <div>
        <h1 className="page-hero-title">{title}</h1>
        <p className="page-hero-description">{description}</p>
      </div>
      {action ? <div className="page-hero-action">{action}</div> : null}
    </div>
  );
}
