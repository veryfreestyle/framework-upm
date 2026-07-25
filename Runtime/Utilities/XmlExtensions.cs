using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace VeryFS.Framework.Runtime.Utilities
{
    public static class XmlExtensions
    {
        public static Vector2[] GetAttributeAsVector2Array(this XElement element, string name)
        {
            if (element == null)
            {
                return null;
            }

            var data = element.GetAttributeAs<string>(name);
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var vectors = from v in data.Split(' ')
                let a = v.Split(',').ToArray()
                let x = Convert.ToSingle(a[0], CultureInfo.InvariantCulture)
                let y = Convert.ToSingle(a[1], CultureInfo.InvariantCulture)
                select new Vector2(x, y);
            return vectors.ToArray();
        }

        public static Color GetAttributeAsColor(this XElement element, string name, Color defaultColor)
        {
            XAttribute attr = element.Attribute(name);
            if (attr == null)
            {
                return defaultColor;
            }

            string htmlColor = element.GetAttributeAs<string>(name);
            return htmlColor.ToColor(defaultColor);
        }

        public static T GetAttributeAs<T>(this XElement element, string name, T defaultValue) where T : IConvertible
        {
            if (element == null)
            {
                return defaultValue;
            }

            var attribute = element.Attribute(name);
            if (attribute == null)
            {
                return defaultValue;
            }

            string value = attribute.Value;

            // Special case for enum
            if (typeof(T).IsEnum)
            {
                return value.ToEnum<T>();
            }

            // Special case for bool
            if (typeof(T) == typeof(bool))
            {
                if (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    value = bool.TrueString;
                }
                else if (value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    value = bool.FalseString;
                }
            }

            if (value.IndexOf('.') > 0 && (typeof(T) == typeof(int) || typeof(T) == typeof(uint)))
            {
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float resultFloat))
                {
                    value = ((int)resultFloat).ToString();
                }
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"Failed to convert attribute '{name}' with value '{value}' to type {typeof(T)}: {e.Message}");
                throw;
            }

        }

        public static T GetAttributeAs<T>(this XElement element, string name) where T : IConvertible
        {
            return element.GetAttributeAs<T>(name, default(T));
        }

    }

}
