using System;
using System.Collections.Generic;
using System.Dynamic;

namespace NEmberJS.Validations
{
    public class Errors : DynamicObject
    {
        private readonly Dictionary<string, object> _properties;

        public Errors(Dictionary<string, object> properties)
        {
            _properties = properties;
        }

        public Errors()
        {
            _properties = new Dictionary<string, object>();
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                result = _properties[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                _properties[binder.Name] = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateErrorItem(string key, string value)
        {
            if (_properties.ContainsKey(key))
                AddNewErrorMessageToArray(key, value);
            else
            {
                _properties.Add(key, new List<String> { value });
            }

        }

        private void AddNewErrorMessageToArray(string key, string message)
        {
            var itemKey = _properties[key];
            var array = (List<string>)(itemKey);
            array.Add(message);
        }

        public int Length()
        {
            return _properties.Count;
        }

    }
}
