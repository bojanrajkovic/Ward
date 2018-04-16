using System;

namespace Ward.Benchmarks
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class RunInCIAttribute : System.Attribute
    {
    }
}
