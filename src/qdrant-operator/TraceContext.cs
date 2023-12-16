using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Neon.Diagnostics;


namespace QdrantOperator
{
    internal static class TraceContext
    {
        internal static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();

        internal static readonly string ActivitySourceName = AssemblyName.Name;

        internal static readonly Version Version = AssemblyName.Version;

        internal static ActivitySource ActivitySource => Cached.Source.Value;

        static class Cached
        {
            internal static readonly Lazy<ActivitySource> Source = new Lazy<ActivitySource>(
            () =>
            {
                return new ActivitySource(ActivitySourceName, Version.ToString());
            });
        }
    }
}
