namespace Programmed.MAX.Function.Utilities
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object o)
        {
            return o == null;
        }

        public static bool IsNotNull(this object o)
        {
            return o != null;
        }

        public static T AsType<T>(this object o) where T : class
        {
            return o as T;
        }

    }
}
