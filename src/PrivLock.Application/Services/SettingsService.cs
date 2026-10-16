using PrivLock.Domain.Results;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Application.Services;

/// <summary>
/// Manages application-level settings, autostart configuration, and state persistence.
/// </summary>
public sealed class SettingsService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<SettingsService>();

    private readonly IAutostartProvider _autostartProvider;
    private readonly IStateStore _stateStore;

    public SettingsService(IAutostartProvider autostartProvider, IStateStore stateStore)
    {
        _autostartProvider = autostartProvider;
        _stateStore = stateStore;
    }

    public Task<AutostartStatus> GetAutostartStatusAsync()
    {
        return _autostartProvider.GetAutostartStatusAsync();
    }

    public async Task<OperationResult> SetAutostartAsync(bool enable)
    {
        Log.Information("Setting autostart to {Enable}", enable);

        var result = enable
            ? await _autostartProvider.EnableAutostartAsync().ConfigureAwait(false)
            : await _autostartProvider.DisableAutostartAsync().ConfigureAwait(false);

        if (result.Success)
        {
            var effective = await _autostartProvider.GetAutostartStatusAsync().ConfigureAwait(false);
            if (effective.IsEnabled != enable)
            {
                return OperationResult.Fail(
                    effective.Message ??
                    $"Autostart verification failed; the effective state is {effective.State}.");
            }

            try
            {
                var state = _stateStore.Load();
                state.Autostart = enable;
                _stateStore.Save(state);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Autostart changed but its application preference could not be persisted");
                return OperationResult.Fail(
                    $"Autostart changed, but PrivGvard could not save the preference: {ex.Message}");
            }
        }

        return result;
    }

    public DesiredState GetSettings()
    {
        return _stateStore.Load();
    }

    public void SaveSettings(DesiredState state)
    {
        _stateStore.Save(state);
    }
}
