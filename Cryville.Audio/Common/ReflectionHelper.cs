using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Cryville.Common {
	public static class ReflectionHelper {
		static readonly Type[] emptyTypeArray = {};
		public static ConstructorInfo GetEmptyConstructor(Type type) {
			return type.GetConstructor(emptyTypeArray);
		}
		static readonly object[] emptyObjectArray = {};
		public static object InvokeEmptyConstructor(Type type) {
			return GetEmptyConstructor(type).Invoke(emptyObjectArray);
		}
		
		public static bool TryFindMemberWithAttribute<T>(Type t, out MemberInfo mi) where T : Attribute {
			try {
				mi = FindMemberWithAttribute<T>(t);
				return true;
			}
			catch (MissingMemberException) {
				mi = null;
				return false;
			}
		}
		public static MemberInfo FindMemberWithAttribute<T>(Type type) where T : Attribute {
			var mil = type.FindMembers(
				MemberTypes.Field | MemberTypes.Property,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
				(m, o) => m.GetCustomAttributes(typeof(T), true).Length != 0,
				null
			);
			if (mil.Length != 1)
				throw new MissingMemberException(type.Name, typeof(T).Name);
			return mil[0];
		}

		public static bool IsGenericDictionary(Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
		}
		
		public static MemberInfo GetMember(Type type, string name) {
			var mil = type.GetMember(
				name,
				MemberTypes.Field | MemberTypes.Property,
				BindingFlags.Public | BindingFlags.Instance
			);
			if (mil.Length != 1)
				throw new MissingMemberException(type.Name, name);
			return mil[0];
		}
		
		public static Type GetMemberType(MemberInfo mi) {
			if (mi is FieldInfo)
				return ((FieldInfo)mi).FieldType;
			if (mi is PropertyInfo)
				return ((PropertyInfo)mi).PropertyType;
			else
				throw new ArgumentException();
		}
		
		public static object GetValue(MemberInfo mi, object obj) {
			if (mi is FieldInfo)
				return ((FieldInfo)mi).GetValue(obj);
			else if (mi is PropertyInfo)
				return ((PropertyInfo)mi).GetValue(obj, new object[]{});
			else
				throw new ArgumentException();
		}
		
		public static void SetValue(MemberInfo mi, object obj, object value, Binder binder = null) {
			if (mi is FieldInfo)
				((FieldInfo)mi).SetValue(obj, value, BindingFlags.Default, binder, null);
			else if (mi is PropertyInfo)
				((PropertyInfo)mi).SetValue(obj, value, BindingFlags.Default, binder, emptyObjectArray, null);
			else
				throw new ArgumentException();
		}

		public static Type[] GetSubclassesOf<T>() where T : class {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			IEnumerable<Type> r = new List<Type>();
			foreach (var a in assemblies)
				r = r.Concat(a.GetTypes().Where(
					t => t.IsClass
					&& !t.IsAbstract
					&& t.IsSubclassOf(typeof(T))
				));
			return r.ToArray();
		}
	}
}
