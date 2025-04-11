using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StaffManagementApi.Data;
using StaffManagementApi.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Получение строки подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Добавление контекста базы данных с использованием строки подключения
builder.Services.AddDbContext<ContextStaffManagement>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.19-mysql")));

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5286") // URL клиентского приложения
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Настройка аутентификации JWT
var key = builder.Configuration["JwtSettings:SecretKey"]; // Секретный ключ для JWT

if (string.IsNullOrEmpty(key))
{
    throw new Exception("Секретный ключ JWT не настроен в appsettings.json");
}

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Установите false для целей разработки
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), // Секретный ключ для подписи токена
        ValidateIssuer = false, // Установите true, если необходимо проверять издателя токена
        ValidateAudience = false // Установите true, если необходимо проверять аудиторию токена
    };
});

// Добавьте эту строку в раздел с другими сервисами
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Правильный порядок middleware
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication(); // Добавлено отсутствующее middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
