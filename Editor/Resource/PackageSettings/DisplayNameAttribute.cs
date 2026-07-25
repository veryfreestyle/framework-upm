

using System;

namespace VeryFS.Framework.Editor
{
    /// <summary>
    /// 编辑器显示名字
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName;
        public int Order;

        public DisplayNameAttribute(string name,int order=0)
        {
            this.DisplayName = name;
            this.Order = order;
        }
    }
}