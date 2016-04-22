using System;
using System.Linq;

namespace ExplorerClient.Core.Objects
{
    public class RemoteFile 
    {
        public string Uuid { get; set; }

        public string Size { get; set; }

        public string Name { get; set; }

        public string LoadTime { get; set; }

        public string Type => Name != String.Empty ? Name.Split('.').Last() : String.Empty;

        public bool IsEncrypted { get; set; }

        public bool IsDamaged { get; set; }

        public string LastDamageCheck { get; set; }

        public string Owner { get; set; }
    }
}