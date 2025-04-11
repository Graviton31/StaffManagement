using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StaffManagementApi.Data;
using StaffManagementApi.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������ �����������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ���������� ��������� ���� ������ � �������������� ������ �����������
builder.Services.AddDbContext<ContextStaffManagement>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.19-mysql")));

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5286") // URL ����������� ����������
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// ��������� �������������� JWT
var key = builder.Configuration["JwtSettings:SecretKey"]; // ��������� ���� ��� JWT

if (string.IsNullOrEmpty(key))
{
    throw new Exception("��������� ���� JWT �� �������� � appsettings.json");
}

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // ���������� false ��� ����� ����������
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), // ��������� ���� ��� ������� ������
        ValidateIssuer = false, // ���������� true, ���� ���������� ��������� �������� ������
        ValidateAudience = false // ���������� true, ���� ���������� ��������� ��������� ������
    };
});

// �������� ��� ������ � ������ � ������� ���������
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

// ���������� ������� middleware
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication(); // ��������� ������������� middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
