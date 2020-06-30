using System;

namespace MPN.Framework.Data
{
    public struct SortProperty
    {
        #region Properties
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null || value.Trim().Length == 0)
                    throw new ArgumentException("A property cannot have an empty name.");

                _name = value.Trim();
            }
        }

        private bool _descending;
        public bool Descending
        {
            get { return _descending; }
            set { _descending = value; }
        }
        #endregion

        #region Constructors
        public SortProperty(string propertyName)
        {
            if (propertyName == null || propertyName.Trim().Length == 0)
                throw new ArgumentException("A property cannot have an empty name.");

            _name = propertyName.Trim();
            _descending = false;
        }

        public SortProperty(string propertyName, bool sortDescending)
        {
            if (propertyName == null || propertyName.Trim().Length == 0)
                throw new ArgumentException("A property cannot have an empty name.");

            _name = propertyName.Trim();
            _descending = sortDescending;
        }
        #endregion
    }
}