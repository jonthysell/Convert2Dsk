// Copyright (c) 2020 Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Reflection;

namespace Convert2Dsk
{
    public static class AppInfo
    {
        public static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
        private static Assembly _assembly = null;

        public static string Name => _name ??= Assembly.GetName().Name;
        private static string _name = null;

        public static string Version => _version ??= Assembly.GetName().Version.ToString();
        private static string _version = null;
    }
}
