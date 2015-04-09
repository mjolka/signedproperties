namespace Generated
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Security.Cryptography;
    using System.Text;

    using Md5Properties;

    public static class Program
    {
        static void Main()
        {
            var properties = (typeof(Foo))
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(SignedAttribute)))
                .OrderBy(p => p.Name)
                .ToArray();

            var getSignatureMethod = new DynamicMethod(
                "GetSignature",
                typeof(string),
                new[] { typeof(Foo) },
                typeof(Foo).Module);

            var generator = getSignatureMethod.GetILGenerator();
            generator.DeclareLocal(typeof(StringBuilder));
            generator.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc_0);
            generator.Emit(OpCodes.Ldloc_0);

            var append = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
            foreach (var property in properties)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, property.GetGetMethod());
                if (property.PropertyType.BaseType == typeof(Enum))
                {
                    generator.Emit(OpCodes.Box, property.PropertyType);
                }

                generator.Emit(OpCodes.Call, typeof(Program).GetMethod("ValueToString", new[] { property.PropertyType }));
                generator.Emit(OpCodes.Callvirt, append);
            }

            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString", new Type[] { }));
            generator.Emit(OpCodes.Ret);

            var getSignature = (Func<Foo, string>)
                getSignatureMethod.CreateDelegate(typeof(Func<Foo, string>));

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
                    var value = getSignature(foo);
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

        public static string ValueToString(string value)
        {
            return value ?? string.Empty;
        }

        public static string ValueToString(Enum value)
        {
            return Convert.ToInt32(value).ToString("X");
        }

        public static string ValueToString(decimal value)
        {
            return value.ToString("0.############################", CultureInfo.InvariantCulture);
        }

        public static string ValueToString(DateTime value)
        {
            return value.Ticks.ToString("X");
        }
    }
}