using System;
using System.Collections.Generic;
using System.IO;

namespace OpenEpl.TextECode
{
    public class EComSearcher : IEComSearcher
    {
        public static EComSearcher Default = new(Array.Empty<string>());

        public EComSearcher(IEnumerable<string> dirs)
        {
            Dirs = dirs ?? throw new ArgumentNullException(nameof(dirs));
        }

        public IEnumerable<string> Dirs { get; }

        public string Search(string name, string path)
        {
            if (File.Exists(path))
            {
                return path;
            }
            var fileName = Path.GetFileName(path);
            foreach (var dir in Dirs)
            {
                var possible = Path.Combine(dir, fileName);
                if (File.Exists(possible))
                {
                    return possible;
                }
            }
            return null;
        }
    }
}
