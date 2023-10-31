using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.PropSys {
	[ComImport]
	[Guid("6f79d558-3e96-4549-a1d1-7d75d2288814")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IPropertyDescription {
		void GetPropertyKey(out PROPERTYKEY pkey);

		void GetCanonicalName([MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

		void GetPropertyType(out UInt16 pvartype);

		void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

		void GetEditInvitation([MarshalAs(UnmanagedType.LPWStr)] out string ppszInvite);

		void GetTypeFlags(
			PROPDESC_TYPE_FLAGS mask,
			out PROPDESC_TYPE_FLAGS ppdtFlags
		);

		void GetViewFlags(out PROPDESC_VIEW_FLAGS ppdvFlags);

		void GetDefaultColumnWidth(out UInt32 pcxChars);

		void GetDisplayType(out PROPDESC_DISPLAYTYPE pdisplaytype);

		void GetColumnState(out SHCOLSTATE pcsFlags);

		void GetGroupingRange(out PROPDESC_GROUPING_RANGE pgr);

		void GetRelativeDescriptionType(out PROPDESC_RELATIVEDESCRIPTION_TYPE prdt);

		void GetRelativeDescription(
			ref PROPVARIANT propvar1,
			ref PROPVARIANT propvar2,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppszDesc1,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppszDesc2
		);

		void GetSortDescription(out PROPDESC_SORTDESCRIPTION psd);

		void GetSortDescriptionLabel(
			[MarshalAs(UnmanagedType.Bool)] bool fDescending,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppszDescription);

		void GetAggregationType(out PROPDESC_AGGREGATION_TYPE paggtype);

		void GetConditionType(
			out PROPDESC_CONDITION_TYPE pcontype,
			out CONDITION_OPERATION popDefault
		);

		void GetEnumTypeList(
			ref Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);

		/* [local] */
		void CoerceToCanonicalValue(ref PROPVARIANT ppropvar);

		void FormatForDisplay(
			ref PROPVARIANT propvar,
			PROPDESC_FORMAT_FLAGS pdfFlags,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppszDisplay
		);

		void IsValueCanonical(ref PROPVARIANT propvar);
	}

	[ComImport]
	[Guid("1f9fc1d0-c39b-4b26-817f-011967d3440e")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IPropertyDescriptionList {
        int GetCount(out UInt32 pcElem);
        
        int GetAt(
			UInt32 iElem,
			ref Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
    }

	[ComImport]
	[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IPropertyStore {
		void GetCount(out UInt32 cProps);

		void GetAt(
			UInt32 iProp,
			out PROPERTYKEY pkey
		);

		void GetValue(
			ref PROPERTYKEY key,
			out PROPVARIANT pv
		);

		void SetValue(
			ref PROPERTYKEY key,
			ref PROPVARIANT propvar
		);

		void Commit();
	}

	internal static class NativeMethods {
#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("propsys.dll", PreserveSig = false)]
		public static extern int PSEnumeratePropertyDescriptions(
			PROPDESC_ENUMFILTER filterOn,
			ref Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
#if USE_SAFE_DLL_IMPORT
		[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#endif
		[DllImport("propsys.dll", PreserveSig = false)]
		public static extern int PSGetPropertyDescription(
			ref PROPERTYKEY propkey,
			ref Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
	}
	internal enum PROPDESC_AGGREGATION_TYPE : UInt32 {
		DEFAULT   = 0,
		FIRST     = 1,
		SUM       = 2,
		AVERAGE   = 3,
		DATERANGE = 4,
		UNION     = 5,
		MAX       = 6,
		MIN       = 7,
	}
	internal enum PROPDESC_CONDITION_TYPE : UInt32 {
		NONE     = 0,
		STRING   = 1,
		SIZE     = 2,
		DATETIME = 3,
		BOOLEAN  = 4,
		NUMBER   = 5,
	}
	internal enum PROPDESC_DISPLAYTYPE : UInt32 {
		STRING     = 0,
		NUMBER     = 1,
		BOOLEAN    = 2,
		DATETIME   = 3,
		ENUMERATED = 4,
	}
	internal enum PROPDESC_ENUMFILTER : UInt32 {
		ALL             = 0,
		SYSTEM          = 1,
		NONSYSTEM       = 2,
		VIEWABLE        = 3,
		QUERYABLE       = 4,
		INFULLTEXTQUERY = 5,
		COLUMN          = 6,
	}
	[Flags]
	internal enum PROPDESC_FORMAT_FLAGS : UInt32 {
		DEFAULT            =      0,
		PREFIXNAME         =    0x1,
		FILENAME           =    0x2,
		ALWAYSKB           =    0x4,
		// RESERVED_RIGHTTOLEFT =    0x8,
		SHORTTIME          =   0x10,
		LONGTIME           =   0x20,
		HIDETIME           =   0x40,
		SHORTDATE          =   0x80,
		LONGDATE           =  0x100,
		HIDEDATE           =  0x200,
		RELATIVEDATE       =  0x400,
		USEEDITINVITATION  =  0x800,
		READONLY           = 0x1000,
		NOAUTOREADINGORDER = 0x2000,
	}
	internal enum PROPDESC_GROUPING_RANGE : UInt32 {
		DISCRETE     = 0,
		ALPHANUMERIC = 1,
		SIZE         = 2,
		DYNAMIC      = 3,
		DATE         = 4,
		PERCENT      = 5,
		ENUMERATED   = 6,
	}
	internal enum PROPDESC_RELATIVEDESCRIPTION_TYPE : UInt32 {
		GENERAL   =  0,
		DATE      =  1,
		SIZE      =  2,
		COUNT     =  3,
		REVISION  =  4,
		LENGTH    =  5,
		DURATION  =  6,
		SPEED     =  7,
		RATE      =  8,
		RATING    =  9,
		PRIORITY  = 10,
	}
	internal enum PROPDESC_SORTDESCRIPTION : UInt32 {
		GENERAL          = 0,
		A_Z              = 1,
		LOWEST_HIGHEST   = 2,
		SMALLEST_BIGGEST = 3,
		OLDEST_NEWEST    = 4,
	}
	[Flags]
	internal enum PROPDESC_TYPE_FLAGS : UInt32 {
		DEFAULT                   =          0,
		MULTIPLEVALUES            =        0x1,
		ISINNATE                  =        0x2,
		ISGROUP                   =        0x4,
		CANGROUPBY                =        0x8,
		CANSTACKBY                =       0x10,
		ISTREEPROPERTY            =       0x20,
		INCLUDEINFULLTEXTQUERY    =       0x40,
		ISVIEWABLE                =       0x80,
		ISQUERYABLE               =      0x100,
		CANBEPURGED               =      0x200,
		SEARCHRAWVALUE            =      0x400,
		DONTCOERCEEMPTYSTRINGS    =      0x800,
		ALWAYSINSUPPLEMENTALSTORE =     0x1000,
		ISSYSTEMPROPERTY          = 0x80000000,
		MASK_ALL                  = 0x80001fff,
	}
	[Flags]
	internal enum PROPDESC_VIEW_FLAGS : UInt32 {
		DEFAULT             =      0,
		CENTERALIGN         =    0x1,
		RIGHTALIGN          =    0x2,
		BEGINNEWGROUP       =    0x4,
		FILLAREA            =    0x8,
		SORTDESCENDING      =   0x10,
		SHOWONLYIFPRESENT   =   0x20,
		SHOWBYDEFAULT       =   0x40,
		SHOWINPRIMARYLIST   =   0x80,
		SHOWINSECONDARYLIST =  0x100,
		HIDELABEL           =  0x200,
		HIDDEN              =  0x800,
		CANWRAP             = 0x1000,
		MASK_ALL            = 0x1bff,
	}
}
