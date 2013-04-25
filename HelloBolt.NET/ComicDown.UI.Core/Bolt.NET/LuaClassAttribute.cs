using System;
using System.Collections.Generic;
using System.Text;

namespace ComicDown.UI.Core.Bolt
{
    public enum CreatePolicy { Factory, Singleton}

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LuaClassAttribute:Attribute
    {
        private CreatePolicy createPolicy;
        public CreatePolicy CreatePolicy
        {
            get
            {
                return this.createPolicy;
            }
        }
        public LuaClassAttribute(CreatePolicy policy)
        {
            this.createPolicy = policy;
        }
    }
}