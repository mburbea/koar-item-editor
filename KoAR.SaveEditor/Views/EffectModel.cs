using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views
{
    public sealed class EffectModel : NotifierBase
    {
        private readonly ItemModel _parent;

        public EffectModel(ItemModel parent, IEffectInfo effect)
        {
            (this._parent, this.Effect) = (parent, effect);
        }

        internal IEffectInfo Effect
        {
            get;
        }

        public uint Code
        {
            get => this.Effect.Code;
            set
            {
                if (this.Effect.Code == value)
                {
                    return;
                }
                this.Effect.Code = value;
                this.OnPropertyChanged();
                this._parent.OnEffectChanged(this.Effect);
            }
        }
    }
}
