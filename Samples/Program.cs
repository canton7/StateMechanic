using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    class Program
    {
        public static int Main(string[] args)
        {
            var failed = false;

            var types =
                from type in typeof(Program).Assembly.GetTypes()
                let methods = (
                    from method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    let attribute = method.GetCustomAttribute<DescriptionAttribute>()
                    where attribute != null
                    orderby attribute.Order
                    select new { MethodInfo = method, Description = attribute.Description }
                ).ToArray()
                where methods.Any()
                select new { Type = type, Methods = methods };

            foreach (var type in types)
            {
                Console.WriteLine($"==================== {type.Type.Name} ====================");
                foreach (var method in type.Methods)
                {
                    Console.WriteLine();
                    Console.WriteLine($"========== {method.Description} ==========");
                    Console.WriteLine();
                    try
                    {
                        method.MethodInfo.Invoke(null, null);
                    }
                    catch (TargetInvocationException e)
                    {
                        Console.WriteLine($"!!! {e.InnerException.Message}");
                        failed = true;
                    }
                }
            }

            return failed ? 1 : 0;
        }
    }
}
