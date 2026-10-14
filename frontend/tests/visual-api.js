import contracts from './contracts.json' with { type: 'json' };

export const demoId = '11111111-1111-4111-8111-111111111111';
const date = '2026-10-14T12:00:00Z';
const sample = (name, values) => ({ ...contracts[name], ...values });
export const student = sample('StudentResponse', {
  id: demoId, name: 'Marina Costa Almeida — acompanhamento de força e mobilidade',
  email: 'marina@example.invalid', status: 'Active', goal: 'Força e mobilidade', phone: '51999999999',
});
const exercise = sample('ExerciseResponse', { id: demoId, name: 'Agachamento livre', muscleGroup: 'Pernas', equipment: 'Barra', category: 'Strength', description: 'Controle o movimento e respeite a amplitude.' });
const workout = sample('WorkoutResponse', { id: demoId, name: 'Força e mobilidade', goal: 'Força', level: 'Intermediate', status: 'Active', exercises: [sample('WorkoutExerciseResponse', { id: demoId, sets: 3, reps: '12', restSeconds: 60, exercise })] });
const post = sample('PostResponse', { id: demoId, title: 'Cada passo conta', description: 'Constância para construir sua melhor forma.', type: 'Article', status: 'Published', publishedAt: date, tags: ['Treino'], authorName: 'Marina Costa', media: [] });
const plan = sample('PlatformPlanResponse', { id: demoId, code: 'BASIC', name: 'Basic', description: 'Organize sua rotina', monthlyPrice: 97, maxActiveStudents: 20, active: true, isPublic: true, isAvailableForPurchase: true, prices: [sample('PlanPriceResponse', { billingCycle: 'Monthly', price: 97 })] });
const profile = sample('TrainerProfileResponse', { id: demoId, name: 'Marina Costa', brandName: 'Marina Coach', bio: 'Treinos com intenção.', primaryColor: '#0f766e', secondaryColor: '#0891b2', publicSlug: 'marina-coach', specialties: 'Força,Mobilidade' });
export const publicPage = sample('PublicPageResponse', { ...profile, publicHeadline: 'Sua evolução começa aqui.', publicPageEnabled: true, showTestimonials: false, posts: [post], serviceOffers: [], transformations: [], testimonials: [], publicStudentCount: 24, yearsExperience: 8, publicReviews: '4.9', bannerUrl: '/brand/Capa.png', profilePhotoUrl: '/brand/IconePrincipalUpCoach.png', whatsappNumber: '51999999999' });

export async function installVisualApi(page, role = 'Trainer', options = {}) {
  const user = { id: demoId, userId: demoId, name: 'Marina Costa', email: 'marina@example.invalid', role,
    isExplorer: role === 'Student', hasActiveTrainerLink: role === 'Student', studentId: role === 'Student' ? demoId : null,
    studentProfileId: role === 'Student' ? demoId : null, trainerId: role === 'Trainer' ? demoId : null };
  await page.addInitScript(value => {
    localStorage.setItem('token', 'visual-test-token');
    localStorage.setItem('user', JSON.stringify(value));
  }, user);
  const unhandled = [];
  await page.route('**/api/**', async route => {
    const path = new URL(route.request().url()).pathname.replace(/^\/api/, '');
    let data;
    const emptyLists = ['/notifications','/chat/conversations','/privacy/consents','/privacy/requests/my','/owner/privacy/requests','/owner/privacy/incidents','/owner/privacy/vendors','/trainer/leads','/trainer/service-offers','/trainer/service-orders','/student/workout-sessions','/student/check-ins','/trainer/check-ins/recent','/trainer/check-ins/missing-current-week','/exercise-library','/workout-templates','/testimonials','/transformations','/student/testimonial-requests','/student/gamification/achievements','/student/following-trainers','/student/saved-trainers','/trainer/appointments','/student/appointments'];
    if (path === '/auth/me') data = user;
    else if (path === '/auth/login') data = { token: 'visual-test-token', user };
    else if (path.endsWith('/unread-count')) data = 0;
    else if (emptyLists.includes(path)) data = [];
    else if (path === '/students') data = options.emptyStudents ? [] : [student];
    else if (path === `/students/${demoId}`) data = student;
    else if (path === '/exercises') data = [exercise];
    else if (path === '/workouts' || path === '/student/workouts') data = [workout];
    else if (path === `/student/workouts/${demoId}`) data = workout;
    else if (path === '/posts' || path === '/student/posts' || path === '/explore/feed') data = [post];
    else if (path === `/student/posts/${demoId}`) data = post;
    else if (path === '/platform-plans' || path === '/public/platform-plans') data = [plan];
    else if (path === '/trainer/profile') data = profile;
    else if (path === '/trainer/public-page') data = publicPage;
    else if (path.startsWith('/public/trainers/')) data = path.endsWith('/service-offers') ? [] : publicPage;
    else if (path === '/trainer/subscription') data = sample('SubscriptionResponse', { id: demoId, planName: 'Basic', status: 'Active', billingCycle: 'Monthly', endDate: '2027-01-01T00:00:00Z' });
    else if (path === '/trainer/dashboard') data = sample('TrainerDashboardResponse', { activeStudents: 24, totalWorkouts: 12, totalPublishedPosts: 4, hasActiveSubscription: true, recentActivities: [] });
    else if (path === '/trainer/reports/overview') data = sample('OverviewReportResponse', { totalStudents: 24, activeStudents: 24, studentEngagement: [] });
    else if (path === '/owner/dashboard') data = sample('OwnerDashboardResponse', { range: 30 });
    else if (path === '/student/access-status') data = { allowed: true, reason: 'Active', message: 'Seu acesso está ativo.' };
    else if (path === '/student/dashboard') data = { trainerBrand: 'Marina Coach', todayWorkout: workout, weekSchedule: [], lastWeight: 72, lastWaist: 80, lastCheckInAt: date, nextAppointment: null };
    else if (path === '/student/habits/today') data = sample('StudentHabitTodayResponse', { items: [], totalHabits: 0, completedHabits: 0 });
    else if (path.includes('/gamification/summary')) data = sample('GamificationSummaryResponse', {});
    else if (path.includes('/gamification/monthly-goals')) data = null;
    else if (path.endsWith('/nutrition-guidance')) data = sample('StudentNutritionGuidanceResponse', { guidanceText: 'Mantenha sua hidratação ao longo do dia.' });
    else if (path.endsWith('/habits/adherence')) data = null;
    else if (path.endsWith('/habits') || path.endsWith('/schedule') || path.endsWith('/progress') || path.endsWith('/progress-photos') || path.endsWith('/photos') || path.endsWith('/internal-notes')) data = [];
    else if (path.includes('/progress/summary')) data = sample('ProgressSummaryDto', {});
    else if (path.endsWith('/anamnesis')) data = sample('AnamnesisResponse', {});
    else if (path === '/student/check-ins/current-week' || path === '/privacy/data-export/latest') data = null;
    else if (path === '/trainer/service-orders-summary') data = { totalOrders: 0, approvedOrders: 0, totalRevenue: 0 };
    else if (path.startsWith('/explore/trainers')) data = { items: [], total: 0, page: 1, pageSize: 20 };
    else if (path === '/legal/documents/active') data = [];
    else if (path.startsWith('/legal/')) data = { title: 'Documento', contentMarkdown: 'Informações da plataforma e do tratamento de dados.', version: '1.0' };
    else { unhandled.push(path); data = null; }
    await route.fulfill({ json: { success: true, data, message: null, errors: [] } });
  });
  return { unhandled };
}
