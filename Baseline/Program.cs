namespace Baseline
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    using Md5Properties;

    static class Program
    {
        static void Main()
        {
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
                    var value = FooToString(foo);
                    var bytes = Encoding.UTF8.GetBytes(value);
                    md5.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
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

        private static string FooToString(Foo foo)
        {
            var sb = new StringBuilder();
            sb.Append(foo.Name);
            sb.Append(foo.Price.ToString(
                "0.############################",
                CultureInfo.InvariantCulture));
            sb.Append(((int) foo.Thing).ToString("X"));
            sb.Append(foo.Timestamp.Ticks.ToString("X"));
            return sb.ToString();
        }
    }
}
