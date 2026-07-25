/** 
* Scorpio
* @author JiangHao 
* @date 2019.3
*/

using System.Collections.Generic;

namespace VeryFS.Framework.Runtime.Utilities
{
    public class I18N
    {
        public string Value { get; private set; }
        public string ID { get; }

        public I18N(string id_, string value_)
        {
            ID = id_;
            Value = value_;
        }

        public override string ToString()
        {
            return Value;
        }
        
        private static readonly  Dictionary<string, I18N> s_data = new Dictionary<string, I18N>(); // 翻译字符串集合
        private static string s_language = "unknown";

        public static string Language => s_language;

        public static void InitLanguage(string language,Dictionary<string,string> data)
        {
            s_language = language;
            foreach (var item in data)
            {
                if (s_data.TryGetValue(item.Key, out var obj))
                {
                    obj.Value = item.Value;
                }
                else
                {
                    obj = new I18N(item.Key, item.Value);
                    s_data.Add(item.Key, obj);
                }
            }
        }

        public string Format(params object[] args)
        {
            return string.Format(Value, args);
        }

        public static implicit operator string(I18N i18N)
        {
            return i18N.Value;
        }
        
        public static I18N Str(string id,string desc="")
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!s_data.TryGetValue(id, out var value))
            {
                var val = id;
                if (!string.IsNullOrEmpty(desc))
                    val = desc;
                
                value=new I18N(id, val);
                s_data.Add(value.ID, value);
            }
            return value;
        }
    }
}