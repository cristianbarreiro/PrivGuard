using PrivLock.Platform.Abstractions;
using Serilog;

namespace PrivLock.Platform.Windows.System;

public sealed class WindowsApplicationActivationListener : IApplicationActivationListener
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WindowsApplicationActivationListener>();
    internal const string ActivationEventName = @"Global\PrivLock_ActivateExistingInstance";

    private readonly object _sync = new();
    private readonly EventWaitHandle _activationEvent;
    private readonly CancellationTokenSource _stop = new();
    private readonly bool _createdActivationEvent;
    private EventHandler? _activationRequested;
    private Task? _listenerTask;
    private bool _activationPending;
    private bool _disposed;

    public WindowsApplicationActivationListener()
        : this(ActivationEventName)
    {
    }

    internal WindowsApplicationActivationListener(string activationEventName)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows application activation is supported only on Windows.");

        _activationEvent = new EventWaitHandle(
            initialState: false,
            EventResetMode.AutoReset,
            activationEventName,
            out var createdActivationEvent);
        _createdActivationEvent = createdActivationEvent;
    }

    public event EventHandler? ActivationRequested
    {
        add
        {
            EventHandler? handlerToInvoke = null;
            lock (_sync)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _activationRequested += value;
                if (_activationPending && _activationRequested != null)
                {
                    _activationPending = false;
                    handlerToInvoke = _activationRequested;
                }
            }

            InvokeHandler(handlerToInvoke);
        }
        remove
        {
            lock (_sync)
            {
                _activationRequested -= value;
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
            _activationEvent.Set();
            listenerTask = _listenerTask;
        }

        listenerTask?.GetAwaiter().GetResult();
        _activationEvent.Dispose();
        _stop.Dispose();
    }

    public bool TryRequestExistingInstanceActivation(out string? error)
    {
        error = null;
        if (_createdActivationEvent)
            return false;

        try
        {
            if (_activationEvent.Set())
                return true;

            error = "The running PrivGvard instance could not receive the window activation request.";
            return false;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or WaitHandleCannotBeOpenedException)
        {
            error = $"The running PrivGvard instance could not receive the window activation request ({ex.GetType().Name}).";
            Log.Error(ex, "Failed to signal the running PrivGvard instance for window activation");
            return false;
        }
    }

    private void Listen()
    {
        var handles = new[] { _activationEvent, _stop.Token.WaitHandle };
        while (true)
        {
            var signaledHandle = WaitHandle.WaitAny(handles);
            if (_stop.IsCancellationRequested || signaledHandle == 1)
                return;

            EventHandler? handler;
            lock (_sync)
            {
                handler = _activationRequested;
                if (handler == null)
                    _activationPending = true;
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
            Log.Error(ex, "The application activation request handler failed");
        }
    }
}
