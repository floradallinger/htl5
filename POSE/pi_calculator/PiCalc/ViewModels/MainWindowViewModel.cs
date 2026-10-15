using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodaTime;
using NodaTime.Text;
using PiCalc.Core;

namespace PiCalc.ViewModels;

/// <summary>
///     View model for the main window of the PiCalc application
/// </summary>
public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    // TODO
    private readonly IRunner _runner;
    private readonly IClock _clock;
    private CancellationTokenSource? _cts;
    
    public ExperimentRendererViewModel RendererViewModel { get; } = new();
    
    public int MaxThreads = Environment.ProcessorCount;
    public int MinThreads = 1;

    [ObservableProperty]
    public partial int Threads { get; set; }
    [ObservableProperty]
    public partial bool ProgressReporting { get; set; }
    [ObservableProperty]
    public partial bool ResultRendering { get; set; }
    [ObservableProperty]
    public partial bool VectorizedCalculation { get; set; }
    [ObservableProperty]
    public partial long Experiments { get; set; }
    [ObservableProperty]
    public partial double Progress { get; set; }
    [ObservableProperty]
    public partial string ResultText { get; set; }
    [ObservableProperty]
    public partial string ElapsedTimeText { get; set;}

    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleEnabled))]
    public partial bool IsRunning {get; set;}
    
    private bool ToggleEnabled => !IsRunning;
    
    public string ExperimentLabels => Pluralize("Experiment", Experiments);
    public string ThreadLabels => Pluralize("Thread", Threads);
    
    

    /// <summary>
    ///     Creates a new instance of the <see cref="MainWindowViewModel" /> class
    /// </summary>
    /// <param name="runner">Experiment runner instance</param>
    /// <param name="clock">Clock instance for timing</param>
    public MainWindowViewModel(IRunner runner, IClock clock)
    {
        // TODO
        _runner = runner;
        _clock = clock;

        Threads = MaxThreads;
        Experiments = 1000000;
        ProgressReporting = true;
        ResultRendering = true;
        ResultText =  string.Empty;
        ElapsedTimeText = FormatElapsed(TimeSpan.Zero);
    }

    private static string FormatElapsed(TimeSpan t) =>  t.ToString(@"hh\:mm\:ss\.fff");

    // TODO

    partial void OnThreadsChanged(int value)
    {
        int champed = Math.Clamp(value, MinThreads, MaxThreads);
        if (champed != value)
        {
            Threads = champed; 
        }
        OnPropertyChanged(nameof(Threads));
    }

    partial void OnExperimentsChanged(long value)
    {
        if (value < 1)
        {
            Experiments = 1;
            return;
        }
        OnPropertyChanged(nameof(ExperimentLabels));
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        RendererViewModel.Clear();
        Progress = 0;
        ResultText = string.Empty;
        ElapsedTimeText = FormatElapsed(TimeSpan.Zero);
        
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        
        IsRunning = true;
        
        var start = _clock.GetCurrentInstant();
        IRunner.ProgressReport? progressReport = null;
        if (ProgressReporting)
        {
            progressReport = (processed, total) =>
                Dispatcher.UIThread.Post(() =>
                {
                    Progress = (double) processed / total;
                    if (ResultRendering)
                    {
                        RendererViewModel.RenderResults();
                    }
                });
        }
        
        IRunner.ExperimentResultReport? resultReport = null;
        if (ResultRendering)
        {
            resultReport = (xs, ys, inside) =>
            {
                for (var i = 0; i < xs.Length; i++)
                {
                    RendererViewModel.AddExperimentResult(xs[i], ys[i], inside[i]);
                }
            };
        }

        try
        {
            var pi = await _runner.RunSimulation(Threads, Experiments, VectorizedCalculation,
                progressReport, resultReport, _cts.Token);

            ResultText = pi.ToString(CultureInfo.InvariantCulture);
            Progress = 1;
            if (ResultRendering)
            {
                RendererViewModel.RenderResults(); 
            }
        }
        catch (OperationCanceledException)
        {
            ResultText = "Cancelled";
        }
        finally
        {
            var elapsed = _clock.GetCurrentInstant() - start;
            ElapsedTimeText = FormatElapsed(elapsed.ToTimeSpan());
            IsRunning = false;
        }
    }
    
    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cts?.Cancel();

    private bool CanStop() => IsRunning;


    private static string Pluralize<T>(string noun, T count) where T : INumber<T> => count == T.One ? noun : $"{noun}s";
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

public sealed class DesignMainWindowViewModel() : MainWindowViewModel(null!, null!);
