# ArrayBuffer
Just use Span<T> if you can. I couldn't.


# History
Here's some history; I wanted to get rid of codes like this:
```csharp
list.Add("aaa");
list.Add("bbb");
list.Add("ccc");

if(x)
{
    list.Add("ddd");
    list.Add("eee");
}
else
{
    list.Add("fff");
    list.Add("ggg");
    list.Add("hhh");
}

```
So I first tried doing something like this:
```csharp
var list = new List<string>
{
    "aaa",
    "bbb",
    "ccc",
};

if(x)
{
    var additions = new[]
    {
        "ddd",
        "eee",
    };
    list.AddRange(additions);
}
else
{
    var additions = new[]
    {
        "fff",
        "ggg",
        "hhh",
    };
    list.AddRange(additions);
}

```
Which made it look cleaner, but I wasn't so happy about making so many arrays just for the sake of making the code look cleaner.

`ArrayBuffer` makes the code look like this:
```csharp
var buffer = new ArrayBuffer<string>(3);
var list = new List<string>();

// Yay! Local functions!
void AddRange(ArrayBuffer<string>.Span span) { using (span) list.AddRange(span); }

AddRange(new ArrayBuffer<string>.Span(buffer, 3)
{
  "aaa",
  "bbb",
  "ccc",
});

if(x)
{
    AddRange(new ArrayBuffer<string>.Span(buffer, 2)
    {
      "ddd",
      "eee",
    });
}
else
{
    AddRange(new ArrayBuffer<string>.Span(buffer, 2)
    {
      "fff",
      "ggg",
      "hhh",
    });
}

```

Which is still readable, and no more multiple allocations. Just keep the initial size the max size you need (`new ArrayBuffer<string>(3)`).

# Usage

1. Create a buffer first.

```csharp
var buffer = new ArrayBuffer<string>(10);
```

2. There are 2 ways you can create buffers.
    - Use `Take`.
    ```csharp
    var span = buffer.Take(10);
    ```
    - Create a new `Span`
    ```csharp
    var span = new ArrayBuffer<string>.Span(buffer, 10);
    ```
    - These are both essentially the same (`Take` just calls `new Span`) but creating a `Span` will give you the capability to use the Collection Initializer.

3. Add items like normal arrays.
```csharp
span[0] = "hello";
```

4. When you're done, call `Return`. After calling `Return`, you will no longer be able to access the items in the Span. Although accessing them will not crash your program, you should just put the `Span` in a `using` block.
```csharp
span.Return();

// better
using(var span = buffer.Take(10))
{
    ...
}
```

# Notice

**Do not use the `Add` method!**
The `Add` method is only there for using Collection Initializers. You will get an exception if you try to use `Add`.
