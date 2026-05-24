using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // Plans
        var basicPlan = new PlatformPlan
        {
            Id = Guid.NewGuid(),
            Code = "BASIC",
            Name = "Basic",
            Description = "Tudo que o personal precisa para gerenciar alunos, montar treinos e acompanhar a evolução em uma plataforma profissional.",
            MonthlyPrice = 59.90m,
            MaxActiveStudents = 0,
            HasUnlimitedStudents = true,
            Active = true,
            IsPublic = true,
            IsComingSoon = false,
            IsAvailableForPurchase = true
        };
        var proPlan = new PlatformPlan { Id = Guid.NewGuid(), Code = "PRO", Name = "Pro", Description = "Para automatizar atendimento e retenção", MonthlyPrice = 197.00m, MaxActiveStudents = 50, HasUnlimitedStudents = false, Active = true, IsPublic = true, IsComingSoon = true, IsAvailableForPurchase = false };
        var growthPlan = new PlatformPlan { Id = Guid.NewGuid(), Code = "GROWTH", Name = "Growth", Description = "Para captar alunos e crescer o negócio", MonthlyPrice = 297.00m, MaxActiveStudents = 100, HasUnlimitedStudents = false, Active = true, IsPublic = true, IsComingSoon = true, IsAvailableForPurchase = false };
        await context.PlatformPlans.AddRangeAsync(basicPlan, proPlan, growthPlan);

        // Plan prices per billing cycle
        var prices = new List<PlatformPlanPrice>
        {
            new() { Id = Guid.NewGuid(), PlatformPlanId = basicPlan.Id, BillingCycle = BillingFrequency.Monthly,   Price = 59.90m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = basicPlan.Id, BillingCycle = BillingFrequency.Quarterly, Price = 161.73m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = basicPlan.Id, BillingCycle = BillingFrequency.Semiannual, Price = 305.49m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = basicPlan.Id, BillingCycle = BillingFrequency.Yearly,    Price = 575.04m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = proPlan.Id,     BillingCycle = BillingFrequency.Monthly,   Price = 197.00m,  Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = proPlan.Id,     BillingCycle = BillingFrequency.Quarterly, Price = 547.00m,  Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = proPlan.Id,     BillingCycle = BillingFrequency.Semiannual, Price = 1004.70m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = proPlan.Id,     BillingCycle = BillingFrequency.Yearly,    Price = 1997.00m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = growthPlan.Id,  BillingCycle = BillingFrequency.Monthly,   Price = 297.00m,  Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = growthPlan.Id,  BillingCycle = BillingFrequency.Quarterly, Price = 797.00m,  Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = growthPlan.Id,  BillingCycle = BillingFrequency.Semiannual, Price = 1514.70m, Active = true },
            new() { Id = Guid.NewGuid(), PlatformPlanId = growthPlan.Id,  BillingCycle = BillingFrequency.Yearly,    Price = 2997.00m, Active = true },
        };
        await context.PlatformPlanPrices.AddRangeAsync(prices);

        // Users
        var ownerUser = new User { Id = Guid.NewGuid(), Name = "Admin Plataforma", Email = "admin@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = UserRole.Owner, IsActive = true };
        var trainerUser = new User { Id = Guid.NewGuid(), Name = "Carlos Trainer", Email = "trainer@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = UserRole.Trainer, IsActive = true };
        var studentUser = new User { Id = Guid.NewGuid(), Name = "JoÃ£o Silva", Email = "aluno@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = UserRole.Student, IsActive = true };
        await context.Users.AddRangeAsync(ownerUser, trainerUser, studentUser);

        var trainer = new Trainer
        {
            Id = Guid.NewGuid(), UserId = trainerUser.Id, BrandName = "Carlos Trainer Consultoria",
            Phone = "(11) 99999-9999", Bio = "Personal trainer com 10 anos de experiÃªncia.",
            CREF = "012345-G/SP", City = "SÃ£o Paulo", State = "SP"
        };
        await context.Trainers.AddAsync(trainer);

        var student = new Student { Id = Guid.NewGuid(), UserId = studentUser.Id, TrainerId = trainer.Id, Phone = "(11) 88888-8888", Goal = "Hipertrofia", Notes = "Sem lesÃµes", Status = StudentStatus.Active };
        await context.Students.AddAsync(student);

        var basicMonthlyPrice = prices.First(p => p.PlatformPlanId == basicPlan.Id && p.BillingCycle == BillingFrequency.Monthly);
        var subscription = new TrainerSubscription
        {
            Id = Guid.NewGuid(), TrainerId = trainer.Id, PlatformPlanId = basicPlan.Id,
            PlatformPlanPriceId = basicMonthlyPrice.Id, BillingCycle = BillingFrequency.Monthly,
            Status = TrainerSubscriptionStatus.Active, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1)
        };
        await context.TrainerSubscriptions.AddAsync(subscription);

        var ex1 = new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Supino Reto", MuscleGroup = "Peitoral", Description = "ExercÃ­cio clÃ¡ssico para peitoral", Instructions = "Deite no banco, segure a barra, desÃ§a atÃ© o peito e empurre.", Level = ExerciseLevel.Intermediate };
        var ex2 = new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Agachamento Livre", MuscleGroup = "QuadrÃ­ceps", Description = "ExercÃ­cio composto para membros inferiores", Instructions = "Fique em pÃ© com os pÃ©s na largura dos ombros, dobre os joelhos.", Level = ExerciseLevel.Intermediate };
        var ex3 = new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Remada Curvada", MuscleGroup = "Costas", Description = "ExercÃ­cio para dorsal", Instructions = "Incline o tronco, segure a barra e puxe atÃ© o abdÃ´men.", Level = ExerciseLevel.Intermediate };
        var ex4 = new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Rosca Direta", MuscleGroup = "BÃ­ceps", Description = "ExercÃ­cio para bÃ­ceps", Instructions = "Em pÃ©, segure a barra e flexione os cotovelos.", Level = ExerciseLevel.Beginner };
        var ex5 = new Exercise { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "TrÃ­ceps Pulley", MuscleGroup = "TrÃ­ceps", Description = "ExercÃ­cio para trÃ­ceps", Instructions = "No cabo, empurre a barra para baixo estendendo os cotovelos.", Level = ExerciseLevel.Beginner };
        await context.Exercises.AddRangeAsync(ex1, ex2, ex3, ex4, ex5);

        var workoutA = new Workout { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Treino A - Peito e TrÃ­ceps", Goal = "Hipertrofia de Peito e TrÃ­ceps", Level = ExerciseLevel.Intermediate, Status = WorkoutStatus.Active };
        var workoutB = new Workout { Id = Guid.NewGuid(), TrainerId = trainer.Id, Name = "Treino B - Costas e BÃ­ceps", Goal = "Hipertrofia de Costas e BÃ­ceps", Level = ExerciseLevel.Intermediate, Status = WorkoutStatus.Active };
        await context.Workouts.AddRangeAsync(workoutA, workoutB);

        await context.WorkoutExercises.AddRangeAsync(
            new WorkoutExercise { Id = Guid.NewGuid(), WorkoutId = workoutA.Id, ExerciseId = ex1.Id, Sets = 4, Reps = "8-12", SuggestedLoad = "80kg", RestSeconds = 90, OrderIndex = 1 },
            new WorkoutExercise { Id = Guid.NewGuid(), WorkoutId = workoutA.Id, ExerciseId = ex5.Id, Sets = 3, Reps = "12-15", SuggestedLoad = "30kg", RestSeconds = 60, OrderIndex = 2 },
            new WorkoutExercise { Id = Guid.NewGuid(), WorkoutId = workoutB.Id, ExerciseId = ex3.Id, Sets = 4, Reps = "8-12", SuggestedLoad = "70kg", RestSeconds = 90, OrderIndex = 1 },
            new WorkoutExercise { Id = Guid.NewGuid(), WorkoutId = workoutB.Id, ExerciseId = ex4.Id, Sets = 3, Reps = "12-15", SuggestedLoad = "25kg", RestSeconds = 60, OrderIndex = 2 }
        );

        await context.StudentWorkoutSchedules.AddRangeAsync(
            new StudentWorkoutSchedule { Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id, WorkoutId = workoutA.Id, DayOfWeek = 1, Notes = "Foco na execuÃ§Ã£o" },
            new StudentWorkoutSchedule { Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id, WorkoutId = workoutB.Id, DayOfWeek = 3, Notes = "Aumentar carga no remada" },
            new StudentWorkoutSchedule { Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id, WorkoutId = workoutA.Id, DayOfWeek = 5 }
        );

        // Posts com visibility/tags/publishedAt
        var publishedAt1 = DateTime.UtcNow.AddDays(-15);
        var publishedAt2 = DateTime.UtcNow.AddDays(-10);
        var publishedAt3 = DateTime.UtcNow.AddDays(-5);
        var publishedAt4 = DateTime.UtcNow.AddDays(-2);

        await context.Posts.AddRangeAsync(
            new Post
            {
                Id = Guid.NewGuid(), TrainerId = trainer.Id,
                Title = "Dicas de NutriÃ§Ã£o PÃ³s-Treino",
                Description = "ApÃ³s o treino, consuma proteÃ­nas e carboidratos em atÃ© 30 minutos para potencializar a recuperaÃ§Ã£o muscular. Uma boa opÃ§Ã£o Ã© whey protein com banana.",
                Status = PostStatus.Published, Visibility = PostVisibility.Public,
                Tags = "NutriÃ§Ã£o,Dicas,RecuperaÃ§Ã£o", PublishedAt = publishedAt1
            },
            new Post
            {
                Id = Guid.NewGuid(), TrainerId = trainer.Id,
                Title = "3 erros comuns no treino de peito",
                Description = "Evite erros que sabotam o seu desenvolvimento. Aprenda a corrigir a postura, a amplitude e o controle do movimento no supino.",
                Status = PostStatus.Published, Visibility = PostVisibility.Public,
                Tags = "Hipertrofia,Treino,Dicas", PublishedAt = publishedAt2
            },
            new Post
            {
                Id = Guid.NewGuid(), TrainerId = trainer.Id,
                Title = "Como ajustar sua carga com seguranÃ§a",
                Description = "Progredir de carga Ã© fundamental para evoluÃ§Ã£o. Aprenda o mÃ©todo de sobrecarga progressiva sem se machucar.",
                Status = PostStatus.Published, Visibility = PostVisibility.StudentsOnly,
                Tags = "Treino,ForÃ§a,EvoluÃ§Ã£o", PublishedAt = publishedAt3
            },
            new Post
            {
                Id = Guid.NewGuid(), TrainerId = trainer.Id,
                Title = "A importÃ¢ncia do descanso na hipertrofia",
                Description = "Muitos alunos subestimam o descanso. Saiba por que dormir bem e ter dias de recuperaÃ§Ã£o Ã© essencial para ganhar mÃºsculo.",
                Status = PostStatus.Published, Visibility = PostVisibility.Public,
                Tags = "Hipertrofia,Descanso,Sono", PublishedAt = publishedAt4
            },
            new Post
            {
                Id = Guid.NewGuid(), TrainerId = trainer.Id,
                Title = "Planejamento semanal exclusivo para alunos",
                Description = "Confira o planejamento desta semana com ajustes personalizados para cada objetivo.",
                Status = PostStatus.Published, Visibility = PostVisibility.StudentsOnly,
                Tags = "Planejamento,Exclusivo", PublishedAt = DateTime.UtcNow
            }
        );

        // 2 progress records para JoÃ£o (para ter comparaÃ§Ã£o)
        await context.StudentProgressRecords.AddRangeAsync(
            new StudentProgress
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                Weight = 85.0m, Height = 178m, Chest = 98m, Waist = 88m, Abdomen = 91m,
                Hip = 99m, BodyFatPercentage = 18.5m,
                ProgressDate = DateTime.UtcNow.AddMonths(-2),
                CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer,
                Notes = "AvaliaÃ§Ã£o inicial â€” inÃ­cio do programa"
            },
            new StudentProgress
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                Weight = 82.5m, Height = 178m, Chest = 100m, Waist = 85m, Abdomen = 88m,
                Hip = 97m, BodyFatPercentage = 16.2m,
                ProgressDate = DateTime.UtcNow.AddMonths(-1),
                CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer,
                Notes = "Boa evoluÃ§Ã£o no primeiro mÃªs. Continuar com dÃ©ficit calÃ³rico moderado."
            },
            new StudentProgress
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                Weight = 80.3m, Height = 178m, Chest = 102m, Waist = 82m, Abdomen = 85m,
                Hip = 96m, BodyFatPercentage = 14.8m,
                ProgressDate = DateTime.UtcNow.AddDays(-7),
                CreatedByUserId = studentUser.Id, CreatedByRole = ProgressCreatedByRole.Student,
                Notes = "Me sinto muito melhor! Roupas estÃ£o mais folgadas."
            }
        );

        // Progress photos para JoÃ£o
        await context.StudentProgressPhotos.AddRangeAsync(
            new StudentProgressPhoto
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                ImageUrl = "https://placehold.co/400x600/1a1a2e/ffffff?text=Antes",
                Description = "Foto inicial â€” frente",
                PhotoDate = DateTime.UtcNow.AddMonths(-2),
                CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer
            },
            new StudentProgressPhoto
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                ImageUrl = "https://placehold.co/400x600/16213e/ffffff?text=1+MÃªs",
                Description = "Foto com 1 mÃªs de treino â€” frente",
                PhotoDate = DateTime.UtcNow.AddMonths(-1),
                CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer
            },
            new StudentProgressPhoto
            {
                Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                ImageUrl = "https://placehold.co/400x600/0f3460/ffffff?text=2+Meses",
                Description = "Foto com 2 meses de treino â€” frente",
                PhotoDate = DateTime.UtcNow.AddDays(-7),
                CreatedByUserId = studentUser.Id, CreatedByRole = ProgressCreatedByRole.Student
            }
        );

        // Testimonial published
        await context.StudentTestimonials.AddAsync(new StudentTestimonial
        {
            Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id,
            Text = "O Carlos Ã© incrÃ­vel! Em 2 meses jÃ¡ perdi 5kg e ganhei muito mais disposiÃ§Ã£o. Os treinos sÃ£o desafiadores mas sempre dentro do meu limite. Super recomendo!",
            Rating = 5, ApprovedByStudent = true, Published = true
        });

        // Transformation published
        await context.StudentTransformations.AddAsync(new StudentTransformation
        {
            Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id,
            BeforePhotoUrl = "https://placehold.co/400x600/1a1a2e/ffffff?text=Antes",
            AfterPhotoUrl = "https://placehold.co/400x600/0f3460/ffffff?text=Depois",
            Description = "TransformaÃ§Ã£o em 2 meses: -4.7kg de gordura, +mÃºsculo e muito mais disposiÃ§Ã£o!",
            ApprovedByStudent = true, Published = true
        });

        // Configurar pÃ¡gina pÃºblica do trainer
        trainer.PublicSlug = "carlos-trainer";
        trainer.PublicPageEnabled = true;
        trainer.PublicSearchEnabled = true;
        trainer.AcceptingStudents = true;
        trainer.PublicHeadline = "Personal Trainer Especialista em Hipertrofia";
        trainer.PublicDescription = "Ajudo vocÃª a transformar seu corpo com treinos personalizados e acompanhamento completo. 10 anos de experiÃªncia, mais de 200 alunos transformados.";
        trainer.WhatsappNumber = "11999999999";
        trainer.ShowTestimonials = true;
        trainer.ShowInstagram = true;
        trainer.Instagram = "@carlostrainer";
        trainer.ProfilePhotoUrl = "https://placehold.co/400x400/1a1a2e/ffffff?text=CT";
        trainer.BannerUrl = "https://placehold.co/1200x400/0f3460/ffffff?text=Carlos+Trainer";
        trainer.PrimaryColor = "#0f3460";
        trainer.SecondaryColor = "#e94560";
        trainer.ServiceMode = "Hybrid";
        trainer.Latitude = -23.550520;
        trainer.Longitude = -46.633308;
        trainer.Specialties = "Hipertrofia,Emagrecimento,Consultoria Online";

        // Platform features
        var features = new[]
        {
            ("STUDENT_PROGRESS", "Progresso do aluno"), ("PROGRESS_PHOTOS", "Fotos de progresso"),
            ("WEEKLY_CHECKIN", "Check-in semanal"), ("WORKOUT_COMPLETION", "ConclusÃ£o de treino"),
            ("EXERCISE_LIBRARY", "Biblioteca de exercÃ­cios"), ("WORKOUT_TEMPLATES", "Templates de treino"),
            ("PUBLIC_PROFILE_PAGE", "PÃ¡gina pÃºblica"), ("NOTIFICATIONS", "NotificaÃ§Ãµes"),
            ("REPORTS", "RelatÃ³rios"), ("VISUAL_CUSTOMIZATION", "PersonalizaÃ§Ã£o visual"),
            ("ANAMNESIS", "Anamnese"), ("INTERNAL_NOTES", "Notas internas"),
            ("TESTIMONIALS", "Depoimentos"), ("PROGRESS_COMMENTS", "ComentÃ¡rios no progresso"),
        };
        var featureEntities = features.Select(f => new PlatformFeature { Id = Guid.NewGuid(), Code = f.Item1, Name = f.Item2, Active = true }).ToList();
        await context.PlatformFeatures.AddRangeAsync(featureEntities);

        // Enable all features on all plans by default
        var planFeatures = new List<PlatformPlanFeature>();
        foreach (var plan in new[] { basicPlan, proPlan, growthPlan })
            foreach (var feature in featureEntities)
                planFeatures.Add(new PlatformPlanFeature { Id = Guid.NewGuid(), PlatformPlanId = plan.Id, PlatformFeatureId = feature.Id, Enabled = true });
        await context.PlatformPlanFeatures.AddRangeAsync(planFeatures);

        // Exercise library (20 exercises)
        var libItems = new[]
        {
            ("Supino Reto com Barra", "Peitoral", ExerciseLevel.Intermediate), ("Supino Inclinado com Halteres", "Peitoral", ExerciseLevel.Intermediate),
            ("Crucifixo", "Peitoral", ExerciseLevel.Beginner), ("Agachamento Livre", "QuadrÃ­ceps", ExerciseLevel.Intermediate),
            ("Leg Press 45Â°", "QuadrÃ­ceps", ExerciseLevel.Beginner), ("Afundo", "QuadrÃ­ceps", ExerciseLevel.Beginner),
            ("Levantamento Terra", "Posterior/Lombar", ExerciseLevel.Advanced), ("Cadeira Extensora", "QuadrÃ­ceps", ExerciseLevel.Beginner),
            ("Mesa Flexora", "Posterior", ExerciseLevel.Beginner), ("Panturrilha em PÃ©", "Panturrilha", ExerciseLevel.Beginner),
            ("Remada Curvada", "Costas", ExerciseLevel.Intermediate), ("Puxada Frontal", "Costas", ExerciseLevel.Beginner),
            ("Remada Unilateral", "Costas", ExerciseLevel.Intermediate), ("Rosca Direta", "BÃ­ceps", ExerciseLevel.Beginner),
            ("Rosca Alternada", "BÃ­ceps", ExerciseLevel.Beginner), ("TrÃ­ceps Pulley", "TrÃ­ceps", ExerciseLevel.Beginner),
            ("TrÃ­ceps Testa", "TrÃ­ceps", ExerciseLevel.Intermediate), ("Desenvolvimento com Halteres", "Ombros", ExerciseLevel.Intermediate),
            ("ElevaÃ§Ã£o Lateral", "Ombros", ExerciseLevel.Beginner), ("Abdominal Crunch", "AbdÃ´men", ExerciseLevel.Beginner),
        };
        var libEntities = libItems.Select(l => new ExerciseLibraryItem { Id = Guid.NewGuid(), Name = l.Item1, MuscleGroup = l.Item2, Level = l.Item3, IsActive = true }).ToList();
        await context.ExerciseLibraryItems.AddRangeAsync(libEntities);

        // Workout templates
        var tplHyper = new WorkoutTemplate { Id = Guid.NewGuid(), Name = "Hipertrofia Iniciante", Goal = "Hipertrofia", Level = ExerciseLevel.Beginner, IsActive = true };
        var tplFat = new WorkoutTemplate { Id = Guid.NewGuid(), Name = "Emagrecimento", Goal = "Emagrecimento", Level = ExerciseLevel.Beginner, IsActive = true };
        await context.WorkoutTemplates.AddRangeAsync(tplHyper, tplFat);

        var supino = libEntities.First(l => l.Name == "Supino Reto com Barra");
        var agach = libEntities.First(l => l.Name == "Agachamento Livre");
        var remada = libEntities.First(l => l.Name == "Remada Curvada");
        var rosca = libEntities.First(l => l.Name == "Rosca Direta");
        var tricep = libEntities.First(l => l.Name == "TrÃ­ceps Pulley");

        await context.WorkoutTemplateExercises.AddRangeAsync(
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplHyper.Id, ExerciseLibraryItemId = supino.Id, Sets = 4, Reps = "8-12", RestSeconds = 90, OrderIndex = 1 },
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplHyper.Id, ExerciseLibraryItemId = agach.Id, Sets = 4, Reps = "8-12", RestSeconds = 90, OrderIndex = 2 },
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplHyper.Id, ExerciseLibraryItemId = remada.Id, Sets = 3, Reps = "10-12", RestSeconds = 75, OrderIndex = 3 },
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplFat.Id, ExerciseLibraryItemId = agach.Id, Sets = 4, Reps = "15-20", RestSeconds = 45, OrderIndex = 1 },
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplFat.Id, ExerciseLibraryItemId = rosca.Id, Sets = 3, Reps = "15", RestSeconds = 30, OrderIndex = 2 },
            new WorkoutTemplateExercise { Id = Guid.NewGuid(), WorkoutTemplateId = tplFat.Id, ExerciseLibraryItemId = tricep.Id, Sets = 3, Reps = "15", RestSeconds = 30, OrderIndex = 3 }
        );

        // Terms
        await context.TermsDocuments.AddRangeAsync(
            new TermsDocument { Id = Guid.NewGuid(), Type = TermsType.TermsOfUse, Version = "1.0", Title = "Termos de Uso", Content = "Ao usar a FitPlatform vocÃª concorda com os termos de uso.", Active = true },
            new TermsDocument { Id = Guid.NewGuid(), Type = TermsType.PrivacyPolicy, Version = "1.0", Title = "PolÃ­tica de Privacidade", Content = "Seus dados sÃ£o tratados conforme a LGPD.", Active = true },
            new TermsDocument { Id = Guid.NewGuid(), Type = TermsType.ProgressPhotoConsent, Version = "1.0", Title = "Consentimento para Fotos de Progresso", Content = "Suas fotos sÃ£o privadas e visÃ­veis apenas para vocÃª e seu personal trainer.", Active = true }
        );

        await context.PrivacyPolicyVersions.AddRangeAsync(
            new PrivacyPolicyVersion
            {
                DocumentType = LegalDocumentType.PrivacyPolicy,
                Version = "1.0.0",
                Title = "Politica de Privacidade - Forma Training",
                ContentMarkdown = "Texto inicial de produto. Revisao juridica final obrigatoria antes da publicacao comercial.\n\n# Politica de Privacidade\n\nA {{COMPANY_NAME}} trata dados cadastrais, comerciais, de treino, habitos, nutricao, agenda, chat, midia e geolocalizacao conforme esta politica. Contato: {{PRIVACY_CONTACT_EMAIL}}.",
                IsActive = true,
                PublishedAt = DateTime.UtcNow
            },
            new PrivacyPolicyVersion
            {
                DocumentType = LegalDocumentType.TermsOfUse,
                Version = "1.0.0",
                Title = "Termos de Uso - Forma Training",
                ContentMarkdown = "Texto inicial de produto. Revisao juridica final obrigatoria antes da publicacao comercial.\n\n# Termos de Uso\n\nA {{COMPANY_NAME}} disponibiliza um SaaS para trainers e students, com regras de uso, cobranca e perfil publico.",
                IsActive = true,
                PublishedAt = DateTime.UtcNow
            });

        await context.ConsentDefinitions.AddRangeAsync(
            new ConsentDefinition { Code = "PUBLIC_PROFILE_VISIBILITY", Name = "Exibicao de perfil publico", Description = "Permitir que meu perfil publico seja exibido na pagina de exploracao e em URL publica.", Category = "PublicProfile", IsRequired = false, IsActive = true },
            new ConsentDefinition { Code = "GEOLOCATION_FOR_EXPLORE", Name = "Uso de geolocalizacao no Explore", Description = "Permitir uso da minha localizacao aproximada para buscar trainers proximos.", Category = "Explore", IsRequired = false, IsActive = true },
            new ConsentDefinition { Code = "MARKETING_EMAIL", Name = "Marketing por e-mail", Description = "Aceito receber novidades, promocoes e conteudos da Forma Training por e-mail.", Category = "Marketing", IsRequired = false, IsActive = true },
            new ConsentDefinition { Code = "MARKETING_WHATSAPP", Name = "Marketing por WhatsApp", Description = "Aceito receber novidades, promocoes e conteudos da Forma Training por WhatsApp.", Category = "Marketing", IsRequired = false, IsActive = true },
            new ConsentDefinition { Code = "HEALTH_RELATED_DATA_PROCESSING", Name = "Aviso sobre dados de saude e evolucao", Description = "Texto editavel para revisao juridica: ciente do tratamento de dados relacionados a treino, evolucao corporal, habitos e orientacoes para prestacao da plataforma.", Category = "SensitiveData", IsRequired = true, IsActive = true });

        await context.DataProcessorVendors.AddRangeAsync(
            new DataProcessorVendor { Name = "AbacatePay", Purpose = "Processamento de pagamentos e assinaturas", DataCategories = "Dados de cobranca e identificadores transacionais", CountryOrRegion = "Brasil", HasInternationalTransfer = false, IsActive = true, PrivacyPolicyReference = "https://abacatepay.com" },
            new DataProcessorVendor { Name = "Cloudinary", Purpose = "Armazenamento e entrega de midia", DataCategories = "Imagens e videos enviados pelos usuarios", CountryOrRegion = "Global", HasInternationalTransfer = true, IsActive = true, PrivacyPolicyReference = "https://cloudinary.com/privacy" },
            new DataProcessorVendor { Name = "Hosting Provider", Purpose = "Hospedagem da aplicacao", DataCategories = "Dados de aplicacao e logs tecnicos", CountryOrRegion = "A definir", HasInternationalTransfer = true, IsActive = true });
        // Sample check-in for JoÃ£o Silva
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        await context.StudentWeeklyCheckIns.AddAsync(new StudentWeeklyCheckIn
        {
            Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
            WeekStartDate = weekStart, WeekEndDate = weekStart.AddDays(6),
            Weight = 82.0m, MoodLevel = 4, EnergyLevel = 3, SleepQuality = 4,
            DietAdherence = 3, TrainingAdherence = 5, CompletedWorkoutsCount = 3,
            HasPain = false, Notes = "Boa semana, consegui fazer todos os treinos."
        });

        // Sample anamnesis
        await context.StudentAnamnesisRecords.AddAsync(new StudentAnamnesis
        {
            Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
            MainGoal = "Hipertrofia e ganho de forÃ§a", TrainingExperience = "2 anos de musculaÃ§Ã£o",
            Injuries = "Nenhuma lesÃ£o ativa", HealthRestrictions = "Nenhuma",
            AvailableDaysPerWeek = 4, TrainingLocation = "Academia",
            AvailableEquipment = "Equipamento completo de academia", SleepQuality = 4,
            StressLevel = 3, FoodRoutineNotes = "Come bem, faz suplementaÃ§Ã£o bÃ¡sica",
            SubmittedAt = DateTime.UtcNow.AddDays(-7)
        });

        await context.SaveChangesAsync();
    }

    // Enriquece dados existentes de forma idempotente (runs always after SeedAsync)
    public static async Task EnrichExistingDataAsync(AppDbContext context)
    {
        Trainer? trainer;
        try
        {
            trainer = await context.Trainers
                .FirstOrDefaultAsync(t => t.PublicSlug == null || t.PublicSlug == "");
        }
        catch (SqlException ex) when (ex.Number is 207 or 208)
        {
            return;
        }

        if (trainer == null) return; // Already enriched or no trainer

        trainer.PublicSlug = "carlos-trainer";
        trainer.PublicPageEnabled = true;
        trainer.PublicSearchEnabled = true;
        trainer.AcceptingStudents = true;
        trainer.PublicHeadline = "Personal Trainer Especialista em Hipertrofia";
        trainer.PublicDescription = "Ajudo vocÃª a transformar seu corpo com treinos personalizados e acompanhamento completo. 10 anos de experiÃªncia, mais de 200 alunos transformados.";
        trainer.WhatsappNumber = "11999999999";
        trainer.ShowTestimonials = true;
        trainer.ShowInstagram = true;
        trainer.Instagram = "@carlostrainer";
        trainer.ProfilePhotoUrl ??= "https://placehold.co/400x400/1a1a2e/ffffff?text=CT";
        trainer.BannerUrl ??= "https://placehold.co/1200x400/0f3460/ffffff?text=Carlos+Trainer";
        trainer.PrimaryColor ??= "#0f3460";
        trainer.SecondaryColor ??= "#e94560";
        trainer.ServiceMode ??= "Hybrid";
        trainer.Latitude ??= -23.550520;
        trainer.Longitude ??= -46.633308;
        trainer.Specialties ??= "Hipertrofia,Emagrecimento,Consultoria Online";

        // Add public posts if none exist with Visibility field
        var hasPublicPosts = await context.Posts
            .AnyAsync(p => p.TrainerId == trainer.Id && p.Visibility == PostVisibility.Public);

        if (!hasPublicPosts)
        {
            var existingPosts = await context.Posts.Where(p => p.TrainerId == trainer.Id).ToListAsync();
            foreach (var post in existingPosts)
            {
                post.Visibility = PostVisibility.Public;
                post.PublishedAt ??= post.CreatedAt;
            }
        }

        await context.SaveChangesAsync();

        // Add extra published posts if fewer than 3 public posts
        var publicPostsCount = await context.Posts
            .CountAsync(p => p.TrainerId == trainer.Id && p.Status == PostStatus.Published && p.Visibility == PostVisibility.Public);

        if (publicPostsCount < 3)
        {
            await context.Posts.AddRangeAsync(
                new Post
                {
                    Id = Guid.NewGuid(), TrainerId = trainer.Id,
                    Title = "3 erros comuns no treino de peito",
                    Description = "Evite erros que sabotam o seu desenvolvimento. Aprenda a corrigir a postura, a amplitude e o controle do movimento no supino.",
                    Status = PostStatus.Published, Visibility = PostVisibility.Public,
                    Tags = "Hipertrofia,Treino,Dicas", PublishedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Post
                {
                    Id = Guid.NewGuid(), TrainerId = trainer.Id,
                    Title = "A importÃ¢ncia do descanso na hipertrofia",
                    Description = "Muitos alunos subestimam o descanso. Saiba por que dormir bem e ter dias de recuperaÃ§Ã£o Ã© essencial para ganhar mÃºsculo.",
                    Status = PostStatus.Published, Visibility = PostVisibility.Public,
                    Tags = "Hipertrofia,Descanso,Sono", PublishedAt = DateTime.UtcNow.AddDays(-5)
                }
            );
        }

        // Add testimonial if none published
        var hasTestimonial = await context.StudentTestimonials
            .AnyAsync(t => t.TrainerId == trainer.Id && t.Published);
        if (!hasTestimonial)
        {
            var student = await context.Students.FirstOrDefaultAsync(s => s.TrainerId == trainer.Id);
            if (student != null)
            {
                context.StudentTestimonials.Add(new StudentTestimonial
                {
                    Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id,
                    Text = "O Carlos Ã© incrÃ­vel! Em 2 meses jÃ¡ perdi 5kg e ganhei muito mais disposiÃ§Ã£o. Super recomendo!",
                    Rating = 5, ApprovedByStudent = true, Published = true
                });
            }
        }

        // Add transformation if none published
        var hasTransformation = await context.StudentTransformations
            .AnyAsync(t => t.TrainerId == trainer.Id && t.Published);
        if (!hasTransformation)
        {
            var student = await context.Students.FirstOrDefaultAsync(s => s.TrainerId == trainer.Id);
            if (student != null)
            {
                context.StudentTransformations.Add(new StudentTransformation
                {
                    Id = Guid.NewGuid(), TrainerId = trainer.Id, StudentId = student.Id,
                    BeforePhotoUrl = "https://placehold.co/400x600/1a1a2e/ffffff?text=Antes",
                    AfterPhotoUrl = "https://placehold.co/400x600/0f3460/ffffff?text=Depois",
                    Description = "TransformaÃ§Ã£o em 2 meses: -4.7kg de gordura, ganho de mÃºsculo e muito mais disposiÃ§Ã£o!",
                    ApprovedByStudent = true, Published = true
                });
            }
        }

        // Add extra progress records if only 1 exists
        var progressCount = await context.StudentProgressRecords.CountAsync();
        if (progressCount <= 1)
        {
            var student = await context.Students.FirstOrDefaultAsync(s => s.TrainerId == trainer.Id);
            var trainerUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Trainer);
            var studentUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Student);

            if (student != null && trainerUser != null && studentUser != null)
            {
                await context.StudentProgressRecords.AddRangeAsync(
                    new StudentProgress
                    {
                        Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                        Weight = 85.0m, Height = 178m, Chest = 98m, Waist = 88m, Abdomen = 91m, Hip = 99m, BodyFatPercentage = 18.5m,
                        ProgressDate = DateTime.UtcNow.AddMonths(-2),
                        CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer,
                        Notes = "AvaliaÃ§Ã£o inicial â€” inÃ­cio do programa"
                    },
                    new StudentProgress
                    {
                        Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                        Weight = 80.3m, Height = 178m, Chest = 102m, Waist = 82m, Abdomen = 85m, Hip = 96m, BodyFatPercentage = 14.8m,
                        ProgressDate = DateTime.UtcNow.AddDays(-7),
                        CreatedByUserId = studentUser.Id, CreatedByRole = ProgressCreatedByRole.Student,
                        Notes = "Me sinto muito melhor! Roupas estÃ£o mais folgadas."
                    }
                );
            }
        }

        // Add progress photos if none exist
        var hasPhotos = await context.StudentProgressPhotos.AnyAsync();
        if (!hasPhotos)
        {
            var student = await context.Students.FirstOrDefaultAsync(s => s.TrainerId == trainer.Id);
            var trainerUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Trainer);
            var studentUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Student);

            if (student != null && trainerUser != null && studentUser != null)
            {
                await context.StudentProgressPhotos.AddRangeAsync(
                    new StudentProgressPhoto
                    {
                        Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                        ImageUrl = "https://placehold.co/400x600/1a1a2e/ffffff?text=Antes",
                        Description = "Foto inicial â€” frente",
                        PhotoDate = DateTime.UtcNow.AddMonths(-2),
                        CreatedByUserId = trainerUser.Id, CreatedByRole = ProgressCreatedByRole.Trainer
                    },
                    new StudentProgressPhoto
                    {
                        Id = Guid.NewGuid(), StudentId = student.Id, TrainerId = trainer.Id,
                        ImageUrl = "https://placehold.co/400x600/0f3460/ffffff?text=Depois",
                        Description = "Foto com 2 meses de treino â€” frente",
                        PhotoDate = DateTime.UtcNow.AddDays(-7),
                        CreatedByUserId = studentUser.Id, CreatedByRole = ProgressCreatedByRole.Student
                    }
                );
            }
        }

        await context.SaveChangesAsync();

        await EnsureExploreTrainerAsync(
            context,
            "trainer.poa@test.com",
            "Marina Rocha",
            "Marina Performance",
            "marina-performance",
            "Porto Alegre",
            "RS",
            "Moinhos de Vento",
            -30.027704,
            -51.228735,
            "InPerson",
            "Emagrecimento,Funcional",
            "Treinos presenciais para condicionamento e emagrecimento."
        );
        await EnsureExploreTrainerAsync(
            context,
            "trainer.canoas@test.com",
            "Lucas Farias",
            "Lucas Fit",
            "lucas-fit-canoas",
            "Canoas",
            "RS",
            "Centro",
            -29.917881,
            -51.183228,
            "Hybrid",
            "Hipertrofia,ForÃ§a",
            "Acompanhamento hÃ­brido para ganho de massa e forÃ§a."
        );
        await EnsureExploreTrainerAsync(
            context,
            "trainer.scs@test.com",
            "Paula Martins",
            "Paula Trainer",
            "paula-trainer-scs",
            "Santa Cruz do Sul",
            "RS",
            "Centro",
            -29.722019,
            -52.434444,
            "InPerson",
            "ReabilitaÃ§Ã£o,Funcional",
            "Treinos personalizados com foco em mobilidade e saÃºde."
        );
        await EnsureExploreTrainerAsync(
            context,
            "trainer.sp@test.com",
            "Renato Alves",
            "Renato Personal",
            "renato-personal-sp",
            "SÃ£o Paulo",
            "SP",
            "Vila Mariana",
            -23.589949,
            -46.634596,
            "Hybrid",
            "Hipertrofia,Emagrecimento",
            "Consultoria presencial e online para evoluÃ§Ã£o consistente."
        );
        await EnsureExploreTrainerAsync(
            context,
            "trainer.online@test.com",
            "Carla Mendes",
            "Carla Online Coach",
            "carla-online-coach",
            "Remoto",
            "BR",
            null,
            null,
            null,
            "Online",
            "Consultoria Online,Emagrecimento",
            "Atendimento 100% online com plano semanal."
        );
    }

    private static async Task EnsureExploreTrainerAsync(
        AppDbContext context,
        string email,
        string name,
        string brandName,
        string slug,
        string city,
        string state,
        string? neighborhood,
        double? latitude,
        double? longitude,
        string serviceMode,
        string specialties,
        string bio)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Trainer,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var trainer = await context.Trainers.FirstOrDefaultAsync(t => t.UserId == user.Id);
        if (trainer == null)
        {
            trainer = new Trainer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BrandName = brandName
            };
            context.Trainers.Add(trainer);
        }

        trainer.BrandName = brandName;
        trainer.City = city;
        trainer.State = state;
        trainer.Neighborhood = neighborhood;
        trainer.Latitude = latitude;
        trainer.Longitude = longitude;
        trainer.ServiceMode = serviceMode;
        trainer.Specialties = specialties;
        trainer.Bio = bio;
        trainer.PublicSlug = slug;
        trainer.PublicHeadline = $"Personal Trainer - {city}/{state}";
        trainer.PublicDescription = bio;
        trainer.PublicPageEnabled = true;
        trainer.PublicSearchEnabled = true;
        trainer.AcceptingStudents = true;
        trainer.ShowInstagram = true;
        trainer.ShowTestimonials = true;

        await context.SaveChangesAsync();
    }
}


