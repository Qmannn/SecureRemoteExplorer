using System.Collections.Generic;
using ExplorerClient.Core;

namespace ExplorerClient.Gui
{
    public static class StaticValueBox
    {
        public static string Key { get; set; } = null;
        public static List<User> Users { get; set; } = null;
        public static string Comment { get; set; } = null;
    }
}