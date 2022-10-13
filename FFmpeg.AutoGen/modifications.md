This file states modifications to FFmpeg.AutoGen.

## `Native/FunctionLoader.cs`
- Line 36, 54: Added symbols `NET35` and `NETSTANDARD2_0`

## `Native/LibraryLoader.cs`
- Line 17: Added symbols `NET35` and `NETSTANDARD2_0`
- Line 33: Removed version suffix

## `FFmpeg.arrays.g.cs`
- Line 60~63, 68: Commented two methods because their return type triggers a bug of Unity IL2CPP ([UUM-14015](https://issuetracker.unity3d.com/issues/function-with-a-return-type-of-void-star-causes-a-c-plus-plus-compiler-error-on-il2cpp), fixed in 2021.3.9f1, 2022.1.12f1, 2023.1.0a5)
