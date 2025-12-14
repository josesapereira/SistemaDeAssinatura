using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncludeAttribute : Attribute
    {
    }
}
