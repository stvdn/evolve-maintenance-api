using Application.DTOs.Metrics;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using FluentAssertions;

namespace UnitTests.Services;

public class MetricServiceTests
{
    private readonly Mock<IMetricRepository> _metricRepoMock;
    private readonly Mock<IMaintenanceRepository> _maintRepoMock;
    private readonly MetricService _service;

    public MetricServiceTests()
    {
        _metricRepoMock = new Mock<IMetricRepository>();
        _maintRepoMock = new Mock<IMaintenanceRepository>();
        _service = new MetricService(_metricRepoMock.Object, _maintRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsMetricDto()
    {
        // Arrange
        var maintenanceId = Guid.NewGuid();
        var request = new CreateMetricRequest
        {
            MaintenanceId = maintenanceId,
            CoveragePercentage = 85.5m,
            CyclomaticComplexity = 12,
            CodeSmells = 3,
            Vulnerabilities = 0,
            DuplicatedLinesPercentage = 2.1m,
            LinesOfCode = 1250
        };

        _maintRepoMock.Setup(r => r.GetByIdAsync(maintenanceId))
            .ReturnsAsync(new Maintenance { Id = maintenanceId, Name = "Test", ApplicationName = "app", EstimatedHours = 100, Squad = "S", AssignedArchitect = "a@b.com" });
        _metricRepoMock.Setup(r => r.CreateAsync(It.IsAny<CodeMetric>()))
            .ReturnsAsync((CodeMetric m) => m);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CoveragePercentage.Should().Be(85.5m);
        result.CyclomaticComplexity.Should().Be(12);
        result.Vulnerabilities.Should().Be(0);
        result.MaintenanceId.Should().Be(maintenanceId);
    }

    [Fact]
    public async Task CreateAsync_WhenMaintenanceNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateMetricRequest { MaintenanceId = Guid.NewGuid(), CoveragePercentage = 80, CyclomaticComplexity = 5, CodeSmells = 1, Vulnerabilities = 0, DuplicatedLinesPercentage = 1 };
        _maintRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Maintenance?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByMaintenanceIdAsync_WhenExists_ReturnsLatestMetric()
    {
        // Arrange
        var maintenanceId = Guid.NewGuid();
        var metric = new CodeMetric
        {
            Id = Guid.NewGuid(),
            MaintenanceId = maintenanceId,
            CoveragePercentage = 90,
            CyclomaticComplexity = 8,
            CodeSmells = 1,
            Vulnerabilities = 0,
            DuplicatedLinesPercentage = 1.5m
        };

        _metricRepoMock.Setup(r => r.GetLatestByMaintenanceIdAsync(maintenanceId)).ReturnsAsync(metric);

        // Act
        var result = await _service.GetByMaintenanceIdAsync(maintenanceId);

        // Assert
        result.CoveragePercentage.Should().Be(90);
        result.MaintenanceId.Should().Be(maintenanceId);
    }

    [Fact]
    public async Task GetByMaintenanceIdAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _metricRepoMock.Setup(r => r.GetLatestByMaintenanceIdAsync(id)).ReturnsAsync((CodeMetric?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByMaintenanceIdAsync(id));
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var maintenanceId = Guid.NewGuid();
        var metrics = new List<CodeMetric>
        {
            new() { MaintenanceId = maintenanceId, CoveragePercentage = 85, CyclomaticComplexity = 10, CodeSmells = 2, Vulnerabilities = 0, DuplicatedLinesPercentage = 1 },
            new() { MaintenanceId = maintenanceId, CoveragePercentage = 90, CyclomaticComplexity = 8, CodeSmells = 1, Vulnerabilities = 0, DuplicatedLinesPercentage = 0.5m }
        };

        _metricRepoMock.Setup(r => r.GetHistoryByMaintenanceIdAsync(maintenanceId, 1, 10))
            .ReturnsAsync((metrics.AsEnumerable(), 2));

        // Act
        var result = await _service.GetHistoryAsync(maintenanceId, 1, 10);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Pagination!.Total.Should().Be(2);
    }
}
