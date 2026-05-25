import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { useToast } from '../../components/ui/Toast';
import { authService } from '../../services/authService';
import { useAuth } from '../../contexts/AuthContext';
import { BrandLogo } from '../../components/brand/BrandLogo';
import { useI18n } from '../../i18n';

const empty = {
  fullName: '',
  email: '',
  password: '',
  phone: '',
  city: '',
  state: '',
  goal: '',
  interests: '',
  trainingLevel: '',
  preferredTrainingMode: 'online',
  acceptPrivacyPolicy: false,
  acceptTermsOfUse: false,
  marketingEmail: false,
  marketingWhatsapp: false,
  healthRelatedDataProcessingAcknowledged: false,
};

export function StudentRegisterPage() {
  const [form, setForm] = useState(empty);
  const [loading, setLoading] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();
  const { login } = useAuth();
  const { t } = useI18n();

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    try {
      await authService.registerStudent(form);
      await login(form.email, form.password);
      navigate('/explore', { replace: true });
    } catch (error) {
      toast(error?.response?.data?.message || t('auth.studentRegister.createError'), 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 p-4 sm:p-8">
      <div className="mx-auto max-w-2xl rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
        <div className="mb-3 inline-flex"><BrandLogo showText={false} size="lg" /></div>
        <h1 className="text-2xl font-bold text-slate-900">{t('auth.studentRegister.title')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('auth.studentRegister.subtitle')}</p>

        <form onSubmit={handleSubmit} className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <Input label={t('auth.studentRegister.fullName')} value={form.fullName} onChange={(e) => setForm((p) => ({ ...p, fullName: e.target.value }))} required />
          </div>
          <Input label={t('auth.studentRegister.email')} type="email" value={form.email} onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))} required />
          <Input label={t('auth.studentRegister.password')} type="password" value={form.password} onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))} required />
          <Input label={t('auth.studentRegister.phone')} value={form.phone} onChange={(e) => setForm((p) => ({ ...p, phone: e.target.value }))} />
          <Input label={t('student.city')} value={form.city} onChange={(e) => setForm((p) => ({ ...p, city: e.target.value }))} />
          <Input label={t('student.state')} value={form.state} onChange={(e) => setForm((p) => ({ ...p, state: e.target.value }))} />
          <Input label={t('auth.studentRegister.goal')} value={form.goal} onChange={(e) => setForm((p) => ({ ...p, goal: e.target.value }))} />
          <Input label={t('auth.studentRegister.interests')} value={form.interests} onChange={(e) => setForm((p) => ({ ...p, interests: e.target.value }))} />
          <Input label={t('auth.studentRegister.trainingLevel')} value={form.trainingLevel} onChange={(e) => setForm((p) => ({ ...p, trainingLevel: e.target.value }))} />
          <div>
            <label className="mb-1 block text-sm font-medium text-slate-700">{t('auth.studentRegister.preferredMode')}</label>
            <select className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm" value={form.preferredTrainingMode} onChange={(e) => setForm((p) => ({ ...p, preferredTrainingMode: e.target.value }))}>
              <option value="online">{t('domain.serviceMode.Online')}</option>
              <option value="presencial">{t('domain.serviceMode.InPerson')}</option>
              <option value="hibrido">{t('domain.serviceMode.Hybrid')}</option>
            </select>
          </div>
          <div className="sm:col-span-2 mt-2">
            <label className="text-xs block"><input type="checkbox" checked={form.acceptTermsOfUse} onChange={(e) => setForm((p) => ({ ...p, acceptTermsOfUse: e.target.checked }))} /> {t('auth.studentRegister.acceptTermsPrefix')} <Link to="/terms-of-use" className="text-indigo-600">{t('auth.studentRegister.terms')}</Link>.</label>
            <label className="text-xs block"><input type="checkbox" checked={form.acceptPrivacyPolicy} onChange={(e) => setForm((p) => ({ ...p, acceptPrivacyPolicy: e.target.checked }))} /> {t('auth.studentRegister.acceptPrivacyPrefix')} <Link to="/privacy-policy" className="text-indigo-600">{t('auth.studentRegister.privacyPolicy')}</Link>.</label>
            <label className="text-xs block"><input type="checkbox" checked={form.healthRelatedDataProcessingAcknowledged} onChange={(e) => setForm((p) => ({ ...p, healthRelatedDataProcessingAcknowledged: e.target.checked }))} /> {t('auth.studentRegister.healthDataNotice')}</label>
            <label className="text-xs block"><input type="checkbox" checked={form.marketingEmail} onChange={(e) => setForm((p) => ({ ...p, marketingEmail: e.target.checked }))} /> {t('auth.studentRegister.marketingEmail')}</label>
            <label className="text-xs block"><input type="checkbox" checked={form.marketingWhatsapp} onChange={(e) => setForm((p) => ({ ...p, marketingWhatsapp: e.target.checked }))} /> {t('auth.studentRegister.marketingWhatsapp')}</label>
            <Button type="submit" className="w-full" loading={loading}>{t('auth.studentRegister.createAccount')}</Button>
          </div>
        </form>
        <p className="mt-4 text-center text-sm text-slate-500">{t('auth.studentRegister.hasAccount')} <Link to="/login" className="font-semibold text-indigo-600">{t('auth.login.signIn')}</Link></p>
      </div>
    </div>
  );
}

