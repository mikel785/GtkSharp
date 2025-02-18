// GtkSharp.Generation.EnumGen.cs - The Enumeration Generatable.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001 Mike Kestner
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


using System.Runtime.InteropServices;

namespace GtkSharp.Generation {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using System.Text.RegularExpressions;

	public class EnumGen : GenBase {
		
		string enum_type = String.Empty;
		IList<string> members = new List<string> ();

		public EnumGen (XmlElement ns, XmlElement elem) : base (ns, elem)
		{
			foreach (XmlElement member in elem.ChildNodes) {
				if (member.Name != "member")
					continue;

				string result = "\t\t" + member.GetAttribute("name");
				if (member.HasAttribute ("value")) {
					string value = member.GetAttribute ("value").Trim ();
					foreach (Match match in Regex.Matches (value, "[0-9]+([UL]{1,2})", RegexOptions.IgnoreCase)) {
						switch (match.Groups[1].Value.ToUpper ()) {
							case "U": enum_type = " : uint"; break;
							case "L": enum_type = " : long"; break;
							case "UL": enum_type = " : ulong"; break;
						}
					}
					result += " = " + value;
				}
				members.Add (result + ",");
			}
			if (elem.HasAttribute ("enum_type"))
				enum_type = " : " + elem.GetAttribute ("enum_type");
		}

		public override bool Validate ()
		{
			return true;
		}

		public override string DefaultValue {
			get {
				return "(" + QualifiedName + ") 0";
			}
		}

		public override string MarshalType {
			get {
				return "int";
			}
		}

		public override string CallByName (string var_name)
		{
			return "(int) " + var_name;
		}
		
		public override string FromNative(string var)
		{
			return "(" + QualifiedName + ") " + var;
		}

		public override string GenerateAlign () {
			return null;
		}

		
		public override void Generate (GenerationInfo gen_info)
		{
			StreamWriter sw = gen_info.OpenStream (Name, NS);

			sw.WriteLine ("namespace " + NS + " {");
			sw.WriteLine ();
			sw.WriteLine ("\tusing System;");
			sw.WriteLine ("\tusing System.Runtime.InteropServices;");
			sw.WriteLine ();

			sw.WriteLine ("#region Autogenerated code");
					
			if (Elem.GetAttribute("type") == "flags")
				sw.WriteLine ("\t[Flags]");
			if (Elem.HasAttribute("gtype"))
				sw.WriteLine ("\t[GLib.GType (typeof (" + NS + "." + Name + "GType))]");

			string access = IsInternal ? "internal" : "public";
			sw.WriteLine ("\t" + access + " enum " + Name + enum_type + " {");
			sw.WriteLine ();
				
			foreach (string member in members)
				sw.WriteLine (member);

			sw.WriteLine ("\t}");
			if (Elem.HasAttribute ("gtype")) {
				sw.WriteLine ();
				sw.WriteLine ("\tinternal class " + Name + "GType {");
                var funcname = Elem.GetAttribute("gtype");
                sw.WriteLine ("\t\t[UnmanagedFunctionPointer (CallingConvention.Cdecl)]");
                sw.WriteLine ("\t\tdelegate IntPtr d_" + funcname + "();");
				sw.WriteLine ("\t\tstatic d_" + funcname + " " + funcname + " = FuncLoader.LoadFunction<d_" + funcname + ">(FuncLoader.GetProcAddress(GLibrary.Load(\"" + LibraryName + "\"), \"" + funcname + "\"));");
				sw.WriteLine ();
				sw.WriteLine ("\t\tpublic static GLib.GType GType {");
				sw.WriteLine ("\t\t\tget {");
				sw.WriteLine ("\t\t\t\treturn new GLib.GType (" + Elem.GetAttribute ("gtype") + " ());");
				sw.WriteLine ("\t\t\t}");
				sw.WriteLine ("\t\t}");
				sw.WriteLine ("\t}");
			}

			sw.WriteLine ("#endregion");
			sw.WriteLine ("}");
			sw.Close ();
			Statistics.EnumCount++;
		}
	}
}

