using System;
using System.Reflection;

namespace TheThingBelow.Tests;

/// <summary>
/// One object of the built Game assembly, which a test reads and calls with no engine (D-614).
/// Tests takes no reference to Game, so each call goes through reflection, and a fault of the
/// called member reaches the test as its own exception.
/// </summary>
internal sealed class GameValue
{
    /// <summary>The namespace of the UI types of Game.</summary>
    public const string Ui = "TheThingBelow.Game.Ui.";

    private GameValue(object value) => this.Value = value;

    /// <summary>The object of the Game assembly.</summary>
    public object Value { get; }

    /// <summary>Wraps an object that a member of the Game assembly gave.</summary>
    /// <param name="value">The object.</param>
    /// <returns>The wrapper.</returns>
    public static GameValue Of(object value) => new(value ?? throw new ArgumentNullException(nameof(value)));

    /// <summary>Makes an object of one type of the UI of Game.</summary>
    /// <param name="type">The name of the type, with no namespace, such as `MenuPath`.</param>
    /// <param name="arguments">The arguments of the constructor.</param>
    /// <returns>The object.</returns>
    public static GameValue New(string type, params object?[] arguments)
    {
        try
        {
            return new GameValue(Activator.CreateInstance(GameAssemblyFile.Type(Ui + type), arguments)!);
        }
        catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            throw thrown.InnerException;
        }
    }

    /// <summary>Calls a static method of one type of the UI of Game.</summary>
    /// <param name="type">The name of the type, with no namespace.</param>
    /// <param name="method">The name of the method.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The value that the method gave.</returns>
    public static object? Static(string type, string method, params object?[] arguments)
    {
        try
        {
            return GameAssemblyFile.Type(Ui + type).GetMethod(method)!.Invoke(null, arguments);
        }
        catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            throw thrown.InnerException;
        }
    }

    /// <summary>Reads a static property of one type of the UI of Game.</summary>
    /// <param name="type">The name of the type, with no namespace.</param>
    /// <param name="property">The name of the property.</param>
    /// <returns>The value.</returns>
    public static object? StaticProperty(string type, string property) =>
        GameAssemblyFile.Type(Ui + type).GetProperty(property)!.GetValue(null);

    /// <summary>Reads a constant of one type of the UI of Game.</summary>
    /// <param name="type">The name of the type, with no namespace.</param>
    /// <param name="name">The name of the constant.</param>
    /// <returns>The value.</returns>
    public static object Constant(string type, string name) =>
        GameAssemblyFile.Type(Ui + type).GetField(name)!.GetValue(null)!;

    /// <summary>Gives one value of an enum of the UI of Game.</summary>
    /// <param name="type">The name of the enum, with no namespace.</param>
    /// <param name="name">The name of the value.</param>
    /// <returns>The value.</returns>
    public static object Enum(string type, string name) => System.Enum.Parse(GameAssemblyFile.Type(Ui + type), name);

    /// <summary>Calls a method of the object.</summary>
    /// <param name="method">The name of the method.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The value that the method gave.</returns>
    public object? Call(string method, params object?[] arguments)
    {
        try
        {
            return this.Value.GetType().GetMethod(method)!.Invoke(this.Value, arguments);
        }
        catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            throw thrown.InnerException;
        }
    }

    /// <summary>Reads a property of the object.</summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="property">The name of the property.</param>
    /// <returns>The value.</returns>
    public T Read<T>(string property)
    {
        try
        {
            return (T)this.Value.GetType().GetProperty(property)!.GetValue(this.Value)!;
        }
        catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            throw thrown.InnerException;
        }
    }

    /// <summary>Reads a property of the object as the name of its value, such as the name of an enum value.</summary>
    /// <param name="property">The name of the property.</param>
    /// <returns>The text of the value.</returns>
    public string Name(string property) => this.Read<object>(property).ToString()!;
}
