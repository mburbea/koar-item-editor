using System;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Attribute Information
    /// </summary>
    public class AttributeInfo
    {
        private String attributeId;
        private String attributeText;

        /// <summary>
        /// Attribute ID
        /// </summary>
        public String AttributeId
        {
            get { return attributeId; }
            set { attributeId = value; }
        }
        
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
