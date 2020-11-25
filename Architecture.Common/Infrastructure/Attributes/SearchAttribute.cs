using System;
using System.Collections.Generic;
using System.Text;

namespace Architecture.Common.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SearchAttribute : Attribute
    {
    }
}
