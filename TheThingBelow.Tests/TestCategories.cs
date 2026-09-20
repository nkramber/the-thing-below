namespace TheThingBelow.Tests;

/// <summary>
/// The test categories of this project (D-592). The test command of the Makefile and of the CI
/// jobs excludes the Smoke category, and a test of that category starts the Godot build. No
/// test carries the trait yet, and the PR that adds the first one adds the CI step that runs
/// the category.
/// </summary>
public static class TestCategories
{
    /// <summary>The name of the trait that holds the category.</summary>
    public const string Trait = "Category";

    /// <summary>The category of a test that starts the Godot build.</summary>
    public const string Smoke = "Smoke";

    /// <summary>The option of the test command that excludes the Smoke category.</summary>
    public const string ExcludeSmoke = "--filter-not-trait \"" + Trait + "=" + Smoke + "\"";
}
