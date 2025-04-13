using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using StaffManagementApi.Models;

namespace StaffManagementApi.Data;

public partial class ContextStaffManagement : DbContext
{
    public ContextStaffManagement()
    {
    }

    public ContextStaffManagement(DbContextOptions<ContextStaffManagement> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StatusesWorker> StatusesWorkers { get; set; }

    public virtual DbSet<VwWorkerDetail> VwWorkerDetails { get; set; }

    public virtual DbSet<VwWorkerInfo> VwWorkerInfos { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    public virtual DbSet<WorkingCondition> WorkingConditions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=staff_management;uid=root;pwd=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.19-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.IdDepartment).HasName("PRIMARY");

            entity.ToTable("departments");

            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.Descriptions)
                .HasMaxLength(500)
                .HasColumnName("descriptions");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.IdPost).HasName("PRIMARY");

            entity.ToTable("posts");

            entity.Property(e => e.IdPost).HasColumnName("id_post");
            entity.Property(e => e.Descriptions)
                .HasMaxLength(500)
                .HasColumnName("descriptions");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<StatusesWorker>(entity =>
        {
            entity.HasKey(e => e.IdStatusWorker).HasName("PRIMARY");

            entity.ToTable("statuses_workers");

            entity.HasIndex(e => e.IdDepartment, "fk_statuses_workers_departments1_idx");

            entity.HasIndex(e => e.IdPost, "fk_statuses_workers_posts1_idx");

            entity.HasIndex(e => e.IdWorker, "fk_statuses_workers_workers1_idx");

            entity.Property(e => e.IdStatusWorker).HasColumnName("id_status_worker");
            entity.Property(e => e.Duties)
                .HasMaxLength(500)
                .HasColumnName("duties");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IdDepartment).HasColumnName("id_department");
            entity.Property(e => e.IdPost).HasColumnName("id_post");
            entity.Property(e => e.IdWorker).HasColumnName("id_worker");
            entity.Property(e => e.Salary).HasColumnName("salary");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.IdDepartmentNavigation).WithMany(p => p.StatusesWorkers)
                .HasForeignKey(d => d.IdDepartment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_statuses_workers_departments1");

            entity.HasOne(d => d.IdPostNavigation).WithMany(p => p.StatusesWorkers)
                .HasForeignKey(d => d.IdPost)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_statuses_workers_posts1");

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.StatusesWorkers)
                .HasForeignKey(d => d.IdWorker)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_statuses_workers_workers1");
        });

        modelBuilder.Entity<VwWorkerDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_worker_details");

            entity.Property(e => e.Avatar)
                .HasMaxLength(60)
                .HasColumnName("avatar");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("department");
            entity.Property(e => e.Duties)
                .HasMaxLength(500)
                .HasColumnName("duties");
            entity.Property(e => e.EndDateStatus).HasColumnName("end_date_status");
            entity.Property(e => e.EndDateWorkingConditions).HasColumnName("end_date_working_conditions");
            entity.Property(e => e.FullWorkerName)
                .HasMaxLength(137)
                .HasDefaultValueSql("''")
                .HasColumnName("full_worker_name");
            entity.Property(e => e.IdWorker).HasColumnName("id_worker");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.PcNumber)
                .HasMaxLength(15)
                .HasColumnName("pc_number");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Post)
                .HasMaxLength(50)
                .HasColumnName("post");
            entity.Property(e => e.Salary).HasColumnName("salary");
            entity.Property(e => e.StartDateStatus).HasColumnName("start_date_status");
            entity.Property(e => e.StartDateWorkingConditions).HasColumnName("start_date_working_conditions");
            entity.Property(e => e.WorkEmail)
                .HasMaxLength(30)
                .HasColumnName("work_email");
        });

        modelBuilder.Entity<VwWorkerInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_worker_info");

            entity.Property(e => e.Avatar)
                .HasMaxLength(60)
                .HasColumnName("avatar");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .HasColumnName("department");
            entity.Property(e => e.FullWorkerName)
                .HasMaxLength(137)
                .HasDefaultValueSql("''")
                .HasColumnName("full_worker_name");
            entity.Property(e => e.IdWorker).HasColumnName("id_worker");
            entity.Property(e => e.Post)
                .HasMaxLength(50)
                .HasColumnName("post");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.IdWorker).HasName("PRIMARY");

            entity.ToTable("workers");

            entity.HasIndex(e => e.IdRole, "id_role_idx");

            entity.Property(e => e.IdWorker).HasColumnName("id_worker");
            entity.Property(e => e.Avatar)
                .HasMaxLength(60)
                .HasColumnName("avatar");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.IdRole)
                .HasDefaultValueSql("'1'")
                .HasColumnName("id_role");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.PcNumber)
                .HasMaxLength(15)
                .HasColumnName("pc_number");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .HasColumnName("surname");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.WorkEmail)
                .HasMaxLength(30)
                .HasColumnName("work_email");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Workers)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("id_role");
        });

        modelBuilder.Entity<WorkingCondition>(entity =>
        {
            entity.HasKey(e => e.IdWorkCondition).HasName("PRIMARY");

            entity.ToTable("working_conditions");

            entity.HasIndex(e => e.IdWorker, "fk_working_conditions_workers_idx");

            entity.Property(e => e.IdWorkCondition).HasColumnName("id_work_condition");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IdWorker).HasColumnName("id_worker");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.WorkingConditions)
                .HasForeignKey(d => d.IdWorker)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_working_conditions_workers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
