using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data; 

var builder = WebApplication.CreateBuilder(args);

// Получение строки подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Добавление контекста базы данных с использованием строки подключения
builder.Services.AddDbContext<ContextStaffManagement>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.19-mysql")));

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

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
