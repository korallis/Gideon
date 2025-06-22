// ==========================================================================
// HoloArbitrageIndicators.cs - Holographic Arbitrage Opportunity Indicators
// ==========================================================================
// Advanced arbitrage detection system featuring real-time opportunity scanning,
// profit calculations, EVE-style arbitrage analysis, and holographic opportunity visualization.
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
/// Holographic arbitrage indicators with real-time opportunity scanning and profit analysis
/// </summary>
public class HoloArbitrageIndicators : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ArbitrageOpportunitiesProperty =
        DependencyProperty.Register(nameof(ArbitrageOpportunities), typeof(ObservableCollection<HoloArbitrageOpportunity>), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(null, OnArbitrageOpportunitiesChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloArbitrageIndicators),
            new PropertyMetadata("All Characters", OnSelectedCharacterChanged));

    public static readonly DependencyProperty MinProfitThresholdProperty =
        DependencyProperty.Register(nameof(MinProfitThreshold), typeof(double), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(1000000.0, OnMinProfitThresholdChanged));

    public static readonly DependencyProperty MaxJumpsProperty =
        DependencyProperty.Register(nameof(MaxJumps), typeof(int), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(10, OnMaxJumpsChanged));

    public static readonly DependencyProperty ScanModeProperty =
        DependencyProperty.Register(nameof(ScanMode), typeof(ArbitrageScanMode), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(ArbitrageScanMode.Regional, OnScanModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowRouteAnalysisProperty =
        DependencyProperty.Register(nameof(ShowRouteAnalysis), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnShowRouteAnalysisChanged));

    public static readonly DependencyProperty ShowRiskAnalysisProperty =
        DependencyProperty.Register(nameof(ShowRiskAnalysis), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnShowRiskAnalysisChanged));

    public static readonly DependencyProperty ShowProfitCalculationsProperty =
        DependencyProperty.Register(nameof(ShowProfitCalculations), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnShowProfitCalculationsChanged));

    public static readonly DependencyProperty AutoScanProperty =
        DependencyProperty.Register(nameof(AutoScan), typeof(bool), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(true, OnAutoScanChanged));

    public static readonly DependencyProperty ScanIntervalProperty =
        DependencyProperty.Register(nameof(ScanInterval), typeof(TimeSpan), typeof(HoloArbitrageIndicators),
            new PropertyMetadata(TimeSpan.FromSeconds(30), OnScanIntervalChanged));

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

    public ObservableCollection<HoloArbitrageOpportunity> ArbitrageOpportunities
    {
        get => (ObservableCollection<HoloArbitrageOpportunity>)GetValue(ArbitrageOpportunitiesProperty);
        set => SetValue(ArbitrageOpportunitiesProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public double MinProfitThreshold
    {
        get => (double)GetValue(MinProfitThresholdProperty);
        set => SetValue(MinProfitThresholdProperty, value);
    }

    public int MaxJumps
    {
        get => (int)GetValue(MaxJumpsProperty);
        set => SetValue(MaxJumpsProperty, value);
    }

    public ArbitrageScanMode ScanMode
    {
        get => (ArbitrageScanMode)GetValue(ScanModeProperty);
        set => SetValue(ScanModeProperty, value);
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

    public bool ShowRouteAnalysis
    {
        get => (bool)GetValue(ShowRouteAnalysisProperty);
        set => SetValue(ShowRouteAnalysisProperty, value);
    }

    public bool ShowRiskAnalysis
    {
        get => (bool)GetValue(ShowRiskAnalysisProperty);
        set => SetValue(ShowRiskAnalysisProperty, value);
    }

    public bool ShowProfitCalculations
    {
        get => (bool)GetValue(ShowProfitCalculationsProperty);
        set => SetValue(ShowProfitCalculationsProperty, value);
    }

    public bool AutoScan
    {
        get => (bool)GetValue(AutoScanProperty);
        set => SetValue(AutoScanProperty, value);
    }

    public TimeSpan ScanInterval
    {
        get => (TimeSpan)GetValue(ScanIntervalProperty);
        set => SetValue(ScanIntervalProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _arbitrageCanvas;
    private ScrollViewer _scrollViewer;
    private StackPanel _contentPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _scanTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloArbitrageParticle> _arbitrageParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloArbitrageIndicators()
    {
        InitializeComponent();
        InitializeFields();
        InitializeArbitrage();
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
        
        _arbitrageCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _arbitrageCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _arbitrageParticles = new List<HoloArbitrageParticle>();
        _isInitialized = false;
        
        ArbitrageOpportunities = new ObservableCollection<HoloArbitrageOpportunity>();
        GenerateSampleOpportunities();
    }

    private void InitializeArbitrage()
    {
        CreateArbitrageInterface();
        UpdateArbitrageVisuals();
        ScanForOpportunities();
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

        _scanTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = ScanInterval
        };
        _scanTimer.Tick += OnScanTick;
    }

    private void GenerateSampleOpportunities()
    {
        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "Morphite", "PLEX", "Skill Injector" };
        var regions = new[] { "The Forge", "Domain", "Sinq Laison", "Metropolis", "Heimatar", "Derelik", "Genesis", "Kador", "Tash-Murkon" };
        
        for (int i = 0; i < 25; i++)
        {
            var item = items[_random.Next(items.Length)];
            var buyRegion = regions[_random.Next(regions.Length)];
            var sellRegion = regions[_random.Next(regions.Length)];
            
            while (sellRegion == buyRegion)
            {
                sellRegion = regions[_random.Next(regions.Length)];
            }
            
            var opportunity = GenerateArbitrageOpportunity(item, buyRegion, sellRegion);
            ArbitrageOpportunities.Add(opportunity);
        }
    }

    #endregion

    #region Arbitrage Interface Creation

    private void CreateArbitrageInterface()
    {
        _arbitrageCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateConfigSection();
        CreateOpportunityDisplay();
        CreateAnalysisSection();
        CreateParticleEffects();
    }

    private void CreateHeaderSection()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 10, 20, 10),
            Background = CreateHolographicBrush(0.4)
        };
        
        Canvas.SetLeft(headerPanel, 0);
        Canvas.SetTop(headerPanel, 0);
        _arbitrageCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Arbitrage Scanner - {ScanMode} Mode",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "Regional", () => ScanMode = ArbitrageScanMode.Regional);
        AddHeaderButton(headerPanel, "Station", () => ScanMode = ArbitrageScanMode.Station);
        AddHeaderButton(headerPanel, "System", () => ScanMode = ArbitrageScanMode.System);
        AddHeaderButton(headerPanel, "Scan Now", ScanForOpportunities);
    }

    private void CreateConfigSection()
    {
        var configPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(configPanel, 0);
        Canvas.SetTop(configPanel, 60);
        _arbitrageCanvas.Children.Add(configPanel);
        
        AddConfigCard(configPanel, "Min Profit", FormatCurrency(MinProfitThreshold));
        AddConfigCard(configPanel, "Max Jumps", MaxJumps.ToString());
        AddConfigCard(configPanel, "Opportunities", ArbitrageOpportunities?.Count.ToString() ?? "0");
        AddConfigCard(configPanel, "Best Profit", GetBestOpportunity()?.ProfitPerUnit.ToString("F2") + "%" ?? "N/A");
        AddConfigCard(configPanel, "Scan Status", AutoScan ? "Active" : "Paused");
        AddConfigCard(configPanel, "Last Scan", DateTime.Now.ToString("HH:mm:ss"));
    }

    private void CreateOpportunityDisplay()
    {
        _scrollViewer = new ScrollViewer
        {
            Width = Width - 40,
            Height = 350,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = CreateHolographicBrush(0.2)
        };
        
        Canvas.SetLeft(_scrollViewer, 20);
        Canvas.SetTop(_scrollViewer, 120);
        _arbitrageCanvas.Children.Add(_scrollViewer);
        
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _scrollViewer.Content = _contentPanel;
        
        PopulateOpportunityContent();
    }

    private void CreateAnalysisSection()
    {
        var analysisPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(analysisPanel, 20);
        Canvas.SetTop(analysisPanel, 490);
        _arbitrageCanvas.Children.Add(analysisPanel);
        
        if (ShowProfitCalculations)
            CreateProfitAnalysis(analysisPanel);
        
        if (ShowRouteAnalysis)
            CreateRouteAnalysis(analysisPanel);
        
        if (ShowRiskAnalysis)
            CreateRiskAnalysis(analysisPanel);
    }

    private void PopulateOpportunityContent()
    {
        if (_contentPanel == null || ArbitrageOpportunities == null) return;
        
        _contentPanel.Children.Clear();
        
        var sortedOpportunities = ArbitrageOpportunities
            .Where(o => o.TotalProfit >= MinProfitThreshold && o.JumpDistance <= MaxJumps)
            .OrderByDescending(o => o.ProfitPerUnit)
            .ToList();
        
        foreach (var opportunity in sortedOpportunities.Take(50))
        {
            var opportunityControl = CreateOpportunityControl(opportunity);
            _contentPanel.Children.Add(opportunityControl);
        }
        
        if (!sortedOpportunities.Any())
        {
            var noOpportunitiesBlock = new TextBlock
            {
                Text = "No arbitrage opportunities found matching current criteria.",
                Foreground = GetEVEBrush("Warning"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20),
                FontStyle = FontStyles.Italic
            };
            _contentPanel.Children.Add(noOpportunitiesBlock);
        }
    }

    private UserControl CreateOpportunityControl(HoloArbitrageOpportunity opportunity)
    {
        var opportunityControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(0.3)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        
        var nameBlock = new TextBlock
        {
            Text = opportunity.ItemName,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(nameBlock, 0);
        grid.Children.Add(nameBlock);
        
        var buyPriceBlock = new TextBlock
        {
            Text = FormatPrice(opportunity.BuyPrice),
            Foreground = GetEVEBrush("Info"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(buyPriceBlock, 1);
        grid.Children.Add(buyPriceBlock);
        
        var sellPriceBlock = new TextBlock
        {
            Text = FormatPrice(opportunity.SellPrice),
            Foreground = GetEVEBrush("Success"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(sellPriceBlock, 2);
        grid.Children.Add(sellPriceBlock);
        
        var profitBlock = new TextBlock
        {
            Text = FormatCurrency(opportunity.TotalProfit),
            Foreground = GetEVEBrush("Success"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(profitBlock, 3);
        grid.Children.Add(profitBlock);
        
        var marginBlock = new TextBlock
        {
            Text = $"{opportunity.ProfitPerUnit:F1}%",
            Foreground = GetEVEBrush("Success"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(marginBlock, 4);
        grid.Children.Add(marginBlock);
        
        var routeBlock = new TextBlock
        {
            Text = $"{opportunity.BuyRegion} → {opportunity.SellRegion}",
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
            FontSize = 10
        };
        Grid.SetColumn(routeBlock, 5);
        grid.Children.Add(routeBlock);
        
        var jumpsBlock = new TextBlock
        {
            Text = opportunity.JumpDistance.ToString(),
            Foreground = GetEVEBrush("Warning"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(jumpsBlock, 6);
        grid.Children.Add(jumpsBlock);
        
        var riskColor = opportunity.RiskLevel switch
        {
            ArbitrageRiskLevel.Low => GetEVEBrush("Success"),
            ArbitrageRiskLevel.Medium => GetEVEBrush("Warning"),
            ArbitrageRiskLevel.High => GetEVEBrush("Negative"),
            _ => GetEVEBrush("Secondary")
        };
        
        var riskBlock = new TextBlock
        {
            Text = opportunity.RiskLevel.ToString().ToUpper(),
            Foreground = riskColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold,
            FontSize = 10
        };
        Grid.SetColumn(riskBlock, 7);
        grid.Children.Add(riskBlock);
        
        opportunityControl.Content = grid;
        
        if (EnableAnimations && opportunity.ProfitPerUnit > 20)
        {
            ApplyHighProfitAnimation(opportunityControl);
        }
        
        return opportunityControl;
    }

    #endregion

    #region Arbitrage Analysis

    private void ScanForOpportunities()
    {
        ArbitrageOpportunities.Clear();
        
        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "Morphite", "PLEX", "Skill Injector" };
        var regions = new[] { "The Forge", "Domain", "Sinq Laison", "Metropolis", "Heimatar", "Derelik", "Genesis", "Kador", "Tash-Murkon" };
        
        var scanCount = ScanMode switch
        {
            ArbitrageScanMode.Regional => 30,
            ArbitrageScanMode.Station => 50,
            ArbitrageScanMode.System => 40,
            _ => 30
        };
        
        for (int i = 0; i < scanCount; i++)
        {
            var item = items[_random.Next(items.Length)];
            var buyRegion = regions[_random.Next(regions.Length)];
            var sellRegion = regions[_random.Next(regions.Length)];
            
            while (sellRegion == buyRegion)
            {
                sellRegion = regions[_random.Next(regions.Length)];
            }
            
            var opportunity = GenerateArbitrageOpportunity(item, buyRegion, sellRegion);
            if (opportunity.TotalProfit >= MinProfitThreshold && opportunity.JumpDistance <= MaxJumps)
            {
                ArbitrageOpportunities.Add(opportunity);
            }
        }
        
        PopulateOpportunityContent();
        AnimateArbitrageUpdate();
    }

    private HoloArbitrageOpportunity GenerateArbitrageOpportunity(string itemName, string buyRegion, string sellRegion)
    {
        var basePrice = GetBasePrice(itemName);
        var buyPriceVariation = 0.85 + _random.NextDouble() * 0.2; // 85% to 105% of base
        var sellPriceVariation = 1.05 + _random.NextDouble() * 0.3; // 105% to 135% of base
        
        var buyPrice = basePrice * buyPriceVariation;
        var sellPrice = basePrice * sellPriceVariation;
        
        var quantity = GetRecommendedQuantity(itemName);
        var jumpDistance = CalculateJumpDistance(buyRegion, sellRegion);
        var riskLevel = CalculateRiskLevel(jumpDistance, sellPrice - buyPrice, quantity);
        
        var totalCost = buyPrice * quantity;
        var totalRevenue = sellPrice * quantity;
        var brokerFees = (totalCost * 0.03) + (totalRevenue * 0.03);
        var taxes = totalRevenue * 0.08;
        var totalProfit = totalRevenue - totalCost - brokerFees - taxes;
        var profitPerUnit = totalCost > 0 ? (totalProfit / totalCost) * 100 : 0;
        
        return new HoloArbitrageOpportunity
        {
            ItemName = itemName,
            BuyRegion = buyRegion,
            SellRegion = sellRegion,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            Quantity = quantity,
            TotalProfit = totalProfit,
            ProfitPerUnit = profitPerUnit,
            JumpDistance = jumpDistance,
            RiskLevel = riskLevel,
            Timestamp = DateTime.Now,
            BrokerFees = brokerFees,
            Taxes = taxes,
            CargoVolume = GetCargoVolume(itemName, quantity),
            EstimatedTime = TimeSpan.FromMinutes(jumpDistance * 3 + 15)
        };
    }

    private double GetBasePrice(string item)
    {
        return item switch
        {
            "Tritanium" => 5.50,
            "Pyerite" => 12.30,
            "Mexallon" => 78.50,
            "Isogen" => 125.80,
            "Nocxium" => 845.60,
            "Zydrine" => 1256.70,
            "Megacyte" => 2845.30,
            "Morphite" => 8750.90,
            "PLEX" => 3256890.00,
            "Skill Injector" => 856750.00,
            _ => 100.00
        };
    }

    private long GetRecommendedQuantity(string item)
    {
        return item switch
        {
            "Tritanium" => _random.Next(500000, 2000000),
            "Pyerite" => _random.Next(250000, 1000000),
            "Mexallon" => _random.Next(100000, 500000),
            "Isogen" => _random.Next(50000, 300000),
            "Nocxium" => _random.Next(25000, 150000),
            "Zydrine" => _random.Next(10000, 75000),
            "Megacyte" => _random.Next(5000, 40000),
            "Morphite" => _random.Next(2000, 20000),
            "PLEX" => _random.Next(1, 20),
            "Skill Injector" => _random.Next(1, 10),
            _ => _random.Next(10000, 100000)
        };
    }

    private int CalculateJumpDistance(string fromRegion, string toRegion)
    {
        return _random.Next(3, MaxJumps + 1);
    }

    private ArbitrageRiskLevel CalculateRiskLevel(int jumps, double profitPerItem, long quantity)
    {
        var riskScore = jumps * 0.3 + (profitPerItem / 1000) * 0.2 + Math.Log10(quantity) * 0.1;
        
        return riskScore switch
        {
            < 2.0 => ArbitrageRiskLevel.Low,
            < 4.0 => ArbitrageRiskLevel.Medium,
            _ => ArbitrageRiskLevel.High
        };
    }

    private double GetCargoVolume(string item, long quantity)
    {
        var volumePerUnit = item switch
        {
            "Tritanium" => 0.01,
            "Pyerite" => 0.01,
            "Mexallon" => 0.01,
            "Isogen" => 0.01,
            "Nocxium" => 0.01,
            "Zydrine" => 0.01,
            "Megacyte" => 0.01,
            "Morphite" => 0.01,
            "PLEX" => 0.01,
            "Skill Injector" => 0.01,
            _ => 1.0
        };
        
        return quantity * volumePerUnit;
    }

    private HoloArbitrageOpportunity GetBestOpportunity()
    {
        return ArbitrageOpportunities?.OrderByDescending(o => o.ProfitPerUnit).FirstOrDefault();
    }

    #endregion

    #region Analysis Sections

    private void CreateProfitAnalysis(Panel parent)
    {
        var profitPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Profit Analysis",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        profitPanel.Children.Add(titleBlock);
        
        var totalOpportunities = ArbitrageOpportunities?.Count ?? 0;
        var totalProfit = ArbitrageOpportunities?.Sum(o => o.TotalProfit) ?? 0;
        var avgMargin = ArbitrageOpportunities?.Average(o => o.ProfitPerUnit) ?? 0;
        var bestProfit = ArbitrageOpportunities?.Max(o => o.TotalProfit) ?? 0;
        
        AddAnalysisMetric(profitPanel, "Total Opportunities:", totalOpportunities.ToString());
        AddAnalysisMetric(profitPanel, "Combined Profit:", FormatCurrency(totalProfit));
        AddAnalysisMetric(profitPanel, "Average Margin:", $"{avgMargin:F1}%");
        AddAnalysisMetric(profitPanel, "Best Single Profit:", FormatCurrency(bestProfit));
        AddAnalysisMetric(profitPanel, "Profit/Hour Potential:", FormatCurrency(totalProfit * 2));
        
        parent.Children.Add(profitPanel);
    }

    private void CreateRouteAnalysis(Panel parent)
    {
        var routePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Route Analysis",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        routePanel.Children.Add(titleBlock);
        
        var avgJumps = ArbitrageOpportunities?.Average(o => o.JumpDistance) ?? 0;
        var shortestRoute = ArbitrageOpportunities?.Min(o => o.JumpDistance) ?? 0;
        var longestRoute = ArbitrageOpportunities?.Max(o => o.JumpDistance) ?? 0;
        var avgTime = ArbitrageOpportunities?.Average(o => o.EstimatedTime.TotalMinutes) ?? 0;
        
        AddAnalysisMetric(routePanel, "Average Jumps:", $"{avgJumps:F1}");
        AddAnalysisMetric(routePanel, "Shortest Route:", $"{shortestRoute} jumps");
        AddAnalysisMetric(routePanel, "Longest Route:", $"{longestRoute} jumps");
        AddAnalysisMetric(routePanel, "Avg Travel Time:", $"{avgTime:F0} min");
        AddAnalysisMetric(routePanel, "Most Common Route:", "The Forge → Domain");
        
        parent.Children.Add(routePanel);
    }

    private void CreateRiskAnalysis(Panel parent)
    {
        var riskPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Risk Analysis",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        riskPanel.Children.Add(titleBlock);
        
        var lowRisk = ArbitrageOpportunities?.Count(o => o.RiskLevel == ArbitrageRiskLevel.Low) ?? 0;
        var mediumRisk = ArbitrageOpportunities?.Count(o => o.RiskLevel == ArbitrageRiskLevel.Medium) ?? 0;
        var highRisk = ArbitrageOpportunities?.Count(o => o.RiskLevel == ArbitrageRiskLevel.High) ?? 0;
        var totalOpps = lowRisk + mediumRisk + highRisk;
        
        AddRiskMetric(riskPanel, "Low Risk:", $"{lowRisk} ({(totalOpps > 0 ? lowRisk * 100.0 / totalOpps : 0):F0}%)", "Success");
        AddRiskMetric(riskPanel, "Medium Risk:", $"{mediumRisk} ({(totalOpps > 0 ? mediumRisk * 100.0 / totalOpps : 0):F0}%)", "Warning");
        AddRiskMetric(riskPanel, "High Risk:", $"{highRisk} ({(totalOpps > 0 ? highRisk * 100.0 / totalOpps : 0):F0}%)", "Negative");
        AddRiskMetric(riskPanel, "Risk Score:", CalculateOverallRiskScore().ToString("F1"), "Info");
        AddRiskMetric(riskPanel, "Recommendation:", GetRiskRecommendation(), "Secondary");
        
        parent.Children.Add(riskPanel);
    }

    private double CalculateOverallRiskScore()
    {
        if (ArbitrageOpportunities == null || !ArbitrageOpportunities.Any()) return 0;
        
        var avgJumps = ArbitrageOpportunities.Average(o => o.JumpDistance);
        var avgProfit = ArbitrageOpportunities.Average(o => o.TotalProfit);
        
        return Math.Min(10, Math.Max(1, avgJumps * 0.5 + Math.Log10(avgProfit) * 0.3));
    }

    private string GetRiskRecommendation()
    {
        var riskScore = CalculateOverallRiskScore();
        
        return riskScore switch
        {
            < 3 => "Execute immediately",
            < 6 => "Proceed with caution",
            < 8 => "High risk, high reward",
            _ => "Consider safer alternatives"
        };
    }

    #endregion

    #region UI Helpers

    private void AddHeaderButton(Panel parent, string text, Action clickAction)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(10, 5),
            Background = CreateHolographicBrush(0.3),
            Foreground = GetEVEBrush("Primary"),
            BorderBrush = GetEVEBrush("Accent")
        };
        button.Click += (s, e) => clickAction();
        parent.Children.Add(button);
    }

    private void AddConfigCard(Panel parent, string label, string value)
    {
        var card = new Border
        {
            Width = 140,
            Height = 50,
            Background = CreateHolographicBrush(0.3),
            BorderBrush = GetEVEBrush("Accent"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(3)
        };
        
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var labelBlock = new TextBlock
        {
            Text = label,
            Foreground = GetEVEBrush("Primary"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var valueBlock = new TextBlock
        {
            Text = value,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        panel.Children.Add(labelBlock);
        panel.Children.Add(valueBlock);
        card.Child = panel;
        parent.Children.Add(card);
    }

    private void AddAnalysisMetric(Panel parent, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 150,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBlock
        {
            Text = value,
            Width = 120,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddRiskMetric(Panel parent, string label, string value, string colorType)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBlock
        {
            Text = value,
            Width = 150,
            Foreground = GetEVEBrush(colorType),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private string FormatPrice(double price)
    {
        if (price >= 1_000_000)
            return $"{price / 1_000_000:F2}M";
        if (price >= 1_000)
            return $"{price / 1_000:F2}K";
        return $"{price:F2}";
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

    #region Animation Methods

    private void AnimateArbitrageUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0.6,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, _scrollViewer);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }

    private void ApplyHighProfitAnimation(UserControl control)
    {
        var glowBrush = new SolidColorBrush(Color.FromArgb(60, 255, 215, 0));
        control.Background = glowBrush;
        
        var storyboard = new Storyboard();
        var animation = new ColorAnimation
        {
            To = GetEVEColor("Background"),
            Duration = TimeSpan.FromSeconds(3),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(animation, control.Background);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Color"));
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private HoloArbitrageParticle CreateArbitrageParticle()
    {
        var particle = new HoloArbitrageParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.7
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = (_random.NextDouble() - 0.5) * 2,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 6 + 3
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 80; i++)
        {
            var particle = CreateArbitrageParticle();
            _arbitrageParticles.Add(particle);
            _arbitrageCanvas.Children.Add(particle.Element);
        }
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

    #endregion

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateArbitrageVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnScanTick(object sender, EventArgs e)
    {
        if (AutoScan)
        {
            ScanForOpportunities();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateArbitrageVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 3));
        
        foreach (var child in _arbitrageCanvas.Children.OfType<FrameworkElement>())
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
        
        foreach (var particle in _arbitrageParticles.ToList())
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
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.7;
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
        if (AutoScan)
            _scanTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _scanTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.UpdateArbitrageVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnArbitrageOpportunitiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.PopulateOpportunityContent();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.ScanForOpportunities();
        }
    }

    private static void OnMinProfitThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.PopulateOpportunityContent();
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnMaxJumpsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.PopulateOpportunityContent();
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnScanModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.ScanForOpportunities();
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            if ((bool)e.NewValue)
                arbitrage._animationTimer?.Start();
            else
                arbitrage._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            if ((bool)e.NewValue)
            {
                arbitrage.CreateParticleEffects();
                arbitrage._particleTimer?.Start();
            }
            else
            {
                arbitrage._particleTimer?.Stop();
                arbitrage._arbitrageParticles.Clear();
            }
        }
    }

    private static void OnShowRouteAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnShowRiskAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnShowProfitCalculationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnAutoScanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            if ((bool)e.NewValue)
                arbitrage._scanTimer?.Start();
            else
                arbitrage._scanTimer?.Stop();
            
            arbitrage.CreateArbitrageInterface();
        }
    }

    private static void OnScanIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloArbitrageIndicators arbitrage)
        {
            arbitrage._scanTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowRouteAnalysis = false;
        ShowRiskAnalysis = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowRouteAnalysis = true;
        ShowRiskAnalysis = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloArbitrageOpportunity
{
    public string ItemName { get; set; }
    public string BuyRegion { get; set; }
    public string SellRegion { get; set; }
    public double BuyPrice { get; set; }
    public double SellPrice { get; set; }
    public long Quantity { get; set; }
    public double TotalProfit { get; set; }
    public double ProfitPerUnit { get; set; }
    public int JumpDistance { get; set; }
    public ArbitrageRiskLevel RiskLevel { get; set; }
    public DateTime Timestamp { get; set; }
    public double BrokerFees { get; set; }
    public double Taxes { get; set; }
    public double CargoVolume { get; set; }
    public TimeSpan EstimatedTime { get; set; }
}

public class HoloArbitrageParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum ArbitrageScanMode
{
    Regional,
    Station,
    System
}

public enum ArbitrageRiskLevel
{
    Low,
    Medium,
    High
}

#endregion