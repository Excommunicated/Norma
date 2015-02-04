﻿using System.Reflection;
using System.Threading;
using Xunit;

namespace Norma.Core.Tests
{
    internal class GlobalLockAttribute : BeforeAfterTestAttribute
    {
         private readonly object _globalLock = new object();
         public string Reason { get; set; }

        public override void Before(MethodInfo methodUnderTest)
        {
            Monitor.Enter(_globalLock);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Monitor.Exit(_globalLock);
        }
    }
}