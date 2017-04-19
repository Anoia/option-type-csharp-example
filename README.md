# An option type in C# #

## Abstract

If you’ve been programming C# you’ve probably shouted at your screen because of a NullReferenceException at some point in the past. This usually happens because some method returned null when you weren’t expecting it and thus did not handle that possibility in your code. Null is often (ab)used to represent absent or optional values, which makes unexpected exceptions even more likely. But there’s a better way: an option type!
It’s a strongly typed alternative to null values that not only lets you avoid NullReferenceExceptions but also declare your intent and write more focused code.
I’ll give you an overview of this awesome concept and how to use it even though C# does not have a built-in option type.

## Intro

So, what is this option type?

An **option type** is a generic type that can encapsulate an optional value. In other words, it's a container that may or may not contain a value.
A lot of functional programming languages have option types build in, like F#, Scala or OCaml. In some other languages like Haskell it's called **maybe**, but the concept is the same.

It's very useful for example as a return value for a function, that could return a missing or invalid value.

Here's an example from F#:

```fsharp
type Option<'t> =
   | Some of 't
   | None

 let someValue = Some 1
 let noValue = None

 match someValue with
 | Some x -> printfn "the value is %A" x
 | None -> printfn "the value is None"
```

`Option<T>` is a container for an optional value of type `T`. If the value is present, `Option<T>` is an instance of `Some<T>`, which contains the given value. If there is no value, then `Option<T>` is `None`.

In F#, Option replaces null almost completely!

### Why would I do that?

Using an option type has a few advantages over null. It allows you to reduce the amount of possible NullReferenceExceptions and at the same time cut down on manual null checks, which focuses your code and allows for a more concise definition of the business logic. In addition to that using an option type instead of null signals your intent and lets you model your data more explicitly.

But before continuing with that, let's look at what C# offers us out of the box.

## What about null and Nullable?

