import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './components/ui/Toast';
import { ProtectedRoute } from './routes/ProtectedRoute';

import { TrainerLayout } from './layouts/TrainerLayout';
import { StudentLayout } from './layouts/StudentLayout';
import { OwnerLayout } from './layouts/OwnerLayout';

import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { SetPasswordPage } from './pages/auth/SetPasswordPage';

import { OwnerDashboard } from './pages/owner/OwnerDashboard';
import { PlansPage } from './pages/owner/PlansPage';

import { TrainerDashboard } from './pages/trainer/TrainerDashboard';
import { StudentsPage } from './pages/trainer/StudentsPage';
import { StudentDetailPage } from './pages/trainer/StudentDetailPage';
import { ExercisesPage } from './pages/trainer/ExercisesPage';
import { ExerciseLibraryPage } from './pages/trainer/ExerciseLibraryPage';
import { WorkoutsPage } from './pages/trainer/WorkoutsPage';
import { SchedulePage } from './pages/trainer/SchedulePage';
import { PostsPage } from './pages/trainer/PostsPage';
import { SubscriptionPage } from './pages/trainer/SubscriptionPage';
import { ProfilePage } from './pages/trainer/ProfilePage';
import { ReportsPage } from './pages/trainer/ReportsPage';
import { PublicPageSettingsPage } from './pages/trainer/PublicPageSettingsPage';

import { StudentDashboard } from './pages/student/StudentDashboard';
import { StudentWorkoutsPage, StudentWorkoutDetailPage } from './pages/student/StudentWorkoutsPage';
import { StudentPostsPage, StudentPostDetailPage } from './pages/student/StudentPostsPage';
import { StudentProgressPage } from './pages/student/StudentProgressPage';
import { StudentPhotosPage } from './pages/student/StudentPhotosPage';
import { StudentAccessPage } from './pages/student/StudentAccessPage';
import { StudentCheckInPage } from './pages/student/StudentCheckInPage';
import { StudentAnamnesisPage } from './pages/student/StudentAnamnesisPage';

import { TrainerPublicPage } from './pages/public/TrainerPublicPage';

function HomeRedirect() {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  if (user.role === 'Owner') return <Navigate to="/owner" replace />;
  if (user.role === 'Trainer') return <Navigate to="/trainer" replace />;
  return <Navigate to="/student" replace />;
}

const T = ({ children }) => <ProtectedRoute roles={['Trainer']}><TrainerLayout>{children}</TrainerLayout></ProtectedRoute>;
const S = ({ children }) => <ProtectedRoute roles={['Student']}><StudentLayout>{children}</StudentLayout></ProtectedRoute>;
const O = ({ children }) => <ProtectedRoute roles={['Owner']}><OwnerLayout>{children}</OwnerLayout></ProtectedRoute>;

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <Routes>
            <Route path="/" element={<HomeRedirect />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/set-password" element={<SetPasswordPage />} />
            <Route path="/p/:slug" element={<TrainerPublicPage />} />

            {/* Owner */}
            <Route path="/owner" element={<O><OwnerDashboard /></O>} />
            <Route path="/owner/plans" element={<O><PlansPage /></O>} />

            {/* Trainer */}
            <Route path="/trainer" element={<T><TrainerDashboard /></T>} />
            <Route path="/trainer/students" element={<T><StudentsPage /></T>} />
            <Route path="/trainer/students/:id" element={<T><StudentDetailPage /></T>} />
            <Route path="/trainer/reports" element={<T><ReportsPage /></T>} />
            <Route path="/trainer/exercises" element={<T><ExercisesPage /></T>} />
            <Route path="/trainer/workouts" element={<T><WorkoutsPage /></T>} />
            <Route path="/trainer/library" element={<T><ExerciseLibraryPage /></T>} />
            <Route path="/trainer/schedule" element={<T><SchedulePage /></T>} />
            <Route path="/trainer/posts" element={<T><PostsPage /></T>} />
            <Route path="/trainer/public-page" element={<T><PublicPageSettingsPage /></T>} />
            <Route path="/trainer/subscription" element={<T><SubscriptionPage /></T>} />
            <Route path="/trainer/profile" element={<T><ProfilePage /></T>} />

            {/* Student */}
            <Route path="/student" element={<S><StudentDashboard /></S>} />
            <Route path="/student/workouts" element={<S><StudentWorkoutsPage /></S>} />
            <Route path="/student/workouts/:id" element={<S><StudentWorkoutDetailPage /></S>} />
            <Route path="/student/check-in" element={<S><StudentCheckInPage /></S>} />
            <Route path="/student/anamnesis" element={<S><StudentAnamnesisPage /></S>} />
            <Route path="/student/posts" element={<S><StudentPostsPage /></S>} />
            <Route path="/student/posts/:id" element={<S><StudentPostDetailPage /></S>} />
            <Route path="/student/progress" element={<S><StudentProgressPage /></S>} />
            <Route path="/student/photos" element={<S><StudentPhotosPage /></S>} />
            <Route path="/student/access" element={<S><StudentAccessPage /></S>} />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

