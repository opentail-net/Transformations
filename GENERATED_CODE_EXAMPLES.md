# Generated Code Examples - Transformations.Mapping Optimizations

This document shows actual generated code examples from the optimized source generator.

## Example 1: Basic User Mapping with All Optimizations

### Source Code
```csharp
[MapTo<UserDto>]
public partial class User
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
}

public class UserDto
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
}
```

### Generated Code (Simplified)
```csharp
// ========== INSTANCE MAPPING ==========

partial class User
{
	public UserDto ToUserDto()
	{
		return new UserDto
		{
			Id = this.Id,
			Name = this.Name,
			Email = this.Email,
		};
	}

	public void MapToUserDto(UserDto target)
	{
		target.Id = this.Id;
		target.Name = this.Name;
		target.Email = this.Email;
	}
}

// ========== COLLECTION EXTENSIONS ==========

public static class UserMappingExtensions
{
	// 1. LAZY ENUMERABLE (with AggressiveInlining)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<UserDto> ToUserDtoEnumerable(
		this IEnumerable<User> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		foreach (var item in source)
		{
			yield return item == null ? null! : item.ToUserDto();
		}
	}

	// 2. ASYNC ENUMERABLE (NEW!)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async IAsyncEnumerable<UserDto> ToUserDtoAsyncEnumerable(
		this IAsyncEnumerable<User> source,
		[EnumeratorCancellation] CancellationToken ct = default)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		await foreach (var item in source.WithCancellation(ct))
		{
			yield return item == null ? null! : item.ToUserDto();
		}
	}

	// 3. GENERIC LIST (fallback for IEnumerable)
	public static List<UserDto> ToUserDtoList(
		this IEnumerable<User> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		if (source is ICollection<User> collection)
		{
			var list = new List<UserDto>(collection.Count);
			foreach (var item in source)
			{
				list.Add(item == null ? null! : item.ToUserDto());
			}
			return list;
		}

		return global::System.Linq.Enumerable.ToList(
			source.ToUserDtoEnumerable());
	}

	// 4. LIST FAST-PATH (CollectionsMarshal + PreAllocation + AggressiveInlining)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<UserDto> ToUserDtoList(
		this List<User> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var list = new List<UserDto>(source.Count);  // Pre-allocate exact capacity
		foreach (var item in CollectionsMarshal.AsSpan(source))  // Zero-copy span
		{
			list.Add(item == null ? null! : item.ToUserDto());
		}
		return list;
	}

	// 5. GENERIC ARRAY (fallback for IEnumerable)
	public static UserDto[] ToUserDtoArray(
		this IEnumerable<User> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		if (source is ICollection<User> collection)
		{
			var array = new UserDto[collection.Count];
			int i = 0;
			foreach (var item in source)
			{
				array[i++] = item == null ? null! : item.ToUserDto();
			}
			return array;
		}

		return global::System.Linq.Enumerable.ToArray(
			source.ToUserDtoEnumerable());
	}

	// 6. ARRAY FAST-PATH (Bounds-Elision + AggressiveInlining)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static UserDto[] ToUserDtoArray(
		this User[] source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var array = new UserDto[source.Length];
		for (int i = 0; i < source.Length; i++)  // Bounds can be eliminated
		{
			var item = source[i];
			array[i] = item == null ? null! : item.ToUserDto();
		}
		return array;
	}

	// 7. MEMORY FAST-PATH (Zero-Copy + AggressiveInlining)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static UserDto[] ToUserDtoArray(
		this Memory<User> source)
	{
		var span = source.Span;  // Zero-copy span view
		var array = new UserDto[span.Length];
		for (int i = 0; i < span.Length; i++)
		{
			var item = span[i];
			array[i] = item == null ? null! : item.ToUserDto();
		}
		return array;
	}

	// 8. READONLY MEMORY FAST-PATH (Zero-Copy + AggressiveInlining)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static UserDto[] ToUserDtoArray(
		this ReadOnlyMemory<User> source)
	{
		var span = source.Span;  // Zero-copy readonly span view
		var array = new UserDto[span.Length];
		for (int i = 0; i < span.Length; i++)
		{
			var item = span[i];
			array[i] = item == null ? null! : item.ToUserDto();
		}
		return array;
	}

	// 9. QUERYABLE PROJECTION (Static Expression Caching)
	private static readonly Expression<Func<User, UserDto>> 
		s_projectToUserDtoExpression = item => new UserDto()
		{
			@Id = item.Id,
			@Name = item.Name,
			@Email = item.Email,
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IQueryable<UserDto> ProjectToUserDto(
		this IQueryable<User> source)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		return Queryable.Select(source, s_projectToUserDtoExpression);
	}
}
```

---

## Example 2: Struct Type Optimization

### Source Code
```csharp
[MapTo<PointDto>]
public partial struct Point  // VALUE TYPE
{
	public int X { get; set; }
	public int Y { get; set; }
}

public struct PointDto
{
	public int X { get; set; }
	public int Y { get; set; }
}
```

