namespace Original
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Md5Properties;

    static class Program
    {
        static void Main()
        {
            var properties = typeof(Foo)
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(SignedAttribute)))
                .OrderBy(p => p.Name)
                .ToArray();

            var foo = new Foo
            {
                Id = 1,
                Name = "Thing",
                Price = 123.45m,
                Thing = SomeEnum.Two,
                Timestamp = new DateTime(2015, 04, 07)
            };

            byte[] hash;
            var sw = Stopwatch.StartNew();
            using (var md5 = MD5.Create())
            {
                for (var i = 0; i < 300000; i++)
                {
                    foreach (var property in properties)
                    {
                        var value = ValueToString(property.GetValue(foo));
                        var bytes = Encoding.UTF8.GetBytes(value);
                        md5.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                    }
                }

                md5.TransformFinalBlock(new byte[0], 0, 0);
                hash = md5.Hash;
            }

            sw.Stop();

            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            Console.WriteLine(sb.ToString());
            Console.WriteLine(sw.Elapsed);
        }

        static string ValueToString(object value)
        {
            if (value == null)
                return "";

            string value_string = value as string;
            if (value_string != null)
                return value_string;

            // This is slowest place.
            if (value is Enum)
                return ((int)value).ToString("X");

            // This is second slow.
            // Dummy formatter to trim trailing zeros in decimal
            // Is it possible to speed up this?
            decimal value_decimal;
            if (TryCast(value, out value_decimal))
                return value_decimal.ToString(
                    "0.############################",
                    CultureInfo.InvariantCulture);

            DateTime value_DateTime;
            if (TryCast(value, out value_DateTime))
                return value_DateTime.Ticks.ToString("X");

            // Actually, not used for slow table.
            // All props are string, Enum, decimal or DateTime there.
            return Convert.ToString(value,
                CultureInfo.InvariantCulture);
        }

        static bool TryCast<T>(object o, out T r)
        {
            if (o is T)
            {
                r = (T)o;
                return true;
            }

            r = default(T);
            return false;
        }
    }
}
