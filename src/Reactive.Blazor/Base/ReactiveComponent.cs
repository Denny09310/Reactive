using Microsoft.AspNetCore.Components;
using Reactive.Core;
using Reactive.Core.Extensions;

namespace Reactive.Blazor;

/// <summary>
/// Base component for Blazor that automatically tracks and disposes
/// reactive <see cref="Effect"/> instances when the component is disposed.
///
/// Inherit from this class to safely register Effects inside components without
/// manually managing their lifecycle.
/// </summary>
/// <remarks>
/// This component provides two overloads of <see cref="Effect"/>:
/// - <c>Effect(Action)</c>: for simple effects.
/// - <c>Effect(Func&lt;Action?&gt;)</c>: for effects with optional cleanup logic.
///
/// All effects are automatically disposed when the component is destroyed (e.g., removed from the render tree).
/// </remarks>
public class ReactiveComponent : ComponentBase, IDisposable
{
    private readonly HashSet<Effect> _effects = [];

    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var effect in _effects.ToList())
            {
                effect.Dispose();
            }

            _effects.Clear();
        }

        _disposed = true;
    }

    protected void Effect(Func<Action?> callback)
    {
        var effect = Reactivity.Effect(callback);
        _effects.Add(effect);
    }

    protected void Effect(Action callback)
    {
        var effect = Reactivity.Effect(callback);
        _effects.Add(effect);
    }
}