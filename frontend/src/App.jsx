import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { ToastProvider } from './components/ui/Toast';
import { ProtectedRoute, ProtectedStudentAreaRoute } from './routes/ProtectedRoute';

import { TrainerLayout } from './layouts/TrainerLayout';
import { StudentLayout } from './layouts/StudentLayout';
import { OwnerLayout } from './layouts/OwnerLayout';

import { LoginPage } from './pages/auth/LoginPage';
import { RegisterPage } from './pages/auth/RegisterPage';
import { OnboardingPaymentStatusPage } from './pages/auth/OnboardingPaymentStatusPage';
import { SetPasswordPage } from './pages/auth/SetPasswordPage';
import { StudentRegisterPage } from './pages/auth/StudentRegisterPage';

import { OwnerDashboard } from './pages/owner/OwnerDashboard';
import { PlansPage } from './pages/owner/PlansPage';

import { TrainerDashboard } from './pages/trainer/TrainerDashboard';
import { StudentsPage } from './pages/trainer/StudentsPage';
import { StudentDetailPage } from './pages/trainer/StudentDetailPage';
import { ExercisesPage } from './pages/trainer/ExercisesPage';
import { ExerciseLibraryPage } from './pages/trainer/ExerciseLibraryPage';
import { WorkoutsPage } from './pages/trainer/WorkoutsPage';
import { SchedulePage } from './pages/trainer/SchedulePage';
import { AppointmentsPage } from './pages/trainer/AppointmentsPage';
import { PostsPage } from './pages/trainer/PostsPage';
import { SubscriptionPage } from './pages/trainer/SubscriptionPage';
import { ProfilePage } from './pages/trainer/ProfilePage';
import { ReportsPage } from './pages/trainer/ReportsPage';
import { PublicPageSettingsPage } from './pages/trainer/PublicPageSettingsPage';
import { TrainerLeadsPage } from './pages/trainer/TrainerLeadsPage';
import { ServiceSalesPage } from './pages/trainer/ServiceSalesPage';
import { MessagesPage } from './pages/shared/MessagesPage';

import { StudentDashboard } from './pages/student/StudentDashboard';
import { StudentAppointmentsPage } from './pages/student/StudentAppointmentsPage';
import { StudentWorkoutsPage, StudentWorkoutDetailPage } from './pages/student/StudentWorkoutsPage';
import { StudentPostsPage, StudentPostDetailPage } from './pages/student/StudentPostsPage';
import { StudentProgressPage } from './pages/student/StudentProgressPage';
import { StudentPhotosPage } from './pages/student/StudentPhotosPage';
import { StudentAccessPage } from './pages/student/StudentAccessPage';
import { StudentCheckInPage } from './pages/student/StudentCheckInPage';
import { StudentAnamnesisPage } from './pages/student/StudentAnamnesisPage';
import { ExploreFeedPage } from './pages/student/ExploreFeedPage';
import { ExploreTrainersPage } from './pages/student/ExploreTrainersPage';
import { ExploreSavedPage } from './pages/student/ExploreSavedPage';
import { ExploreFollowingPage } from './pages/student/ExploreFollowingPage';

import { TrainerPublicPage } from './pages/public/TrainerPublicPage';
import { PrivacyPolicyPage, TermsOfUsePage } from './pages/public/LegalPages';
import { PrivacySettingsPage } from './pages/shared/PrivacySettingsPage';
import { OwnerPrivacyPage } from './pages/owner/OwnerPrivacyPage';

function HomeRedirect() {
  const { user, loading, isExplorerStudent, isLinkedStudent } = useAuth();
  if (loading) return null;
  if (!user) return <Navigate to="/login" replace />;
  if (user.role === 'Owner') return <Navigate to="/owner" replace />;
  if (user.role === 'Trainer') return <Navigate to="/trainer/dashboard" replace />;
  if (isLinkedStudent) return <Navigate to="/student/dashboard" replace />;
  if (isExplorerStudent) return <Navigate to="/explore" replace />;
  return <Navigate to="/explore" replace />;
}

const T = ({ children }) => <ProtectedRoute roles={['Trainer']}><TrainerLayout>{children}</TrainerLayout></ProtectedRoute>;
const S = ({ children }) => <ProtectedRoute roles={['Student']}><StudentLayout>{children}</StudentLayout></ProtectedRoute>;
const O = ({ children }) => <ProtectedRoute roles={['Owner']}><OwnerLayout>{children}</OwnerLayout></ProtectedRoute>;
const SL = ({ children }) => <ProtectedStudentAreaRoute><StudentLayout>{children}</StudentLayout></ProtectedStudentAreaRoute>;

