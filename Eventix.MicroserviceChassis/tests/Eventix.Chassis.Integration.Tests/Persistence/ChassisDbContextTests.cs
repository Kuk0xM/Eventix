using Eventix.Chassis.Core.Persistence.Outbox;
using Eventix.Chassis.Infrastructure.Persistence;
using Eventix.Chassis.Testing.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eventix.Chassis.Integration.Tests.Persistence;

// Test DbContext for this suite
public class TestDbContext : ChassisDbContext<TestDbContext>
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.ToTable("TestEntities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        base.OnModelCreating(modelBuilder);
    }
}

public class TestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ChassisDbContextTests : IClassFixture<IntegrationTestFixture<TestDbContext>>
{
    private readonly IntegrationTestFixture<TestDbContext> _fixture;

    public ChassisDbContextTests(IntegrationTestFixture<TestDbContext> fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OutboxMessages_ShouldBePersisted_WhenSaveChangesIsCalled()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestEvent",
            Content = "{\"data\":\"test\"}",
            OccurredOnUtc = DateTime.UtcNow
        };

        // Act
        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        // Assert
        var saved = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        saved.Should().NotBeNull();
        saved!.Type.Should().Be("TestEvent");
        saved.ProcessedOnUtc.Should().BeNull();
        saved.Error.Should().BeNull();
        saved.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task OutboxMessages_ShouldBeMarkedAsProcessed_WhenProcessedOnUtcIsSet()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestEvent",
            Content = "{\"data\":\"test\"}",
            OccurredOnUtc = DateTime.UtcNow
        };

        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        // Act
        message.ProcessedOnUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // Assert
        var saved = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        saved.Should().NotBeNull();
        saved!.ProcessedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UnprocessedMessages_ShouldBeQueryable_WithProcessedOnUtcIsNull()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Clean first
        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        // Add 3 messages: 2 unprocessed, 1 processed
        dbContext.OutboxMessages.AddRange(
            new OutboxMessage { Type = "Event1", Content = "{}" },
            new OutboxMessage { Type = "Event2", Content = "{}" },
            new OutboxMessage { Type = "Event3", Content = "{}", ProcessedOnUtc = DateTime.UtcNow }
        );
        await dbContext.SaveChangesAsync();

        // Act
        var unprocessed = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .ToListAsync();

        // Assert
        unprocessed.Should().HaveCount(2);
        unprocessed.Should().OnlyContain(m => m.ProcessedOnUtc == null);
    }
}