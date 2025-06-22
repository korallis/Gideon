// ==========================================================================
// ParticleSystemDemo.xaml.cs - Particle System Demo Code-Behind
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Presentation.Views;

public partial class ParticleSystemDemo : Page
{
    private readonly Random _random = new();

    public ParticleSystemDemo()
    {
        InitializeComponent();
    }

    private void OnCreatePricePulse(object sender, RoutedEventArgs e)
    {
        var position = new Point(_random.NextDouble() * 400, _random.NextDouble() * 300);
        var direction = (MarketDirection)_random.Next(0, 3);
        ParticleEngine.CreatePricePulse(position, direction);
    }

    private void OnCreateVolumeBurst(object sender, RoutedEventArgs e)
    {
        var position = new Point(_random.NextDouble() * 400, _random.NextDouble() * 300);
        var volume = (VolumeLevel)_random.Next(0, 3);
        ParticleEngine.CreateVolumeBurst(position, volume);
    }

    private void OnCreateTradeFlash(object sender, RoutedEventArgs e)
    {
        var position = new Point(_random.NextDouble() * 400, _random.NextDouble() * 300);
        var direction = (MarketDirection)_random.Next(0, 2);
        ParticleEngine.CreateTradeFlash(position, direction);
    }

    private void OnCreateTrendFlow(object sender, RoutedEventArgs e)
    {
        var position = new Point(_random.NextDouble() * 400, _random.NextDouble() * 300);
        var direction = (MarketDirection)_random.Next(0, 2);
        ParticleEngine.CreateTrendFlow(position, direction);
    }

    private void OnClearParticles(object sender, RoutedEventArgs e)
    {
        ParticleEngine.ClearAllParticles();
    }
}