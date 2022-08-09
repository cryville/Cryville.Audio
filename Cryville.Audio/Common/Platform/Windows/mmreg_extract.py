import re

with open("MmReg.cs", "w", newline="\r\n") as fo:
    fo.write("""using System;

namespace Cryville.Common.Platform.Windows.MmReg {
	public enum WAVE_FORMAT : UInt32 {
"""
    )
    with open("mmreg.h", "r") as fi:
        while True:
            l = fi.readline()
            if l == "": break
            l = l.strip()
            if not l.startswith("#define"): continue
            m = re.match(r"#define\s+WAVE_FORMAT_(\S+)\s+(\S+)", l)
            if m == None: continue
            fo.write(f"		WAVE_FORMAT_{m[1]} = {m[2]},\n")
    fo.write("""	}
}"""
    )
