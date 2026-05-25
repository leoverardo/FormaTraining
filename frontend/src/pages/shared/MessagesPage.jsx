import { useEffect, useMemo, useRef, useState } from 'react';
import { MessageCircle, Send, RefreshCw, ChevronLeft } from 'lucide-react';
import { chatService } from '../../services/chatService';
import { studentService } from '../../services/studentService';
import { useToast } from '../../components/ui/Toast';
import { useAuth } from '../../contexts/AuthContext';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { Button } from '../../components/ui/Button';
import { useI18n } from '../../i18n';

const formatDateTime = (value, locale = 'pt-BR') => {
  if (!value) return '';
  const date = new Date(value);
  return date.toLocaleString(locale, { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
};

export function MessagesPage() {
  const { user } = useAuth();
  const { t, language } = useI18n();
  const { toast } = useToast();
  const [loading, setLoading] = useState(true);
  const [conversations, setConversations] = useState([]);
  const [selectedId, setSelectedId] = useState(null);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [messages, setMessages] = useState([]);
  const [composer, setComposer] = useState('');
  const [sending, setSending] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [mobileThreadOpen, setMobileThreadOpen] = useState(false);
  const [students, setStudents] = useState([]);
  const [starterStudentId, setStarterStudentId] = useState('');
  const [starterMessage, setStarterMessage] = useState('');
  const [creatingConversation, setCreatingConversation] = useState(false);
  const messagesEndRef = useRef(null);

  const selectedConversation = useMemo(
    () => conversations.find((c) => c.conversationId === selectedId) || null,
    [conversations, selectedId]
  );

  const loadConversations = async (withSpinner = true) => {
    if (withSpinner) setLoading(true);
    try {
      const response = await chatService.getConversations();
      const items = response.data.data || [];
      setConversations(items);
      if (!selectedId && items.length > 0) setSelectedId(items[0].conversationId);
      if (selectedId && !items.some((c) => c.conversationId === selectedId)) {
        setSelectedId(items[0]?.conversationId || null);
      }
    } catch (err) {
      toast(err.response?.data?.message || t('messages.loadConversationsError'), 'error');
    } finally {
      if (withSpinner) setLoading(false);
    }
  };

  const loadMessages = async (conversationId) => {
    if (!conversationId) return;
    setMessagesLoading(true);
    try {
      const response = await chatService.getConversation(conversationId);
      const data = response.data.data;
      setMessages(data?.messages || []);
      setConversations((prev) =>
        prev.map((item) =>
          item.conversationId === conversationId
            ? { ...item, unreadCount: 0, lastMessageAt: data?.messages?.[data.messages.length - 1]?.createdAt || item.lastMessageAt }
            : item
        )
      );
    } catch (err) {
      toast(err.response?.data?.message || t('messages.loadError'), 'error');
    } finally {
      setMessagesLoading(false);
    }
  };

  useEffect(() => {
    loadConversations();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (user?.role !== 'Trainer') return;
    studentService
      .getAll()
      .then((response) => {
        const activeStudents = (response.data.data || []).filter((s) => s.status === 'Active');
        setStudents(activeStudents);
        if (activeStudents.length > 0) setStarterStudentId(activeStudents[0].id);
      })
      .catch(() => {});
  }, [user?.role]);

  useEffect(() => {
    loadMessages(selectedId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth', block: 'end' });
  }, [messages]);

  const handleSend = async () => {
    if (!composer.trim() || !selectedConversation || sending) return;
    setSending(true);
    try {
      const response = await chatService.sendMessage({
        conversationId: selectedConversation.conversationId,
        content: composer,
      });
      const created = response.data.data;
      setMessages((prev) => [...prev, created]);
      setComposer('');
      setConversations((prev) =>
        prev
          .map((item) =>
            item.conversationId === selectedConversation.conversationId
              ? { ...item, lastMessagePreview: created.content, lastMessageAt: created.createdAt }
              : item
          )
          .sort((a, b) => new Date(b.lastMessageAt || 0) - new Date(a.lastMessageAt || 0))
      );
    } catch (err) {
      toast(err.response?.data?.message || t('messages.sendError'), 'error');
    } finally {
      setSending(false);
    }
  };

  const handleStartConversation = async () => {
    if (!starterStudentId || !starterMessage.trim() || creatingConversation) return;
    setCreatingConversation(true);
    try {
      await chatService.sendMessage({
        studentId: starterStudentId,
        content: starterMessage,
      });
      setStarterMessage('');
      await loadConversations(false);
    } catch (err) {
      toast(err.response?.data?.message || t('messages.startConversationError'), 'error');
    } finally {
      setCreatingConversation(false);
    }
  };

  const refresh = async () => {
    setRefreshing(true);
    await loadConversations(false);
    if (selectedId) await loadMessages(selectedId);
    setRefreshing(false);
  };

  if (loading) return <LoadingState />;

  if (conversations.length === 0) {
    if (user?.role === 'Trainer') {
      return (
        <div className="max-w-2xl mx-auto space-y-4">
          <EmptyState
            icon={MessageCircle}
            title={t('messages.noConversations')}
            description={t('messages.pickStudentFirstMessage')}
          />
          <div className="rounded-2xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 p-4 space-y-3">
            {students.length === 0 ? (
              <p className="text-sm text-slate-500 dark:text-slate-400">{t('messages.noActiveStudents')}</p>
            ) : (
              <>
                <select
                  value={starterStudentId}
                  onChange={(e) => setStarterStudentId(e.target.value)}
                  className="w-full rounded-xl border border-slate-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
                >
                  {students.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>
                <textarea
                  rows={3}
                  value={starterMessage}
                  onChange={(e) => setStarterMessage(e.target.value)}
                  placeholder={t('messages.typeFirstMessage')}
                  className="w-full rounded-xl border border-slate-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
                />
                <div className="flex justify-end">
                  <Button onClick={handleStartConversation} disabled={!starterStudentId || !starterMessage.trim()} loading={creatingConversation}>
                    <Send size={16} />
                    {t('messages.startConversation')}
                  </Button>
                </div>
              </>
            )}
          </div>
        </div>
      );
    }

    return (
      <EmptyState
        icon={MessageCircle}
        title={t('messages.noConversations')}
        description={user?.role === 'Trainer' ? t('messages.trainerEmpty') : t('messages.studentEmpty')}
      />
    );
  }

  return (
    <div className="h-[calc(100vh-9rem)] min-h-[520px] rounded-2xl border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900 overflow-hidden">
      <div className="grid h-full md:grid-cols-[320px_1fr]">
        <aside className={`border-r border-slate-200 overflow-y-auto ${mobileThreadOpen ? 'hidden md:block' : 'block'}`}>
          <div className="p-4 border-b border-slate-200 dark:border-white/10 flex items-center justify-between">
            <h1 className="font-semibold text-slate-900 dark:text-white">{t('messages.title')}</h1>
            <button className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-white/10 text-slate-500 dark:text-slate-300" onClick={refresh} aria-label={t('messages.refresh')}>
              <RefreshCw size={16} className={refreshing ? 'animate-spin' : ''} />
            </button>
          </div>
          <div className="p-2 space-y-1">
            {conversations.map((c) => (
              <button
                key={c.conversationId}
                onClick={() => {
                  setSelectedId(c.conversationId);
                  setMobileThreadOpen(true);
                }}
                className={`w-full text-left p-3 rounded-xl transition ${
                  selectedId === c.conversationId ? 'bg-indigo-50 dark:bg-indigo-500/15 border border-indigo-100 dark:border-indigo-400/30' : 'hover:bg-slate-50 dark:hover:bg-white/5 border border-transparent'
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <p className="font-medium text-slate-900 dark:text-white truncate">{c.participantName}</p>
                  <span className="text-xs text-slate-400 shrink-0">{formatDateTime(c.lastMessageAt, language === 'pt-BR' ? 'pt-BR' : 'en-US')}</span>
                </div>
                <div className="mt-1 flex items-center justify-between gap-2">
                  <p className="text-sm text-slate-500 dark:text-slate-400 truncate">{c.lastMessagePreview || t('messages.noMessagesYet')}</p>
                  {c.unreadCount > 0 && (
                    <span className="h-5 min-w-5 px-1 rounded-full bg-rose-500 text-white text-xs flex items-center justify-center shrink-0">
                      {c.unreadCount > 99 ? '99+' : c.unreadCount}
                    </span>
                  )}
                </div>
              </button>
            ))}
          </div>
        </aside>

        <section className={`h-full flex flex-col ${mobileThreadOpen ? 'flex' : 'hidden md:flex'}`}>
          {!selectedConversation ? (
            <div className="h-full flex items-center justify-center text-slate-400">{t('messages.selectConversation')}</div>
          ) : (
            <>
              <header className="px-4 py-3 border-b border-slate-200 dark:border-white/10 flex items-center gap-2">
                <button className="md:hidden p-1 rounded hover:bg-slate-100 dark:hover:bg-white/10 text-slate-500 dark:text-slate-300" onClick={() => setMobileThreadOpen(false)} aria-label={t('common.back')}>
                  <ChevronLeft size={18} />
                </button>
                <p className="font-medium text-slate-900 dark:text-white">{selectedConversation.participantName}</p>
              </header>
              <div className="flex-1 overflow-y-auto p-4 space-y-3 bg-slate-50/40 dark:bg-slate-950/40">
                {messagesLoading ? (
                  <LoadingState />
                ) : messages.length === 0 ? (
                  <EmptyState icon={MessageCircle} title={t('messages.emptyConversation')} description={t('messages.sendFirst')} />
                ) : (
                  messages.map((m) => {
                    const mine = m.senderUserId === user?.id;
                    return (
                      <div key={m.id} className={`flex ${mine ? 'justify-end' : 'justify-start'}`}>
                        <div className={`max-w-[82%] rounded-2xl px-3 py-2 ${mine ? 'bg-indigo-600 text-white' : 'bg-white dark:bg-slate-900 border border-slate-200 dark:border-white/10 text-slate-800 dark:text-slate-100'}`}>
                          <p className="text-sm whitespace-pre-wrap">{m.content}</p>
                          <p className={`text-[11px] mt-1 ${mine ? 'text-indigo-100' : 'text-slate-400'}`}>{formatDateTime(m.createdAt, language === 'pt-BR' ? 'pt-BR' : 'en-US')}</p>
                        </div>
                      </div>
                    );
                  })
                )}
                <div ref={messagesEndRef} />
              </div>
              <footer className="p-3 border-t border-slate-200 dark:border-white/10 bg-white dark:bg-slate-900">
                <div className="flex gap-2">
                  <textarea
                    rows={2}
                    value={composer}
                    onChange={(e) => setComposer(e.target.value)}
                    placeholder={t('messages.typeMessage')}
                    className="flex-1 rounded-xl border border-slate-300 dark:border-white/10 bg-white dark:bg-slate-950 text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500 px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
                  />
                  <Button onClick={handleSend} disabled={sending || !composer.trim()} className="h-10 self-end">
                    <Send size={16} />
                  </Button>
                </div>
              </footer>
            </>
          )}
        </section>
      </div>
    </div>
  );
}

