// Development-only, isolated in the preview iframe. No real API request is sent.
import api from '../services/api';

const date = '2026-10-02T12:00:00Z';
const students = ['Lucas Almeida', 'Ana Oliveira', 'Pedro Santos', 'Julia Costa', 'Rafael Lima'].map((name, i) => ({ id: String(i + 1), name, email: `aluno${i + 1}@example.com`, goal: 'Hipertrofia', status: 'Active', phone: '', notes: 'Acompanhamento demonstrativo.', createdAt: date, monitoringStatus: 'OnTrack', checkInsThisMonth: 4, workoutsCompletedThisMonth: 12 }));
const exercises = ['Agachamento livre', 'Remada com halteres', 'Supino reto', 'Elevação lateral', 'Prancha'].map((name, i) => ({ id: String(i + 1), name, muscleGroup: i === 0 ? 'Pernas' : 'Superiores', category: 'Força', equipment: 'Halteres', description: 'Controle o movimento e mantenha uma postura confortável.', instructions: 'Execute conforme orientação do seu personal.', level: 'Intermediate', isActive: true }));
const workouts = ['Treino A · Força e base', 'Treino B · Superiores', 'Treino C · Mobilidade'].map((name, i) => ({ id: String(i + 1), name, goal: 'Hipertrofia', level: 'Intermediate', status: 'Active', description: 'Uma rotina para evoluir com consistência.', exercises: exercises.slice(0, 3).map((exercise, j) => ({ id: String(j + 1), exerciseId: exercise.id, exercise, exerciseName: exercise.name, sets: 3, reps: '12', restSeconds: 60, orderIndex: j, suggestedLoad: 'Conforme orientação' })) }));
const plans = ['Starter', 'Pro', 'Growth'].map((name, i) => ({ id: String(i + 1), name, description: 'Treinos, agenda e acompanhamento em um só lugar.', monthlyPrice: [97,197,297][i], quarterlyPrice: [267,547,797][i], yearlyPrice: [997,1997,2997][i], price: [97,197,297][i], maxStudents: [20,50,100][i], active: true, isPublic: true, features: 'Treinos e exercícios;Agenda;Página pública' }));
const profile = { id: '1', name: 'Marina Costa', fullName: 'Marina Costa', email: 'marina@example.com', brandName: 'Marina Costa Training', bio: 'Movimento com propósito. Acompanhamento com presença.', publicHeadline: 'Seu próximo passo começa aqui.', publicDescription: 'Treinos personalizados para a sua rotina.', specialties: 'Força, Mobilidade, Condicionamento', primaryColor: '#1d1d1f', secondaryColor: '#707070', publicPageEnabled: true, publicPageSlug: 'marina', slug: 'marina', showTestimonials: true, posts: [], testimonials: [], transformations: [], serviceOffers: [], publicStudentCount: 24, yearsExperience: 8 };
const progress = Array.from({ length: 6 }, (_, i) => ({ id: String(i+1), date: `2026-09-${String(2+i*4).padStart(2,'0')}T12:00:00Z`, progressDate: `2026-09-${String(2+i*4).padStart(2,'0')}T12:00:00Z`, weight: 78-i*.4, bodyFat: 20-i*.3, waist: 84-i*.4, chest: 98, hips: 96, notes: 'Registro demonstrativo.' }));
const posts = [{ id: '1', title: 'Consistência se constrói um dia de cada vez.', description: 'Reserve um momento para reconhecer seus avanços. Cada treino conta na construção de uma rotina mais ativa.', createdAt: date, publishedAt: date, status: 'Published', type: 'Text', visibility: 'Public', authorName: profile.name, trainerName: profile.name, media: [], mediaItems: [], likesCount: 12, commentsCount: 0 }];
const access = { allowed: true, message: 'Seu acompanhamento está ativo.', trainerName: profile.name, trainerBrand: profile.brandName, status: 'Active' };

