using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class BuffCollectionManager : Control
    {
        public static readonly DependencyProperty AddBuffCommandProperty = DependencyProperty.Register(nameof(BuffCollectionManager.AddBuffCommand), typeof(ICommand), typeof(BuffCollectionManager));

        public static readonly DependencyProperty BuffsFilterProperty = DependencyProperty.Register(nameof(BuffCollectionManager.BuffsFilter), typeof(BuffsFilter), typeof(BuffCollectionManager));

        public static readonly DependencyProperty BuffsProperty = DependencyProperty.Register(nameof(BuffCollectionManager.Buffs), typeof(IReadOnlyList<Buff>), typeof(BuffCollectionManager));

        public static readonly DependencyProperty CollectionProperty = DependencyProperty.Register(nameof(BuffCollectionManager.Collection), typeof(IReadOnlyCollection<Buff>), typeof(BuffCollectionManager));

        public static readonly DependencyProperty DeleteBuffCommandProperty = DependencyProperty.Register(nameof(BuffCollectionManager.DeleteBuffCommand), typeof(ICommand), typeof(BuffCollectionManager));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(BuffCollectionManager.Header), typeof(object), typeof(BuffCollectionManager));

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(BuffCollectionManager.HeaderTemplate), typeof(DataTemplate), typeof(BuffCollectionManager));

        public static readonly DependencyProperty PendingBuffIdProperty = DependencyProperty.Register(nameof(BuffCollectionManager.PendingBuffId), typeof(uint?), typeof(BuffCollectionManager));

        public static readonly DependencyProperty PendingBuffProperty = DependencyProperty.Register(nameof(BuffCollectionManager.PendingBuff), typeof(Buff), typeof(BuffCollectionManager),
            new PropertyMetadata(BuffCollectionManager.PendingBuffProperty_ValueChanged));

        public static readonly DependencyProperty UnsupportedFormatProperty = DependencyProperty.Register(nameof(BuffCollectionManager.UnsupportedFormat), typeof(bool), typeof(BuffCollectionManager),
            new PropertyMetadata(BooleanBoxes.False));

        static BuffCollectionManager() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BuffCollectionManager), new FrameworkPropertyMetadata(typeof(BuffCollectionManager)));

        public ICommand? AddBuffCommand
        {
            get => (ICommand?)this.GetValue(BuffCollectionManager.AddBuffCommandProperty);
            set => this.SetValue(BuffCollectionManager.AddBuffCommandProperty, value);
        }

        public IReadOnlyList<Buff>? Buffs
        {
            get => (IReadOnlyList<Buff>?)this.GetValue(BuffCollectionManager.BuffsProperty);
            set => this.SetValue(BuffCollectionManager.BuffsProperty, value);
        }

        public BuffsFilter BuffsFilter
        {
            get => (BuffsFilter)this.GetValue(BuffCollectionManager.BuffsFilterProperty);
            set => this.SetValue(BuffCollectionManager.BuffsFilterProperty, value);
        }

        public IReadOnlyCollection<Buff>? Collection
        {
            get => (IReadOnlyCollection<Buff>?)this.GetValue(BuffCollectionManager.CollectionProperty);
            set => this.SetValue(BuffCollectionManager.CollectionProperty, value);
        }

        public ICommand? DeleteBuffCommand
        {
            get => (ICommand?)this.GetValue(BuffCollectionManager.DeleteBuffCommandProperty);
            set => this.SetValue(BuffCollectionManager.DeleteBuffCommandProperty, value);
        }

        public object? Header
        {
            get => this.GetValue(BuffCollectionManager.HeaderProperty);
            set => this.SetValue(BuffCollectionManager.HeaderProperty, value);
        }

        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)this.GetValue(BuffCollectionManager.HeaderTemplateProperty);
            set => this.SetValue(BuffCollectionManager.HeaderTemplateProperty, value);
        }

        public Buff? PendingBuff
        {
            get => (Buff?)this.GetValue(BuffCollectionManager.PendingBuffProperty);
            set => this.SetValue(BuffCollectionManager.PendingBuffProperty, value);
        }

        public uint? PendingBuffId
        {
            get => (uint?)this.GetValue(BuffCollectionManager.PendingBuffIdProperty);
            set => this.SetValue(BuffCollectionManager.PendingBuffIdProperty, value);
        }

        public bool UnsupportedFormat
        {
            get => (bool)this.GetValue(BuffCollectionManager.UnsupportedFormatProperty);
            set => this.SetValue(BuffCollectionManager.UnsupportedFormatProperty, BooleanBoxes.GetBox(value));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Template.FindName("PART_TreeView", this) is TreeView treeView)
            {
                treeView.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, BuffCollectionManager.CopyCommand_Executed));
            }
        }

        private static void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            Buff buff = (Buff)ItemsControl.ItemsControlFromItemContainer(item).ItemContainerGenerator.ItemFromContainer(item);
            Clipboard.SetText(buff.Id.ToString("X6"));
            e.Handled = true;
        }

        private static void PendingBuffProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BuffCollectionManager)d).PendingBuffId = ((Buff?)e.NewValue)?.Id;
        }
    }
}
