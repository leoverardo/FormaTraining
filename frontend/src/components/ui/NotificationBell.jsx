import { useEffect, useState } from 'react';
import { notificationService } from '../../services/notificationService';
import { Bell } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';
import { useI18n } from '../../i18n';

export function NotificationBell() {
  const { user } = useAuth();
  const { t, language } = useI18n();
  const [count, setCount] = useState(0);
  const [open, setOpen] = useState(false);
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    if (!user) return;
    const load = () => {
      notificationService.getUnreadCount().then((r) => setCount(r.data.data || 0)).catch(() => {});
    };
    load();
    const interval = setInterval(load, 30000);
    return () => clearInterval(interval);
  }, [user]);

  const handleOpen = async () => {
    if (!open) {
      try {
        const r = await notificationService.getAll();
        setNotifications(r.data.data || []);
      } catch {
        return;
      }
    }
    setOpen((v) => !v);
  };

  const resolveNotificationId = (notification) => notification?.id ?? notification?.notificationId;

  const formatNotificationType = (value) => {
    const map = {
      WorkoutAssigned: t('notifications.types.workoutAssigned'),
      MessageReceived: t('notifications.types.messageReceived'),
      PaymentConfirmed: t('notifications.types.paymentConfirmed'),
      SubscriptionOverdue: t('notifications.types.subscriptionOverdue'),
      LeadCreated: t('notifications.types.leadCreated')
    };
    return map[value] || value;
  };

  const markRead = async (id) => {
    if (!id) return;
    try {
      await notificationService.markRead(id);
      setNotifications((prev) => prev.map((n) => (resolveNotificationId(n) === id ? { ...n, isRead: true } : n)));
      setCount((prev) => Math.max(0, prev - 1));
    } catch {
      return;
    }
  };

  if (!user) return null;

  return (
    <div className="relative">
      <button onClick={handleOpen} className="relative p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-white/10 text-gray-500 dark:text-slate-300 transition-colors" aria-label={t('common.notifications')}>
        <Bell size={20} />
        {count > 0 && (
          <span className="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-xs rounded-full flex items-center justify-center leading-none">
            {count > 9 ? '9+' : count}
          </span>
        )}
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
          <div className="absolute right-0 top-10 w-80 bg-white dark:bg-slate-900 rounded-2xl border border-gray-200 dark:border-white/10 shadow-xl z-50 overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100 dark:border-white/10 flex justify-between items-center">
              <span className="font-semibold text-sm text-gray-900 dark:text-white">{t('common.notifications')}</span>
              {count > 0 && (
                <button
                  onClick={async () => {
                    try {
                      await notificationService.markAllRead();
                    } catch {
                      return;
                    }
                    setNotifications((p) => p.map((n) => ({ ...n, isRead: true })));
                    setCount(0);
                  }}
                  className="text-xs text-indigo-600 hover:underline"
                >
                  {t('common.markAllRead')}
                </button>
              )}
            </div>
            <div className="max-h-80 overflow-y-auto">
              {notifications.length === 0 ? (
                <p className="text-center text-sm text-gray-400 dark:text-slate-400 py-8">{t('common.noNotifications')}</p>
              ) : (
                notifications.map((n, index) => (
                  <div
                    key={resolveNotificationId(n) ?? `notification-${index}`}
                    onClick={() => markRead(resolveNotificationId(n))}
                    className={`px-4 py-3 border-b border-gray-50 dark:border-white/5 cursor-pointer hover:bg-gray-50 dark:hover:bg-white/5 transition-colors ${!n.isRead ? 'bg-indigo-50/40 dark:bg-indigo-500/10' : ''}`}
                  >
                    <div className="flex items-start gap-2">
                      {!n.isRead && <div className="w-2 h-2 bg-indigo-500 rounded-full mt-1.5 shrink-0" />}
                      <div className={!n.isRead ? '' : 'ml-4'}>
                        <p className="text-sm font-medium text-gray-900 dark:text-white">{formatNotificationType(n.title || n.type || '')}</p>
                        <p className="text-xs text-gray-500 dark:text-slate-400 mt-0.5">{n.message}</p>
                        <p className="text-xs text-gray-300 dark:text-slate-500 mt-1">{new Date(n.createdAt).toLocaleDateString(language === 'pt-BR' ? 'pt-BR' : 'en-US')}</p>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
