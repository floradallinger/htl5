using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PuppyAdoption.Shared;
using PuppyAdoption.Views;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyList" /> control
/// </summary>
public partial class PuppyListViewModel : ViewModelBase
{
    // TODO
    [ObservableProperty]
    public partial IReadOnlyCollection<PuppyInformation> Puppies { get; set; }
    
    
}

public sealed class DesignPuppyListViewModel : PuppyListViewModel;
