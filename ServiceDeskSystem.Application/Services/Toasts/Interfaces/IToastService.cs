using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceDeskSystem.Application.Services.Toasts.Models;

namespace ServiceDeskSystem.Application.Services.Toasts.Interfaces;

/// <summary>
/// Service for managing global toast notifications.
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Event triggered when the list of active toasts changes.
    /// </summary>
    event EventHandler? OnToastsChanged;

    /// <summary>
    /// Gets the list of active toast notifications.
    /// </summary>
    IReadOnlyList<ToastMessage> Toasts { get; }

    /// <summary>
    /// Shows a toast notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of notification.</param>
    /// <param name="durationMs">The visibility duration in milliseconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 4000);

    /// <summary>
    /// Removes a toast notification.
    /// </summary>
    /// <param name="toast">The toast message to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveToastAsync(ToastMessage toast);
}
