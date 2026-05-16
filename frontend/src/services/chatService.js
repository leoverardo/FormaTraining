import api from './api';

export const chatService = {
  getConversations: () => api.get('/chat/conversations'),
  getConversation: (conversationId) => api.get(`/chat/conversations/${conversationId}`),
  sendMessage: (data) => api.post('/chat/messages', data),
  getUnreadCount: () => api.get('/chat/unread-count'),
};

