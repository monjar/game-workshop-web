using todoapp.Config;
using todoapp.Middlewares.Auth;
using todoapp.Models;
using todoapp.Services;

var builder = WebApplication.CreateBuilder(args);


// add services to DI container
{
    var services = builder.Services;
    services.AddCors();
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddAuthorization();
    services.Configure<WorkshopDatabaseSettings>(
        builder.Configuration.GetSection("WorkshopDatabase"));

    // configure strongly typed settings object
    services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICourseService, CourseService>();


}

var app = builder.Build();

// configure HTTP request pipeline
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
    
    // custom jwt auth middleware
    app.UseMiddleware<JwtMiddleware>();

    app.MapControllers();
}

app.Run();
