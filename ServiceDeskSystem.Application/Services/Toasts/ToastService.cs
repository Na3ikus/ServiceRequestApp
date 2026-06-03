using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;

namespace ServiceDeskSystem.Application.Services.Toasts;

/// <summary>
/// Implementation of global toast notification service.
/// </summary>
public sealed class ToastService : IToastService
{
    private readonly List<ToastMessage> toasts = [];

    /// <inheritdoc/>
    public event EventHandler? OnToastsChanged;

    /// <inheritdoc/>
    public IReadOnlyList<ToastMessage> Toasts => this.toasts;

    /// <inheritdoc/>
    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 4000)
    {
        var toast = new ToastMessage { Message = message, Type = type };
        this.toasts.Add(toast);
        this.NotifyStateChanged();

        _ = Task.Run(async () =>
        {
            await Task.Delay(durationMs).ConfigureAwait(false);

            toast.IsHiding = true;
            this.NotifyStateChanged();

            await Task.Delay(300).ConfigureAwait(false);
            this.toasts.Remove(toast);
            this.NotifyStateChanged();
        });

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task RemoveToastAsync(ToastMessage toast)
    {
        toast.IsHiding = true;
        this.NotifyStateChanged();
        await Task.Delay(300).ConfigureAwait(false);
        this.toasts.Remove(toast);
        this.NotifyStateChanged();
    }

    private void NotifyStateChanged() => this.OnToastsChanged?.Invoke(this, EventArgs.Empty);
}