### Generated Code - Key Differences for Structs
```csharp
public static class PointMappingExtensions
{
	// ... standard methods omitted ...

	// STRUCT LIST FAST-PATH (in parameter + foreach in for zero copies)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<PointDto> ToPointDtoList(
		in List<Point> source)  // <-- 'in' prevents List copy
	{
		var list = new List<PointDto>(source.Count);
		foreach (in var item in CollectionsMarshal.AsSpan(source))  // <-- 'in' prevents struct copies
		{
			list.Add(item.ToPointDto());  // No null check - it's a struct
		}
		return list;
	}

	// STRUCT ARRAY FAST-PATH (in parameter for zero copies)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PointDto[] ToPointDtoArray(
		in Point[] source)  // <-- 'in' prevents array copy
	{
		var array = new PointDto[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = source[i].ToPointDto();  // Struct: no null check needed
		}
		return array;
	}

	// Standard array fast-path ALSO generated (overload)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PointDto[] ToPointDtoArray(
		this Point[] source)  // <-- Standard extension method
	{
		var array = new PointDto[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			var item = source[i];
			array[i] = item == null ? null! : item.ToPointDto();  // Null-safe for reference types
		}
		return array;
	}
}
```

**Key Differences for Struct Types:**
1. `in` parameter modifier prevents copying the collection itself
2. `foreach in` prevents copying each struct element
3. No null checks needed inside struct mappings
4. Both `in` and standard overloads generated for maximum compatibility

---

## Example 3: Async Streaming Usage

### Usage Pattern
```csharp
// Async data streaming from Entity Framework
var users = dbContext.Users.AsAsyncEnumerable();

// NEW: Direct async enumerable mapping with cancellation support
var cts = new CancellationTokenSource(timeout: TimeSpan.FromSeconds(30));

await foreach (var userDto in 
	users.ToUserDtoAsyncEnumerable(cts.Token))
{
	await ProcessUserAsync(userDto);

	// Cancellation is properly propagated
	if (cts.Token.IsCancellationRequested)
		break;
}
```

### Generated Signature
```csharp
public static async IAsyncEnumerable<UserDto> ToUserDtoAsyncEnumerable(
	this IAsyncEnumerable<User> source,
	[EnumeratorCancellation] CancellationToken ct = default)
```

**Benefits:**
- Cancellation token properly threaded through async iteration
- No allocations from expression compilation
- Type-safe mapping during async enumeration
- Direct replacement for manual async mapping code

---

## Example 4: Memory<T> for High-Performance Buffers

### Usage Pattern
```csharp
// Scenario: Processing user data from network buffer
public class NetworkBuffer
{
	private byte[] buffer = new byte[1024];

	public UserDto[] DecodeUsers(int count)
	{
		// Unsafe pointer conversion (advanced)
		GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		try
		{
			// Cast to Memory<User> for zero-copy mapping
			Memory<User> userMemory = new Memory<User>(/*...*/);
			return userMemory.ToUserDtoArray();  // NEW!
		}
		finally
		{
			handle.Free();
		}
	}
}

// Scenario 2: Stack-allocated temporary buffer
public UserDto[] ProcessStackAllocUsers()
{
	Span<User> tempBuffer = stackalloc User[100];

	// Fill tempBuffer...

	// Wrap in Memory for mapping
	return new ReadOnlyMemory<User>(tempBuffer.ToArray()).ToUserDtoArray();
}
```

### Generated Code
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static UserDto[] ToUserDtoArray(
	this Memory<User> source)
{
	var span = source.Span;  // Zero-copy conversion
	var array = new UserDto[span.Length];
	for (int i = 0; i < span.Length; i++)
	{
		var item = span[i];
		array[i] = item == null ? null! : item.ToUserDto();
	}
	return array;
}
```

---

## Performance Characteristics

### Method Call Overhead Elimination
```
[MethodImpl(AggressiveInlining)]
public static List<T> ToXxxList(this List<T> source)
{
	// JIT will inline this at call site:
	var list = new List<T>(source.Count);
	foreach (var item in CollectionsMarshal.AsSpan(source))
	{
		list.Add(item.ToXxx());
	}
	return list;
}

// Usage: source.ToXxxList() 
// Compiled as if you wrote the loop directly!
```

### Allocation Reduction Pattern
```
Pre-allocation:           new List<T>(source.Count)
↓
Exact capacity =          No resizes during Add()
↓
Single allocation =       Minimal GC pressure
↓
Zero temporary objects =  Better cache locality
```

### Span-Based Fast Path
```
CollectionsMarshal.AsSpan(list)
↓
Stack-allocated Span<T> struct
↓
Direct array pointer access
↓
Hardware can elide bounds checks
↓
Baseline-level performance
```

---

## Compilation Options

These optimizations work with all .NET targets:
- ✅ .NET 8
- ✅ .NET 9
- ✅ .NET 10
- ✅ .NET Standard 2.0 (with limitations on async features)

All collections used are from `System.Collections.Generic`, ensuring compatibility.
