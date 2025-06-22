// ==========================================================================
// HoloPortfolioTracker.cs - Holographic Portfolio Tracking Display
// ==========================================================================
// Advanced portfolio tracking featuring real-time asset monitoring,
// performance analytics, EVE-style portfolio management, and holographic portfolio visualization.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic portfolio tracker with real-time asset monitoring and performance analytics
/// </summary>
public class HoloPortfolioTracker : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPortfolioTracker),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPortfolioTracker),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty PortfolioDataProperty =
        DependencyProperty.Register(nameof(PortfolioData), typeof(ObservableCollection<HoloPortfolioAsset>), typeof(HoloPortfolioTracker),
            new PropertyMetadata(null, OnPortfolioDataChanged));

    public static readonly DependencyProperty PortfolioSummaryProperty =
        DependencyProperty.Register(nameof(PortfolioSummary), typeof(HoloPortfolioSummary), typeof(HoloPortfolioTracker),
            new PropertyMetadata(null, OnPortfolioSummaryChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloPortfolioTracker),
            new PropertyMetadata("All Characters", OnSelectedCharacterChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(PortfolioViewMode), typeof(HoloPortfolioTracker),
            new PropertyMetadata(PortfolioViewMode.Overview, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPerformanceMetricsProperty =
        DependencyProperty.Register(nameof(ShowPerformanceMetrics), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnShowPerformanceMetricsChanged));

    public static readonly DependencyProperty ShowAssetBreakdownProperty =
        DependencyProperty.Register(nameof(ShowAssetBreakdown), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnShowAssetBreakdownChanged));

    public static readonly DependencyProperty ShowRiskAnalysisProperty =
        DependencyProperty.Register(nameof(ShowRiskAnalysis), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnShowRiskAnalysisChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloPortfolioTracker),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloPortfolioTracker),
            new PropertyMetadata(TimeSpan.FromSeconds(30), OnUpdateIntervalChanged));

    public static readonly DependencyProperty TimeFrameProperty =
        DependencyProperty.Register(nameof(TimeFrame), typeof(PortfolioTimeFrame), typeof(HoloPortfolioTracker),
            new PropertyMetadata(PortfolioTimeFrame.Month, OnTimeFrameChanged));

    public double HolographicIntensity
    {
        get => (double)GetValue(HolographicIntensityProperty);
        set => SetValue(HolographicIntensityProperty, value);
    }

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public ObservableCollection<HoloPortfolioAsset> PortfolioData
    {
        get => (ObservableCollection<HoloPortfolioAsset>)GetValue(PortfolioDataProperty);
        set => SetValue(PortfolioDataProperty, value);
    }

    public HoloPortfolioSummary PortfolioSummary
    {
        get => (HoloPortfolioSummary)GetValue(PortfolioSummaryProperty);
        set => SetValue(PortfolioSummaryProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public PortfolioViewMode ViewMode
    {
        get => (PortfolioViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowPerformanceMetrics
    {
        get => (bool)GetValue(ShowPerformanceMetricsProperty);
        set => SetValue(ShowPerformanceMetricsProperty, value);
    }

    public bool ShowAssetBreakdown
    {
        get => (bool)GetValue(ShowAssetBreakdownProperty);
        set => SetValue(ShowAssetBreakdownProperty, value);
    }

    public bool ShowRiskAnalysis
    {
        get => (bool)GetValue(ShowRiskAnalysisProperty);
        set => SetValue(ShowRiskAnalysisProperty, value);
    }

    public bool AutoRefresh
    {
        get => (bool)GetValue(AutoRefreshProperty);
        set => SetValue(AutoRefreshProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    public PortfolioTimeFrame TimeFrame
    {
        get => (PortfolioTimeFrame)GetValue(TimeFrameProperty);
        set => SetValue(TimeFrameProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _portfolioCanvas;
    private ModelVisual3D _portfolioModel;
    private GeometryModel3D _primaryPortfolioGeometry;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloPortfolioParticle> _portfolioParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloPortfolioTracker()
    {
        InitializeComponent();
        InitializeFields();
        InitializePortfolio();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 200));
        
        _portfolioCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _portfolioCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _portfolioParticles = new List<HoloPortfolioParticle>();
        _isInitialized = false;
        
        PortfolioData = new ObservableCollection<HoloPortfolioAsset>();
        InitializeSampleData();
    }

    private void InitializePortfolio()
    {
        CreatePortfolioInterface();
        UpdatePortfolioVisuals();
        CalculatePortfolioSummary();
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;

        _particleTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _particleTimer.Tick += OnParticleTick;

        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void InitializeSampleData()
    {
        var sampleAssets = new List<HoloPortfolioAsset>
        {
            new() { ItemName = "Tritanium", Quantity = 1_000_000, CurrentPrice = 5.50, AveragePrice = 5.25, Location = "Jita", Character = "Main Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Pyerite", Quantity = 500_000, CurrentPrice = 12.30, AveragePrice = 11.85, Location = "Jita", Character = "Main Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Mexallon", Quantity = 200_000, CurrentPrice = 78.50, AveragePrice = 75.20, Location = "Amarr", Character = "Main Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Isogen", Quantity = 150_000, CurrentPrice = 125.80, AveragePrice = 130.45, Location = "Jita", Character = "Alt Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Nocxium", Quantity = 75_000, CurrentPrice = 845.60, AveragePrice = 820.35, Location = "Dodixie", Character = "Alt Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Zydrine", Quantity = 30_000, CurrentPrice = 1_256.70, AveragePrice = 1_180.25, Location = "Rens", Character = "Main Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Megacyte", Quantity = 15_000, CurrentPrice = 2_845.30, AveragePrice = 2_950.75, Location = "Hek", Character = "Alt Character", LastUpdated = DateTime.Now },
            new() { ItemName = "Morphite", Quantity = 5_000, CurrentPrice = 8_750.90, AveragePrice = 8_500.15, Location = "Jita", Character = "Main Character", LastUpdated = DateTime.Now }
        };
        
        foreach (var asset in sampleAssets)
        {
            PortfolioData.Add(asset);
        }
    }

    #endregion

    #region Portfolio Interface Creation

    private void CreatePortfolioInterface()
    {
        _portfolioCanvas.Children.Clear();
        
        CreateSummarySection();
        CreateAssetBreakdownSection();
        CreatePerformanceSection();
        CreateRiskAnalysisSection();
        CreateParticleEffects();
    }

    private void CreateSummarySection()
    {
        var summaryPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(summaryPanel, 20);
        Canvas.SetTop(summaryPanel, 20);
        _portfolioCanvas.Children.Add(summaryPanel);
        
        var summary = PortfolioSummary ?? new HoloPortfolioSummary();
        
        AddSummaryDisplay(summaryPanel, "Total Value:", summary.TotalValue);
        AddSummaryDisplay(summaryPanel, "Total Cost:", summary.TotalCost);
        AddSummaryDisplay(summaryPanel, "Unrealized P&L:", summary.UnrealizedPnL);
        AddSummaryDisplay(summaryPanel, "Total Return:", summary.TotalReturn);
        AddSummaryDisplay(summaryPanel, "Asset Count:", summary.AssetCount);
        AddSummaryDisplay(summaryPanel, "Diversity Score:", summary.DiversityScore);
        AddSummaryDisplay(summaryPanel, "Risk Rating:", summary.RiskRating);
        AddSummaryDisplay(summaryPanel, "Last Updated:", summary.LastUpdated.ToString("HH:mm:ss"));
    }

    private void CreateAssetBreakdownSection()
    {
        if (!ShowAssetBreakdown) return;
        
        var assetPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(assetPanel, 320);
        Canvas.SetTop(assetPanel, 20);
        _portfolioCanvas.Children.Add(assetPanel);
        
        var titleBlock = new TextBlock
        {
            Text = "Asset Breakdown",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(5)
        };
        assetPanel.Children.Add(titleBlock);
        
        foreach (var asset in PortfolioData.Take(10))
        {
            AddAssetDisplay(assetPanel, asset);
        }
    }

    private void CreatePerformanceSection()
    {
        if (!ShowPerformanceMetrics) return;
        
        var perfPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.35)
        };
        
        Canvas.SetLeft(perfPanel, 620);
        Canvas.SetTop(perfPanel, 20);
        _portfolioCanvas.Children.Add(perfPanel);
        
        var titleBlock = new TextBlock
        {
            Text = "Performance Metrics",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(5)
        };
        perfPanel.Children.Add(titleBlock);
        
        var performance = CalculatePerformanceMetrics();
        AddPerformanceDisplay(perfPanel, "Daily Change:", performance.DailyChange);
        AddPerformanceDisplay(perfPanel, "Weekly Change:", performance.WeeklyChange);
        AddPerformanceDisplay(perfPanel, "Monthly Change:", performance.MonthlyChange);
        AddPerformanceDisplay(perfPanel, "Best Performer:", performance.BestPerformer);
        AddPerformanceDisplay(perfPanel, "Worst Performer:", performance.WorstPerformer);
        AddPerformanceDisplay(perfPanel, "Volatility:", performance.Volatility);
        AddPerformanceDisplay(perfPanel, "Sharpe Ratio:", performance.SharpeRatio);
    }

    private void CreateRiskAnalysisSection()
    {
        if (!ShowRiskAnalysis) return;
        
        var riskPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(riskPanel, 20);
        Canvas.SetTop(riskPanel, 380);
        _portfolioCanvas.Children.Add(riskPanel);
        
        var titleBlock = new TextBlock
        {
            Text = "Risk Analysis",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(5)
        };
        riskPanel.Children.Add(titleBlock);
        
        var riskAnalysis = CalculateRiskAnalysis();
        AddRiskDisplay(riskPanel, "Concentration Risk:", riskAnalysis.ConcentrationRisk);
        AddRiskDisplay(riskPanel, "Liquidity Risk:", riskAnalysis.LiquidityRisk);
        AddRiskDisplay(riskPanel, "Market Risk:", riskAnalysis.MarketRisk);
        AddRiskDisplay(riskPanel, "Diversification:", riskAnalysis.DiversificationScore);
        AddRiskDisplay(riskPanel, "Value at Risk (95%):", riskAnalysis.ValueAtRisk);
        AddRiskDisplay(riskPanel, "Recommended Actions:", riskAnalysis.RecommendedActions);
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 100; i++)
        {
            var particle = CreatePortfolioParticle();
            _portfolioParticles.Add(particle);
            _portfolioCanvas.Children.Add(particle.Element);
        }
    }

    #endregion

    #region Portfolio Calculations

    private void CalculatePortfolioSummary()
    {
        var summary = new HoloPortfolioSummary();
        
        if (PortfolioData != null && PortfolioData.Any())
        {
            summary.TotalValue = PortfolioData.Sum(a => a.CurrentPrice * a.Quantity);
            summary.TotalCost = PortfolioData.Sum(a => a.AveragePrice * a.Quantity);
            summary.UnrealizedPnL = summary.TotalValue - summary.TotalCost;
            summary.TotalReturn = summary.TotalCost > 0 ? (summary.UnrealizedPnL / summary.TotalCost) * 100 : 0;
            summary.AssetCount = PortfolioData.Count;
            summary.DiversityScore = CalculateDiversityScore();
            summary.RiskRating = CalculateRiskRating();
            summary.LastUpdated = DateTime.Now;
        }
        
        PortfolioSummary = summary;
    }

    private double CalculateDiversityScore()
    {
        if (PortfolioData == null || !PortfolioData.Any()) return 0;
        
        var totalValue = PortfolioData.Sum(a => a.CurrentPrice * a.Quantity);
        if (totalValue == 0) return 0;
        
        var weights = PortfolioData.Select(a => (a.CurrentPrice * a.Quantity) / totalValue).ToList();
        var herfindahlIndex = weights.Sum(w => w * w);
        
        return (1 - herfindahlIndex) * 100;
    }

    private string CalculateRiskRating()
    {
        var diversityScore = CalculateDiversityScore();
        var concentrationRisk = CalculateConcentrationRisk();
        
        if (diversityScore > 80 && concentrationRisk < 30)
            return "Low";
        if (diversityScore > 60 && concentrationRisk < 50)
            return "Medium";
        if (diversityScore > 40 && concentrationRisk < 70)
            return "High";
        
        return "Very High";
    }

    private HoloPortfolioPerformance CalculatePerformanceMetrics()
    {
        var performance = new HoloPortfolioPerformance();
        
        if (PortfolioData != null && PortfolioData.Any())
        {
            var assets = PortfolioData.ToList();
            var dailyChanges = assets.Select(a => ((a.CurrentPrice - a.AveragePrice) / a.AveragePrice) * 100).ToList();
            
            performance.DailyChange = dailyChanges.Average();
            performance.WeeklyChange = performance.DailyChange * 7;
            performance.MonthlyChange = performance.DailyChange * 30;
            
            var bestAsset = assets.OrderByDescending(a => ((a.CurrentPrice - a.AveragePrice) / a.AveragePrice) * 100).FirstOrDefault();
            var worstAsset = assets.OrderBy(a => ((a.CurrentPrice - a.AveragePrice) / a.AveragePrice) * 100).FirstOrDefault();
            
            performance.BestPerformer = bestAsset?.ItemName ?? "N/A";
            performance.WorstPerformer = worstAsset?.ItemName ?? "N/A";
            
            performance.Volatility = CalculateVolatility();
            performance.SharpeRatio = CalculateSharpeRatio();
        }
        
        return performance;
    }

    private double CalculateVolatility()
    {
        if (PortfolioData == null || !PortfolioData.Any()) return 0;
        
        var returns = PortfolioData.Select(a => ((a.CurrentPrice - a.AveragePrice) / a.AveragePrice) * 100).ToList();
        var meanReturn = returns.Average();
        var variance = returns.Select(r => Math.Pow(r - meanReturn, 2)).Average();
        
        return Math.Sqrt(variance);
    }

    private double CalculateSharpeRatio()
    {
        var volatility = CalculateVolatility();
        if (volatility == 0) return 0;
        
        var performance = CalculatePerformanceMetrics();
        var riskFreeRate = 2.0; // Assume 2% risk-free rate
        
        return (performance.DailyChange - riskFreeRate) / volatility;
    }

    private HoloPortfolioRiskAnalysis CalculateRiskAnalysis()
    {
        var riskAnalysis = new HoloPortfolioRiskAnalysis();
        
        riskAnalysis.ConcentrationRisk = CalculateConcentrationRisk();
        riskAnalysis.LiquidityRisk = CalculateLiquidityRisk();
        riskAnalysis.MarketRisk = CalculateMarketRisk();
        riskAnalysis.DiversificationScore = CalculateDiversityScore();
        riskAnalysis.ValueAtRisk = CalculateValueAtRisk();
        riskAnalysis.RecommendedActions = GenerateRecommendations();
        
        return riskAnalysis;
    }

    private double CalculateConcentrationRisk()
    {
        if (PortfolioData == null || !PortfolioData.Any()) return 0;
        
        var totalValue = PortfolioData.Sum(a => a.CurrentPrice * a.Quantity);
        if (totalValue == 0) return 0;
        
        var maxConcentration = PortfolioData.Max(a => (a.CurrentPrice * a.Quantity) / totalValue);
        return maxConcentration * 100;
    }

    private double CalculateLiquidityRisk()
    {
        if (PortfolioData == null || !PortfolioData.Any()) return 0;
        
        var lowLiquidityAssets = PortfolioData.Where(a => a.Quantity > 100_000).Count();
        return (double)lowLiquidityAssets / PortfolioData.Count * 100;
    }

    private double CalculateMarketRisk()
    {
        var volatility = CalculateVolatility();
        return Math.Min(volatility * 2, 100);
    }

    private double CalculateValueAtRisk()
    {
        if (PortfolioSummary == null) return 0;
        
        var volatility = CalculateVolatility();
        var confidence = 1.96; // 95% confidence interval
        
        return PortfolioSummary.TotalValue * volatility * confidence / 100;
    }

    private string GenerateRecommendations()
    {
        var recommendations = new List<string>();
        
        var concentrationRisk = CalculateConcentrationRisk();
        if (concentrationRisk > 50)
            recommendations.Add("Reduce concentration in top assets");
        
        var diversityScore = CalculateDiversityScore();
        if (diversityScore < 50)
            recommendations.Add("Improve portfolio diversification");
        
        var liquidityRisk = CalculateLiquidityRisk();
        if (liquidityRisk > 30)
            recommendations.Add("Consider more liquid assets");
        
        return recommendations.Any() ? string.Join(", ", recommendations) : "Portfolio is well-balanced";
    }

    #endregion

    #region UI Helpers

    private void AddSummaryDisplay(Panel parent, string label, object value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueText = value switch
        {
            double d when label.Contains("Value") || label.Contains("Cost") || label.Contains("P&L") => FormatCurrency(d),
            double d when label.Contains("Return") || label.Contains("Score") => $"{d:F2}%",
            double d => d.ToString("F2"),
            int i => i.ToString("N0"),
            _ => value.ToString()
        };
        
        var valueColor = value switch
        {
            double d when label.Contains("P&L") => d >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative"),
            double d when label.Contains("Return") => d >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative"),
            _ => GetEVEBrush("Secondary")
        };
        
        var valueElement = new TextBlock
        {
            Text = valueText,
            Width = 100,
            Foreground = valueColor,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddAssetDisplay(Panel parent, HoloPortfolioAsset asset)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
        
        var nameElement = new TextBlock
        {
            Text = asset.ItemName,
            Width = 80,
            Foreground = GetEVEBrush("Primary"),
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var currentValue = asset.CurrentPrice * asset.Quantity;
        var costBasis = asset.AveragePrice * asset.Quantity;
        var pnl = currentValue - costBasis;
        var pnlPercent = costBasis > 0 ? (pnl / costBasis) * 100 : 0;
        
        var valueElement = new TextBlock
        {
            Text = FormatCurrency(currentValue),
            Width = 80,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 10,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var pnlElement = new TextBlock
        {
            Text = $"{pnlPercent:+0.0;-0.0;0.0}%",
            Width = 50,
            Foreground = pnl >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative"),
            FontSize = 10,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(nameElement);
        panel.Children.Add(valueElement);
        panel.Children.Add(pnlElement);
        parent.Children.Add(panel);
    }

    private void AddPerformanceDisplay(Panel parent, string label, object value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 100,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueText = value switch
        {
            double d when label.Contains("Change") => $"{d:+0.00;-0.00;0.00}%",
            double d => d.ToString("F2"),
            _ => value.ToString()
        };
        
        var valueColor = value switch
        {
            double d when label.Contains("Change") => d >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative"),
            _ => GetEVEBrush("Secondary")
        };
        
        var valueElement = new TextBlock
        {
            Text = valueText,
            Width = 80,
            Foreground = valueColor,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddRiskDisplay(Panel parent, string label, object value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueText = value switch
        {
            double d when label.Contains("Risk") || label.Contains("Score") => $"{d:F1}%",
            double d when label.Contains("Value at Risk") => FormatCurrency(d),
            double d => d.ToString("F2"),
            _ => value.ToString()
        };
        
        var valueColor = value switch
        {
            double d when label.Contains("Risk") => d > 50 ? GetEVEBrush("Negative") : d > 25 ? GetEVEBrush("Warning") : GetEVEBrush("Success"),
            string s when s.Contains("High") => GetEVEBrush("Negative"),
            string s when s.Contains("Medium") => GetEVEBrush("Warning"),
            string s when s.Contains("Low") => GetEVEBrush("Success"),
            _ => GetEVEBrush("Secondary")
        };
        
        var valueElement = new TextBlock
        {
            Text = valueText,
            Width = 150,
            Foreground = valueColor,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    #endregion

    #region Animation Methods

    private void AnimatePortfolioUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var scaleAnimation = new DoubleAnimation
        {
            From = 0.95,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleTransform = new ScaleTransform();
        RenderTransform = scaleTransform;
        RenderTransformOrigin = new Point(0.5, 0.5);
        
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        storyboard.Children.Add(scaleAnimation);
        
        var scaleYAnimation = scaleAnimation.Clone();
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }

    private HoloPortfolioParticle CreatePortfolioParticle()
    {
        var particle = new HoloPortfolioParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.6
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 1.5,
            VelocityY = (_random.NextDouble() - 0.5) * 1.5,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 8 + 4
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    #endregion

    #region Visual Helpers

    private Brush CreateHolographicBrush(double opacity)
    {
        var color = GetEVEColor("Background");
        color.A = (byte)(255 * opacity * HolographicIntensity);
        return new SolidColorBrush(color);
    }

    private Brush GetEVEBrush(string type)
    {
        return new SolidColorBrush(GetEVEColor(type));
    }

    private Color GetEVEColor(string type)
    {
        return type switch
        {
            "Primary" => EVEColorScheme switch
            {
                EVEColorScheme.ElectricBlue => Color.FromRgb(0, 150, 255),
                EVEColorScheme.Gold => Color.FromRgb(255, 215, 0),
                EVEColorScheme.Silver => Color.FromRgb(192, 192, 192),
                _ => Color.FromRgb(0, 150, 255)
            },
            "Secondary" => Color.FromRgb(150, 200, 255),
            "Accent" => Color.FromRgb(255, 200, 0),
            "Background" => Color.FromRgb(0, 30, 60),
            "Success" => Color.FromRgb(0, 255, 100),
            "Warning" => Color.FromRgb(255, 165, 0),
            "Negative" => Color.FromRgb(255, 50, 50),
            "Info" => Color.FromRgb(100, 200, 255),
            _ => Color.FromRgb(255, 255, 255)
        };
    }

    private string FormatCurrency(double value)
    {
        if (Math.Abs(value) >= 1_000_000_000)
            return $"{value / 1_000_000_000:F2}B ISK";
        if (Math.Abs(value) >= 1_000_000)
            return $"{value / 1_000_000:F2}M ISK";
        if (Math.Abs(value) >= 1_000)
            return $"{value / 1_000:F2}K ISK";
        return $"{value:F2} ISK";
    }

    #endregion

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdatePortfolioVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        if (AutoRefresh)
        {
            CalculatePortfolioSummary();
            CreatePortfolioInterface();
        }
    }

    #endregion

    #region Update Methods

    private void UpdatePortfolioVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.8 + 0.2 * Math.Sin(time * 1.5));
        
        foreach (var child in _portfolioCanvas.Children.OfType<FrameworkElement>())
        {
            if (child.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                color.A = (byte)(color.A * intensity);
                child.Background = new SolidColorBrush(color);
            }
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        foreach (var particle in _portfolioParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.1;
            
            if (particle.X < 0 || particle.X > Width || particle.Y < 0 || particle.Y > Height || particle.Life <= 0)
            {
                particle.X = _random.NextDouble() * Width;
                particle.Y = _random.NextDouble() * Height;
                particle.Life = particle.MaxLife;
            }
            
            Canvas.SetLeft(particle.Element, particle.X);
            Canvas.SetTop(particle.Element, particle.Y);
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.6;
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        
        if (EnableAnimations)
            _animationTimer.Start();
        if (EnableParticleEffects)
            _particleTimer.Start();
        if (AutoRefresh)
            _updateTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _updateTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.UpdatePortfolioVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnPortfolioDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CalculatePortfolioSummary();
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnPortfolioSummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.AnimatePortfolioUpdate();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            if ((bool)e.NewValue)
                tracker._animationTimer?.Start();
            else
                tracker._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            if ((bool)e.NewValue)
            {
                tracker.CreateParticleEffects();
                tracker._particleTimer?.Start();
            }
            else
            {
                tracker._particleTimer?.Stop();
                tracker._portfolioParticles.Clear();
            }
        }
    }

    private static void OnShowPerformanceMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnShowAssetBreakdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnShowRiskAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CreatePortfolioInterface();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            if ((bool)e.NewValue)
                tracker._updateTimer?.Start();
            else
                tracker._updateTimer?.Stop();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private static void OnTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortfolioTracker tracker)
        {
            tracker.CalculatePortfolioSummary();
            tracker.CreatePortfolioInterface();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowRiskAnalysis = false;
        ShowPerformanceMetrics = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowRiskAnalysis = true;
        ShowPerformanceMetrics = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloPortfolioAsset
{
    public string ItemName { get; set; }
    public long Quantity { get; set; }
    public double CurrentPrice { get; set; }
    public double AveragePrice { get; set; }
    public string Location { get; set; }
    public string Character { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class HoloPortfolioSummary
{
    public double TotalValue { get; set; }
    public double TotalCost { get; set; }
    public double UnrealizedPnL { get; set; }
    public double TotalReturn { get; set; }
    public int AssetCount { get; set; }
    public double DiversityScore { get; set; }
    public string RiskRating { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class HoloPortfolioPerformance
{
    public double DailyChange { get; set; }
    public double WeeklyChange { get; set; }
    public double MonthlyChange { get; set; }
    public string BestPerformer { get; set; }
    public string WorstPerformer { get; set; }
    public double Volatility { get; set; }
    public double SharpeRatio { get; set; }
}

public class HoloPortfolioRiskAnalysis
{
    public double ConcentrationRisk { get; set; }
    public double LiquidityRisk { get; set; }
    public double MarketRisk { get; set; }
    public double DiversificationScore { get; set; }
    public double ValueAtRisk { get; set; }
    public string RecommendedActions { get; set; }
}

public class HoloPortfolioParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum PortfolioViewMode
{
    Overview,
    Detailed,
    Performance,
    Risk
}

public enum PortfolioTimeFrame
{
    Day,
    Week,
    Month,
    Quarter,
    Year
}

#endregion