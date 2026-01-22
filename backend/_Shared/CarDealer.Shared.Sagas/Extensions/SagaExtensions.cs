using CarDealer.Shared.Sagas.Configuration;
using CarDealer.Shared.Sagas.StateMachines;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Sagas.Extensions;

/// <summary>
/// Extensiones para configurar MassTransit con Sagas
/// </summary>
public static class SagaExtensions
{
    /// <summary>
    /// Agrega MassTransit con configuración de Sagas desde appsettings
    /// </summary>
    public static IServiceCollection AddMassTransitWithSagas(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var options = new SagaOptions();
        configuration.GetSection(SagaOptions.SectionName).Bind(options);

        return services.AddMassTransitWithSagas(options, configureConsumers);
    }

    /// <summary>
    /// Agrega MassTransit con Sagas usando opciones manuales
    /// </summary>
    public static IServiceCollection AddMassTransitWithSagas(
        this IServiceCollection services,
        SagaOptions options,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        if (!options.Enabled)
            return services;

        services.AddMassTransit(x =>
        {
            // Registrar State Machines
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .ConfigureSagaRepository(options);

            // Configurar consumers adicionales
            configureConsumers?.Invoke(x);

            // Configurar RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = options.RabbitMq;
                
                cfg.Host(rabbitMq.Host, (ushort)rabbitMq.Port, rabbitMq.VirtualHost, h =>
                {
                    h.Username(rabbitMq.Username);
                    h.Password(rabbitMq.Password);
                });

                // Configurar reintentos
                if (options.Retry.MaxRetries > 0)
                {
                    cfg.UseMessageRetry(r =>
                    {
                        if (options.Retry.UseExponentialBackoff)
                        {
                            r.Exponential(
                                options.Retry.MaxRetries,
                                TimeSpan.FromSeconds(options.Retry.IntervalSeconds),
                                TimeSpan.FromSeconds(options.Retry.IntervalSeconds * Math.Pow(2, options.Retry.MaxRetries)),
                                TimeSpan.FromSeconds(options.Retry.IntervalSeconds));
                        }
                        else
                        {
                            r.Interval(options.Retry.MaxRetries, TimeSpan.FromSeconds(options.Retry.IntervalSeconds));
                        }
                    });
                }

                // Configurar endpoints automáticamente
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(options.RabbitMq.QueuePrefix, false));
            });
        });

        // Registrar opciones
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Configura el repositorio de Saga según el tipo configurado
    /// </summary>
    private static ISagaRegistrationConfigurator<OrderState> ConfigureSagaRepository(
        this ISagaRegistrationConfigurator<OrderState> configurator,
        SagaOptions options)
    {
        switch (options.Repository.Type)
        {
            case SagaRepositoryType.InMemory:
                configurator.InMemoryRepository();
                break;

            case SagaRepositoryType.Redis:
                if (string.IsNullOrEmpty(options.Repository.RedisConnectionString))
                    throw new InvalidOperationException("Redis connection string is required for Redis saga repository");

                configurator.RedisRepository(r =>
                {
                    r.ConnectionFactory(provider => 
                        StackExchange.Redis.ConnectionMultiplexer.Connect(options.Repository.RedisConnectionString));
                    r.KeyPrefix = options.Repository.RedisKeyPrefix;
                    r.Expiry = TimeSpan.FromMinutes(options.Repository.RedisExpirationMinutes);
                });
                break;

            case SagaRepositoryType.EntityFramework:
            default:
                configurator.EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<SagaDbContext>();
                    r.UsePostgres();
                });
                break;
        }

        return configurator;
    }

    /// <summary>
    /// Agrega MassTransit simplificado solo con RabbitMQ (sin Sagas)
    /// </summary>
    public static IServiceCollection AddMassTransitRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var options = new SagaOptions();
        configuration.GetSection(SagaOptions.SectionName).Bind(options);

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = options.RabbitMq;
                
                cfg.Host(rabbitMq.Host, (ushort)rabbitMq.Port, rabbitMq.VirtualHost, h =>
                {
                    h.Username(rabbitMq.Username);
                    h.Password(rabbitMq.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

/// <summary>
/// DbContext para persistencia de estados de Saga
/// </summary>
public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options) { }

    public DbSet<OrderState> OrderStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.ToTable("order_saga_states");
            entity.HasKey(x => x.CorrelationId);
            
            entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
            entity.Property(x => x.CurrentState).HasColumnName("current_state");
            entity.Property(x => x.CustomerId).HasColumnName("customer_id");
            entity.Property(x => x.VehicleId).HasColumnName("vehicle_id");
            entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
            entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.PaymentId).HasColumnName("payment_id");
            entity.Property(x => x.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
            entity.Property(x => x.ReservedUntil).HasColumnName("reserved_until");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(50);
            entity.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(x => x.RetryCount).HasColumnName("retry_count");

            // Índices
            entity.HasIndex(x => x.CurrentState);
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.CreatedAt);
        });
    }
}
