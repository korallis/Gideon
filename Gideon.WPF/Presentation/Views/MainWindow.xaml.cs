using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Gideon.WPF.Presentation.ViewModels.Shared;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Holographic main window with Windows 11 aesthetics and EVE styling
/// </summary>
public partial class MainWindow : Window
{
    #region Fields

    private readonly MainWindowViewModel _viewModel;
    private bool _isInitialized = false;

    #endregion

    #region Constructor

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        InitializeHolographicWindow();
        SetupNavigationSystem();
        SetupWindowControls();
        
        Loaded += OnMainWindowLoaded;
        StateChanged += OnWindowStateChanged;
    }

    #endregion

    #region Private Methods

    private void InitializeHolographicWindow()
    {
        // Enable Windows 11 features
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        ResizeMode = ResizeMode.CanResize;
        
        // Set up drag behavior for custom title bar
        MouseLeftButtonDown += OnWindowMouseLeftButtonDown;
        
        // Handle resize grips
        SetupResizeGrips();
    }

    private void SetupNavigationSystem()
    {
        // Initialize navigation items
        var dashboardItem = new HoloNavigationItem
        {
            Title = "Dashboard",
            Icon = "ViewDashboard",
            Content = "DashboardView"
        };

        var shipFittingItem = new HoloNavigationItem
        {
            Title = "Ship Fitting",
            Icon = "Rocket",
            Content = "ShipFittingView"
        };

        var marketItem = new HoloNavigationItem
        {
            Title = "Market Analysis",
            Icon = "TrendingUp",
            Content = "MarketAnalysisView"
        };

        var characterItem = new HoloNavigationItem
        {
            Title = "Character Planning",
            Icon = "Account",
            Content = "CharacterPlanningView"
        };

        var settingsItem = new HoloNavigationItem
        {
            Title = "Settings",
            Icon = "Settings",
            Content = "SettingsView"
        };

        // Add navigation items
        MainNavigation.AddNavigationItem(dashboardItem);
        MainNavigation.AddNavigationItem(shipFittingItem);
        MainNavigation.AddNavigationItem(marketItem);
        MainNavigation.AddNavigationItem(characterItem);
        MainNavigation.AddNavigationItem(settingsItem);

        // Set up navigation events
        MainNavigation.NavigationRequested += OnNavigationRequested;
        MainNavigation.NavigationCompleted += OnNavigationCompleted;

        // Set up sidebar button events
        DashboardButton.Click += (s, e) => NavigateToModule(dashboardItem);
        ShipFittingButton.Click += (s, e) => NavigateToModule(shipFittingItem);
        MarketButton.Click += (s, e) => NavigateToModule(marketItem);
        CharacterButton.Click += (s, e) => NavigateToModule(characterItem);
        SettingsButton.Click += (s, e) => NavigateToModule(settingsItem);
        GetStartedButton.Click += (s, e) => ShowWelcomeFlow();
    }

    private void SetupWindowControls()
    {
        // Window control button events
        MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
        MaximizeButton.Click += (s, e) => ToggleMaximized();
        CloseButton.Click += (s, e) => Close();
    }

    private void SetupResizeGrips()
    {
        // Corner resize grips
        ResizeGripNE.MouseLeftButtonDown += (s, e) => StartResize(ResizeDirection.TopRight);
        ResizeGripNW.MouseLeftButtonDown += (s, e) => StartResize(ResizeDirection.TopLeft);
        ResizeGripSE.MouseLeftButtonDown += (s, e) => StartResize(ResizeDirection.BottomRight);
        ResizeGripSW.MouseLeftButtonDown += (s, e) => StartResize(ResizeDirection.BottomLeft);
    }

    private void OnNavigationRequested(object sender, HoloNavigationEventArgs e)
    {
        // Handle navigation request
        if (e.Item != null)
        {
            NavigateToModule(e.Item);
        }
    }

    private void OnNavigationCompleted(object sender, HoloNavigationEventArgs e)
    {
        // Update UI after navigation completes
        UpdateSidebarSelection(e.Item);
    }

    private void NavigateToModule(HoloNavigationItem item)
    {
        if (item == null) return;

        // Hide welcome content
        WelcomeContent.Visibility = Visibility.Collapsed;
        ContentFrame.Visibility = Visibility.Visible;

        // Navigate using the navigation system
        MainNavigation.NavigateToItem(item);

        // Update status bar
        MainStatusBar.AddStatusItem(new HoloStatusItem
        {
            Text = $"Navigated to {item.Title}",
            Status = StatusType.Info
        });

        // TODO: Implement actual view navigation
        // This would typically load the appropriate UserControl or Page
        // ContentFrame.Navigate(new Uri($"Views/{item.Content}.xaml", UriKind.Relative));
    }

    private void UpdateSidebarSelection(HoloNavigationItem selectedItem)
    {
        // Reset all button states
        DashboardButton.ButtonState = HoloButtonState.Normal;
        ShipFittingButton.ButtonState = HoloButtonState.Normal;
        MarketButton.ButtonState = HoloButtonState.Normal;
        CharacterButton.ButtonState = HoloButtonState.Normal;
        SettingsButton.ButtonState = HoloButtonState.Normal;

        // Highlight selected button
        if (selectedItem?.Title == "Dashboard")
            DashboardButton.ButtonState = HoloButtonState.Success;
        else if (selectedItem?.Title == "Ship Fitting")
            ShipFittingButton.ButtonState = HoloButtonState.Success;
        else if (selectedItem?.Title == "Market Analysis")
            MarketButton.ButtonState = HoloButtonState.Success;
        else if (selectedItem?.Title == "Character Planning")
            CharacterButton.ButtonState = HoloButtonState.Success;
        else if (selectedItem?.Title == "Settings")
            SettingsButton.ButtonState = HoloButtonState.Success;
    }

    private void ShowWelcomeFlow()
    {
        // Start authentication or welcome process
        var welcomeItem = new HoloStatusItem
        {
            Text = "Welcome to Gideon! Please authenticate with EVE Online to begin.",
            Status = StatusType.Info
        };
        
        MainStatusBar.AddStatusItem(welcomeItem);

        // Trigger authentication flow (placeholder)
        // _viewModel.StartAuthentication();
    }

    private void ToggleMaximized()
    {
        WindowState = WindowState == WindowState.Maximized ? 
            WindowState.Normal : WindowState.Maximized;
    }

    private void StartResize(ResizeDirection direction)
    {
        if (WindowState == WindowState.Maximized) return;

        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            NativeMethods.SendMessage(hwnd, 0x112, (IntPtr)(0xF000 + direction), IntPtr.Zero);
        }
    }

    private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            // Only allow drag on title bar area
            var position = e.GetPosition(this);
            if (position.Y <= 40) // Title bar height
            {
                DragMove();
            }
        }
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized) return;
        
        _isInitialized = true;
        
        // Start with dashboard selected
        var dashboardItem = MainNavigation.NavigationItems.FirstOrDefault(i => i.Title == "Dashboard");
        if (dashboardItem != null)
        {
            NavigateToModule(dashboardItem);
        }

        // Initialize status bar
        MainStatusBar.AddStatusItem(new HoloStatusItem
        {
            Text = "Gideon initialized successfully",
            Status = StatusType.Normal
        });
    }

    private void OnWindowStateChanged(object sender, EventArgs e)
    {
        // Update maximize button icon based on window state
        if (WindowState == WindowState.Maximized)
        {
            // Change to restore icon
            var restoreIcon = MaximizeButton.Content as Path;
            if (restoreIcon != null)
            {
                restoreIcon.Data = Geometry.Parse("M4,8H8V4H20V16H16V20H4V8M16,8V14H18V6H10V8H16Z");
            }
        }
        else
        {
            // Change to maximize icon
            var maxIcon = MaximizeButton.Content as Path;
            if (maxIcon != null)
            {
                maxIcon.Data = Geometry.Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z");
            }
        }
    }

    #endregion

    #region Nested Types

    private enum ResizeDirection
    {
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8,
    }

    #endregion
}

/// <summary>
/// Native methods for window resizing
/// </summary>
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}