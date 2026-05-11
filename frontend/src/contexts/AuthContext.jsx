import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { authService } from '../services/authService';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try { return JSON.parse(localStorage.getItem('user')); } catch { return null; }
  });
  const [loading, setLoading] = useState(true);

  const normalizeUser = (rawUser) => {
    if (!rawUser) return null;

    return {
      ...rawUser,
      id: rawUser.id ?? rawUser.userId ?? null,
      role: rawUser.role ?? '',
      isExplorer: rawUser.isExplorer === true,
      hasActiveTrainerLink: rawUser.hasActiveTrainerLink === true,
      studentProfileId: rawUser.studentProfileId ?? null,
      studentId: rawUser.studentId ?? null,
      trainerId: rawUser.trainerId ?? null,
    };
  };

  const persistUser = (nextUser) => {
    if (!nextUser) {
      localStorage.removeItem('user');
      setUser(null);
      return;
    }

    localStorage.setItem('user', JSON.stringify(nextUser));
    setUser(nextUser);
  };

  const login = async (email, password) => {
    const res = await authService.login({ email, password });
    const { token, user: payloadUser } = res.data.data;
    const nextUser = normalizeUser(payloadUser);
    localStorage.setItem('token', token);
    persistUser(nextUser);
    return nextUser;
  };

  const refreshMe = async () => {
    const token = localStorage.getItem('token');
    if (!token) {
      persistUser(null);
      return null;
    }

    const res = await authService.me();
    const meUser = normalizeUser(res.data?.data);
    persistUser(meUser);
    return meUser;
  };

  useEffect(() => {
    let mounted = true;
    const bootstrap = async () => {
      try {
        await refreshMe();
      } catch {
        localStorage.removeItem('token');
        if (mounted) persistUser(null);
      } finally {
        if (mounted) setLoading(false);
      }
    };

    bootstrap();
    return () => { mounted = false; };
  }, []);

  const logout = () => {
    localStorage.removeItem('token');
    persistUser(null);
  };

  const authFlags = useMemo(() => {
    const isStudent = user?.role === 'Student';
    return {
      isTrainer: user?.role === 'Trainer',
      isOwner: user?.role === 'Owner',
      isStudent,
      isExplorerStudent: isStudent && user?.isExplorer === true,
      isLinkedStudent: isStudent && user?.hasActiveTrainerLink === true,
    };
  }, [user]);

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refreshMe, ...authFlags }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};

