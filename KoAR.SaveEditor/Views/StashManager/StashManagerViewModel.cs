using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;
using KoAR.SaveEditor.Views.ChangeOrAddItem;
using KoAR.SaveEditor.Views.Main;

namespace KoAR.SaveEditor.Views.StashManager
{
    public sealed class StashManagerViewModel : ManagerViewModelBase<StashItemModel>
    {
        public StashManagerViewModel(MainWindowViewModel mainWindowViewModel)
            : base(mainWindowViewModel, ManagementMode.Stash, gameSave => gameSave.Stash!.Items.Select(item => new StashItemModel(item)))
        {
            this.AddItemCommand = new DelegateCommand(this.AddItem);
            this.DeleteItemCommand = new DelegateCommand<StashItemModel>(this.DeleteItem);
        }

        public DelegateCommand AddItemCommand { get; }

        public DelegateCommand<StashItemModel> DeleteItemCommand { get; }

        public Stash Stash => this.GameSave.Stash!;

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
            StashItem stashItem = this.Stash.AddItem(viewModel.Definition);
            this.AddItem(new StashItemModel(stashItem));
            this.MainWindowViewModel.RegisterUnsavedChange();
        }

        private void DeleteItem(StashItemModel item)
        {
            this.RemoveItem(item);
            this.Stash.DeleteItem(item.Item);
            this.MainWindowViewModel.RegisterUnsavedChange();
        }
    }
}
