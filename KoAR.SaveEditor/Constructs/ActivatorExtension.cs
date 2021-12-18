using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace KoAR.SaveEditor.Constructs;

public sealed class ActivatorExtension : MarkupExtension
{
    private static readonly Dictionary<Type, Func<object>> _functions = new();

    public ActivatorExtension(Type type) => this.Type = type;

    [ConstructorArgument("type")]
    public Type Type { get; }

    public override object ProvideValue(IServiceProvider serviceProvider) => ActivatorExtension.GetFunction(this.Type).Invoke();

    private static Func<object> GetFunction(Type type)
    {
        if (!ActivatorExtension._functions.TryGetValue(type, out Func<object>? function))
        {
            ActivatorExtension._functions.Add(type, function = Expression.Lambda<Func<object>>(
                Expression.Convert(
                    Expression.New(type),
                    typeof(object)
                )
            ).Compile());
        }
        return function;
    }
}
