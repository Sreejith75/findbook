type ProgressBarProps = {
  label?: string;
  value: number;
};

export function ProgressBar({ label, value }: ProgressBarProps) {
  return (
    <div className="progress-block">
      {label ? <div className="progress-label">{label}</div> : null}
      <div className="progress-track" aria-hidden="true">
        <div className="progress-fill" style={{ width: `${value}%` }} />
      </div>
    </div>
  );
}
