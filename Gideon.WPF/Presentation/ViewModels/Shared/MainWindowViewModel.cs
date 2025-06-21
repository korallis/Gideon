using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Presentation.ViewModels.Shared;

/// <summary>
/// Main window ViewModel handling navigation and global application state
/// </summary>
public partial class MainWindowViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private Character? currentCharacter;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionStatus = "Disconnected";

    public MainWindowViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        
        // Subscribe to authentication events
        _authenticationService.ActiveCharacterChanged += OnActiveCharacterChanged;
        _authenticationService.CharacterAuthenticated += OnCharacterAuthenticated;
        _authenticationService.CharacterLoggedOut += OnCharacterLoggedOut;
        _authenticationService.AuthenticationError += OnAuthenticationError;
        
        // Initialize
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _authenticationService.InitializeAsync();
            
            CurrentCharacter = _authenticationService.CurrentCharacter;
            UpdateConnectionStatus();
            
            // Set default view
            if (CurrentCharacter != null)
            {
                NavigateToShipFitting();
            }
            else
            {
                ShowAuthentication();
            }
        }, "Initializing application...");
    }

    private void OnActiveCharacterChanged(object? sender, Character character)
    {
        CurrentCharacter = character;
        UpdateConnectionStatus();
    }

    private void OnCharacterAuthenticated(object? sender, Character character)
    {
        CurrentCharacter = character;
        UpdateConnectionStatus();
        SetStatus($"Successfully authenticated as {character.Name}");
        
        // Navigate to main content after authentication
        NavigateToShipFitting();
    }

    private void OnCharacterLoggedOut(object? sender, Character character)
    {
        SetStatus($"Logged out {character.Name}");
        UpdateConnectionStatus();
        
        // If no characters remain, show authentication
        if (!_authenticationService.IsAuthenticated)
        {
            CurrentCharacter = null;
            ShowAuthentication();
        }
    }

    private void OnAuthenticationError(object? sender, string errorMessage)
    {
        SetError($"Authentication error: {errorMessage}");
    }

    private void UpdateConnectionStatus()
    {
        IsConnected = _authenticationService.IsAuthenticated;
        ConnectionStatus = IsConnected ? "Connected" : "Disconnected";
    }

    // Navigation commands
    [RelayCommand]
    private void NavigateToShipFitting()
    {
        if (!EnsureAuthenticated()) return;
        
        // TODO: Create and set ShipFittingViewModel
        SetStatus("Navigating to Ship Fitting...");
        CurrentView = "ShipFitting"; // Placeholder
    }

    [RelayCommand]
    private void NavigateToMarketAnalysis()
    {
        if (!EnsureAuthenticated()) return;
        
        // TODO: Create and set MarketAnalysisViewModel
        SetStatus("Navigating to Market Analysis...");
        CurrentView = "MarketAnalysis"; // Placeholder
    }

    [RelayCommand]
    private void NavigateToCharacterPlanning()
    {
        if (!EnsureAuthenticated()) return;
        
        // TODO: Create and set CharacterPlanningViewModel
        SetStatus("Navigating to Character Planning...");
        CurrentView = "CharacterPlanning"; // Placeholder
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        // Settings doesn't require authentication
        // TODO: Create and set SettingsViewModel
        SetStatus("Navigating to Settings...");
        CurrentView = "Settings"; // Placeholder
    }

    // Authentication commands
    [RelayCommand]
    private async Task ShowAuthenticationAsync()
    {
        await ExecuteAsync(async () =>
        {
            // TODO: Create and set AuthenticationViewModel
            CurrentView = "Authentication"; // Placeholder
            SetStatus("Please authenticate with EVE Online");
        });
    }

    [RelayCommand]
    private async Task StartAuthenticationAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _authenticationService.StartAuthenticationAsync();
            if (!result)
            {
                SetError("Failed to start authentication process");
            }
        }, "Starting authentication...");
    }

    [RelayCommand]
    private async Task LogoutCurrentCharacterAsync()
    {
        if (CurrentCharacter == null) return;
        
        await ExecuteAsync(async () =>
        {
            await _authenticationService.LogoutAsync(CurrentCharacter.CharacterId);
        }, "Logging out...");
    }

    [RelayCommand]
    private async Task LogoutAllCharactersAsync()
    {
        await ExecuteAsync(async () =>
        {
            await _authenticationService.LogoutAllAsync();
        }, "Logging out all characters...");
    }

    [RelayCommand]
    private async Task RefreshTokenAsync()
    {
        if (CurrentCharacter == null) return;
        
        await ExecuteAsync(async () =>
        {
            var result = await _authenticationService.RefreshTokenAsync(CurrentCharacter.CharacterId);
            if (result)
            {
                SetStatus("Token refreshed successfully");
            }
            else
            {
                SetError("Failed to refresh token");
            }
        }, "Refreshing authentication token...");
    }

    // Character management
    [RelayCommand]
    private async Task SwitchCharacterAsync(int characterId)
    {
        await ExecuteAsync(async () =>
        {
            var result = await _authenticationService.SwitchActiveCharacterAsync(characterId);
            if (result)
            {
                SetStatus("Character switched successfully");
            }
            else
            {
                SetError("Failed to switch character");
            }
        }, "Switching character...");
    }

    // Helper methods
    private bool EnsureAuthenticated()
    {
        if (!_authenticationService.IsAuthenticated)
        {
            ShowAuthentication();
            SetError("Please authenticate with EVE Online first");
            return false;
        }
        return true;
    }

    private void ShowAuthentication()
    {
        // TODO: Create and set AuthenticationViewModel
        CurrentView = "Authentication"; // Placeholder
    }

    public override void Cleanup()
    {
        // Unsubscribe from events
        _authenticationService.ActiveCharacterChanged -= OnActiveCharacterChanged;
        _authenticationService.CharacterAuthenticated -= OnCharacterAuthenticated;
        _authenticationService.CharacterLoggedOut -= OnCharacterLoggedOut;
        _authenticationService.AuthenticationError -= OnAuthenticationError;
        
        base.Cleanup();
    }
}