﻿using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.Stash;

public sealed class StashManagerViewModel : ManagerViewModelBase<StashItemModel>
{
    public StashManagerViewModel(MainWindowViewModel mainWindowViewModel)
        : base(mainWindowViewModel, gameSave => gameSave.Stash!.Items.Select(item => new StashItemModel(item)))
    {
        this.AddItemCommand = new(this.AddItem);
        this.DeleteItemCommand = new(this.DeleteItem);
    }

    public DelegateCommand AddItemCommand { get; }

    public DelegateCommand<StashItemModel> DeleteItemCommand { get; }

    private void AddItem()
    {
        ChangeOrAddItemViewModel viewModel = new();
        ChangeOrAddItemView view = new()
        {
            Owner = Application.Current.MainWindow,
            DataContext = viewModel
        };
        if (view.ShowDialog() != true || viewModel.Definition == null)
        {
            return;
        }
        StashItem stashItem = this.GameSave.Stash!.AddItem(viewModel.Definition);
        this.AddItem(new(stashItem));
        this.MainWindowViewModel.RegisterUnsavedChange();
    }

    private void DeleteItem(StashItemModel item)
    {
        this.RemoveItem(item);
        this.GameSave.Stash!.DeleteItem(item.Item);
        this.MainWindowViewModel.RegisterUnsavedChange();
    }
}
