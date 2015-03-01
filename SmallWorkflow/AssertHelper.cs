using System;

namespace SmallWorkflow
{
    internal static class AssertHelper
    {
        public static void ThrowIfNull(object obj, string name)
        {
            if (obj != null) return;
            throw new ArgumentNullException(name);
        }
    }
}
