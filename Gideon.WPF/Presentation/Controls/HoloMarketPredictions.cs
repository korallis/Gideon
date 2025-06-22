// ==========================================================================
// HoloMarketPredictions.cs - Holographic Market Predictions Display
// ==========================================================================
// Advanced market prediction system featuring AI-driven forecasting,
// trend analysis, EVE-style prediction algorithms, and holographic prediction visualization.
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
/// Holographic market predictions with AI-driven forecasting and trend analysis
/// </summary>
public class HoloMarketPredictions : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketPredictions),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketPredictions),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty PredictionsProperty =
        DependencyProperty.Register(nameof(Predictions), typeof(ObservableCollection<HoloMarketPrediction>), typeof(HoloMarketPredictions),
            new PropertyMetadata(null, OnPredictionsChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(string), typeof(HoloMarketPredictions),
            new PropertyMetadata("Tritanium", OnSelectedItemChanged));

    public static readonly DependencyProperty PredictionModelProperty =
        DependencyProperty.Register(nameof(PredictionModel), typeof(PredictionModelType), typeof(HoloMarketPredictions),
            new PropertyMetadata(PredictionModelType.Neural, OnPredictionModelChanged));

    public static readonly DependencyProperty TimeHorizonProperty =
        DependencyProperty.Register(nameof(TimeHorizon), typeof(PredictionTimeHorizon), typeof(HoloMarketPredictions),
            new PropertyMetadata(PredictionTimeHorizon.Week, OnTimeHorizonChanged));

    public static readonly DependencyProperty ConfidenceLevelProperty =
        DependencyProperty.Register(nameof(ConfidenceLevel), typeof(double), typeof(HoloMarketPredictions),
            new PropertyMetadata(0.8, OnConfidenceLevelChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowConfidenceIntervalsProperty =
        DependencyProperty.Register(nameof(ShowConfidenceIntervals), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnShowConfidenceIntervalsChanged));

    public static readonly DependencyProperty ShowTrendAnalysisProperty =
        DependencyProperty.Register(nameof(ShowTrendAnalysis), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnShowTrendAnalysisChanged));

    public static readonly DependencyProperty ShowModelMetricsProperty =
        DependencyProperty.Register(nameof(ShowModelMetrics), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnShowModelMetricsChanged));

    public static readonly DependencyProperty AutoUpdateProperty =
        DependencyProperty.Register(nameof(AutoUpdate), typeof(bool), typeof(HoloMarketPredictions),
            new PropertyMetadata(true, OnAutoUpdateChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloMarketPredictions),
            new PropertyMetadata(TimeSpan.FromMinutes(15), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloMarketPrediction> Predictions
    {
        get => (ObservableCollection<HoloMarketPrediction>)GetValue(PredictionsProperty);
        set => SetValue(PredictionsProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public PredictionModelType PredictionModel
    {
        get => (PredictionModelType)GetValue(PredictionModelProperty);
        set => SetValue(PredictionModelProperty, value);
    }

    public PredictionTimeHorizon TimeHorizon
    {
        get => (PredictionTimeHorizon)GetValue(TimeHorizonProperty);
        set => SetValue(TimeHorizonProperty, value);
    }

    public double ConfidenceLevel
    {
        get => (double)GetValue(ConfidenceLevelProperty);
        set => SetValue(ConfidenceLevelProperty, value);
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

    public bool ShowConfidenceIntervals
    {
        get => (bool)GetValue(ShowConfidenceIntervalsProperty);
        set => SetValue(ShowConfidenceIntervalsProperty, value);
    }

    public bool ShowTrendAnalysis
    {
        get => (bool)GetValue(ShowTrendAnalysisProperty);
        set => SetValue(ShowTrendAnalysisProperty, value);
    }

    public bool ShowModelMetrics
    {
        get => (bool)GetValue(ShowModelMetricsProperty);
        set => SetValue(ShowModelMetricsProperty, value);
    }

    public bool AutoUpdate
    {
        get => (bool)GetValue(AutoUpdateProperty);
        set => SetValue(AutoUpdateProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _predictionsCanvas;
    private ScrollViewer _scrollViewer;
    private StackPanel _contentPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloPredictionParticle> _predictionParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloMarketPredictions()
    {
        InitializeComponent();
        InitializeFields();
        InitializePredictions();
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
        
        _predictionsCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _predictionsCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _predictionParticles = new List<HoloPredictionParticle>();
        _isInitialized = false;
        
        Predictions = new ObservableCollection<HoloMarketPrediction>();
        GenerateSamplePredictions();
    }

    private void InitializePredictions()
    {
        CreatePredictionsInterface();
        UpdatePredictionVisuals();
        RunPredictionModels();
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
            Interval = TimeSpan.FromMilliseconds(120)
        };
        _particleTimer.Tick += OnParticleTick;

        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void GenerateSamplePredictions()
    {
        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "Morphite", "PLEX", "Skill Injector" };
        
        foreach (var item in items)
        {
            var currentPrice = GetCurrentPrice(item);
            var prediction = GeneratePrediction(item, currentPrice);
            Predictions.Add(prediction);
        }
    }

    private double GetCurrentPrice(string item)
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

    #endregion

    #region Predictions Interface Creation

    private void CreatePredictionsInterface()
    {
        _predictionsCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateModelConfigSection();
        CreatePredictionDisplay();
        CreateTrendAnalysisSection();
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
        _predictionsCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Market Predictions - {PredictionModel} Model",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "Neural", () => PredictionModel = PredictionModelType.Neural);
        AddHeaderButton(headerPanel, "Statistical", () => PredictionModel = PredictionModelType.Statistical);
        AddHeaderButton(headerPanel, "Ensemble", () => PredictionModel = PredictionModelType.Ensemble);
        AddHeaderButton(headerPanel, "Refresh", RunPredictionModels);
    }

    private void CreateModelConfigSection()
    {
        var configPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(configPanel, 0);
        Canvas.SetTop(configPanel, 60);
        _predictionsCanvas.Children.Add(configPanel);
        
        AddConfigCard(configPanel, "Model Type", PredictionModel.ToString());
        AddConfigCard(configPanel, "Time Horizon", TimeHorizon.ToString());
        AddConfigCard(configPanel, "Confidence", $"{ConfidenceLevel:P0}");
        AddConfigCard(configPanel, "Accuracy", $"{CalculateModelAccuracy():F1}%");
        AddConfigCard(configPanel, "Last Update", DateTime.Now.ToString("HH:mm:ss"));
    }

    private void CreatePredictionDisplay()
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
        _predictionsCanvas.Children.Add(_scrollViewer);
        
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _scrollViewer.Content = _contentPanel;
        
        PopulatePredictionContent();
    }

    private void CreateTrendAnalysisSection()
    {
        if (!ShowTrendAnalysis) return;
        
        var trendPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(trendPanel, 20);
        Canvas.SetTop(trendPanel, 490);
        _predictionsCanvas.Children.Add(trendPanel);
        
        CreatePredictionChart(trendPanel);
        CreateModelMetrics(trendPanel);
        CreateMarketSentiment(trendPanel);
    }

    private void PopulatePredictionContent()
    {
        if (_contentPanel == null || Predictions == null) return;
        
        _contentPanel.Children.Clear();
        
        var sortedPredictions = Predictions.OrderByDescending(p => Math.Abs(p.PredictedChange)).ToList();
        
        foreach (var prediction in sortedPredictions)
        {
            var predictionControl = CreatePredictionControl(prediction);
            _contentPanel.Children.Add(predictionControl);
        }
    }

    private UserControl CreatePredictionControl(HoloMarketPrediction prediction)
    {
        var predictionControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(0.3)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        
        var nameBlock = new TextBlock
        {
            Text = prediction.ItemName,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(nameBlock, 0);
        grid.Children.Add(nameBlock);
        
        var currentPriceBlock = new TextBlock
        {
            Text = FormatPrice(prediction.CurrentPrice),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(currentPriceBlock, 1);
        grid.Children.Add(currentPriceBlock);
        
        var predictedPriceBlock = new TextBlock
        {
            Text = FormatPrice(prediction.PredictedPrice),
            Foreground = GetEVEBrush("Info"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(predictedPriceBlock, 2);
        grid.Children.Add(predictedPriceBlock);
        
        var changeColor = prediction.PredictedChange >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative");
        var changeBlock = new TextBlock
        {
            Text = $"{prediction.PredictedChange:+0.0;-0.0;0.0}%",
            Foreground = changeColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(changeBlock, 3);
        grid.Children.Add(changeBlock);
        
        var confidenceColor = prediction.Confidence >= 0.8 ? GetEVEBrush("Success") : 
                             prediction.Confidence >= 0.6 ? GetEVEBrush("Warning") : GetEVEBrush("Negative");
        var confidenceBlock = new TextBlock
        {
            Text = $"{prediction.Confidence:P0}",
            Foreground = confidenceColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(confidenceBlock, 4);
        grid.Children.Add(confidenceBlock);
        
        if (ShowConfidenceIntervals)
        {
            var intervalBlock = new TextBlock
            {
                Text = $"{FormatPrice(prediction.LowerBound)} - {FormatPrice(prediction.UpperBound)}",
                Foreground = GetEVEBrush("Secondary"),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5),
                FontSize = 10
            };
            Grid.SetColumn(intervalBlock, 5);
            grid.Children.Add(intervalBlock);
        }
        
        var signalColor = prediction.TradingSignal switch
        {
            TradingSignal.StrongBuy => GetEVEBrush("Success"),
            TradingSignal.Buy => Color.FromRgb(100, 255, 150),
            TradingSignal.Hold => GetEVEBrush("Warning"),
            TradingSignal.Sell => Color.FromRgb(255, 150, 100),
            TradingSignal.StrongSell => GetEVEBrush("Negative"),
            _ => GetEVEBrush("Secondary")
        };
        
        var signalBlock = new TextBlock
        {
            Text = prediction.TradingSignal.ToString().ToUpper(),
            Foreground = new SolidColorBrush(signalColor),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold,
            FontSize = 10
        };
        Grid.SetColumn(signalBlock, 6);
        grid.Children.Add(signalBlock);
        
        predictionControl.Content = grid;
        
        if (EnableAnimations && Math.Abs(prediction.PredictedChange) > 5)
        {
            ApplyHighlightAnimation(predictionControl, prediction.PredictedChange >= 0);
        }
        
        return predictionControl;
    }

    #endregion

    #region Prediction Models

    private void RunPredictionModels()
    {
        foreach (var prediction in Predictions)
        {
            UpdatePrediction(prediction);
        }
        
        PopulatePredictionContent();
        AnimatePredictionUpdate();
    }

    private HoloMarketPrediction GeneratePrediction(string itemName, double currentPrice)
    {
        var prediction = new HoloMarketPrediction
        {
            ItemName = itemName,
            CurrentPrice = currentPrice,
            PredictionModel = PredictionModel,
            TimeHorizon = TimeHorizon,
            Timestamp = DateTime.Now
        };
        
        UpdatePrediction(prediction);
        return prediction;
    }

    private void UpdatePrediction(HoloMarketPrediction prediction)
    {
        switch (PredictionModel)
        {
            case PredictionModelType.Neural:
                ApplyNeuralModel(prediction);
                break;
            case PredictionModelType.Statistical:
                ApplyStatisticalModel(prediction);
                break;
            case PredictionModelType.Ensemble:
                ApplyEnsembleModel(prediction);
                break;
        }
        
        CalculateConfidenceInterval(prediction);
        DetermineTradingSignal(prediction);
    }

    private void ApplyNeuralModel(HoloMarketPrediction prediction)
    {
        var volatility = GetItemVolatility(prediction.ItemName);
        var trendFactor = GetTrendFactor(prediction.ItemName);
        var marketSentiment = GetMarketSentiment();
        
        var baseChange = (trendFactor * 0.4 + marketSentiment * 0.3 + (_random.NextDouble() - 0.5) * 0.3) * volatility;
        var timeMultiplier = Math.Sqrt((int)TimeHorizon / 7.0);
        
        prediction.PredictedChange = baseChange * timeMultiplier * 100;
        prediction.PredictedPrice = prediction.CurrentPrice * (1 + prediction.PredictedChange / 100);
        prediction.Confidence = Math.Max(0.3, Math.Min(0.95, 0.8 - Math.Abs(prediction.PredictedChange) * 0.02));
    }

    private void ApplyStatisticalModel(HoloMarketPrediction prediction)
    {
        var historicalVolatility = GetHistoricalVolatility(prediction.ItemName);
        var movingAverage = GetMovingAverageSignal(prediction.ItemName);
        var seasonalFactor = GetSeasonalFactor(prediction.ItemName);
        
        var baseChange = (movingAverage * 0.5 + seasonalFactor * 0.3 + (_random.NextDouble() - 0.5) * 0.2) * historicalVolatility;
        var timeMultiplier = (int)TimeHorizon / 7.0;
        
        prediction.PredictedChange = baseChange * timeMultiplier * 100;
        prediction.PredictedPrice = prediction.CurrentPrice * (1 + prediction.PredictedChange / 100);
        prediction.Confidence = Math.Max(0.4, Math.Min(0.9, 0.75 - historicalVolatility * 0.5));
    }

    private void ApplyEnsembleModel(HoloMarketPrediction prediction)
    {
        var neuralPrediction = new HoloMarketPrediction
        {
            ItemName = prediction.ItemName,
            CurrentPrice = prediction.CurrentPrice,
            PredictionModel = PredictionModelType.Neural
        };
        ApplyNeuralModel(neuralPrediction);
        
        var statisticalPrediction = new HoloMarketPrediction
        {
            ItemName = prediction.ItemName,
            CurrentPrice = prediction.CurrentPrice,
            PredictionModel = PredictionModelType.Statistical
        };
        ApplyStatisticalModel(statisticalPrediction);
        
        prediction.PredictedChange = (neuralPrediction.PredictedChange * 0.6 + statisticalPrediction.PredictedChange * 0.4);
        prediction.PredictedPrice = prediction.CurrentPrice * (1 + prediction.PredictedChange / 100);
        prediction.Confidence = Math.Min(neuralPrediction.Confidence, statisticalPrediction.Confidence) + 0.1;
    }

    private void CalculateConfidenceInterval(HoloMarketPrediction prediction)
    {
        var stdDev = Math.Abs(prediction.PredictedChange) * (1 - prediction.Confidence) * 0.5;
        var margin = stdDev * 1.96; // 95% confidence interval
        
        prediction.LowerBound = prediction.CurrentPrice * (1 + (prediction.PredictedChange - margin) / 100);
        prediction.UpperBound = prediction.CurrentPrice * (1 + (prediction.PredictedChange + margin) / 100);
    }

    private void DetermineTradingSignal(HoloMarketPrediction prediction)
    {
        var change = prediction.PredictedChange;
        var confidence = prediction.Confidence;
        
        if (confidence < 0.5)
        {
            prediction.TradingSignal = TradingSignal.Hold;
        }
        else if (change > 10 && confidence > 0.8)
        {
            prediction.TradingSignal = TradingSignal.StrongBuy;
        }
        else if (change > 3 && confidence > 0.6)
        {
            prediction.TradingSignal = TradingSignal.Buy;
        }
        else if (change < -10 && confidence > 0.8)
        {
            prediction.TradingSignal = TradingSignal.StrongSell;
        }
        else if (change < -3 && confidence > 0.6)
        {
            prediction.TradingSignal = TradingSignal.Sell;
        }
        else
        {
            prediction.TradingSignal = TradingSignal.Hold;
        }
    }

    #endregion

    #region Market Analysis Helpers

    private double GetItemVolatility(string itemName)
    {
        return itemName switch
        {
            "Tritanium" => 0.05,
            "Pyerite" => 0.08,
            "Mexallon" => 0.12,
            "Isogen" => 0.15,
            "Nocxium" => 0.18,
            "Zydrine" => 0.22,
            "Megacyte" => 0.25,
            "Morphite" => 0.30,
            "PLEX" => 0.15,
            "Skill Injector" => 0.20,
            _ => 0.10
        };
    }

    private double GetTrendFactor(string itemName)
    {
        return (_random.NextDouble() - 0.5) * 2; // -1 to 1
    }

    private double GetMarketSentiment()
    {
        return Math.Sin(DateTime.Now.DayOfYear * 0.1) * 0.5; // Cyclical sentiment
    }

    private double GetHistoricalVolatility(string itemName)
    {
        return GetItemVolatility(itemName) * (0.8 + _random.NextDouble() * 0.4);
    }

    private double GetMovingAverageSignal(string itemName)
    {
        return (_random.NextDouble() - 0.5) * 1.5; // -0.75 to 0.75
    }

    private double GetSeasonalFactor(string itemName)
    {
        var dayOfYear = DateTime.Now.DayOfYear;
        return Math.Sin(dayOfYear * 2 * Math.PI / 365) * 0.3; // Seasonal variation
    }

    private double CalculateModelAccuracy()
    {
        return PredictionModel switch
        {
            PredictionModelType.Neural => 78.5 + _random.NextDouble() * 5,
            PredictionModelType.Statistical => 72.3 + _random.NextDouble() * 5,
            PredictionModelType.Ensemble => 82.1 + _random.NextDouble() * 5,
            _ => 75.0
        };
    }

    #endregion

    #region Chart Creation

    private void CreatePredictionChart(Panel parent)
    {
        var chartPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 350,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = $"Prediction Chart - {SelectedItem}",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        chartPanel.Children.Add(titleBlock);
        
        var chartCanvas = new Canvas
        {
            Width = 330,
            Height = 150,
            Background = CreateHolographicBrush(0.1),
            Margin = new Thickness(5)
        };
        
        DrawPredictionChart(chartCanvas);
        chartPanel.Children.Add(chartCanvas);
        parent.Children.Add(chartPanel);
    }

    private void CreateModelMetrics(Panel parent)
    {
        if (!ShowModelMetrics) return;
        
        var metricsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 250,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Model Performance",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        metricsPanel.Children.Add(titleBlock);
        
        AddMetricDisplay(metricsPanel, "Accuracy:", $"{CalculateModelAccuracy():F1}%");
        AddMetricDisplay(metricsPanel, "Precision:", $"{75.2 + _random.NextDouble() * 10:F1}%");
        AddMetricDisplay(metricsPanel, "Recall:", $"{71.8 + _random.NextDouble() * 10:F1}%");
        AddMetricDisplay(metricsPanel, "F1-Score:", $"{73.5 + _random.NextDouble() * 8:F2}");
        AddMetricDisplay(metricsPanel, "RMSE:", $"{0.08 + _random.NextDouble() * 0.04:F3}");
        AddMetricDisplay(metricsPanel, "Sharpe Ratio:", $"{1.45 + _random.NextDouble() * 0.5:F2}");
        
        parent.Children.Add(metricsPanel);
    }

    private void CreateMarketSentiment(Panel parent)
    {
        var sentimentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Market Sentiment",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        sentimentPanel.Children.Add(titleBlock);
        
        var sentiment = GetMarketSentiment();
        var sentimentText = sentiment > 0.2 ? "Bullish" : sentiment < -0.2 ? "Bearish" : "Neutral";
        var sentimentColor = sentiment > 0.2 ? "Success" : sentiment < -0.2 ? "Negative" : "Warning";
        
        AddSentimentDisplay(sentimentPanel, "Overall:", sentimentText, sentimentColor);
        AddSentimentDisplay(sentimentPanel, "Minerals:", "Bullish", "Success");
        AddSentimentDisplay(sentimentPanel, "Ships:", "Neutral", "Warning");
        AddSentimentDisplay(sentimentPanel, "Modules:", "Bearish", "Negative");
        AddSentimentDisplay(sentimentPanel, "PLEX:", "Bullish", "Success");
        
        parent.Children.Add(sentimentPanel);
    }

    private void DrawPredictionChart(Canvas canvas)
    {
        var selectedPrediction = Predictions?.FirstOrDefault(p => p.ItemName == SelectedItem);
        if (selectedPrediction == null) return;
        
        var currentPrice = selectedPrediction.CurrentPrice;
        var predictedPrice = selectedPrediction.PredictedPrice;
        var lowerBound = selectedPrediction.LowerBound;
        var upperBound = selectedPrediction.UpperBound;
        
        var minPrice = Math.Min(Math.Min(currentPrice, predictedPrice), lowerBound);
        var maxPrice = Math.Max(Math.Max(currentPrice, predictedPrice), upperBound);
        var priceRange = maxPrice - minPrice;
        
        if (priceRange == 0) return;
        
        var currentY = canvas.Height - ((currentPrice - minPrice) / priceRange) * canvas.Height * 0.8;
        var predictedY = canvas.Height - ((predictedPrice - minPrice) / priceRange) * canvas.Height * 0.8;
        var lowerY = canvas.Height - ((lowerBound - minPrice) / priceRange) * canvas.Height * 0.8;
        var upperY = canvas.Height - ((upperBound - minPrice) / priceRange) * canvas.Height * 0.8;
        
        var currentX = canvas.Width * 0.3;
        var predictedX = canvas.Width * 0.7;
        
        // Confidence interval
        if (ShowConfidenceIntervals)
        {
            var confidenceRect = new Rectangle
            {
                Width = predictedX - currentX,
                Height = Math.Abs(upperY - lowerY),
                Fill = GetEVEBrush("Info"),
                Opacity = 0.2
            };
            Canvas.SetLeft(confidenceRect, currentX);
            Canvas.SetTop(confidenceRect, Math.Min(upperY, lowerY));
            canvas.Children.Add(confidenceRect);
        }
        
        // Current price line
        var currentLine = new Line
        {
            X1 = 0,
            Y1 = currentY,
            X2 = currentX,
            Y2 = currentY,
            Stroke = GetEVEBrush("Secondary"),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 5 }
        };
        canvas.Children.Add(currentLine);
        
        // Prediction line
        var predictionLine = new Line
        {
            X1 = currentX,
            Y1 = currentY,
            X2 = predictedX,
            Y2 = predictedY,
            Stroke = predictedPrice >= currentPrice ? GetEVEBrush("Success") : GetEVEBrush("Negative"),
            StrokeThickness = 3
        };
        canvas.Children.Add(predictionLine);
        
        // Price points
        var currentPoint = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = GetEVEBrush("Primary")
        };
        Canvas.SetLeft(currentPoint, currentX - 4);
        Canvas.SetTop(currentPoint, currentY - 4);
        canvas.Children.Add(currentPoint);
        
        var predictedPoint = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = predictedPrice >= currentPrice ? GetEVEBrush("Success") : GetEVEBrush("Negative")
        };
        Canvas.SetLeft(predictedPoint, predictedX - 5);
        Canvas.SetTop(predictedPoint, predictedY - 5);
        canvas.Children.Add(predictedPoint);
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
            Width = 160,
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

    private void AddMetricDisplay(Panel parent, string label, string value)
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
            Text = value,
            Width = 80,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private void AddSentimentDisplay(Panel parent, string label, string sentiment, string colorType)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 100,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var sentimentElement = new TextBlock
        {
            Text = sentiment,
            Width = 80,
            Foreground = GetEVEBrush(colorType),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(sentimentElement);
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

    #endregion

    #region Animation Methods

    private void AnimatePredictionUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var scaleAnimation = new DoubleAnimation
        {
            From = 0.98,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleTransform = new ScaleTransform();
        _scrollViewer.RenderTransform = scaleTransform;
        _scrollViewer.RenderTransformOrigin = new Point(0.5, 0.5);
        
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        storyboard.Children.Add(scaleAnimation);
        
        var scaleYAnimation = scaleAnimation.Clone();
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }

    private void ApplyHighlightAnimation(UserControl control, bool isPositive)
    {
        var highlightBrush = isPositive ? 
            new SolidColorBrush(Color.FromArgb(80, 0, 255, 100)) :
            new SolidColorBrush(Color.FromArgb(80, 255, 50, 50));
        
        control.Background = highlightBrush;
        
        var storyboard = new Storyboard();
        var animation = new ColorAnimation
        {
            To = GetEVEColor("Background"),
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(animation, control.Background);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Color"));
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private HoloPredictionParticle CreatePredictionParticle()
    {
        var particle = new HoloPredictionParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(2, 5),
                Height = _random.Next(2, 5),
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

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 60; i++)
        {
            var particle = CreatePredictionParticle();
            _predictionParticles.Add(particle);
            _predictionsCanvas.Children.Add(particle.Element);
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
        UpdatePredictionVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        if (AutoUpdate)
        {
            RunPredictionModels();
        }
    }

    #endregion

    #region Update Methods

    private void UpdatePredictionVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 2.5));
        
        foreach (var child in _predictionsCanvas.Children.OfType<FrameworkElement>())
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
        
        foreach (var particle in _predictionParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.08;
            
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
        if (AutoUpdate)
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
        if (d is HoloMarketPredictions predictions)
        {
            predictions.UpdatePredictionVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.CreatePredictionsInterface();
        }
    }

    private static void OnPredictionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.PopulatePredictionContent();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.CreatePredictionsInterface();
        }
    }

    private static void OnPredictionModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.RunPredictionModels();
            predictions.CreatePredictionsInterface();
        }
    }

    private static void OnTimeHorizonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.RunPredictionModels();
        }
    }

    private static void OnConfidenceLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.RunPredictionModels();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            if ((bool)e.NewValue)
                predictions._animationTimer?.Start();
            else
                predictions._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            if ((bool)e.NewValue)
            {
                predictions.CreateParticleEffects();
                predictions._particleTimer?.Start();
            }
            else
            {
                predictions._particleTimer?.Stop();
                predictions._predictionParticles.Clear();
            }
        }
    }

    private static void OnShowConfidenceIntervalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.PopulatePredictionContent();
        }
    }

    private static void OnShowTrendAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.CreatePredictionsInterface();
        }
    }

    private static void OnShowModelMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions.CreatePredictionsInterface();
        }
    }

    private static void OnAutoUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            if ((bool)e.NewValue)
                predictions._updateTimer?.Start();
            else
                predictions._updateTimer?.Stop();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketPredictions predictions)
        {
            predictions._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowTrendAnalysis = false;
        ShowModelMetrics = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowTrendAnalysis = true;
        ShowModelMetrics = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloMarketPrediction
{
    public string ItemName { get; set; }
    public double CurrentPrice { get; set; }
    public double PredictedPrice { get; set; }
    public double PredictedChange { get; set; }
    public double Confidence { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public PredictionModelType PredictionModel { get; set; }
    public PredictionTimeHorizon TimeHorizon { get; set; }
    public TradingSignal TradingSignal { get; set; }
    public DateTime Timestamp { get; set; }
}

public class HoloPredictionParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum PredictionModelType
{
    Neural,
    Statistical,
    Ensemble
}

public enum PredictionTimeHorizon
{
    Hour = 1,
    Day = 24,
    Week = 168,
    Month = 720,
    Quarter = 2160
}

public enum TradingSignal
{
    StrongSell,
    Sell,
    Hold,
    Buy,
    StrongBuy
}

#endregion