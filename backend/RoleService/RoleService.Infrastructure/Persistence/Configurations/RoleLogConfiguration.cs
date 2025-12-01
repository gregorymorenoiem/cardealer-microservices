using RoleService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace RoleService.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("error_logs");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.ServiceName)
                .HasColumnName("service_name")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ExceptionType)
                .HasColumnName("exception_type")
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Message)
                .HasColumnName("message")
                .IsRequired();

            builder.Property(e => e.StackTrace)
                .HasColumnName("stack_trace");

            builder.Property(e => e.OccurredAt)
                .HasColumnName("occurred_at")
                .IsRequired();

            builder.Property(e => e.Endpoint)
                .HasColumnName("endpoint")
                .HasMaxLength(500);

            builder.Property(e => e.HttpMethod)
                .HasColumnName("http_method")
                .HasMaxLength(10);

            builder.Property(e => e.StatusCode)
                .HasColumnName("status_code");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(100);

            // Configuración para el diccionario de metadatos (JSONB en PostgreSQL)
            builder.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions()) ?? new Dictionary<string, object>()
                );

            // Índices para búsquedas comunes
            builder.HasIndex(e => e.ServiceName);
            builder.HasIndex(e => e.OccurredAt);
            builder.HasIndex(e => e.StatusCode);
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => new { e.ServiceName, e.OccurredAt });
            // Índices compuestos adicionales para consultas por rango de fecha + filtro
            builder.HasIndex(e => new { e.StatusCode, e.OccurredAt });
            builder.HasIndex(e => new { e.UserId, e.OccurredAt });
        }
    }
}
