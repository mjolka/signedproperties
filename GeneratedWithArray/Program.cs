using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Sigil;
using Sigil.NonGeneric;

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

            var emiter = Emit<Func<Foo, byte[]>>.NewDynamicMethod("GetSignature");
            var enumType = typeof(Enum);
            var valueToStringLookup = new Dictionary<Type, MethodInfo>()
                                      {
                                          {typeof(decimal), typeof(Program).GetMethod("ValueToString", new[] { typeof(decimal) })},
                                          {typeof(string), typeof(Program).GetMethod("ValueToString", new[] { typeof(string) })},
                                          {typeof(SomeEnum), typeof(Program).GetMethod("ValueToString", new[] { typeof(SomeEnum) })},
                                          {typeof(DateTime), typeof(Program).GetMethod("ValueToString", new[] { typeof(DateTime) })}
                                      };
            var writeToStream = typeof(MemoryStream).GetMethod("Write", new[] { typeof(byte[]), typeof(int), typeof(int) });
            using (var stream = emiter.DeclareLocal<MemoryStream>("stream"))
            using (var interimArray = emiter.DeclareLocal<byte[]>("pa"))
            {
                emiter.NewObject<MemoryStream>();
                emiter.StoreLocal(stream);
                for (int index = 0; index < 3; index++)
                {
                    var prop = properties[index];
                    emiter.LoadArgument(0);
                    emiter.CallVirtual(prop.GetGetMethod());
                    if (prop.PropertyType.BaseType == enumType)
                    {
                        emiter.Call(valueToStringLookup[prop.PropertyType]);
                    }
                    else
                    {
                        emiter.Call(valueToStringLookup[prop.PropertyType]);
                    }
                    emiter.StoreLocal(interimArray);
                    emiter.LoadLocal(stream);
                    emiter.LoadLocal(interimArray);
                    emiter.LoadConstant(0);
                    emiter.LoadLocal(interimArray);
                    emiter.LoadLength<byte>();
                    emiter.Call(writeToStream);
                }

                emiter.LoadLocal(stream);
                emiter.Call(typeof(MemoryStream).GetMethod("ToArray", new Type[] { }));
                emiter.Return();
            }
            //var getSignatureMethod = new DynamicMethod(
            //    "GetSignature",
            //    typeof(byte[]),
            //    new[] { typeof(Foo) },
            //    typeof(Foo).Module);

            //var generator = getSignatureMethod.GetILGenerator();
            //var stream =generator.DeclareLocal(typeof(MemoryStream));
            //var interimArray = generator.DeclareLocal(typeof (byte[]));
            //generator.Emit(OpCodes.Newobj, typeof(MemoryStream).GetConstructor(Type.EmptyTypes));
            //generator.Emit(OpCodes.Stloc_0);
            //generator.Emit(OpCodes.Ldloc_0);

            //var write = typeof(MemoryStream).GetMethod("Write", new[] { typeof(byte[]), typeof(int), typeof(int) });
            //foreach (var property in properties)
            //{
            //    generator.Emit(OpCodes.Ldarg_0);
            //    generator.Emit(OpCodes.Callvirt, property.GetGetMethod());
            //    if (property.PropertyType.BaseType == typeof(Enum))
            //    {
            //        generator.Emit(OpCodes.Box, property.PropertyType);
            //    }

            //    generator.Emit(OpCodes.Call, typeof(Program).GetMethod("ValueToString", new[] { property.PropertyType }));
            //    generator.Emit(OpCodes.Stloc_1);
            //    generator.Emit(OpCodes.Ldloc_1);
            //    generator.Emit(OpCodes.Ldc_I4_0);
            //    generator.Emit(OpCodes.Ldloc_1);
            //    generator.Emit(OpCodes.Ldlen);
            //    generator.Emit(OpCodes.Callvirt, write);
            //}

            //generator.Emit(OpCodes.Pop);
            //generator.Emit(OpCodes.Ldloc_0);
            //generator.Emit(OpCodes.Callvirt, typeof(MemoryStream).GetMethod("ToArray", new Type[] { }));
            //generator.Emit(OpCodes.Ret);

            //var getSignature = (Func<Foo, byte[]>)
            //    getSignatureMethod.CreateDelegate(typeof(Func<Foo, byte[]>));

            var getSignature = emiter.CreateDelegate();

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
                    md5.TransformBlock(value, 0, value.Length, value, 0);
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
            Console.ReadKey();
        }

        public static byte[] ValueToString(string value)
        {
            return Encoding.UTF8.GetBytes(value ?? string.Empty);
        }

        public static byte[] ValueToString(SomeEnum value)
        {
            return BitConverter.GetBytes((int)value);
        }

        public static byte[] ValueToString(decimal value)
        {
            return BitConverter.GetBytes(decimal.ToDouble(value));
        }

        public static byte[] ValueToString(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
        }
    }
}
