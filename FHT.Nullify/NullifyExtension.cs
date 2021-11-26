using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FHT.Nullify
{
    public static class NullifyExtension
    {
        internal static class Cache<T>
        {
            public static Func<T, T> Converter { get; set; }
        }
        internal static Func<T, T> makeConverter<T>()
        {
            var type = typeof(T);
            var members = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
                .Select(p => new { Type = p.PropertyType, Name = p.Name })
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(f => new { Type = f.FieldType, Name = f.Name })
                );
            var instExpr = Expression.Parameter(typeof(T), "val");
            var testExpr = members.Select(member =>
            {
                var memberExpr = Expression.PropertyOrField(instExpr, member.Name);
                var defaultExpr = Expression.Default(member.Type);
                return Expression.Equal(memberExpr, defaultExpr);
            }).Aggregate((t1, t2) => Expression.MakeBinary(ExpressionType.AndAlso, t1, t2));

            var body = Expression.Condition(testExpr, Expression.Convert(Expression.Constant(null), type), instExpr);
            return Expression.Lambda<Func<T, T>>(body, instExpr).Compile();
        }

        //	https://github.com/protobuf-net/protobuf-net.Grpc/issues/36
        public static T NullifyIfAllDefault<T>(this T value) where T : class
        {
            if (value == null) return null;
            var converter = Cache<T>.Converter;
            if (converter == null)
            {
                //make converter and cache
                Cache<T>.Converter = converter = makeConverter<T>();
            }
            return converter(value);
        }
    }
}
