using System.Diagnostics;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

public sealed class WindowsUninstallRequestListener : IUninstallRequestListener
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsUninstallRequestListener>();
    private const string RequestEventName = @"Local\PrivLock_UninstallRequested";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    private readonly object _sync = new();
    private readonly EventWaitHandle _requestEvent;
    private readonly CancellationTokenSource _stop = new();
    private EventHandler? _uninstallRequested;
    private Task? _listenerTask;
    private bool _requestPending;
    private bool _disposed;

    public WindowsUninstallRequestListener()
        : this(RequestEventName)
    {
    }

    internal WindowsUninstallRequestListener(string requestEventName)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows uninstall requests are supported only on Windows.");

        _requestEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset,
            requestEventName);
    }

    public event EventHandler? UninstallRequested
    {
        add
        {
            EventHandler? handlerToInvoke = null;
            lock (_sync)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _uninstallRequested += value;
                if (_requestPending && _uninstallRequested != null)
                {
                    _requestPending = false;
                    handlerToInvoke = _uninstallRequested;
                }
            }

            InvokeHandler(handlerToInvoke);
        }
        remove
        {
            lock (_sync)
            {
                _uninstallRequested -= value;
            }
        }
    }

    public void Start()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _listenerTask ??= Task.Run(Listen);
        }
    }

    public void Dispose()
    {
        Task? listenerTask;
        lock (_sync)
        {
            if (_disposed)
                return;

            _disposed = true;
            _stop.Cancel();
            _requestEvent.Set();
            listenerTask = _listenerTask;
        }

        listenerTask?.GetAwaiter().GetResult();
        _requestEvent.Dispose();
        _stop.Dispose();
    }

    public static bool TryRequestRunningInstanceAndAcquire(
        ISingleInstanceGuard singleInstanceGuard,
        TimeSpan timeout,
        out string? error,
        string requestEventName = RequestEventName,
        string instanceMutexName = WindowsSingleInstanceGuard.MutexName,
        TimeSpan? pollInterval = null)
    {
        ArgumentNullException.ThrowIfNull(singleInstanceGuard);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        error = null;
        var stopwatch = Stopwatch.StartNew();
        var requestSent = false;
        var probeAcquiredAtLeastOnce = false;
        var effectivePollInterval = pollInterval ?? PollInterval;
        var detailedGuard = singleInstanceGuard as IWindowsDetailedSingleInstanceGuard;
        using var instanceMutex = new Mutex(initiallyOwned: false, instanceMutexName);

        while (stopwatch.Elapsed < timeout)
        {
            if (!requestSent)
            {
                requestSent = TrySignalExistingInstance(requestEventName, out error);
                if (error != null)
                    return false;
            }

            var remaining = timeout - stopwatch.Elapsed;
            if (remaining <= TimeSpan.Zero)
                break;

            var waitInterval = TimeSpan.FromMilliseconds(
                Math.Min(effectivePollInterval.TotalMilliseconds, remaining.TotalMilliseconds));
            var instanceMutexAvailable = false;
            try
            {
                instanceMutexAvailable = instanceMutex.WaitOne(waitInterval);
            }
            catch (AbandonedMutexException)
            {
                instanceMutexAvailable = true;
            }

            if (!instanceMutexAvailable)
                continue;

            probeAcquiredAtLeastOnce = true;
            instanceMutex.ReleaseMutex();

            // Attempt full single-instance admission.
            // When retrying, suppress verbose duplicate logs to avoid Serilog flooding.
            var outcome = detailedGuard != null
                ? detailedGuard.TryAcquireSingleInstanceDetail(suppressLogging: true)
                : (singleInstanceGuard.TryAcquireSingleInstance()
                    ? WindowsSingleInstanceAcquisitionResult.Acquired
                    : WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy);

            if (outcome == WindowsSingleInstanceAcquisitionResult.Acquired)
                return true;

            if (outcome == WindowsSingleInstanceAcquisitionResult.AccessOrCreationError)
            {
                error = "Failed to create or acquire single-instance mutex on Windows.";
                return false;
            }

            // The probe mutex was available, but full acquisition was temporarily denied
            // (e.g. release-order race in FUS where UninstallGate is still owned by the exiting instance,
            // or transient third-process contention).
            // Do not abort prematurely; sleep a bounded interval and retry until timeout.
            remaining = timeout - stopwatch.Elapsed;
            if (remaining <= TimeSpan.Zero)
                break;

            var sleepTime = TimeSpan.FromMilliseconds(
                Math.Min(effectivePollInterval.TotalMilliseconds, remaining.TotalMilliseconds));
            if (sleepTime > TimeSpan.Zero)
            {
                Thread.Sleep(sleepTime);
            }
        }

        error = requestSent
            ? "The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired."
            : probeAcquiredAtLeastOnce
                ? "The Windows uninstall gate is owned, but no running PrivGvard instance accepted a shutdown request."
                : "No running PrivGvard instance accepted the uninstall shutdown request.";
        return false;
    }

    internal static bool TrySignalExistingInstance(
        string requestEventName,
        out string? error)
    {
        error = null;
        try
        {
            if (!EventWaitHandle.TryOpenExisting(requestEventName, out var requestEvent))
                return false;

            using (requestEvent)
            {
                if (requestEvent.Set())
                    return true;
            }

            error = "The running PrivGvard instance could not receive the uninstall shutdown request.";
            return false;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or WaitHandleCannotBeOpenedException)
        {
            error = $"The running PrivGvard instance could not receive the uninstall shutdown request ({ex.GetType().Name}).";
            Log.Error(ex, "Failed to signal the running PrivGvard instance for uninstall");
            return false;
        }
    }

    private void Listen()
    {
        var handles = new[] { _requestEvent, _stop.Token.WaitHandle };
        while (WaitHandle.WaitAny(handles) == 0)
        {
            EventHandler? handler;
            lock (_sync)
            {
                handler = _uninstallRequested;
                if (handler == null)
                    _requestPending = true;
            }

            InvokeHandler(handler);
        }
    }

    private void InvokeHandler(EventHandler? handler)
    {
        if (handler == null)
            return;

        try
        {
            handler(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "The uninstall shutdown request handler failed");
        }
    }
}
