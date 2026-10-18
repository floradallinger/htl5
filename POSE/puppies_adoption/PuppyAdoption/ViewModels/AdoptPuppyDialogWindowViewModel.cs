using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Util;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="AdoptPuppyDialogWindow" />
/// </summary>
/// <param name="transaction">Transaction provider</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="logger">Application logger</param>
public partial class AdoptPuppyDialogWindowViewModel(
    ITransactionProvider transaction,
    IToastService toastService,
    IPuppyService puppyService,
    ILogger<AdoptPuppyDialogWindowViewModel> logger)
    : PuppyOperationDialogViewModel<AdoptPuppyDialogWindowViewModel.AdoptionInformation?>
{
    // TODO

    /// <summary>
    ///     Represents the adoption information for a puppy
    /// </summary>
    /// <param name="AdoptedByName">Name of the person who adopted the puppy</param>
    /// <param name="AdoptionDate">The date the adoption went through</param>
    public sealed record AdoptionInformation(string AdoptedByName, LocalDate AdoptionDate);
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdoptCommand), nameof(CancelCommand))]
    public partial bool Processing { get; set; } = false;

    private bool CanCancel => !Processing;
    private bool CanAdopt => !string.IsNullOrWhiteSpace(AdopterName) && !Processing;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdoptCommand))]
    public partial string AdopterName { get; set; } = string.Empty;


    public override string PuppyName { get; set; } = string.Empty;
    

    [RelayCommand(CanExecute = nameof(CanAdopt))]
    private async Task Adopt()
    {
        if (string.IsNullOrWhiteSpace(AdopterName))
        {
            return;
        }

        Processing = true;
        await transaction.BeginTransactionAsync();
        try
        {
            var adoptionResult = await puppyService.AdoptPuppyAsync(PuppyId, AdopterName);
            await adoptionResult.Match<ValueTask>(async success =>
            {
                await transaction.CommitAsync();
                toastService.ShowSuccess("Puppy was successfully adopted!");
                Close(new AdoptionInformation(AdopterName, success.Value));
            },
            async notFound =>
            {
                await transaction.RollbackAsync();
                toastService.ShowMessage($"Puppy with name {PuppyName} could not be found",
                                         "Puppy not Found", NotificationType.Warning);
            },
            async alreadyAdopted =>
            {
                await transaction.RollbackAsync();
                toastService.ShowMessage($"Puppy with name {PuppyName} is already adopted",
                                         "Puppy already adopted", NotificationType.Warning);
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to adopt puppy");
            toastService.ShowError("Failed to adopt puppy. Please try again.");
            await transaction.RollbackAsync();
        }
        finally
        {
            Processing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAdopt))]
    private async Task Cancel()
    {
        Close(null);
    }

}

public sealed class DesignAdoptPuppyDialogWindowViewModel : AdoptPuppyDialogWindowViewModel
{
    public DesignAdoptPuppyDialogWindowViewModel()
        : base(new DesignFacades.DummyTransactionProvider(),
               new DesignFacades.DummyToastService(),
               new DesignFacades.DummyPuppyService(),
               new LoggerFactory().CreateLogger<AdoptPuppyDialogWindowViewModel>())
    {
        PuppyId = 1;
        PuppyName = "Buddy";
    }
}
