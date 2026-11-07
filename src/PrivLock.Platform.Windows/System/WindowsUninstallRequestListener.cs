using System.Diagnostics;
using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

public sealed class WindowsUninstallRequestListener : IUninstallRequestListener
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsUninstallRequestListener>();
    private const string RequestEventName = @"Local\PrivLock_UninstallRequested";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan MinWaitHandleInterval = TimeSpan.FromMilliseconds(1);
    private static readonly TimeSpan MaxWaitHandleInterval = TimeSpan.FromMilliseconds(int.MaxValue);

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
        string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
    {
        return TryRequestRunningInstanceAndAcquire(
            singleInstanceGuard,
            timeout,
            PollInterval,
            out error,
            requestEventName,
            instanceMutexName);
    }

    internal static bool TryRequestRunningInstanceAndAcquire(
        ISingleInstanceGuard singleInstanceGuard,
        TimeSpan timeout,
        TimeSpan pollInterval,
        out string? error,
        string requestEventName = RequestEventName,
        string instanceMutexName = WindowsSingleInstanceGuard.MutexName)
    {
        ArgumentNullException.ThrowIfNull(singleInstanceGuard);
        ValidatePositiveFiniteInterval(timeout, nameof(timeout));
        ValidatePositiveFiniteInterval(pollInterval, nameof(pollInterval));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestEventName);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceMutexName);

        error = null;
        var stopwatch = Stopwatch.StartNew();
        var requestSent = false;
        WindowsSingleInstanceAcquisitionResult? lastContentionOutcome = null;
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

            var waitInterval = pollInterval <= remaining ? pollInterval : remaining;
            if (waitInterval < MinWaitHandleInterval)
                waitInterval = MinWaitHandleInterval;
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

            lastContentionOutcome = outcome;

            // The probe mutex was available, but full acquisition was temporarily denied
            // (e.g. release-order race in FUS where UninstallGate is still owned by the exiting instance,
            // or transient third-process contention).
            // Do not abort prematurely; sleep a bounded interval and retry until timeout.
            remaining = timeout - stopwatch.Elapsed;
            if (remaining <= TimeSpan.Zero)
                break;

            var sleepTime = pollInterval <= remaining ? pollInterval : remaining;
            if (sleepTime < MinWaitHandleInterval)
                sleepTime = MinWaitHandleInterval;
            Thread.Sleep(sleepTime);
        }

        error = FormatTimeoutError(requestSent, lastContentionOutcome);
        return false;
    }

    private static void ValidatePositiveFiniteInterval(TimeSpan value, string paramName)
    {
        if (value < MinWaitHandleInterval || value > MaxWaitHandleInterval)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "Interval must be a positive finite duration between 1 millisecond and Int32.MaxValue milliseconds.");
        }
    }

    private static string FormatTimeoutError(
        bool requestSent,
        WindowsSingleInstanceAcquisitionResult? lastContentionOutcome)
    {
        if (requestSent)
        {
            return "The running PrivGvard instance did not finish its safe shutdown before the uninstall handoff expired.";
        }

        return lastContentionOutcome switch
        {
            WindowsSingleInstanceAcquisitionResult.UninstallGateBusy =>
                "The Windows uninstall gate remained busy before the uninstall handoff expired, and no local shutdown request could be delivered (last observed contention: UninstallGateBusy; mutex state is not guaranteed after timeout).",
            WindowsSingleInstanceAcquisitionResult.SingleInstanceBusy =>
                "The Windows single-instance mutex remained busy before the uninstall handoff expired, and no local shutdown request could be delivered (last observed contention: SingleInstanceBusy; mutex state is not guaranteed after timeout).",
            _ =>
                "Single-instance admission was not obtained before the uninstall handoff expired, and the local shutdown request signal could not be delivered."
        };
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
