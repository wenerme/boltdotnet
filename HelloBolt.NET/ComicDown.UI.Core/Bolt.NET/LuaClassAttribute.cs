using System;

namespace ComicDown.UI.Core.Bolt
{
    public enum CreatePolicy { Factory, Singleton}

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LuaClassAttribute:Attribute
    {
        private readonly CreatePolicy _createPolicy;
        public CreatePolicy CreatePolicy
        {
            get
            {
                return _createPolicy;
            }
        }
        public LuaClassAttribute(CreatePolicy policy)
        {
            _createPolicy = policy;
        }
    }
}