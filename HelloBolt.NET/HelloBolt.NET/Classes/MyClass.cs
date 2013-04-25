using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelloBolt.NET
{
    internal sealed class MyClass
    {
        public event Action<int> OnAddFinish;
        public int Add(int lhs,int rhs)
        {
            int result = lhs + rhs;
            if(OnAddFinish  != null)
            {
                OnAddFinish(result);
            }
            return result;
        }
    }
}