using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    /// <summary>
    /// A behavior which allows running validation rules on a binding without updating either side of the binding.
    /// </summary>
    public static class ValidatingPropertyBehavior
    {
        /// <summary>
        /// Defines the ValidatingProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatingPropertyProperty = DependencyProperty.RegisterAttached("ValidatingProperty", typeof(DependencyProperty), typeof(ValidatingPropertyBehavior),
            new PropertyMetadata(ValidatingPropertyBehavior.ValidatingPropertyProperty_PropertyChanged));

        /// <summary>
        /// Gets the validating dependency property for a dependency object.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns>Dependency property.</returns>
        public static DependencyProperty? GetValidatingProperty(DependencyObject dependencyObject)
        {
            return (DependencyProperty?)dependencyObject?.GetValue(ValidatingPropertyBehavior.ValidatingPropertyProperty);
        }

        /// <summary>
        /// Sets the validating dependency property for a dependency object.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="property">The dependency property.</param>
        public static void SetValidatingProperty(DependencyObject dependencyObject, DependencyProperty? property)
        {
            dependencyObject?.SetValue(ValidatingPropertyBehavior.ValidatingPropertyProperty, property);
        }

        /// <summary>
        /// Called when the value of a validating property changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void DependencyProperty_ValueChanged(object sender, EventArgs e)
        {
            DependencyObject dependencyObject = (DependencyObject)sender;
            DependencyProperty? property = ValidatingPropertyBehavior.GetValidatingProperty(dependencyObject);
            if (property != null)
            {
                dependencyObject.Validate(property);
            }
        }

        /// <summary>
        /// Validate a value against a collection of validation rules.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationRules">The validation rules collection.</param>
        /// <param name="bindingExpression">The binding expression.</param>
        private static void Validate(object value, IEnumerable<ValidationRule>? validationRules, BindingExpressionBase bindingExpression)
        {
            if (validationRules != null)
            {
                foreach (ValidationRule rule in validationRules)
                {
                    ValidationResult result = rule.Validate(value, CultureInfo.InvariantCulture);
                    if (!result.IsValid)
                    {
                        Validation.MarkInvalid(bindingExpression, new ValidationError(rule, bindingExpression.ParentBindingBase, result.ErrorContent, null));
                        return;
                    }
                }
            }
            Validation.ClearInvalid(bindingExpression);
        }

        /// <summary>
        /// Validate that a value of a dependency object/property combination does not violate any binding validation rules.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="dependencyProperty">The dependency property.</param>
        private static void Validate(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpressionBase(dependencyObject, dependencyProperty);
            if (bindingExpression == null)
            {
                return;
            }
            switch (bindingExpression.ParentBindingBase)
            {
                case Binding binding:
                    ValidatingPropertyBehavior.Validate(dependencyObject.GetValue(dependencyProperty), binding.ValidationRules, bindingExpression);
                    break;
                case MultiBinding multiBinding:
                    ValidatingPropertyBehavior.Validate(dependencyObject.GetValue(dependencyProperty), multiBinding.ValidationRules, bindingExpression);
                    break;
            }
        }

        /// <summary>
        /// Called when the value of the ValidatingProperty dependency property changes.
        /// </summary>
        /// <param name="d">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void ValidatingPropertyProperty_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                DependencyProperty property = (DependencyProperty)e.OldValue;
                DependencyPropertyDescriptor.FromProperty(property, d.GetType()).RemoveValueChanged(d, ValidatingPropertyBehavior.DependencyProperty_ValueChanged);
            }
            if (e.NewValue != null)
            {
                DependencyProperty property = (DependencyProperty)e.NewValue;
                DependencyPropertyDescriptor.FromProperty(property, d.GetType()).AddValueChanged(d, ValidatingPropertyBehavior.DependencyProperty_ValueChanged);
                d.Validate(property);
            }
        }
    }
}
