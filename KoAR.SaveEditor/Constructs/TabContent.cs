using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public static class TabContent
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty ContentManagerProperty = DependencyProperty.RegisterAttached(nameof(ContentManager), typeof(ContentManager), typeof(TabContent),
            new PropertyMetadata());

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty PersistedContentProperty = DependencyProperty.RegisterAttached("PersistedContent", typeof(UIElement), typeof(TabContent),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PersistProperty = DependencyProperty.RegisterAttached("Persist", typeof(bool), typeof(TabContent),
            new PropertyMetadata(TabContent.PersistProperty_ValueChanged));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty TabControlProperty = DependencyProperty.RegisterAttached(nameof(TabControl), typeof(TabControl), typeof(TabContent),
            new PropertyMetadata(TabControlProperty_ValueChanged));

        public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent),
            new PropertyMetadata());

        public static readonly DependencyProperty TemplateSelectorProperty = DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent),
            new PropertyMetadata());

        private static readonly DataTemplate _contentTemplate = TabContent.CreateContentTemplate();

        private static readonly DependencyPropertyDescriptor _contentTemplateDescriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty, typeof(TabControl));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ContentManager? GetContentManager(TabControl tabControl)
        {
            return (ContentManager?)tabControl.GetValue(TabContent.ContentManagerProperty);
        }

        public static bool GetPersist(TabControl tabControl)
        {
            return tabControl != null && (bool)tabControl.GetValue(TabContent.PersistProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UIElement? GetPersistedContent(TabItem tabItem)
        {
            return (UIElement?)tabItem.GetValue(TabContent.PersistedContentProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TabControl? GetTabControl(Decorator decorator)
        {
            return (TabControl?)decorator.GetValue(TabContent.TabControlProperty);
        }

        public static DataTemplate? GetTemplate(TabControl tabControl)
        {
            return (DataTemplate?)tabControl.GetValue(TabContent.TemplateProperty);
        }

        public static DataTemplateSelector? GetTemplateSelector(TabControl tabControl)
        {
            return (DataTemplateSelector?)tabControl.GetValue(TabContent.TemplateSelectorProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetContentManager(TabControl tabControl, ContentManager? value)
        {
            tabControl.SetValue(TabContent.ContentManagerProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetPersist(TabControl tabControl, bool value)
        {
            tabControl?.SetValue(TabContent.PersistProperty, BooleanBoxes.GetBox(value));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetPersistedContent(TabItem tabItem, UIElement? value)
        {
            tabItem?.SetValue(TabContent.PersistedContentProperty, value);
        }

        public static void SetTabControl(Decorator decorator, TabControl? value)
        {
            decorator?.SetValue(TabContent.TabControlProperty, value);
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
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(TabContent.TabControlProperty, new Binding
            {
                RelativeSource = new RelativeSource { AncestorType = typeof(TabControl) }
            });
            return new DataTemplate { VisualTree = factory };
        }

        private static ContentManager GetContentManager(TabControl tabControl, Decorator container)
        {
            ContentManager? manager = TabContent.GetContentManager(tabControl);
            if (manager == null)
            {
                TabContent.SetContentManager(tabControl, manager = new ContentManager(tabControl));
            }
            manager.Container = container;
            return manager;
        }

        private static void PersistProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabControl tabControl))
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                if (tabControl.ContentTemplate != null)
                {
                    throw new InvalidOperationException($"{nameof(TabControl)}.{nameof(TabControl.ContentTemplate)} must be null");
                }
                tabControl.ContentTemplate = TabContent._contentTemplate;
                TabContent._contentTemplateDescriptor.AddValueChanged(tabControl, TabControl_ContentTemplateChanged);
            }
            else
            {
                TabContent._contentTemplateDescriptor.RemoveValueChanged(tabControl, TabControl_ContentTemplateChanged);
                tabControl.ContentTemplate = null;
            }

            static void TabControl_ContentTemplateChanged(object sender, EventArgs e)
            {
                throw new InvalidOperationException($"Can not assign to { nameof(TabControl) }.{ nameof(TabControl.ContentTemplate)} when {nameof(TabContent)}.Persist is true.");
            }
        }

        private static void TabControlProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Decorator container) || e.NewValue == null)
            {
                return;
            }
            TabControl tabControl = (TabControl)e.NewValue;
            TabContent.GetContentManager(tabControl, container).UpdateSelectedTab();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class ContentManager : IDisposable
        {
            private readonly TabControl _tabControl;
            private Decorator? _container;

            public ContentManager(TabControl tabControl)
            {
                (this._tabControl = tabControl).SelectionChanged += this.TabControl_SelectionChanged;
            }

            public Decorator? Container
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
                TabItem? tabItem;
                object? item = this._tabControl.SelectedItem;
                if (item == null || (tabItem = (TabItem?)this._tabControl.ItemContainerGenerator.ContainerFromItem(item)) == null)
                {
                    return null;
                }
                if (TabContent.GetPersistedContent(tabItem) is UIElement element)
                {
                    return element;
                }
                ContentPresenter contentPresenter = new ContentPresenter
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
}
