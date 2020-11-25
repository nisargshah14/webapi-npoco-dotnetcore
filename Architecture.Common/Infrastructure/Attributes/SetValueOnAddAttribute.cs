using System;
using System.Collections.Generic;
using System.Text;

namespace Architecture.Common.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SetValueOnAddAttribute : Attribute
    {
        public SetValueOnAddAttribute(int setValue)
        {
            SetValue = setValue;
        }
        public int SetValue { get; set; }
    }
    public enum SetValueConstants
    {
        CurrentTime = 1, LoggedInUserId
    }
}
