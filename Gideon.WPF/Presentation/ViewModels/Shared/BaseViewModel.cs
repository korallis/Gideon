using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gideon.WPF.Presentation.ViewModels.Shared;

/// <summary>
/// Base ViewModel class providing common functionality for all ViewModels
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    /// <summary>
    /// Sets the loading state and optionally a status message
    /// </summary>
    protected void SetLoading(bool loading, string? message = null)
    {
        IsLoading = loading;
        IsBusy = loading;
        StatusMessage = message;
        
        if (loading)
        {
            ClearError();
        }
    }

    /// <summary>
    /// Sets an error state with message
    /// </summary>
    protected void SetError(string message, Exception? exception = null)
    {
        HasError = true;
        ErrorMessage = message;
        IsLoading = false;
        IsBusy = false;
        
        // Log the exception if provided
        if (exception != null)
        {
            // TODO: Integrate with logging service
            System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {message}");
            System.Diagnostics.Debug.WriteLine($"Exception: {exception}");
        }
    }

    /// <summary>
    /// Clears any error state
    /// </summary>
    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Sets a status message
    /// </summary>
    protected void SetStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Clears the status message
    /// </summary>
    protected void ClearStatus()
    {
        StatusMessage = null;
    }

    /// <summary>
    /// Executes an async operation with loading state management
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation, string? loadingMessage = null)
    {
        try
        {
            SetLoading(true, loadingMessage);
            await operation();
        }
        catch (Exception ex)
        {
            SetError("An error occurred while performing the operation.", ex);
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Executes an async operation with result and loading state management
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? loadingMessage = null)
    {
        try
        {
            SetLoading(true, loadingMessage);
            return await operation();
        }
        catch (Exception ex)
        {
            SetError("An error occurred while performing the operation.", ex);
            return default;
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Command to clear any error state
    /// </summary>
    [RelayCommand]
    protected virtual void ClearErrorCommand()
    {
        ClearError();
    }

    /// <summary>
    /// Override this method to perform cleanup when the ViewModel is disposed
    /// </summary>
    public virtual void Cleanup()
    {
        // Override in derived classes for cleanup logic
    }
}