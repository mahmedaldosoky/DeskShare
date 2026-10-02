interface DateFieldProps {
  label: string;
  value: string;
  onChange: (isoDate: string) => void;
  min?: string;
}

export function DateField({ label, value, onChange, min }: DateFieldProps) {
  return (
    <label className="field">
      <span className="field__label">{label}</span>
      <input
        className="field__input"
        type="date"
        value={value}
        min={min}
        required
        onChange={(event) => event.target.value && onChange(event.target.value)}
      />
    </label>
  );
}
