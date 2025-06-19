using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.IRepository;
<<<<<<< Updated upstream
using Repositories.Repository;
=======
using Repositories.IRepository.Admins;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Courses;
using Repositories.IRepository.Users;
using Repositories.Repository;
using Repositories.Repository.Admins;
using Repositories.Repository.Appointments;
using Repositories.Repository.Consultants;
using Repositories.Repository.Courses;
using Repositories.Repository.Users;
>>>>>>> Stashed changes
using Services.IService;
using Services.Service; // Thêm dòng này ở đầu file

// ... các using khác

namespace DrugUsePrevention
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<DrugUsePreventionDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnectionString")
                )
            );
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
<<<<<<< Updated upstream
=======
            builder.Services.AddScoped<IAdminRepository,AdminRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddTransient<ISendMailService, SendMailService>();
            builder.Services.AddScoped<ICourseRegistrationRepository, CourseRegistrationRepository>();
            builder.Services.AddScoped<ICourseContentRepository, CourseContentRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IConsultantRepository, ConsultantRepository>();
            builder.Services.AddScoped<IConsultantService, ConsultantService>();
            builder.Services.AddScoped<IAdminService, AdminService>();

            // Add Controllers
>>>>>>> Stashed changes
            builder.Services.AddControllers();

            // Thêm Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo { Title = "DrugUsePrevention API", Version = "v1" }
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrugUsePrevention API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
