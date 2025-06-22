// ==========================================================================
// HoloTradingCalculator.cs - Holographic Trading Calculator Interface
// ==========================================================================
// Advanced trading calculator featuring real-time profit/loss calculations,
// trade optimization, EVE-style trading analysis, and holographic calculation visualization.
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
/// Holographic trading calculator with real-time P&L analysis and trade optimization
/// </summary>
public class HoloTradingCalculator : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTradingCalculator),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTradingCalculator),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TradeCalculationProperty =
        DependencyProperty.Register(nameof(TradeCalculation), typeof(HoloTradeCalculation), typeof(HoloTradingCalculator),
            new PropertyMetadata(null, OnTradeCalculationChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(string), typeof(HoloTradingCalculator),
            new PropertyMetadata("Tritanium", OnSelectedItemChanged));

    public static readonly DependencyProperty CalculationModeProperty =
        DependencyProperty.Register(nameof(CalculationMode), typeof(TradeCalculationMode), typeof(HoloTradingCalculator),
            new PropertyMetadata(TradeCalculationMode.SingleTrade, OnCalculationModeChanged));

    public static readonly DependencyProperty BuyPriceProperty =
        DependencyProperty.Register(nameof(BuyPrice), typeof(double), typeof(HoloTradingCalculator),
            new PropertyMetadata(0.0, OnBuyPriceChanged));

    public static readonly DependencyProperty SellPriceProperty =
        DependencyProperty.Register(nameof(SellPrice), typeof(double), typeof(HoloTradingCalculator),
            new PropertyMetadata(0.0, OnSellPriceChanged));

    public static readonly DependencyProperty QuantityProperty =
        DependencyProperty.Register(nameof(Quantity), typeof(long), typeof(HoloTradingCalculator),
            new PropertyMetadata(1L, OnQuantityChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTaxCalculationsProperty =
        DependencyProperty.Register(nameof(ShowTaxCalculations), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnShowTaxCalculationsChanged));

    public static readonly DependencyProperty ShowBrokerFeesProperty =
        DependencyProperty.Register(nameof(ShowBrokerFees), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnShowBrokerFeesChanged));

    public static readonly DependencyProperty ShowOptimizationProperty =
        DependencyProperty.Register(nameof(ShowOptimization), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnShowOptimizationChanged));

    public static readonly DependencyProperty AutoCalculateProperty =
        DependencyProperty.Register(nameof(AutoCalculate), typeof(bool), typeof(HoloTradingCalculator),
            new PropertyMetadata(true, OnAutoCalculateChanged));

    public static readonly DependencyProperty BrokerFeeProperty =
        DependencyProperty.Register(nameof(BrokerFee), typeof(double), typeof(HoloTradingCalculator),
            new PropertyMetadata(0.03, OnBrokerFeeChanged));

    public static readonly DependencyProperty SalesTaxProperty =
        DependencyProperty.Register(nameof(SalesTax), typeof(double), typeof(HoloTradingCalculator),
            new PropertyMetadata(0.08, OnSalesTaxChanged));

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

    public HoloTradeCalculation TradeCalculation
    {
        get => (HoloTradeCalculation)GetValue(TradeCalculationProperty);
        set => SetValue(TradeCalculationProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public TradeCalculationMode CalculationMode
    {
        get => (TradeCalculationMode)GetValue(CalculationModeProperty);
        set => SetValue(CalculationModeProperty, value);
    }

    public double BuyPrice
    {
        get => (double)GetValue(BuyPriceProperty);
        set => SetValue(BuyPriceProperty, value);
    }

    public double SellPrice
    {
        get => (double)GetValue(SellPriceProperty);
        set => SetValue(SellPriceProperty, value);
    }

    public long Quantity
    {
        get => (long)GetValue(QuantityProperty);
        set => SetValue(QuantityProperty, value);
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

    public bool ShowTaxCalculations
    {
        get => (bool)GetValue(ShowTaxCalculationsProperty);
        set => SetValue(ShowTaxCalculationsProperty, value);
    }

    public bool ShowBrokerFees
    {
        get => (bool)GetValue(ShowBrokerFeesProperty);
        set => SetValue(ShowBrokerFeesProperty, value);
    }

    public bool ShowOptimization
    {
        get => (bool)GetValue(ShowOptimizationProperty);
        set => SetValue(ShowOptimizationProperty, value);
    }

    public bool AutoCalculate
    {
        get => (bool)GetValue(AutoCalculateProperty);
        set => SetValue(AutoCalculateProperty, value);
    }

    public double BrokerFee
    {
        get => (double)GetValue(BrokerFeeProperty);
        set => SetValue(BrokerFeeProperty, value);
    }

    public double SalesTax
    {
        get => (double)GetValue(SalesTaxProperty);
        set => SetValue(SalesTaxProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _calculatorCanvas;
    private ModelVisual3D _calculatorModel;
    private GeometryModel3D _primaryCalculatorGeometry;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _calculationTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloCalculationParticle> _calculationParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloTradingCalculator()
    {
        InitializeComponent();
        InitializeFields();
        InitializeCalculator();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 200));
        
        _calculatorCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _calculatorCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _calculationParticles = new List<HoloCalculationParticle>();
        _isInitialized = false;
    }

    private void InitializeCalculator()
    {
        CreateCalculatorInterface();
        UpdateCalculatorVisuals();
        PerformTradeCalculation();
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

        _calculationTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _calculationTimer.Tick += OnCalculationTick;
    }

    #endregion

    #region Calculator Interface Creation

    private void CreateCalculatorInterface()
    {
        CreateInputSection();
        CreateCalculationSection();
        CreateResultsSection();
        CreateOptimizationSection();
        CreateParticleEffects();
    }

    private void CreateInputSection()
    {
        var inputPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(inputPanel, 20);
        Canvas.SetTop(inputPanel, 20);
        _calculatorCanvas.Children.Add(inputPanel);
        
        AddInputField(inputPanel, "Item:", SelectedItem);
        AddInputField(inputPanel, "Buy Price:", BuyPrice.ToString("N2"));
        AddInputField(inputPanel, "Sell Price:", SellPrice.ToString("N2"));
        AddInputField(inputPanel, "Quantity:", Quantity.ToString("N0"));
        AddInputField(inputPanel, "Broker Fee:", (BrokerFee * 100).ToString("F1") + "%");
        AddInputField(inputPanel, "Sales Tax:", (SalesTax * 100).ToString("F1") + "%");
    }

    private void CreateCalculationSection()
    {
        var calcPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(calcPanel, 280);
        Canvas.SetTop(calcPanel, 20);
        _calculatorCanvas.Children.Add(calcPanel);
        
        AddCalculationDisplay(calcPanel, "Investment:", CalculateInvestment());
        AddCalculationDisplay(calcPanel, "Revenue:", CalculateRevenue());
        AddCalculationDisplay(calcPanel, "Profit/Loss:", CalculateProfit());
        AddCalculationDisplay(calcPanel, "Margin:", CalculateMargin());
        AddCalculationDisplay(calcPanel, "ROI:", CalculateROI());
        AddCalculationDisplay(calcPanel, "Break-even:", CalculateBreakEven());
    }

    private void CreateResultsSection()
    {
        var resultsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.35)
        };
        
        Canvas.SetLeft(resultsPanel, 540);
        Canvas.SetTop(resultsPanel, 20);
        _calculatorCanvas.Children.Add(resultsPanel);
        
        var profitLoss = CalculateProfit();
        var profitColor = profitLoss >= 0 ? GetEVEColor("Positive") : GetEVEColor("Negative");
        
        AddResultDisplay(resultsPanel, "Net Profit:", profitLoss, profitColor);
        AddResultDisplay(resultsPanel, "Total Fees:", CalculateTotalFees(), GetEVEColor("Warning"));
        AddResultDisplay(resultsPanel, "Efficiency:", CalculateEfficiency(), GetEVEColor("Info"));
    }

    private void CreateOptimizationSection()
    {
        if (!ShowOptimization) return;
        
        var optimPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(optimPanel, 20);
        Canvas.SetTop(optimPanel, 320);
        _calculatorCanvas.Children.Add(optimPanel);
        
        var optimization = CalculateOptimization();
        AddOptimizationDisplay(optimPanel, "Optimal Quantity:", optimization.OptimalQuantity);
        AddOptimizationDisplay(optimPanel, "Max Profit:", optimization.MaxProfit);
        AddOptimizationDisplay(optimPanel, "Recommendation:", optimization.Recommendation);
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 50; i++)
        {
            var particle = CreateCalculationParticle();
            _calculationParticles.Add(particle);
            _calculatorCanvas.Children.Add(particle.Element);
        }
    }

    #endregion

    #region Trade Calculations

    private void PerformTradeCalculation()
    {
        var calculation = new HoloTradeCalculation
        {
            ItemName = SelectedItem,
            BuyPrice = BuyPrice,
            SellPrice = SellPrice,
            Quantity = Quantity,
            BrokerFee = BrokerFee,
            SalesTax = SalesTax,
            Investment = CalculateInvestment(),
            Revenue = CalculateRevenue(),
            Profit = CalculateProfit(),
            Margin = CalculateMargin(),
            ROI = CalculateROI(),
            BreakEvenPrice = CalculateBreakEven(),
            TotalFees = CalculateTotalFees(),
            Efficiency = CalculateEfficiency(),
            Timestamp = DateTime.Now
        };
        
        TradeCalculation = calculation;
        
        if (EnableAnimations)
        {
            AnimateCalculationUpdate();
        }
    }

    private double CalculateInvestment()
    {
        var buyPrice = BuyPrice;
        var brokerFee = buyPrice * BrokerFee;
        return (buyPrice + brokerFee) * Quantity;
    }

    private double CalculateRevenue()
    {
        var sellPrice = SellPrice;
        var brokerFee = sellPrice * BrokerFee;
        var salesTax = sellPrice * SalesTax;
        return (sellPrice - brokerFee - salesTax) * Quantity;
    }

    private double CalculateProfit()
    {
        return CalculateRevenue() - CalculateInvestment();
    }

    private double CalculateMargin()
    {
        var revenue = CalculateRevenue();
        return revenue > 0 ? (CalculateProfit() / revenue) * 100 : 0;
    }

    private double CalculateROI()
    {
        var investment = CalculateInvestment();
        return investment > 0 ? (CalculateProfit() / investment) * 100 : 0;
    }

    private double CalculateBreakEven()
    {
        var totalFeeRate = BrokerFee + SalesTax + BrokerFee;
        return BuyPrice * (1 + totalFeeRate) / (1 - totalFeeRate);
    }

    private double CalculateTotalFees()
    {
        var buyFee = BuyPrice * BrokerFee * Quantity;
        var sellFee = SellPrice * BrokerFee * Quantity;
        var tax = SellPrice * SalesTax * Quantity;
        return buyFee + sellFee + tax;
    }

    private double CalculateEfficiency()
    {
        var totalValue = CalculateRevenue() + CalculateInvestment();
        var fees = CalculateTotalFees();
        return totalValue > 0 ? ((totalValue - fees) / totalValue) * 100 : 0;
    }

    private HoloTradeOptimization CalculateOptimization()
    {
        var optimization = new HoloTradeOptimization();
        
        var currentProfit = CalculateProfit();
        var currentQuantity = Quantity;
        
        var maxProfit = currentProfit;
        var optimalQuantity = currentQuantity;
        
        for (long testQuantity = 1; testQuantity <= currentQuantity * 2; testQuantity += Math.Max(1, currentQuantity / 100))
        {
            var originalQuantity = Quantity;
            Quantity = testQuantity;
            
            var testProfit = CalculateProfit();
            if (testProfit > maxProfit)
            {
                maxProfit = testProfit;
                optimalQuantity = testQuantity;
            }
            
            Quantity = originalQuantity;
        }
        
        optimization.OptimalQuantity = optimalQuantity;
        optimization.MaxProfit = maxProfit;
        
        if (optimalQuantity > currentQuantity * 1.1)
        {
            optimization.Recommendation = "Consider increasing quantity";
        }
        else if (optimalQuantity < currentQuantity * 0.9)
        {
            optimization.Recommendation = "Consider decreasing quantity";
        }
        else
        {
            optimization.Recommendation = "Current quantity is optimal";
        }
        
        return optimization;
    }

    #endregion

    #region UI Helpers

    private void AddInputField(Panel parent, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 80,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBox
        {
            Text = value,
            Width = 120,
            Background = CreateHolographicBrush(0.2),
            Foreground = GetEVEBrush("Secondary"),
            BorderBrush = GetEVEBrush("Accent")
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddCalculationDisplay(Panel parent, string label, double value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 100,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBlock
        {
            Text = FormatCurrency(value),
            Width = 120,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddResultDisplay(Panel parent, string label, double value, Brush color)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 100,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBlock
        {
            Text = FormatCurrency(value),
            Width = 120,
            Foreground = color,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddOptimizationDisplay(Panel parent, string label, object value)
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
            long l => l.ToString("N0"),
            double d => FormatCurrency(d),
            _ => value.ToString()
        };
        
        var valueElement = new TextBlock
        {
            Text = valueText,
            Width = 140,
            Foreground = GetEVEBrush("Success"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    #endregion

    #region Animation Methods

    private void AnimateCalculationUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var opacityAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(opacityAnimation, this);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(opacityAnimation);
        
        storyboard.Begin();
    }

    private HoloCalculationParticle CreateCalculationParticle()
    {
        var particle = new HoloCalculationParticle
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
            MaxLife = _random.NextDouble() * 5 + 2
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
            "Positive" => Color.FromRgb(0, 255, 100),
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
        UpdateCalculatorVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnCalculationTick(object sender, EventArgs e)
    {
        if (AutoCalculate)
        {
            PerformTradeCalculation();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateCalculatorVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.8 + 0.2 * Math.Sin(time * 2));
        
        foreach (var child in _calculatorCanvas.Children.OfType<FrameworkElement>())
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
        
        foreach (var particle in _calculationParticles.ToList())
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
        if (AutoCalculate)
            _calculationTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _calculationTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.UpdateCalculatorVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.UpdateCalculatorVisuals();
        }
    }

    private static void OnTradeCalculationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.UpdateCalculatorVisuals();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnCalculationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnBuyPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnSellPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnQuantityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            if ((bool)e.NewValue)
                calculator._animationTimer?.Start();
            else
                calculator._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            if ((bool)e.NewValue)
            {
                calculator.CreateParticleEffects();
                calculator._particleTimer?.Start();
            }
            else
            {
                calculator._particleTimer?.Stop();
                calculator._calculationParticles.Clear();
            }
        }
    }

    private static void OnShowTaxCalculationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.CreateCalculatorInterface();
        }
    }

    private static void OnShowBrokerFeesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.CreateCalculatorInterface();
        }
    }

    private static void OnShowOptimizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            calculator.CreateCalculatorInterface();
        }
    }

    private static void OnAutoCalculateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator)
        {
            if ((bool)e.NewValue)
                calculator._calculationTimer?.Start();
            else
                calculator._calculationTimer?.Stop();
        }
    }

    private static void OnBrokerFeeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    private static void OnSalesTaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingCalculator calculator && calculator.AutoCalculate)
        {
            calculator.PerformTradeCalculation();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowOptimization = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowOptimization = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloTradeCalculation
{
    public string ItemName { get; set; }
    public double BuyPrice { get; set; }
    public double SellPrice { get; set; }
    public long Quantity { get; set; }
    public double BrokerFee { get; set; }
    public double SalesTax { get; set; }
    public double Investment { get; set; }
    public double Revenue { get; set; }
    public double Profit { get; set; }
    public double Margin { get; set; }
    public double ROI { get; set; }
    public double BreakEvenPrice { get; set; }
    public double TotalFees { get; set; }
    public double Efficiency { get; set; }
    public DateTime Timestamp { get; set; }
}

public class HoloTradeOptimization
{
    public long OptimalQuantity { get; set; }
    public double MaxProfit { get; set; }
    public string Recommendation { get; set; }
}

public class HoloCalculationParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum TradeCalculationMode
{
    SingleTrade,
    BulkTrade,
    Arbitrage,
    Optimization
}

public enum EVEColorScheme
{
    ElectricBlue,
    Gold,
    Silver
}

public interface IAnimationIntensityTarget
{
    double HolographicIntensity { get; set; }
}

public interface IAdaptiveControl
{
    void EnableSimplifiedMode();
    void EnableFullMode();
}

#endregion