function fixture(path) {
  if (path === '/trainer/dashboard') return { activeStudents: 24, totalWorkouts: 18, totalPublishedPosts: 8, missingCheckInsCount: 3, appointmentsTodayCount: 6, recentActivities: posts };
  if (path === '/student/dashboard') return { studentName: students[0].name, trainerBrand: profile.brandName, weekSchedule: [1,3,5].map(dayOfWeek => ({ dayOfWeek, workoutId: '1', workoutName: workouts[0].name })), todayWorkout: workouts[0], totalWorkouts: 12, completedWorkoutsThisWeek: 4, weeklyGoal: 5, latestProgress: progress[5], recentPosts: posts };
  if (path === '/owner/dashboard') return { summary: { totalTrainers: 48, activeTrainers: 42, totalStudents: 386, activeStudents: 342 }, revenue: { mrr: 8274, totalRevenue: 24822 }, subscriptions: { active: 42, trial: 6 }, studentMetrics: { total: 386, active: 342 }, planDistribution: [], recentTrainers: [], recentPayments: [] };
  if (path === '/student/access-status') return access;
  if (path === '/students') return students;
  if (/^\/students\/[^/]+$/.test(path)) return students.find(s => s.id === path.split('/').pop()) || students[0];
  if (path === '/trainer/profile' || path === '/trainer/public-page' || path.startsWith('/public/trainers/')) return profile;
  if (path === '/platform-plans') return plans;
  if (path === '/trainer/subscription') return { id: '1', status: 'Active', planName: 'Pro', platformPlanName: 'Pro', platformPlan: plans[1], billingCycle: 'Monthly', currentPeriodEnd: '2026-11-02T12:00:00Z', endDate: '2026-11-02T12:00:00Z', maxStudents: 50, currentStudents: 24 };
  if (path === '/workouts' || path === '/student/workouts') return workouts;
  if (/\/workouts\/[^/]+$/.test(path)) return workouts[0];
  if (path === '/exercises' || path.includes('exercise-library')) return exercises;
  if (path === '/posts' || path === '/student/posts' || path === '/explore/feed') return posts;
  if (/\/posts\/[^/]+$/.test(path)) return posts[0];
  if (path.endsWith('/progress')) return progress;
  if (path.endsWith('/access-status')) return access;
  if (path === '/notifications/unread-count') return 0;
  if (path.endsWith('/payment-status')) return { isPaymentConfirmed: false, onboardingStatus: 'PendingPayment', subscriptionStatus: 'Pending', paymentStatus: 'Pending' };
  if (path.endsWith('/habits/today')) return { items: [], completedHabits: 0, totalHabits: 0 };
  if (path.endsWith('/nutrition-guidance') || path.endsWith('/gamification/summary') || path.endsWith('/monthly-goals') || path.endsWith('/current-week') || path.endsWith('/latest')) return null;
  if (path.includes('adherence')) return { items: [], adherencePercentage: 0 };
  if (path.includes('anamnesis')) return { mainGoal: 'Hipertrofia', trainingExperience: 'Intermediario', trainingLocation: 'Academia', availableDaysPerWeek: 3, sleepQuality: 4, stressLevel: 2 };
  if (path.startsWith('/explore/trainers')) return { items: [{ trainerId: '1', name: profile.name, brandName: profile.brandName, bio: profile.bio, specialties: profile.specialties, slug: 'marina', publicPageSlug: 'marina', isFollowing: false }], totalCount: 1, page: 1, pageSize: 20 };
  if (path.startsWith('/reports')) return { totalStudents: 24, activeStudents: 24, studentsWithCheckInThisWeek: 21, studentsWithoutCheckInThisWeek: 3, workoutsCompletedThisWeek: 86, progressRecordsThisWeek: 18, unreadNotifications: 0, studentEngagement: students.map(s => ({...s, studentId: s.id})) };
  if (path.endsWith('/summary')) return {};
  if (path.startsWith('/legal/')) return path.endsWith('/active') ? [] : { title: 'Documento demonstrativo', version: 'Prévia', content: 'Esta tela exibe apenas um exemplo visual. Consulte os documentos vigentes na aplicação.', publishedAt: date };
  return [];
}

export async function installDesignPreview() {
  const role = new URLSearchParams(window.location.search).get('previewRole') || 'Trainer';
  const student = role === 'Student' || role === 'Explorer';
  const user = { id: '1', name: student ? 'Lucas Almeida' : role === 'Owner' ? 'Administração Forma' : profile.name, email: 'preview@example.com', role: student ? 'Student' : role, hasActiveTrainerLink: role === 'Student', isExplorer: role === 'Explorer', studentId: '1', studentProfileId: '1', trainerId: '1' };
  api.defaults.adapter = async config => {
    if (config.method !== 'get') {
      const message = 'Prévia visual: alterações e envios estão desativados.';
      const error = new Error(message);
      error.response = { status: 409, data: { message } };
      throw error;
    }
    const path = config.url.replace(/^\/api/, '').split('?')[0];
    return { data: { success: true, data: structuredClone(fixture(path)) }, status: 200, statusText: 'OK', headers: {}, config };
  };
  return user;
}
