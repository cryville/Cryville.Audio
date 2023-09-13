﻿using System;

namespace Microsoft.Windows.PropSys {
	internal enum CONDITION_OPERATION : UInt32 {
		IMPLICIT = 0,
		EQUAL,
		NOTEQUAL,
		LESSTHAN,
		GREATERTHAN,
		LESSTHANOREQUAL,
		GREATERTHANOREQUAL,
		VALUE_STARTSWITH,
		VALUE_ENDSWITH,
		VALUE_CONTAINS,
		VALUE_NOTCONTAINS,
		DOSWILDCARDS,
		WORD_EQUAL,
		WORD_STARTSWITH,
		APPLICATION_SPECIFIC,
	}
}
