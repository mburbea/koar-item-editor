using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private bool _editable;
        private string _text;

        public ItemEditorViewModel(AmalurSaveEditor editor, ItemMemoryInfo item)
        {
            this._editor = editor;
            this._item = item;
            this._initialText = this._text = string.Join(" ", Array.ConvertAll(item.ItemBytes, x => x.ToString("X2")));
            this.AllowEditCommand = new DelegateCommand(this.AllowEdit);
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
        }

        public DelegateCommand AllowEditCommand
        {
            get;
        }

        public string ItemName => this._item.ItemName;

        public bool ReadOnly
        {
            get => !this._editable;
            private set => this.SetValue(ref this._editable, !value);
        }

        public DelegateCommand SaveCommand
        {
            get;
        }

        public string Text
        {
            get => this._text;
            set => this.SetValue(ref this._text, value);
        }

        private void AllowEdit() => this.ReadOnly = false;

        private bool CanSave() => this._editable && this._text != this._initialText;

        private byte[]? GetTextBytes()
        {
            List<byte> list = new List<byte>();
            using TextReader reader = new StringReader(this._text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] words = line.Trim().Split(' ');
                foreach (string word in words)
                {
                    string text = word.Trim();
                    if (text.Length == 0)
                    {
                        continue;
                    }
                    if (text.Length != 2)
                    {
                        return default;
                    }
                    list.Add(byte.Parse(text, NumberStyles.HexNumber));
                }
            }
            return list.ToArray();
        }

        private void Save()
        {
            byte[]? bytes = this.GetTextBytes();
            if (bytes == null)
            {
                MessageBox.Show("Invalid byte text");
                return;
            }
            this._item.ItemBytes = bytes;
            this._editor.WriteEquipmentBytes(this._item);
            ItemEditorView view = Application.Current.Windows.OfType<ItemEditorView>().Single();
            view.DialogResult = true;
            view.Close();
        }
    }
}
