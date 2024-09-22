﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VKCodeGen {

   static class Program {
      static readonly string WinUserH = @"C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\WinUser.h";

      static void Main(string[] args) {
         Console.WriteLine("Generating file...");

         var names = new List<string>(); 
         var outputFileContents = new StringBuilder();

         outputFileContents.Append("//\r\n");
         outputFileContents.Append("// THIS FILE IS AUTO-GENERATED BY VKCodeGen.exe\r\n\r\n");
         outputFileContents.Append("#pragma once\r\n\r\n");

         outputFileContents.Append("#include \"KeyPress.h\"\r\n\r\n");

         outputFileContents.Append("namespace Renfrew::Win32::Interop {\r\n");

         // Numeric Digits.
         for (int digit = '0'; digit <= '9'; digit++) {
            var name = $"D{(char)digit}";
            AddKeyPressClassDef(outputFileContents, name, value: digit);
            names.Add(name);
         }

         // English/Latin Letters.
         for (int letter = 'A'; letter <= 'Z'; letter++) {
            var name = $"{(char)letter}";
            AddKeyPressClassDef(outputFileContents, name, value: letter);
            names.Add(name);
         }

         foreach (var line in File.ReadLines(WinUserH)) {
            var matches = Regex.Matches(line, @"^#define VK_([^ ]+)\s+(0x[0-9A-F]+)");

            if (matches.Count > 0) {
               var name = Pascalize(matches[0].Groups[1].Value);
               var value = Convert.ToUInt16(matches[0].Groups[2].Value, 16);

               // Skip VK_LSHIFT, VK_RSHIFT, etc..
               if (value >= 0xA0 && value <= 0xA5) {
                  continue;
               }

               bool isExtended =
                  name == "Shift" ||
                  name == "Control" ||
                  name == "Menu" ||
                  name == "Win";
               ;

               if (isExtended) {
                  AddExtendedKeyPressClassDef(outputFileContents, name, value);
               } else {
                  AddKeyPressClassDef(outputFileContents, name, value);
               }

               names.Add(name);
            }
         }

         outputFileContents.Append("    public ref class Key abstract sealed {\r\n");

         foreach (var name in names) {
            outputFileContents.AppendFormat(
               "        public: static initonly {0,-32}^ {1,-32} = gcnew {2,-35};\r\n", $"{name}Key", name, $"{name}Key"
            );
         }

         outputFileContents.Append("    };\r\n"); // End of Key class.

         outputFileContents.Append("}\r\n"); // End of namespace.

         if (args.Length > 0) {
            File.WriteAllText(args[0], outputFileContents.ToString());
         } else {
            File.WriteAllText("Keys.h", outputFileContents.ToString());
         }

         Console.WriteLine("Done!");
      }

      static void AddKeyPressClassDef(StringBuilder builder, string name, int value) {
         var classdef =
            "    public ref class {0}Key sealed : KeyPress {{\r\n" +
            "        public: property WORD VirtualKeyCode {{ virtual WORD get() override {{ return {1,5}; }} }}\r\n" +
            "    }};\r\n\r\n"; 

         builder.AppendFormat(classdef, name, value);
      }

      static void AddExtendedKeyPressClassDef(StringBuilder builder, string name, int value) {
         var classdef =
            "    public ref class {0}Key sealed : ExtendedKeyPress {{\r\n" +
            "        public: property WORD VirtualKeyCode {{ virtual WORD  get() override {{ return {1,5}; }} }}\r\n" +
            "    }};\r\n\r\n";

         builder.AppendFormat(classdef, name, value);
      }

      static String Pascalize(string source) {
         switch (source) {
            case "LWIN": return "LWin";
            case "RWIN": return "RWin";
            case "LBUTTON": return "LButton";
            case "RBUTTON": return "RButton";
         }

         var parts = source
            .Split('_')
            .Select(e => e.ToLower())
            .Select(e => {
               var chars = e.ToCharArray();
                
               chars[0] = char.ToUpper(chars[0]);

               return new string(chars);
             });

         return String.Join("", parts);
      }
   }
}
