using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data; 

var builder = WebApplication.CreateBuilder(args);

// ��������� ������ �����������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ���������� ��������� ���� ������ � �������������� ������ �����������
builder.Services.AddDbContext<ContextStaffManagement>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.19-mysql")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// �������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5218") // URL �������
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

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
