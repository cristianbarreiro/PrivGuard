using Moq;
using PrivLock.Application.Services;
using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Xunit;

namespace PrivLock.Application.Tests;

public class SettingsServiceTests
{
    private readonly Mock<IAutostartProvider> _autostartMock;
    private readonly Mock<IStateStore> _storeMock;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _autostartMock = new Mock<IAutostartProvider>();
        _storeMock = new Mock<IStateStore>();
        _storeMock.Setup(s => s.Load()).Returns(new DesiredState());

        _service = new SettingsService(_autostartMock.Object, _storeMock.Object);
    }

    [Fact]
    public async Task SetAutostart_True_EnablesAndSavesState()
    {
        _autostartMock.Setup(a => a.EnableAutostartAsync()).ReturnsAsync(OperationResult.Ok());
        _autostartMock.Setup(a => a.GetAutostartStatusAsync())
            .ReturnsAsync(new AutostartStatus(AutostartState.Enabled));

        var result = await _service.SetAutostartAsync(true);

        Assert.True(result.Success);
        _autostartMock.Verify(a => a.EnableAutostartAsync(), Times.Once);
        _storeMock.Verify(s => s.Save(It.Is<DesiredState>(ds => ds.Autostart)), Times.Once);
    }

    [Fact]
    public async Task SetAutostart_False_DisablesAndSavesState()
    {
        _autostartMock.Setup(a => a.DisableAutostartAsync()).ReturnsAsync(OperationResult.Ok());
        _autostartMock.Setup(a => a.GetAutostartStatusAsync())
            .ReturnsAsync(new AutostartStatus(AutostartState.Disabled));

        var result = await _service.SetAutostartAsync(false);

        Assert.True(result.Success);
        _autostartMock.Verify(a => a.DisableAutostartAsync(), Times.Once);
        _storeMock.Verify(s => s.Save(It.Is<DesiredState>(ds => !ds.Autostart)), Times.Once);
    }

    [Fact]
    public async Task SetAutostart_DoesNotPersistWhenPostOperationStateDoesNotMatch()
    {
        _autostartMock.Setup(a => a.EnableAutostartAsync()).ReturnsAsync(OperationResult.Ok());
        _autostartMock.Setup(a => a.GetAutostartStatusAsync())
            .ReturnsAsync(new AutostartStatus(AutostartState.DisabledByPolicy, "Blocked by policy."));

        var result = await _service.SetAutostartAsync(true);

        Assert.False(result.Success);
        Assert.Contains("Blocked by policy", result.ErrorMessage);
        _storeMock.Verify(s => s.Save(It.IsAny<DesiredState>()), Times.Never);
    }

    [Fact]
    public async Task SetAutostart_DisableDoesNotPersistWhenEffectiveStateIsUnknown()
    {
        _autostartMock.Setup(a => a.DisableAutostartAsync()).ReturnsAsync(OperationResult.Ok());
        _autostartMock.Setup(a => a.GetAutostartStatusAsync())
            .ReturnsAsync(new AutostartStatus(AutostartState.Failed, "Registry read failed."));

        var result = await _service.SetAutostartAsync(false);

        Assert.False(result.Success);
        Assert.Contains("Registry read failed", result.ErrorMessage);
        _storeMock.Verify(s => s.Save(It.IsAny<DesiredState>()), Times.Never);
    }

    [Theory]
    [InlineData(AutostartState.Disabled)]
    [InlineData(AutostartState.DisabledByUser)]
    [InlineData(AutostartState.DisabledByPolicy)]
    public async Task SetAutostart_DisableAcceptsOnlyExplicitDisabledStates(AutostartState state)
    {
        _autostartMock.Setup(a => a.DisableAutostartAsync()).ReturnsAsync(OperationResult.Ok());
        _autostartMock.Setup(a => a.GetAutostartStatusAsync())
            .ReturnsAsync(new AutostartStatus(state));

        var result = await _service.SetAutostartAsync(false);

        Assert.True(result.Success, result.ErrorMessage);
        _storeMock.Verify(s => s.Save(It.Is<DesiredState>(desired => !desired.Autostart)), Times.Once);
    }
}
