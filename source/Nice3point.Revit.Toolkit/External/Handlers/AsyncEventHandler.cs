﻿using System.ComponentModel;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.Toolkit.External.Handlers;

/// <summary>
///     Handler, to provide access to change the Revit document
/// </summary>
/// <remarks>Suitable for cases where it is needed to await the completion of an external event</remarks>
[PublicAPI]
public sealed class AsyncEventHandler : ExternalEventHandler
{
    private Action<UIApplication>? _action;
    private TaskCompletionSource<bool>? _resultTask;

    /// <summary>Callback invoked by Revit. Not used to be called in user code</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Execute(UIApplication uiApplication)
    {
        if (_resultTask is null)
        {
            throw new InvalidOperationException("RaiseAsync() must be called instead of using Execute callback");
        }
        
        if (_action is null)
        {
            _resultTask.SetResult(false);
            return;
        }

        try
        {
            _action(uiApplication);
            _resultTask.SetResult(false);
        }
        catch (Exception exception)
        {
            _resultTask.SetException(exception);
        }
        finally
        {
            _action = null;
            _resultTask = null;
        }
    }

    /// <summary>
    ///     Instructing Revit to queue a handler, raise (signal) the external event and async awaiting for its completion
    /// </summary>
    /// <remarks>
    ///     This method async awaiting completion of the <see cref="Nice3point.Revit.Toolkit.External.Handlers.AsyncEventHandler.Execute" /> method. <br />
    ///     Exceptions in the delegate will not be ignored and will be rethrown in the original synchronization context.<br />
    ///     Use <see langword="await" /> to awaiting.
    ///     <see cref="System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task[])" />, <see cref="System.Threading.Tasks.Task.Wait()" /> will cause a deadlock.
    /// </remarks>
    public async Task RaiseAsync(Action<UIApplication> action)
    {
        if (_action is null) _action = action;
        else _action += action;

        Raise();

        _resultTask ??= new TaskCompletionSource<bool>();
        await _resultTask.Task;
    }
}