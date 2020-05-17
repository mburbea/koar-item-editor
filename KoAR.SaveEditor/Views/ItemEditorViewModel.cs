using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ItemEditorViewModel : NotifierBase
    {
        private readonly AmalurSaveEditor _editor;
        private readonly string _initialText;
        private readonly ItemMemoryInfo _item;
        private string _text;

        public ItemEditorViewModel(AmalurSaveEditor editor, ItemMemoryInfo item)
        {
            this._editor = editor;
            this._item = item;
            this._initialText = this._text = string.Join(" ", Array.ConvertAll(item.ItemBytes, x => x.ToString("X2")));
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
        }

        public string ItemName => this._item.ItemName;

        public DelegateCommand SaveCommand
        {
            get;
        }

        public string Text
        {
            get => this._text;
            set => this.SetValue(ref this._text, value);
        }

        private bool CanSave() => this._text != this._initialText;

        private byte[]? GetTextBytes()
        {
            List<byte> list = new List<byte>();
            foreach (string word in this.Text.Trim().Split(' '))
            {
                string text = word.Trim();
                if (text.Length == 0)
                {
                    continue;
                }
                if (text.Length != 2 || !byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    return default;
                }
                list.Add(b);
            }
            return list.ToArray();
        }

        private void Save()
        {
            byte[]? bytes = this.GetTextBytes();
            if (bytes == null)
            {
                MessageBox.Show("Invalid byte text, (all bytes must be expressed as two character hex).", "KoAR Save Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this._item.ItemBytes = bytes;
            this._editor.WriteEquipmentBytes(this._item, out _);
            ItemEditorView view = Application.Current.Windows.OfType<ItemEditorView>().Single();
            view.DialogResult = true;
            view.Close();
        }
    }
}
