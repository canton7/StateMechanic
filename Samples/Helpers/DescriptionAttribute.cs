using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public int Order { get; }

        public DescriptionAttribute(string description, [CallerLineNumber] int order = 0)
        {
            this.Description = description;
            this.Order = order;
        }
    }
}
