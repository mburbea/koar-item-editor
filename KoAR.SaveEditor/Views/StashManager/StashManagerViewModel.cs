using System.Collections.Generic;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel : ManagerViewModelBase<StashItem, StashItemModel>
    {
        public StashManagerViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel, stashItem => new StashItemModel(stashItem))
        {
            this.AddItemCommand = new DelegateCommand(this.AddItem);
            this.DeleteItemCommand = new DelegateCommand<StashItemModel>(this.DeleteItem);
        }

        public DelegateCommand AddItemCommand { get; }

        public DelegateCommand<StashItemModel> DeleteItemCommand { get; }

        public Stash Stash => this.GameSave.Stash!;

        protected override IReadOnlyCollection<StashItem> GameItems => this.Stash.Items;

        protected override void OnRepopulateItemsRequested()
        {
            this.OnPropertyChanged(nameof(this.Stash));
            base.OnRepopulateItemsRequested();
        }

        private void AddItem()
        {
            ChangeOrAddItemViewModel viewModel = new ChangeOrAddItemViewModel();
            ChangeOrAddItemView view = new ChangeOrAddItemView
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };
            if (view.ShowDialog() != true || viewModel.Definition == null)
            {
                return;
            }
            //this.Stash.AddItem(viewModel.Definition);
            //this.MainWindowViewModel.RegisterDrasticChange();
            StashItem stashItem = this.Stash.AddItem(viewModel.Definition);
            this.AddItem(new StashItemModel(stashItem));
            this.MainWindowViewModel.RegisterUnsavedChange();
        }

        private void DeleteItem(StashItemModel model)
        {
            //this.Stash.DeleteItem(model.Item);
            //this.MainWindowViewModel.RegisterDrasticChange();
            this.RemoveItem(model);
            this.MainWindowViewModel.RegisterUnsavedChange();
        }
    }
}
