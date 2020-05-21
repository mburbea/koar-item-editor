using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace KoAR.SaveEditor.Constructs
{
    public readonly struct DataContainer : IEquatable<DataContainer>
    {
        public static readonly IValueConverter CollectionConverter = new DataContainerCollectionConverter();

        public static readonly DataContainer Empty = default;

        public static readonly IValueConverter ItemConverter = new DataContainerItemConverter();

        public DataContainer(object? data) => this.Data = data;

        public object? Data
        {
            get;
        }

        public static bool operator !=(DataContainer left, DataContainer right) => !left.Equals(right);

        public static bool operator ==(DataContainer left, DataContainer right) => left.Equals(right);

        public override bool Equals(object obj) => obj is DataContainer other && this.Equals(other);

        public bool Equals(DataContainer other) => Object.Equals(this.Data, other.Data);

        public override int GetHashCode() => this.Data == null ? 0 : this.Data.GetHashCode();

        private sealed class DataContainerCollectionConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value switch
                {
                    IEnumerable collection => collection.Cast<object>().Select(DataContainerCollectionConverter.Convert).ToArray(),
                    _ => new[] { DataContainerCollectionConverter.Convert(value) }
                };
            }

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

            private static DataContainer Convert(object value) => value is DataContainer container ? container : new DataContainer(value);
        }

        private sealed class DataContainerItemConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => new DataContainer(value);

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
