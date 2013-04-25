using System;
using System.Collections.Generic;
using System.Text;

namespace ComicDown.UI.Core.Bolt
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LuaClassMethodAttribute : System.Attribute 
    {
        private string name;
        private int permission;
        private bool deleteOld;
        private readonly bool hasName;

        public string Name { get { return name; } }
        public int Permission { get { return permission; } }
        public bool DeleteOld { get { return deleteOld; } }
        public bool HasName { get { return hasName; } }

        public LuaClassMethodAttribute(string name, int permission, bool deleteOld)
        {
            this.name = name;
            this.permission = permission;
            this.deleteOld = deleteOld;
            this.hasName =!string.IsNullOrEmpty(name);
        }
        public LuaClassMethodAttribute(string name)
        {
            this.name = name;
            this.permission = 0;
            this.deleteOld = false;
            this.hasName = !string.IsNullOrEmpty(name);
        }
        public LuaClassMethodAttribute()
        {
            this.permission = 0;
            this.deleteOld = false;
            this.hasName = false;
        }
    }
}