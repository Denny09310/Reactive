using Microsoft.AspNetCore.Components;
using Reactive.Core;
using Reactive.Core.Extensions;

namespace Reactive.Blazor;

/// <summary>
/// Base class for Blazor components that require automatic management of reactive <see cref="Effect"/> instances.
/// <para>
/// Inherit from this class to register effects within your component without manually handling their disposal.
/// All registered effects are automatically disposed when the component is removed from the render tree.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// This class provides two overloads of the <see cref="Effect"/> method:
/// <list type="bullet">
///   <item>
///     <description><c>Effect(Action)</c>: Registers a simple effect without cleanup logic.</description>
///   </item>
///   <item>
///     <description><c>Effect(Func&lt;Action?&gt;)</c>: Registers an effect with optional cleanup logic.</description>
///   </item>
/// </list>
/// </para>
/// <para>
/// All effects are tracked and disposed automatically when the component is destroyed.
/// </para>
/// </remarks>
public class ReactiveComponent : ComponentBase, IDisposable
{
    /// <summary>
    /// Stores all active <see cref="Effect"/> instances registered by this component.
    /// </summary>
    private readonly HashSet<Effect> _effects = [];

    private bool _disposed;

    /// <summary>
    /// Disposes the component and all registered effects.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed resources, including all registered effects.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from <see cref="Dispose()"/>.</param>
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

    /// <summary>
    /// Registers a reactive effect with optional cleanup logic.
    /// The effect will be automatically disposed when the component is destroyed.
    /// </summary>
    /// <param name="callback">
    /// A function that executes the effect logic and optionally returns a cleanup <see cref="Action"/>.
    /// </param>
    protected void Effect(Func<Action?> callback)
    {
        var effect = Reactivity.Effect(callback);
        _effects.Add(effect);
    }

    /// <summary>
    /// Registers a simple reactive effect without cleanup logic.
    /// The effect will be automatically disposed when the component is destroyed.
    /// </summary>
    /// <param name="callback">The effect logic to execute.</param>
    protected void Effect(Action callback)
    {
        var effect = Reactivity.Effect(callback);
        _effects.Add(effect);
    }
}