using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

/// <summary>
/// A behavior which allows running validation rules on a binding without updating either side of the binding.
/// </summary>
public static class ValidatingPropertyBehavior
{
    public static readonly DependencyProperty ValidatingPropertyProperty = DependencyProperty.RegisterAttached("ValidatingProperty", typeof(DependencyProperty), typeof(ValidatingPropertyBehavior),
        new(ValidatingPropertyBehavior.ValidatingPropertyProperty_PropertyChanged));

    public static DependencyProperty? GetValidatingProperty(DependencyObject dependencyObject) => (DependencyProperty?)dependencyObject.GetValue(ValidatingPropertyBehavior.ValidatingPropertyProperty);

    public static void SetValidatingProperty(DependencyObject dependencyObject, DependencyProperty? property) => dependencyObject.SetValue(ValidatingPropertyBehavior.ValidatingPropertyProperty, property);

    private static void DependencyProperty_ValueChanged(object? sender, EventArgs e)
    {
        if (sender is DependencyObject dependencyObject && ValidatingPropertyBehavior.GetValidatingProperty(dependencyObject) is { } property)
        {
            dependencyObject.Validate(property);
        }
    }

    private static void Validate(object value, IEnumerable<ValidationRule> validationRules, BindingExpressionBase bindingExpression)
    {
        foreach (ValidationRule rule in validationRules)
        {
            if (rule.Validate(value, null) is { IsValid: false, ErrorContent: object errorContent })
            {
                Validation.MarkInvalid(bindingExpression, new(rule, bindingExpression.ParentBindingBase, errorContent, null));
                return;
            }
        }
        Validation.ClearInvalid(bindingExpression);
    }

    private static void Validate(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
    {
        BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpressionBase(dependencyObject, dependencyProperty);
        if (bindingExpression == null)
        {
            return;
        }
        ICollection<ValidationRule> rules = bindingExpression.ParentBindingBase switch
        {
            Binding binding => binding.ValidationRules,
            MultiBinding multiBinding => multiBinding.ValidationRules,
            _ => Array.Empty<ValidationRule>()
        };
        if (rules.Count > 0)
        {
            ValidatingPropertyBehavior.Validate(dependencyObject.GetValue(dependencyProperty), rules, bindingExpression);
        }
    }

    private static void ValidatingPropertyProperty_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is DependencyProperty oldProperty)
        {
            DependencyPropertyDescriptor.FromProperty(oldProperty, d.GetType()).RemoveValueChanged(d, ValidatingPropertyBehavior.DependencyProperty_ValueChanged);
        }
        if (e.NewValue is DependencyProperty newProperty)
        {
            DependencyPropertyDescriptor.FromProperty(newProperty, d.GetType()).AddValueChanged(d, ValidatingPropertyBehavior.DependencyProperty_ValueChanged);
            d.Validate(newProperty);
        }
    }
}