So, what exactly is null again? According to [MSDN](https://msdn.microsoft.com/en-us/library/edakx9da.aspx):
> The null keyword is a literal that represents a null reference, one that does not refer to any object.

### Null and type safety

The problem with null is, that is has the same type as the object it could be referring to. This means the type system doesn’t tell you when you have null.

```csharp
string s1 = "abc";
int len1 = s1.Length;

string s2 = null;
int len2 = s2.Length;
```
While the above code obviously produces an exception, the compiler doesn't prevent me from making that mistake. The "null string" appears to have all the same properties and functions as the valid string, except that your code will blow up when you try to use it!

The same wouldn't be possible with an option type: `Option<string>` is of type `Option` and not `string`, so the compiler will prevent me from accidentally accessing the `Length` property on a non existing value! You won't be able to do that until you've *safely* extracted it and *made a choice* about what to do.

### Null vs missing data
As stated above, null in C# represents a reference that doesn’t point to anything. This is completely different from the concept of “missing” data, which is a valid part of modelling any system in any language. Null is often used to represent missing data though.

### Option vs Nullable
The basic idea of an option type and Nullable (see [MSDN](https://msdn.microsoft.com/en-us/library/1t3y8s4s.aspx)) is the same, only Nullable is much weaker. It only works on value types (like `int` or `DateTime`), but not on reference types such as strings or classes. Nullable also doesn't provide much special behavior.

### Null checking in C# #
C# provides some convenience operators when it comes to null values:
```csharp
string s1 = null;
var length = s1 != null ? s1.length : 0;
var length = s1?.length ?? 0;
```

The `??` operator is called null-coalescing operator. It returns the left-hand operand if the operand is not null; otherwise it returns the right hand operand.

The `?.` operator is called null-propagation operator and allows for null checks within invocation chains. It will short-circuit and return null if anything in the chain is null.

These two operators are great and make null checks much easier! However, you are in trouble if you forget to use this operator, and nothing forces you to do so. Hello, unexpected NullReferenceException!

## Option to the rescue!

As I've indicated in the beginning an option type is a much better way to represent optional or invalid data than `null` or `Nullable` are.

Using option instead of null to model missing or invalid data cuts down on manual null checks, which simplifies the control flow of your program, but that's not all. It also avoids null checks you're forgetting to do, which would lead to a runtime exception! The type signature will tell you if a value is optional and require you to handle the case where the value is missing.

And when you write functions that return options instead of a value that could be null, you also make the behavior explicit and again force the caller to handle both cases.

Alright, that sounds awesome but we're still writing C# and not F#...

### An option type in C# #

And although C# does not bring it's own option type there are smart people who implemented it and provide us with a robust and well tested library!

[https://github.com/nlkl/Optional](https://github.com/nlkl/Optional)

### Optional
The Optional library provides a strongly typed alternative to `null` in C#. It's easy to use and has lots of cool features:

First and foremost it provides an implementation for **Option/Maybe** and **Either**

The option/maybe type behaves similar to the functional programming concept we've talked about: `Option<T>` can be `Some<T>` when a value `T` exists or `None` if it doesn't.

The either type is similar, but instead of `None` it provides another *exceptional* value indicating why there is no value or why an operation regarding the optional value failed.

In addition to that, the implementation prevents 'unsafe' access to the internal value of an `Option<T>`. It forces the user to check if a value is actually present, thereby mitigating many of the problems of null values.
You can still retrieve the value directly if you want to (whether it's there or not), but you need to explicitly state that you want to access it in an unsafe way, it can't sneak in by accident.

The library provides a lot of utility methods to make working with the option type more convenient. The most noteworthy feature that makes this implementation of the option type so great is the possibility to treat the option type like a collection with zero or one values. It's possible to apply map and filter functions to the value **without ever unpacking it from it's `Option` container**!

Let me repeat that: I don't actually need unpack the value, I can modify, filter or map the value by defining functions, that are applied only if a value is present.

I didn't realize how great this is until I actually implemented it in a real-world project.

## Example

To demonstrate the benefits of using Optional I created a small example project. It is inspired by a real-world project where Optional helped me write simpler and more robust code.

The function I implemented represents part of the licensing logic of an application. It is called `GetActivation()` and it tries to retrieve a valid activation to check if the application can start. First, it checks if a valid activation is already stored on the computer. If it finds one, it just returns it. If there is no stored activation or if the stored activation is not valid for some reason (maybe it expired or is corrupted) it tries to find a license key and activate it by contacting the license server.
A more detailed representation of the process is displayed in the diagram below.

![Diagram of license logic](option-example.png)

When we follow the 'happy path' the logic is quite simple. The problem is that there a quite a few steps on the way that might fail and need to be handled.

### Implementation using null

Here's the code for an implementation without using the Optional library. Yeah, it's long. Lots of null checks that clutter up the code and make it hard to see what's actually happening. Error handling is also difficult. And I only hope the caller of `GetActivation` will remember to check the return value for null!

You don't need to read the whole thing in detail, just note how incredibly long and complicated it is.

```csharp
public Activation GetActivation()
{
    var savedActivation = GetSavedActivation();
    if (savedActivation != null && savedActivation.IsActivationStillValid())
        return savedActivation;

    var key = savedActivation?.Key ?? ReadKey();
    if (string.IsNullOrWhiteSpace(key))
    {
        Error = LicenseError.NoLicenseKey;
        return null;
    }

    var couldConnect = _connection.TryGetServerResponse(key);
    if (!couldConnect)
    {
        Error = LicenseError.NoServerConnection;  
        return null;
    }
    var licenseStringFromServer = _connection.Response;

    var couldParse = _parser.TryParse(licenseStringFromServer);
    if (!couldParse)
    {
        Error = LicenseError.ParseFailed;
        return null;
    }
    var activationFromServer = _parser.ParsedActivation;

    if (!HasValidActivationTime(activationFromServer))
    {
        Error = LicenseError.InvalidActivationTime;
        return null;
    }

    SaveActivation(activationFromServer);

    return activationFromServer;
}

private Activation GetSavedActivation()
{
   Activation result = null;

   var encodedString = ReadActivationString();

   if (!string.IsNullOrWhiteSpace(encodedString))
   {
       var decoded = Decode(encodedString);
       if (decoded != null)
       {
           var couldParse = _parser.TryParse(decoded);

           if (couldParse)
           {
               result = _parser.ParsedActivation;
           }
           else
           {
               Error = LicenseError.ParseFailed;
           }
       }
       else
       {
           Error = LicenseError.DecodeFailed;
       }
   }
   else
   {
       Error = LicenseError.NoActivation;
   }
   return result;
}
```
I return either the retrieved `Activation` object, or null. If the latter happens, I set the `ErrorCode` property, which can then be read for information on what failed. 
For the parsing and server connection I defined classes that handle those tasks, the methods they provide return a flag that indicates weather the operation was successful or not and provide a property that contains the requested value if they were. 


### Implementation using option type

Alright, on to the nice part!
Now here's the same logic implemented using the Optional library:

```csharp
public Option<Activation, LicenseError> GetActivation()
{
    var savedActivation = ReadActivationString()    //1 
        .FlatMap(Decode)                            
        .FlatMap(TryParseActivation);

    if (savedActivation.Exists(a => a.IsActivationStillValid()))
        return savedActivation;

    var key = savedActivation.Match(
        some: a => a.Key.SomeNotNull(LicenseError.NoLicenseKey),
        none: e => ReadKey());

    var onlineActivation = key.FlatMap(GetActivationStringFromLicenseServer)
        .FlatMap(TryParseActivation)
        .Filter(HasValidActivationTime, LicenseError.InvalidActivationTime);

    onlineActivation.MatchSome(SaveActivation);

    return onlineActivation.HasValue ? onlineActivation : savedActivation;
}
```

Yep, that's it. Really! 

If you're interested in a more detailed explanation: 

The first statement (1) consists of three chained method calls. 
The first call to `ReadActivationString` returns an `Option<string, LicenseError>`. I'm using an either type here and not a maybe because I always want to know why an operation failed. 
There are two possibilities now: The returned option type either contains the string read from the registry or an error code indicating that no license string was found. 

The cool thing is that I don't actually need to check manually which of the two cases I have! I just apply a map function `Decode` to the optional value, without even unpacking the content! The map function is only executed, if my `Option<string, LicenseError>` actually contains a string. That string is then passed to the `Decode` function, which returns another optional value, containing either the decoded string or an error code indicating that the decode failed. What with the map when no string was retrieved from the registry in the first place? Well.. nothing. In that case I had an `Option<string, LicenseError>` that contained an error. When applying a map function it's not executed as there is no value to map. The error code is also not changed though, as the exceptional value in the either type is short-circuiting. 

// TODO

If you want to take a closer look you can find the solution with the rest of the source code [here](https://github.com/Anoia/option-type-csharp-example).


## Conclusion

Alright, so null checking a value is not particularly difficult if you know it could be null. But it's really easy to miss a spot where you're supposed to check, because the type system doesn't help you at all. Using null to represent missing values only makes this worse. 

I hope I could show you that using an option type is a useful alternative when you're dealing with optional values. Take advantage of the help the type system can offer to avoid runtime errors, that only show when something goes wrong, and replace them with compile time errors, that force you to handle all the possible cases while you're writing your code in the first place. 

As shown in the examples, this practice doesn't only lead to less error-prone code, it also vastly simplifies the control flow of the program, making it easier to read, understand and change. 

You can still choose to do the equivalent of `if (null) return` and even directly access optional values in an unsafe way, the difference is that you need to think about what you want to do and then explicitly ask for that behavior, it can't happen accidentally! 

Of course when writing C# we can't eliminate all occurrences of `null`, that's still a language "feature" we have to live with. 
But using an option type to represent optional, invalid or missing data still offer lots of benefits over using null, and we can take advantage of those.


