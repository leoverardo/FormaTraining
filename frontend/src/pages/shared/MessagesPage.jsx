import { useEffect, useMemo, useRef, useState } from 'react';
import { MessageCircle, Send, RefreshCw, ChevronLeft } from 'lucide-react';
import { chatService } from '../../services/chatService';
import { studentService } from '../../services/studentService';
import { useToast } from '../../components/ui/Toast';
import { useAuth } from '../../contexts/AuthContext';
import { EmptyState } from '../../components/ui/EmptyState';
import { LoadingState } from '../../components/ui/LoadingState';
import { Button } from '../../components/ui/Button';

const formatDateTime = (value) => {
  if (!value) return '';
  const date = new Date(value);
  return date.toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
};

export function MessagesPage() {
  const { user } = useAuth();
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
      toast(err.response?.data?.message || 'Erro ao carregar conversas.', 'error');
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
      toast(err.response?.data?.message || 'Erro ao carregar mensagens.', 'error');
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
      toast(err.response?.data?.message || 'Não foi possível enviar a mensagem.', 'error');
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
      toast(err.response?.data?.message || 'Não foi possível iniciar a conversa.', 'error');
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
            title="Nenhuma conversa ainda"
            description="Escolha um aluno ativo e envie a primeira mensagem."
          />
          <div className="rounded-2xl border border-slate-200 bg-white p-4 space-y-3">
            {students.length === 0 ? (
              <p className="text-sm text-slate-500">Você não possui alunos ativos para iniciar conversa.</p>
            ) : (
              <>
                <select
                  value={starterStudentId}
                  onChange={(e) => setStarterStudentId(e.target.value)}
                  className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
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
                  placeholder="Digite a primeira mensagem..."
                  className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
                />
                <div className="flex justify-end">
                  <Button onClick={handleStartConversation} disabled={!starterStudentId || !starterMessage.trim()} loading={creatingConversation}>
                    <Send size={16} />
                    Iniciar conversa
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
        title="Nenhuma conversa ainda"
        description={user?.role === 'Trainer' ? 'As conversas com seus alunos aparecerão aqui.' : 'Sua conversa com seu personal aparecerá aqui.'}
      />
    );
  }

  return (
    <div className="h-[calc(100vh-9rem)] min-h-[520px] rounded-2xl border border-slate-200 bg-white overflow-hidden">
      <div className="grid h-full md:grid-cols-[320px_1fr]">
        <aside className={`border-r border-slate-200 overflow-y-auto ${mobileThreadOpen ? 'hidden md:block' : 'block'}`}>
          <div className="p-4 border-b border-slate-200 flex items-center justify-between">
            <h1 className="font-semibold text-slate-900">Mensagens</h1>
            <button className="p-2 rounded-lg hover:bg-slate-100 text-slate-500" onClick={refresh} aria-label="Atualizar">
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
                  selectedId === c.conversationId ? 'bg-indigo-50 border border-indigo-100' : 'hover:bg-slate-50 border border-transparent'
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <p className="font-medium text-slate-900 truncate">{c.participantName}</p>
                  <span className="text-xs text-slate-400 shrink-0">{formatDateTime(c.lastMessageAt)}</span>
                </div>
                <div className="mt-1 flex items-center justify-between gap-2">
                  <p className="text-sm text-slate-500 truncate">{c.lastMessagePreview || 'Sem mensagens ainda'}</p>
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
            <div className="h-full flex items-center justify-center text-slate-400">Selecione uma conversa</div>
          ) : (
            <>
              <header className="px-4 py-3 border-b border-slate-200 flex items-center gap-2">
                <button className="md:hidden p-1 rounded hover:bg-slate-100 text-slate-500" onClick={() => setMobileThreadOpen(false)} aria-label="Voltar">
                  <ChevronLeft size={18} />
                </button>
                <p className="font-medium text-slate-900">{selectedConversation.participantName}</p>
              </header>
              <div className="flex-1 overflow-y-auto p-4 space-y-3 bg-slate-50/40">
                {messagesLoading ? (
                  <LoadingState />
                ) : messages.length === 0 ? (
                  <EmptyState icon={MessageCircle} title="Conversa vazia" description="Envie a primeira mensagem para iniciar." />
                ) : (
                  messages.map((m) => {
                    const mine = m.senderUserId === user?.id;
                    return (
                      <div key={m.id} className={`flex ${mine ? 'justify-end' : 'justify-start'}`}>
                        <div className={`max-w-[82%] rounded-2xl px-3 py-2 ${mine ? 'bg-indigo-600 text-white' : 'bg-white border border-slate-200 text-slate-800'}`}>
                          <p className="text-sm whitespace-pre-wrap">{m.content}</p>
                          <p className={`text-[11px] mt-1 ${mine ? 'text-indigo-100' : 'text-slate-400'}`}>{formatDateTime(m.createdAt)}</p>
                        </div>
                      </div>
                    );
                  })
                )}
                <div ref={messagesEndRef} />
              </div>
              <footer className="p-3 border-t border-slate-200 bg-white">
                <div className="flex gap-2">
                  <textarea
                    rows={2}
                    value={composer}
                    onChange={(e) => setComposer(e.target.value)}
                    placeholder="Digite sua mensagem..."
                    className="flex-1 rounded-xl border border-slate-300 px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-indigo-500/30"
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