export default function App() {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <AuthProvider>
          <ToastProvider>
            <Routes>
            <Route path="/" element={<HomeRedirect />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/student/register" element={<StudentRegisterPage />} />
            <Route path="/set-password" element={<SetPasswordPage />} />
            <Route path="/onboarding/return" element={<OnboardingPaymentStatusPage />} />
            <Route path="/onboarding/success" element={<OnboardingPaymentStatusPage />} />
            <Route path="/p/:slug" element={<TrainerPublicPage />} />
            <Route path="/privacy-policy" element={<PrivacyPolicyPage />} />
            <Route path="/terms-of-use" element={<TermsOfUsePage />} />

            {/* Owner */}
            <Route path="/owner" element={<O><OwnerDashboard /></O>} />
            <Route path="/owner/plans" element={<O><PlansPage /></O>} />
            <Route path="/owner/privacy" element={<O><OwnerPrivacyPage /></O>} />

            {/* Trainer */}
            <Route path="/trainer" element={<T><TrainerDashboard /></T>} />
            <Route path="/trainer/dashboard" element={<T><TrainerDashboard /></T>} />
            <Route path="/trainer/students" element={<T><StudentsPage /></T>} />
            <Route path="/trainer/students/:id" element={<T><StudentDetailPage /></T>} />
            <Route path="/trainer/reports" element={<T><ReportsPage /></T>} />
            <Route path="/trainer/exercises" element={<T><ExercisesPage /></T>} />
            <Route path="/trainer/workouts" element={<T><WorkoutsPage /></T>} />
            <Route path="/trainer/library" element={<T><ExerciseLibraryPage /></T>} />
            <Route path="/trainer/schedule" element={<T><SchedulePage /></T>} />
            <Route path="/trainer/appointments" element={<T><AppointmentsPage /></T>} />
            <Route path="/trainer/posts" element={<T><PostsPage /></T>} />
            <Route path="/trainer/public-page" element={<T><PublicPageSettingsPage /></T>} />
            <Route path="/trainer/leads" element={<T><TrainerLeadsPage /></T>} />
            <Route path="/trainer/sales" element={<T><ServiceSalesPage /></T>} />
            <Route path="/trainer/subscription" element={<T><SubscriptionPage /></T>} />
            <Route path="/trainer/profile" element={<T><ProfilePage /></T>} />
            <Route path="/trainer/privacy" element={<T><PrivacySettingsPage /></T>} />
            <Route path="/trainer/messages" element={<T><MessagesPage /></T>} />

            {/* Student */}
            <Route path="/student" element={<Navigate to="/student/dashboard" replace />} />
            <Route path="/student/dashboard" element={<SL><StudentDashboard /></SL>} />
            <Route path="/student/workouts" element={<SL><StudentWorkoutsPage /></SL>} />
            <Route path="/student/appointments" element={<SL><StudentAppointmentsPage /></SL>} />
            <Route path="/student/workouts/:id" element={<SL><StudentWorkoutDetailPage /></SL>} />
            <Route path="/student/check-in" element={<SL><StudentCheckInPage /></SL>} />
            <Route path="/student/anamnesis" element={<SL><StudentAnamnesisPage /></SL>} />
            <Route path="/student/posts" element={<SL><StudentPostsPage /></SL>} />
            <Route path="/student/posts/:id" element={<SL><StudentPostDetailPage /></SL>} />
            <Route path="/student/progress" element={<SL><StudentProgressPage /></SL>} />
            <Route path="/student/photos" element={<SL><StudentPhotosPage /></SL>} />
            <Route path="/student/access" element={<SL><StudentAccessPage /></SL>} />
            <Route path="/student/profile" element={<S><StudentAccessPage /></S>} />
            <Route path="/student/privacy" element={<S><PrivacySettingsPage /></S>} />
            <Route path="/student/messages" element={<SL><MessagesPage /></SL>} />
            <Route path="/explore" element={<S><ExploreFeedPage /></S>} />
            <Route path="/explore/trainers" element={<S><ExploreTrainersPage /></S>} />
            <Route path="/explore/saved" element={<S><ExploreSavedPage /></S>} />
            <Route path="/explore/following" element={<S><ExploreFollowingPage /></S>} />

            <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </ToastProvider>
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

