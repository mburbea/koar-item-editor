using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class ChangeDefinitionViewModel
    {
        private readonly ItemModel _item;
        private uint _typeId;

        public ChangeDefinitionViewModel(ItemModel item)
        {
            this._typeId = (this._item = item).TypeId;
            List<TypeDefinition> definitions = Amalur.TypeDefinitions.Values.Where(definition => definition.Category == item.Category).ToList();
            if (!Amalur.TypeDefinitions.ContainsKey(this._typeId))
            {
                definitions.Add(item.Item.GetTypeDefinition());
            }
            this.Definitions = definitions;
            this.ChangeDefinitionCommand = new DelegateCommand(this.ChangeDefinition, () => this._typeId != this._item.TypeId);
        }

        public event EventHandler? TypeIdChanged;

        public EquipmentCategory Category => this._item.Category;

        public DelegateCommand ChangeDefinitionCommand { get; }

        public IReadOnlyList<TypeDefinition> Definitions { get; }

        public string DisplayName => this._item.ItemDisplayName;

        public uint TypeId
        {
            get => this._typeId;
            set
            {
                if (this._typeId == value)
                {
                    return;
                }
                this._typeId = value;
                this.TypeIdChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ChangeDefinition()
        {
            if (!Amalur.TypeDefinitions.TryGetValue(this._typeId, out TypeDefinition definition))
            {
                return;
            }
            this._item.LoadFromTypeDefinition(definition);
            Window window = Application.Current.Windows.OfType<ChangeDefinitionWindow>().Single();
            window.DialogResult = true;
            window.Close();
        }
    }
}
