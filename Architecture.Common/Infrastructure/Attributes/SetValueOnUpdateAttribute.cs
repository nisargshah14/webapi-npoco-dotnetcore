using System;
using System.Collections.Generic;
using System.Text;

namespace Architecture.Common.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SetValueOnUpdateAttribute : Attribute
    {
        public SetValueOnUpdateAttribute(int setValueOnUpdate)
        {
            SetValueOnUpdate = setValueOnUpdate;
        }
        public int SetValueOnUpdate { get; set; }
    }
}
