﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hazelnut
{
    partial class HazelnutExtensions
    {
        /// <summary>
        /// Returns a DirectoryInfo object of the Website root directory.
        /// </summary>
        public static DirectoryInfo WebsiteRoot(this AppDomain @this)
        {
            var root = @this.BaseDirectory.AsDirectory();

            if (root.Name.StartsWith("net")) return root.Parent.Parent.Parent;
            else return root;
        }

        /// <summary>
        /// Returns DirectoryInfo object of the base directory.
        /// </summary>
        public static DirectoryInfo GetBaseDirectory(this AppDomain @this) => @this.BaseDirectory.AsDirectory();

        /// <summary>
        /// loads an assembly given its name.
        /// </summary>
        public static Assembly LoadAssembly(this AppDomain @this, string assemblyName)
        {
            var result = @this.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName);
            if (result != null) return result;

            // Nothing found with exact name. Try with file name.
            var fileName = assemblyName.EnsureEndsWith(".dll", caseSensitive: false);

            var file = @this.GetBaseDirectory().GetFile(fileName);

            if (file.Exists())
                return Assembly.Load(AssemblyName.GetAssemblyName(file.FullName));

            // Maybe absolute file?
            if (File.Exists(fileName))
                return Assembly.Load(AssemblyName.GetAssemblyName(fileName));

            throw new($"Failed to find the requrested assembly: '{assemblyName}'");
        }

        public static Type[] FindImplementers(this AppDomain @this, Type interfaceType) =>
            @this.FindImplementers(interfaceType, ignoreDrivedClasses: true);

        public static Type[] FindImplementers(this AppDomain @this, Type interfaceType, bool ignoreDrivedClasses)
        {
            var result = new List<Type>();

            foreach (var assembly in @this.GetAssemblies()
                .Where(a => a == interfaceType.Assembly || a.References(interfaceType.Assembly)))
            {
                try
                {
                    Log.For(typeof(HazelnutExtensions)).Info($"Looking for {interfaceType.FullName} in " + assembly.FullName);

                    foreach (var type in assembly.GetTypes())
                    {
                        Log.For(typeof(HazelnutExtensions)).Info($"Checking {type}");
                        if (type == interfaceType) continue;
                        if (type.IsInterface) continue;

                        if (type.Implements(interfaceType))
                            result.Add(type);
                        else
                            Log.For(typeof(HazelnutExtensions)).Info($"{type} does not implement " + interfaceType.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Log.For(typeof(HazelnutExtensions)).Info($"Could not load assembly {assembly.FullName}");

                    Log.For(typeof(HazelnutExtensions))
                        .Info($"Could not load assembly {assembly.FullName} because: {ex.Message}");

                    Log.For(typeof(HazelnutExtensions))
                        .Info($"Could not load assembly {assembly.FullName} because: {ex.ToFullMessage()}");
                    // Can't load assembly. No logging is needed.
                }
            }

            // For any type, if it's parent is in the list, exclude it:
            if (ignoreDrivedClasses)
            {
                var typesWithParentsIn = result.Where(x => result.Contains(x.BaseType)).ToArray();

                foreach (var item in typesWithParentsIn)
                    result.Remove(item);
            }

            return result.ToArray();
        }

        public static Type GetTypeByName(this AppDomain @this, Type interfaceType, string typeName)
        {
            foreach (var assembly in @this.GetAssemblies().Where(a => a == interfaceType.Assembly || a.References(interfaceType.Assembly)))
            {
                var type = assembly.GetTypes().SingleOrDefault(x => x.Name == typeName);
                if (type != null) return type;
            }

            return null;
        }
    }
}