using Application.DTOs.Maintenances;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;
using FluentAssertions;

namespace UnitTests.Services;

public class MaintenanceServiceTests
{
    private readonly Mock<IMaintenanceRepository> _repositoryMock;
    private readonly MaintenanceService _service;

    public MaintenanceServiceTests()
    {
        _repositoryMock = new Mock<IMaintenanceRepository>();
        _service = new MaintenanceService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsMaintenanceDto()
    {
        // Arrange
        var request = new CreateMaintenanceRequest
        {
            Name = "Test Maintenance",
            ApplicationName = "core-banking",
            EstimatedHours = 100,
            Squad = "Squad Alpha",
            AssignedArchitect = "arch@banco.com"
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Maintenance>()))
            .ReturnsAsync((Maintenance m) => m);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Maintenance");
        result.ApplicationName.Should().Be("core-banking");
        result.EstimatedHours.Should().Be(100);
        result.Status.Should().Be("draft");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Maintenance>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsMaintenanceDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var maintenance = new Maintenance
        {
            Id = id,
            Name = "Existing Maintenance",
            ApplicationName = "channels-app",
            EstimatedHours = 200,
            Status = MaintenanceStatus.InProgress,
            Squad = "Squad Beta",
            AssignedArchitect = "arch@banco.com"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(maintenance);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be("Existing Maintenance");
        result.Status.Should().Be("in_progress");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Maintenance?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Maintenance
        {
            Id = id,
            Name = "Original Name",
            ApplicationName = "app",
            EstimatedHours = 100,
            Status = MaintenanceStatus.Draft,
            Squad = "Squad A",
            AssignedArchitect = "arch@banco.com"
        };

        var request = new UpdateMaintenanceRequest { Name = "Updated Name", Status = "in_progress" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Maintenance>())).ReturnsAsync((Maintenance m) => m);

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Status.Should().Be("in_progress");
    }

    [Fact]
    public async Task DeleteAsync_WhenCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var maintenance = new Maintenance
        {
            Id = id,
            Name = "Completed Maintenance",
            ApplicationName = "app",
            EstimatedHours = 50,
            Status = MaintenanceStatus.Completed,
            Squad = "Squad A",
            AssignedArchitect = "arch@banco.com"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(maintenance);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id));
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotCompleted_DeletesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var maintenance = new Maintenance
        {
            Id = id,
            Name = "Draft Maintenance",
            ApplicationName = "app",
            EstimatedHours = 50,
            Status = MaintenanceStatus.Draft,
            Squad = "Squad A",
            AssignedArchitect = "arch@banco.com"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(maintenance);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var maintenances = new List<Maintenance>
        {
            new() { Name = "M1", ApplicationName = "app1", EstimatedHours = 100, Status = MaintenanceStatus.InProgress, Squad = "S1", AssignedArchitect = "a@b.com" }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 10, MaintenanceStatus.InProgress, null, null, "createdAt", "desc"))
            .ReturnsAsync((maintenances.AsEnumerable(), 1));

        // Act
        var result = await _service.GetAllAsync(1, 10, "in_progress", null, null, "createdAt", "desc");

        // Assert
        result.Data.Should().HaveCount(1);
        result.Pagination!.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAllAsync(1, 10, "invalid_status", null, null, "createdAt", "desc"));
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsStats()
    {
        // Arrange
        var stats = new { totalMaintenances = 10 };
        _repositoryMock.Setup(r => r.GetStatsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
