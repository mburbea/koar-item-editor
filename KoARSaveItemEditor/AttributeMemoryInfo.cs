using System;

namespace KoARSaveItemEditor
{
    /// <summary>
    /// Attribute Memory Information
    /// </summary>
    public class AttributeMemoryInfo
    {
        private int[] value;
        /// <summary>
        /// Attribute value
        /// </summary>
        public int[] Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private String code;
        /// <summary>
        /// Attribute Code
        /// </summary>
        public String Code
        {
            get { return this.code; }
            set { this.code = value; }
        }

        private string detail;
        /// <summary>
        /// Attribute Description
        /// </summary>
        public string Detail
        {
            get { return detail; }
            set { detail = value; }
        }
    }
}
