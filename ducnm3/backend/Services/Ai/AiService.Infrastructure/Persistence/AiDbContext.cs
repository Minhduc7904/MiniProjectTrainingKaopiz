using System;
using System.Collections.Generic;
using AiService.Infrastructure.Persistence.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AiService.Infrastructure.Persistence;

public partial class AiDbContext : DbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiModel> AiModels { get; set; }

    public virtual DbSet<AiProvider> AiProviders { get; set; }

    public virtual DbSet<AiServiceBinding> AiServiceBindings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AiModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ai_models");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_ai_models_status_created_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.ProviderId, e.ModelName }, "uq_ai_models_provider_id_model_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh AI model")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo model, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasComment("Tên hiển thị cho admin")
                .HasColumnName("display_name");
            entity.Property(e => e.ModelName)
                .HasMaxLength(100)
                .HasComment("Tên model gửi cho provider, ví dụ gpt-4o")
                .HasColumnName("model_name");
            entity.Property(e => e.ProviderId)
                .HasComment("UUID provider sở hữu model")
                .HasColumnName("provider_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("ACTIVE | INACTIVE")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Provider).WithMany(p => p.AiModels)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ai_models_provider_id");
        });

        modelBuilder.Entity<AiProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ai_providers");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "ix_ai_providers_status_created_at").IsDescending(false, true);

            entity.HasIndex(e => e.Name, "uq_ai_providers_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh AI provider")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ApiKeyEncrypted)
                .HasComment("API key đã mã hóa AES-GCM, base64 của nonce, tag và ciphertext")
                .HasColumnType("text")
                .HasColumnName("api_key_encrypted")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.ApiKeyHint)
                .HasMaxLength(16)
                .HasComment("Gợi ý che key, chỉ chứa 4 ký tự cuối, không dùng để giải mã")
                .HasColumnName("api_key_hint")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.BaseUrl)
                .HasMaxLength(500)
                .HasComment("Endpoint OpenAI-compatible mà OpenAI SDK sẽ gọi tới")
                .HasColumnName("base_url");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo provider, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("Tên provider hiển thị cho admin, không trùng nhau")
                .HasColumnName("name");
            entity.Property(e => e.ProviderType)
                .HasMaxLength(20)
                .HasComment("OPENAI | GEMINI | ANTHROPIC | MISTRAL | CUSTOM")
                .HasColumnName("provider_type")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasComment("ACTIVE | INACTIVE")
                .HasColumnName("status")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AiServiceBinding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ai_service_bindings");

            entity.HasIndex(e => e.ModelId, "ix_ai_service_bindings_model_id");

            entity.HasIndex(e => e.ServiceName, "uq_ai_service_bindings_service_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("UUID định danh binding")
                .HasColumnName("id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(6)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm tạo binding, UTC")
                .HasColumnName("created_at");
            entity.Property(e => e.ModelId)
                .HasComment("UUID AI model được gán cho service")
                .HasColumnName("model_id")
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(50)
                .HasComment("Tên service được dùng AI, ví dụ course-service; logical reference")
                .HasColumnName("service_name")
                .UseCollation("ascii_general_ci")
                .HasCharSet("ascii");
            entity.Property(e => e.UpdatedAt)
                .HasMaxLength(6)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .HasComment("Thời điểm cập nhật gần nhất, UTC")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Model).WithMany(p => p.AiServiceBindings)
                .HasForeignKey(d => d.ModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ai_service_bindings_model_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
