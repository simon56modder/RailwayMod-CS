using UnityEngine;

namespace RailwayMod
{
    public static class Utils
    {
        public static Rect ClampRectToScreen(Rect source)
        {
            var rect = new Rect(source);
            if (rect.width > Screen.width || rect.height > Screen.height)
                return rect;
            if (rect.x < 0)
                rect.x = 0;
            if (rect.y < 0)
                rect.y = 0;
            if (rect.x + rect.width > Screen.width)
                rect.x = Screen.width - rect.width;
            if (rect.y + rect.height > Screen.height)
                rect.y = Screen.height - rect.height;
            return rect;
        }
        public static Rect ChangeHeight(Rect source, float height)
        {
            return new Rect(source.x, source.y, source.width, height);
        }
        public static string GetStringBetween(this string source, string from, string to)
        {
            int pFrom = source.IndexOf(from) + from.Length;
            return source.Substring(pFrom, source.LastIndexOf(to) - pFrom);
        }
    }
}
