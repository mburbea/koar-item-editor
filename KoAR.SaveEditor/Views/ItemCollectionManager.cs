using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views;

public sealed class ItemCollectionManager : Control
{
    public static readonly DependencyProperty AllItemsSellableProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsSellable), typeof(bool?), typeof(ItemCollectionManager),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AllItemsStashableProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsStashable), typeof(bool?), typeof(ItemCollectionManager),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty AllItemsStolenProperty = DependencyProperty.Register(nameof(ItemCollectionManager.AllItemsStolen), typeof(bool?), typeof(ItemCollectionManager),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ChangeDefinitionCommandProperty = DependencyProperty.Register(nameof(ItemCollectionManager.ChangeDefinitionCommand), typeof(ICommand), typeof(ItemCollectionManager));

    public static readonly DependencyProperty CollectionViewProperty;

    public static readonly DependencyProperty DeleteItemCommandProperty = DependencyProperty.Register(nameof(ItemCollectionManager.DeleteItemCommand), typeof(ICommand), typeof(ItemCollectionManager));

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(ItemCollectionManager.Items), typeof(IReadOnlyList<ItemModelBase>), typeof(ItemCollectionManager),
        new(ItemCollectionManager.ItemsProperty_ValueChanged));

    public static readonly DependencyProperty ModeProperty;

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(ItemCollectionManager),
        new());

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.RegisterAttached(nameof(ItemCollectionManager.SearchText), typeof(string), typeof(ItemCollectionManager),
        new());

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(ItemCollectionManager.SelectedItem), typeof(object), typeof(ItemCollectionManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached(nameof(ItemCollectionManager.SortDirection), typeof(ListSortDirection), typeof(ItemCollectionManager),
        new());

    public static readonly DependencyProperty SortPropertyProperty = DependencyProperty.RegisterAttached(nameof(ItemCollectionManager.SortProperty), typeof(string), typeof(ItemCollectionManager),
        new(nameof(ItemModelBase.Level)));

    private static readonly DependencyPropertyKey _collectionViewPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemCollectionManager.CollectionView), typeof(ListCollectionView), typeof(ItemCollectionManager),
        new(ItemCollectionManager.CollectionView_ValueChanged));

    private static readonly DependencyPropertyKey _modePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemCollectionManager.Mode), typeof(Mode), typeof(ItemCollectionManager),
        new());

    private ListView? _listView;

    static ItemCollectionManager()
    {
        ItemCollectionManager.CollectionViewProperty = ItemCollectionManager._collectionViewPropertyKey.DependencyProperty;
        ItemCollectionManager.ModeProperty = ItemCollectionManager._modePropertyKey.DependencyProperty;
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemCollectionManager), new FrameworkPropertyMetadata(typeof(ItemCollectionManager)));
    }

    public bool? AllItemsSellable
    {
        get => (bool?)this.GetValue(ItemCollectionManager.AllItemsSellableProperty);
        set => this.SetValue(ItemCollectionManager.AllItemsSellableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
    }

    public bool? AllItemsStashable
    {
        get => (bool?)this.GetValue(ItemCollectionManager.AllItemsStashableProperty);
        set => this.SetValue(ItemCollectionManager.AllItemsStashableProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
    }

    public bool? AllItemsStolen
    {
        get => (bool?)this.GetValue(ItemCollectionManager.AllItemsStolenProperty);
        set => this.SetValue(ItemCollectionManager.AllItemsStolenProperty, value.HasValue ? BooleanBoxes.GetBox(value.Value) : null);
    }

    public ICommand? ChangeDefinitionCommand
    {
        get => (ICommand?)this.GetValue(ItemCollectionManager.ChangeDefinitionCommandProperty);
        set => this.SetValue(ItemCollectionManager.ChangeDefinitionCommandProperty, value);
    }

    public ListCollectionView? CollectionView
    {
        get => (ListCollectionView?)this.GetValue(ItemCollectionManager.CollectionViewProperty);
        private set => this.SetValue(ItemCollectionManager._collectionViewPropertyKey, value);
    }

    public ICommand? DeleteItemCommand
    {
        get => (ICommand?)this.GetValue(ItemCollectionManager.DeleteItemCommandProperty);
        set => this.SetValue(ItemCollectionManager.DeleteItemCommandProperty, value);
    }

    public IReadOnlyList<ItemModelBase>? Items
    {
        get => (IReadOnlyList<ItemModelBase>?)this.GetValue(ItemCollectionManager.ItemsProperty);
        set => this.SetValue(ItemCollectionManager.ItemsProperty, value);
    }

    public Mode Mode
    {
        get => (Mode)this.GetValue(ItemCollectionManager.ModeProperty);
        private set => this.SetValue(ItemCollectionManager._modePropertyKey, value);
    }

    public string? SearchText
    {
        get => (string?)this.GetValue(ItemCollectionManager.SearchTextProperty);
        set => this.SetValue(ItemCollectionManager.SearchTextProperty, value);
    }

    public object? SelectedItem
    {
        get => (object?)this.GetValue(ItemCollectionManager.SelectedItemProperty);
        set => this.SetValue(ItemCollectionManager.SelectedItemProperty, value);
    }

    public ListSortDirection SortDirection
    {
        get => (ListSortDirection)this.GetValue(ItemCollectionManager.SortDirectionProperty);
        set => this.SetValue(ItemCollectionManager.SortDirectionProperty, value);
    }

    public string SortProperty
    {
        get => (string)this.GetValue(ItemCollectionManager.SortPropertyProperty);
        set => this.SetValue(ItemCollectionManager.SortPropertyProperty, value);
    }

    public static string? GetPropertyName(GridViewColumn column) => (string?)column?.GetValue(ItemCollectionManager.PropertyNameProperty);

    public static void SetPropertyName(GridViewColumn column, string? value) => column?.SetValue(ItemCollectionManager.PropertyNameProperty, value);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if ((this._listView = this.Template.FindName("PART_ListView", this) as ListView) == null)
        {
            return;
        }
        ListViewAutoSize.AutoSizeColumns(this._listView);
        this._listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.GridViewColumn_Click));
    }

    private static void CollectionView_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ItemCollectionManager manager = (ItemCollectionManager)d;
        if (e.OldValue is ListCollectionView previous)
        {
            CollectionChangedEventManager.RemoveHandler(previous, manager.CollectionView_CollectionChanged);
        }
        if (e.NewValue is ListCollectionView next)
        {
            CollectionChangedEventManager.AddHandler(next, manager.CollectionView_CollectionChanged);
            manager.Dispatcher.InvokeAsync(manager.OnViewChanged);
        }
    }

    private static void ItemsProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ItemCollectionManager manager = (ItemCollectionManager)d;
        manager.Mode = e.NewValue is IEnumerable<ItemModelBase<StashItem>> ? Mode.Stash : default;
        manager.CollectionView = e.NewValue is not IList list ? null : new(list)
        {
            SortDescriptions =
            {
                new(nameof(ItemModelBase.Category), ListSortDirection.Ascending),
                new(manager.SortProperty, manager.SortDirection),
                new(nameof(ItemModelBase.DisplayName), ListSortDirection.Ascending)
            }
        };
    }

    private void CollectionView_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => this.Dispatcher.InvokeAsync(this.OnViewChanged);

    private void GridViewColumn_Click(object? sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header ||
            ItemCollectionManager.GetPropertyName(header.Column) is not { } name ||
            this.CollectionView?.SortDescriptions is not ([_, { Direction: { } direction, PropertyName: { } propertyName }, _] descriptions))
        {
            return;
        }
        descriptions[1] = new(
            this.SortProperty = name,
            this.SortDirection = name == propertyName
                ? (ListSortDirection)((int)direction ^ 1)
                : ListSortDirection.Ascending
        );
    }

    private void OnViewChanged()
    {
        if (this._listView == null)
        {
            return;
        }
        ListViewAutoSize.AutoSizeColumns(this._listView);
        if (this._listView.ItemsSource is ListCollectionView { IsEmpty: false } view)
        {
            this._listView.ScrollIntoView(view.GetItemAt(0));
        }
    }
}
