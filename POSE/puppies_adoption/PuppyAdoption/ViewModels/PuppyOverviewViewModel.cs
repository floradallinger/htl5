using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.Util;
using PuppyAdoption.Views;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyOverview" /> control
/// </summary>
/// <param name="puppyService">Service for managing puppies</param>
/// <param name="logger">Application logger</param>
public partial class PuppyOverviewViewModel(IPuppyService puppyService, ILogger<PuppyOverviewViewModel> logger)
    : ShelterOperationViewModel
{
    // TODO
    [ObservableProperty]
    public partial PuppyListViewModel PuppyListViewModel { get; set; } = App.CreateViewModel<PuppyListViewModel>();

    [ObservableProperty]
    public partial PuppyDetailsViewModel PuppyDetailsViewModel { get; set; }
        = App.CreateViewModel<PuppyDetailsViewModel>();

    

    public override async ValueTask InitAsync(Shelter shelter)
    {
        var pupps = await puppyService.GetAllPuppiesAsync(shelter);
        logger.LogInformation("Puppies were loaded successfully into the view model");
        PuppyListViewModel.Puppies = pupps;
    }
    
}

public sealed class DesignPuppyOverviewViewModel()
    : PuppyOverviewViewModel(new DesignFacades.DummyPuppyService(),
                             new DesignFacades.DummyLogger<PuppyOverviewViewModel>());
