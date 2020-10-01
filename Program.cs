using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using IKVM.Reflection;
using Type = IKVM.Reflection.Type;

namespace ikvmbug {
	class MainClass {
		
		public static void Main (string [] args)
		{
			var uniA = new Universe (UniverseOptions.EnableFunctionPointers | UniverseOptions.ResolveMissingMembers | UniverseOptions.MetadataOnly);
			var uniB = new Universe (UniverseOptions.EnableFunctionPointers | UniverseOptions.ResolveMissingMembers | UniverseOptions.MetadataOnly);

			var dir = Path.GetDirectoryName (System.Reflection.Assembly.GetExecutingAssembly ().Location);
			while (!File.Exists (Path.Combine (dir, "a.dll")))
				dir = Path.GetDirectoryName (dir);

			var a = uniA.LoadFile (Path.Combine (dir, "a.dll"));
			var b = uniB.LoadFile (Path.Combine (dir, "b.dll"));

			var n = "QuickLook.IQLPreviewItem";
			foreach (var asm in new [] { b, a }) {
				Console.WriteLine (asm.Location);
				var t = asm.GetType (n);
				Console.WriteLine ($"{t.FullName}: MetadataToken {t.MetadataToken} CustomAttributeCount: {t.CustomAttributes.Count ()}");

				var attribCount = 0;
				for (var i = 0; i < t.Module.CustomAttribute.records.Length; i++) {
					var ca = t.Module.CustomAttribute.records [i];
					if (ca.Parent == t.MetadataToken) {
						attribCount++;
					}
				}
				if (t.CustomAttributes.Count () != attribCount)
					Console.WriteLine ($"❌ FAIL: missing {attribCount - t.CustomAttributes.Count ()} attributes on the type");
				else
					Console.WriteLine ("✅ OK: found all attributes");
			}
		}
	}
}
