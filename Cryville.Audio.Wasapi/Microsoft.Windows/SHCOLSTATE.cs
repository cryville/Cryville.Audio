using System;

namespace Microsoft.Windows.PropSys {
	internal enum SHCOLSTATE : UInt32 {
		DEFAULT            =       0,
		TYPE_STR           =     0x1,
		TYPE_INT           =     0x2,
		TYPE_DATE          =     0x3,
		TYPEMASK           =     0xf,
		ONBYDEFAULT        =    0x10,
		SLOW               =    0x20,
		EXTENDED           =    0x40,
		SECONDARYUI        =    0x80,
		HIDDEN             =   0x100,
		PREFER_VARCMP      =   0x200,
		PREFER_FMTCMP      =   0x400,
		NOSORTBYFOLDERNESS =   0x800,
		VIEWONLY           = 0x10000,
		BATCHREAD          = 0x20000,
		NO_GROUPBY         = 0x40000,
		FIXED_WIDTH        =  0x1000,
		NODPISCALE         =  0x2000,
		FIXED_RATIO        =  0x4000,
		DISPLAYMASK        =  0xf000,
	}
}
