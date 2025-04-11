using Microsoft.EntityFrameworkCore;
using StaffManagementApi.Data;
using StaffManagementApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������ �����������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ���������� ��������� ���� ������ � �������������� ������ �����������
builder.Services.AddDbContext<ContextStaffManagement>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.19-mysql")));

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

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
