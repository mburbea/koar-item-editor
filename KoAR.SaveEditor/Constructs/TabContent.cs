using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public static class TabContent
{
    public static readonly DependencyProperty PersistProperty = DependencyProperty.RegisterAttached("Persist", typeof(bool), typeof(TabContent),
        new(TabContent.PersistProperty_ValueChanged));

    public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent),
        new());

    public static readonly DependencyProperty TemplateSelectorProperty = DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent),
        new());

    private static readonly DependencyProperty _contentManagerProperty = DependencyProperty.RegisterAttached(nameof(ContentManager), typeof(ContentManager), typeof(TabContent),
        new());

    private static readonly DependencyPropertyDescriptor _contentTemplateDescriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty, typeof(TabControl));

    private static readonly DependencyPropertyDescriptor _contentTemplateSelectorDescriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateSelectorProperty, typeof(TabControl));

    private static readonly DependencyProperty _persistedContentProperty = DependencyProperty.RegisterAttached("PersistedContent", typeof(UIElement), typeof(TabContent));

    private static readonly DependencyProperty _tabControlProperty = DependencyProperty.RegisterAttached(nameof(TabControl), typeof(TabControl), typeof(TabContent),
        new(TabContent.TabControlProperty_ValueChanged));

    private static readonly DataTemplate _template = TabContent.CreateContentTemplate();

    public static bool GetPersist(TabControl tabControl)
    {
        return tabControl != null && (bool)tabControl.GetValue(TabContent.PersistProperty);
    }

    public static DataTemplate? GetTemplate(TabControl tabControl)
    {
        return (DataTemplate?)tabControl.GetValue(TabContent.TemplateProperty);
    }

    public static DataTemplateSelector? GetTemplateSelector(TabControl tabControl)
    {
        return (DataTemplateSelector?)tabControl.GetValue(TabContent.TemplateSelectorProperty);
    }

    public static void SetPersist(TabControl tabControl, bool value)
    {
        tabControl?.SetValue(TabContent.PersistProperty, BooleanBoxes.GetBox(value));
    }

    public static void SetTemplate(TabControl tabControl, DataTemplate? value)
    {
        tabControl.SetValue(TabContent.TemplateProperty, value);
    }

    public static void SetTemplateSelector(TabControl tabControl, DataTemplateSelector? value)
    {
        tabControl.SetValue(TabContent.TemplateSelectorProperty, value);
    }

    private static DataTemplate CreateContentTemplate()
    {
        FrameworkElementFactory factory = new(typeof(Border));
        factory.SetBinding(TabContent._tabControlProperty, new Binding
        {
            RelativeSource = new RelativeSource { AncestorType = typeof(TabControl) }
        });
        return new() { VisualTree = factory };
    }

    private static ContentManager? GetContentManager(TabControl tabControl)
    {
        return (ContentManager?)tabControl.GetValue(TabContent._contentManagerProperty);
    }

    private static ContentManager GetContentManager(TabControl tabControl, Border border)
    {
        ContentManager? manager = TabContent.GetContentManager(tabControl);
        if (manager == null)
        {
            TabContent.SetContentManager(tabControl, manager = new(tabControl));
        }
        manager.Container = border;
        return manager;
    }

    private static UIElement? GetPersistedContent(TabItem tabItem)
    {
        return (UIElement?)tabItem.GetValue(TabContent._persistedContentProperty);
    }

    private static void PersistProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TabControl tabControl)
        {
            return;
        }
        if ((bool)e.NewValue)
        {
            if (tabControl.ContentTemplate != null || tabControl.ContentTemplateSelector != null)
            {
                throw new InvalidOperationException($"{nameof(TabControl.ContentTemplate)} and {nameof(TabControl.ContentTemplateSelector)} must be null when Persist is true.");
            }
            tabControl.ContentTemplate = TabContent._template;
            TabContent._contentTemplateDescriptor.AddValueChanged(tabControl, TabContent.TabControl_ContentTemplatePropertiesChanged);
            TabContent._contentTemplateSelectorDescriptor.AddValueChanged(tabControl, TabContent.TabControl_ContentTemplatePropertiesChanged);
        }
        else
        {
            TabContent._contentTemplateDescriptor.RemoveValueChanged(tabControl, TabContent.TabControl_ContentTemplatePropertiesChanged);
            TabContent._contentTemplateSelectorDescriptor.RemoveValueChanged(tabControl, TabContent.TabControl_ContentTemplatePropertiesChanged);
            tabControl.ContentTemplate = null;
            using (TabContent.GetContentManager(tabControl))
            {
                TabContent.SetContentManager(tabControl, null);
            }
        }
    }

    private static void SetContentManager(TabControl tabControl, ContentManager? value)
    {
        tabControl.SetValue(TabContent._contentManagerProperty, value);
    }

    private static void SetPersistedContent(TabItem tabItem, UIElement? value)
    {
        tabItem?.SetValue(TabContent._persistedContentProperty, value);
    }

    private static void TabControl_ContentTemplatePropertiesChanged(object? sender, EventArgs e)
    {
        throw new InvalidOperationException($"Can not assign to {nameof(TabControl.ContentTemplate)} or {nameof(TabControl.ContentTemplateSelector)} when Persist is true.");
    }

    private static void TabControlProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null)
        {
            TabContent.GetContentManager((TabControl)e.NewValue, (Border)d).UpdateSelectedTab();
        }
    }

    private sealed class ContentManager : IDisposable
    {
        private readonly TabControl _tabControl;
        private Border? _container;

        public ContentManager(TabControl tabControl)
        {
            (this._tabControl = tabControl).SelectionChanged += this.TabControl_SelectionChanged;
        }

        public Border? Container
        {
            get => this._container;
            set
            {
                if (this._container != value && this._container != null)
                {
                    this._container.Child = null;
                }
                this._container = value;
            }
        }

        public void Dispose() => this._tabControl.SelectionChanged -= this.TabControl_SelectionChanged;

        public void UpdateSelectedTab()
        {
            if (this.Container != null)
            {
                this.Container.Child = this.GetPersistedContent();
            }
        }

        private UIElement? GetPersistedContent()
        {
            if (this._tabControl.SelectedItem is not object item || this._tabControl.ItemContainerGenerator.ContainerFromItem(item) is not TabItem tabItem)
            {
                return null;
            }
            if (TabContent.GetPersistedContent(tabItem) is UIElement element)
            {
                return element;
            }
            ContentPresenter contentPresenter = new()
            {
                DataContext = item,
                ContentTemplate = TabContent.GetTemplate(this._tabControl),
                ContentTemplateSelector = TabContent.GetTemplateSelector(this._tabControl)
            };
            contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding());
            TabContent.SetPersistedContent(tabItem, contentPresenter);
            return contentPresenter;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.UpdateSelectedTab();
    }
}
