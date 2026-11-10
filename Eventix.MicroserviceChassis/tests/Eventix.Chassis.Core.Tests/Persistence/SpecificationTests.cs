using System.Linq.Expressions;
using Eventix.Chassis.Core.Persistence;
using FluentAssertions;
using Xunit;

namespace Eventix.Chassis.Core.Tests.Persistence;

public class SpecificationTests
{
    // Simple test entity (no EF needed - specs are pure expressions)
    private sealed class TestItem
    {
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    private sealed class ActiveItemsSpec : Specification<TestItem>
    {
        public ActiveItemsSpec()
        {
            Criteria = item => item.IsActive;
            OrderBy = item => item.CreatedAt;
            Take = 10;
        }
    }

    private sealed class ItemsByNameSpec : Specification<TestItem>
    {
        public ItemsByNameSpec(string name)
        {
            Criteria = item => item.Name == name;
        }
    }

    [Fact]
    public void Specification_ShouldExposeCriteria()
    {
        // Arrange
        var spec = new ActiveItemsSpec();

        // Assert
        spec.Criteria.Should().NotBeNull();
    }

    [Fact]
    public void Criteria_ShouldFilterCorrectly_WhenCompiled()
    {
        // Arrange
        var spec = new ActiveItemsSpec();
        var predicate = spec.Criteria!.Compile();

        var activeItem = new TestItem { IsActive = true };
        var inactiveItem = new TestItem { IsActive = false };

        // Act & Assert
        predicate(activeItem).Should().BeTrue();
        predicate(inactiveItem).Should().BeFalse();
    }

    [Fact]
    public void Specification_WithParameter_ShouldCaptureParameter()
    {
        // Arrange
        var spec = new ItemsByNameSpec("Coldplay");
        var predicate = spec.Criteria!.Compile();

        // Act & Assert
        predicate(new TestItem { Name = "Coldplay" }).Should().BeTrue();
        predicate(new TestItem { Name = "Metallica" }).Should().BeFalse();
    }

    [Fact]
    public void Specification_ShouldExposePagination()
    {
        // Arrange
        var spec = new ActiveItemsSpec();

        // Assert
        spec.Take.Should().Be(10);
        spec.Skip.Should().BeNull();
    }

    [Fact]
    public void Specification_ShouldStartWithEmptyIncludes()
    {
        // Arrange
        var spec = new ActiveItemsSpec();

        // Assert
        spec.Includes.Should().BeEmpty();
    }
}