import { useI18n } from './index';

function humanize(raw) {
  if (!raw) return '';
  const spaced = String(raw)
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .trim();
  return spaced.charAt(0).toUpperCase() + spaced.slice(1).toLowerCase();
}

export function useDomainLabels() {
  const { t } = useI18n();
  const label = (prefix, value) => {
    const key = `${prefix}.${value}`;
    const translated = t(key);
    return translated === key ? humanize(value) : translated;
  };

  return {
    subscriptionStatusLabel: (value) => label('domain.status', value),
    billingCycleLabel: (value) => label('domain.billingCycle', value),
    planLabel: (value) => label('domain.plan', value),
    roleLabel: (value) => label('roles', value),
    appointmentTypeLabel: (value) => label('domain.appointmentType', value),
    appointmentStatusLabel: (value) => label('domain.appointmentStatus', value),
    leadStatusLabel: (value) => label('domain.leadStatus', value),
    levelLabel: (value) => label('domain.level', value),
    goalLabel: (value) => label('domain.goal', value),
    serviceModeLabel: (value) => label('domain.serviceMode', value),
    paymentStatusLabel: (value) => label('domain.paymentStatus', value)
  };
}
