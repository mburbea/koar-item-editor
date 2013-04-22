using System;
using System.Collections.Generic;
using System.Text;

namespace KingdomsofAmalurReckoningSaveEditer
{
    /// <summary>
    /// Attribute Information
    /// </summary>
    public class AttributeInfo
    {
        private String attributeId;
        /// <summary>
        /// Attribute ID
        /// </summary>
        public String AttributeId
        {
            get { return attributeId; }
            set { attributeId = value; }
        }
        private String attributeText;
        /// <summary>
        /// Attribute Description
        /// </summary>
        public String AttributeText
        {
            get { return attributeText; }
            set { attributeText = value; }
        }
    }
}
