using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class TypeExtensions
    {
        public static bool InheritsOrImplements(this Type child, Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                                   ? child.GetGenericTypeDefinition()
                                   : child;

            while (currentChild != typeof(object))
            {
                if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
                    return true;

                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition()
                                   : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }
            return false;
        }

        public static bool IsNumber(this Type @this)
        {
            @this = Nullable.GetUnderlyingType(@this) ?? @this;

            if (@this.IsPrimitive)
            {
                return @this != typeof(bool) &&
                    @this != typeof(char) &&
                    @this != typeof(IntPtr) &&
                    @this != typeof(UIntPtr);
            }

            return @this == typeof(decimal);
        }


        private static bool HasAnyInterfaces(Type parent, Type child)
        {
            return child.GetInterfaces()
                .Any(childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                });
        }

        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = true;
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent)
                shouldUseGenericType = false;

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();
            return parent;
        }

        // https://stackoverflow.com/questions/5114469/how-to-check-whether-an-object-has-certain-method-property
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        // C# extension methods: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
        // https://stackoverflow.com/questions/521687/foreach-with-index
        public static void Enumerate<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            /* 
             * example usage:
             * 
            var strings = new List<string>();
            strings.Enumerate((str, n) =>
            {
                // hooray
            });
            */
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }



        //Enum.GetValues(typeof(ModifierValue)).Cast<ModifierValue>().Max();

        // https://stackoverflow.com/questions/203377/getting-the-max-value-of-an-enum 
        // https://www.davidyardy.com/blog/visual-studio%E2%80%93how-to-target-differentlatest-c-version-net-core-3-and-c-8/
        // https://devblogs.microsoft.com/dotnet/a-belated-welcome-to-c-7-3/
        public static TEnum GetMaxValue<TEnum>()
        where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Max();
        }

        public static TEnum GetMinValue<TEnum>()
        where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Min();
        }


        public static TEnum GetMaxValueOld<TEnum>()
        where TEnum : Enum
        {
            Type type = typeof(TEnum);

            if (!type.IsSubclassOf(typeof(Enum)))
                throw new
                    InvalidCastException
                        ("Cannot cast '" + type.FullName + "' to System.Enum.");

            return (TEnum)Enum.ToObject(
                type,
                Enum.GetValues(type).Cast<int>().Last()
                );
        }

        // https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
        // return eRat.B.Next();
        public static TEnum EnumRotate<TEnum>(this TEnum src, int step) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(TEnum).FullName));

            TEnum[] Arr = (TEnum[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<TEnum>(Arr, src);

            int count = Arr.Length;
            j = j + step;
            while (true) {
                if (j < 0) {
                    j = j + count;

                } else if (j >= count) {
                    j = j - count;

                } else {
                    break;
                }
            }

            TEnum ret = Arr[j];

            return ret;
        }

        // todo: move to "typeextensions.c"
        // https://stackoverflow.com/questions/3519539/how-to-check-if-a-string-contains-any-of-some-strings
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            // usage: bool anyLuck = s.ContainsAny("a", "b", "c");
            foreach (string needle in needles) {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        // https://stackoverflow.com/questions/20678653/readable-c-sharp-equivalent-of-python-slice-operation
        public static List<T> Slice<T>(this List<T> li, int start, int end)
        {
            if (start < 0)    // support negative indexing
            {
                start = li.Count + start;
            }
            if (end < 0)    // support negative indexing
            {
                end = li.Count + end;
            }
            if (start > li.Count)    // if the start value is too high
            {
                start = li.Count;
            }
            if (end > li.Count)    // if the end value is too high
            {
                end = li.Count;
            }
            var count = end - start;             // calculate count (number of elements)
            return li.GetRange(start, count);    // return a shallow copy of li of count elements
        }

        /*
         Unit test for Slice()

        [Fact]
        public void Slice_list()
        {
            var li1 = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
            Assert.Equal(new List<char> { 'c', 'd' }, li1.Slice(2, 4));
            Assert.Equal(new List<char> { 'b', 'c', 'd', 'e', 'f', 'g' }, li1.Slice(1, li1.Count));
            Assert.Equal(new List<char> { 'a', 'b', 'c' }, li1.Slice(0, 3));
            Assert.Equal(li1, li1.Slice(0, 4).Concat(li1.Slice(4, li1.Count)));
            Assert.Equal(li1, li1.Slice(0, 100));
            Assert.Equal(new List<char>(), li1.Slice(100, 200));

            Assert.Equal(new List<char> { 'g' }, li1.Slice(-1, li1.Count));
            Assert.Equal(new List<char> { 'f', 'g' }, li1.Slice(-2, li1.Count));
            Assert.Equal(new List<char> { 'a', 'b', 'c', 'd', 'e', 'f' }, li1.Slice(0, -1));

            Assert.Equal(new List<char> { 'c', 'd', 'e' }, li1.Slice(2, -2));
        }
        */


        public static Char Next(this Char ch_in, int step=1)
        {
            Char ch_out = (char)(((int)ch_in) + step);
            return ch_out;

        }
    }
}
