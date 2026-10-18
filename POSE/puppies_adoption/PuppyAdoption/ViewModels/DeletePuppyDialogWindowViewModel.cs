using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Util;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="DeletePuppyDialogWindow" />
/// </summary>
/// <param name="transaction">Transaction provider</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="logger">Application logger</param>
public partial class DeletePuppyDialogWindowViewModel(
    IPuppyService puppyService,
    ITransactionProvider transaction,
    IToastService toastService,
    ILogger<DeletePuppyDialogWindowViewModel> logger) : PuppyOperationDialogViewModel<bool>
{
    // TODO
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand), nameof(CancelCommand))]
    public partial bool Processing { get; set; } = false;
    private bool CanCancel => !Processing;

    private bool CanDelete => !string.IsNullOrWhiteSpace(PuppyName) && !Processing;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    public override partial string PuppyName { get; set; } = string.Empty;
    

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        if (string.IsNullOrWhiteSpace(PuppyName))
        {
            return;
        }

        Processing = true;
        await transaction.BeginTransactionAsync();
        try
        {
            OneOf<Success, NotFound> shelterResult
                = await puppyService.DeletePuppyAsync(PuppyId);
            await shelterResult.Match<ValueTask>(async success =>
                                                 {
                                                     await transaction.CommitAsync();
                                                     toastService.ShowSuccess("Puppy was sent off to its new home");
                                                     Close(true);
                                                 },
                                                 async notFound =>
                                                 {
                                                     await transaction.RollbackAsync();
                                                     toastService
                                                         .ShowMessage($"Puppy with name '{PuppyName}' does not exists",
                                                                      "Puppy not found", NotificationType.Warning);
                                                     Close(false);
                                                 });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete puppy");
            toastService.ShowError("Failed to delete puppy.");
            await transaction.RollbackAsync();
            Close(false);
        }
        finally
        {
            Processing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task Cancel()
    {
        Close(false);
    }

}

public sealed class DesignDeletePuppyDialogWindowViewModel : DeletePuppyDialogWindowViewModel
{
    public DesignDeletePuppyDialogWindowViewModel()
        : base(new DesignFacades.DummyPuppyService(),
               new DesignFacades.DummyTransactionProvider(),
               new DesignFacades.DummyToastService(),
               new LoggerFactory().CreateLogger<DeletePuppyDialogWindowViewModel>())
    {
        PuppyId = 1;
        PuppyName = "Buddy";
    }
}
