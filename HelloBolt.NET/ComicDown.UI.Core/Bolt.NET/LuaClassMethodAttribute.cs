using System;

namespace ComicDown.UI.Core.Bolt
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LuaClassMethodAttribute : System.Attribute 
    {
        private readonly string _name;
        private readonly int _permission;
        private readonly bool _deleteOld;
        private readonly bool _hasName;

        public string Name { get { return _name; } }
        public int Permission { get { return _permission; } }
        public bool DeleteOld { get { return _deleteOld; } }
        public bool HasName { get { return _hasName; } }

        public LuaClassMethodAttribute(string name, int permission, bool deleteOld)
        {
            _name = name;
            _permission = permission;
            _deleteOld = deleteOld;
            _hasName =!string.IsNullOrEmpty(name);
        }
        public LuaClassMethodAttribute(string name)
        {
            _name = name;
            _permission = 0;
            _deleteOld = false;
            _hasName = !string.IsNullOrEmpty(name);
        }
        public LuaClassMethodAttribute()
        {
            _permission = 0;
            _deleteOld = false;
            _hasName = false;
        }
    }
}