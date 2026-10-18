using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.Util;
using PuppyAdoption.Views;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyAdmission" /> control
/// </summary>
/// <param name="imageProcessor">Service for processing images</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="dateTimeProvider">A provider for current date & time</param>
/// <param name="logger">Application logger</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="options">Application settings</param>
public partial class PuppyAdmissionViewModel(
    IImageProcessor imageProcessor,
    IPuppyService puppyService,
    IDateTimeProvider dateTimeProvider,
    ILogger<PuppyAdmissionViewModel> logger,
    IToastService toastService,
    IOptions<Settings> options,
    ITransactionProvider transation) : ShelterOperationViewModel
{
    // TODO
    public int MaxImageSize { get; } = options.Value.MaxImageSizePixel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowProgressBar), nameof(CanBeCreated))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private partial bool Processing { get; set; } = false;
    
    
    public bool ShowProgressBar => Processing;
    
    private bool CanBeCreated => !string.IsNullOrEmpty(PuppyName) && !string.IsNullOrEmpty(PuppyBreed) && !Processing;
    
    
    [ObservableProperty]
    public partial Bitmap? SelectedImage { get; set; }
    
    private byte[]? _imageData;
    
    public void SetImage(byte[] imageData)
    {
        _imageData = imageData;
        SelectedImage?.Dispose();
        using var stream = new MemoryStream(imageData);
        SelectedImage = new Bitmap(stream);
    }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeCreated))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string PuppyName { get; set; } = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeCreated))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string PuppyBreed { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeCreated))]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial LocalDate PuppyDob { get; set; } = dateTimeProvider.GetLocalDate();

    [RelayCommand(CanExecute = nameof(CanBeCreated))]
    private async Task Create()
    {
        if (string.IsNullOrEmpty(PuppyName) || string.IsNullOrEmpty(PuppyBreed) || SelectedShelter == null)
        {
            return;
        }

        Processing = true;
        await transation.BeginTransactionAsync();
        try
        {
            byte[]? image = null;
            if (_imageData != null)
            {
                image = imageProcessor.ProcessImage(_imageData);
            }

            var createResult
                = await puppyService.AdmitPuppyAsync(SelectedShelter, PuppyName, PuppyBreed, PuppyDob, image);

            toastService.ShowSuccess("Puppy was added to the shelter");

        }
        catch (Exception e)
        {
            toastService.ShowMessage($"Puppy with name {PuppyName} could not be added to the shelter");
            logger.LogWarning(e, "Failed to add puppy {puppyName} to shelter", PuppyName);
        }
        finally
        {
            Clear();
        }

    }

    private void Clear()
    {
        SelectedImage = null;
        PuppyName = "";
        PuppyBreed = "";
        PuppyDob = dateTimeProvider.GetLocalDate();
        Processing = false;
    }
    
}
