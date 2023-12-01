using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs;

public readonly struct DataContainer(object? data) : IComparable, IComparable<DataContainer>, IEquatable<DataContainer>
{
    public static readonly IValueConverter CollectionConverter = new DataContainerCollectionConverter();

    public static readonly DataContainer Empty = default;

    public object? Data => data;

    public static bool operator !=(DataContainer left, DataContainer right) => !left.Equals(right);

    public static bool operator ==(DataContainer left, DataContainer right) => left.Equals(right);

    public int CompareTo(DataContainer other) => Comparer.Default.Compare(data, other.Data);

    int IComparable.CompareTo(object? obj)
    {
        return obj is DataContainer other ? this.CompareTo(other) : throw new ArgumentException($"{nameof(obj)} is not a {nameof(DataContainer)}");
    }

    public override bool Equals(object? obj) => obj is DataContainer other && this.Equals(other);

    public bool Equals(DataContainer other) => Object.Equals(data, other.Data);

    public override int GetHashCode() => data == null ? 0 : data.GetHashCode();

    public override string? ToString() => data == null ? string.Empty : data.ToString();

    private sealed class DataContainerCollectionConverter : IValueConverter
    {
        private static readonly Func<object, DataContainer> _getContainer = value => value is DataContainer container ? container : new(value);

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IEnumerable collection
                ? collection.Cast<object>().Select(DataContainerCollectionConverter._getContainer).ToArray()
                : [DataContainerCollectionConverter._getContainer(value)];
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
