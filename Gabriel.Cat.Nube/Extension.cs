using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.Extension
{
    public static class Extension
    {

        public static Uri Append(this Uri uri, params string[] paths)
        {//source:https://stackoverflow.com/questions/372865/path-combine-for-urls
            return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
        }

    }
}
