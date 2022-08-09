from glob import glob
import re

def f(p, b):
    while len(p) < b: p = "0" + p
    return p

fl = glob("*.h")
fl.sort()
with open("PKeys.cs", "w", newline="\r\n") as fo:
    fo.write("""using Cryville.Common.Platform.Windows.PropSys;
using System.Collections.Generic;

namespace Cryville.Common.Platform.Windows {
	public static class PKeys {
		public readonly static Dictionary<PROPERTYKEY, string> Keys = new Dictionary<PROPERTYKEY, string> {
"""
    )
    pl = {}
    for i in fl:
        with open(i, "r") as fi:
            while True:
                l = fi.readline()
                if l == "": break
                l = l.strip()
                if not l.startswith("DEFINE_"): continue
                m = re.match(r".*?\(.*?_(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*0x(.*?),\s*(.*?)\);", l)
                if m == None: continue
                pl[f'("{f(m[2],8)}-{f(m[3],4)}-{f(m[4],4)}-{f(m[5],2)}{f(m[6],2)}-{f(m[7],2)}{f(m[8],2)}{f(m[9],2)}{f(m[10],2)}{f(m[11],2)}{f(m[12],2)}", {m[13]})'.lower()] = f'System.{m[1].replace("_", ".")}'
    for i in pl:
        fo.write(f'			{{ new PROPERTYKEY{i}, "{pl[i]}" }},\n')
    fo.write("""		};
	}
}"""
    )
