import { useCountUp } from '../../hooks/useCountUp';

export function CountUp({ value, duration, decimals = 0, className = '' }) {
  const display = useCountUp(value, { duration });
  const text = Number(display).toLocaleString('pt-BR', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });
  return <span className={className}>{text}</span>;
}
