# Continuous Database Development (CDD) Example

Learn more about SuperOffice Continuous Database:

- [Introduction to SuperOffice Continuous Database](https://community.superoffice.com/en/content/content/database/continuous-database/)

- [Using SuperOffice Continuous Database](https://community.superoffice.com/en/content/content/database/using-continuous-database/)

--- 

## This example consists of 1 solution with 3 projects:

### Solution: LearnContinuousDatabase.sln

1. ExampleDictionaryStep.csproj: DictionaryStep.
2. SuperOffice.DevNet.CDD.csproj: AppDomain for performing updates.
3. LearnContinuousDatabase.csproj: Main project.

## ExampleDictionarySteps

Contains two classes that pertain to one DictionaryStep "DemoFeature".

1. DemoClass1 adds one new table "Demo" with several fields.
2. DemoClassUninstall drops the table "Demo"

Key differences is the DictionaryAttribute decorating each class.

``` CSharp
[Dictionary("DemoFeature", 1, ReleaseState.Released)
public class DemoClass1 : DictionaryStep
{ ... }

[Dictionary("DemoFeature", int.MaxValue, ReleaseState.Released)]
public class DemoClassUninstall : DictionaryStep
{ ... }
```

## SuperOffice.DevNet.CDD

Project reponsible for creating an isolated AppDomain in which to apply the database changes.  

#### Use:
``` CSharp
/// <summary>
/// Get all DictionarySteps defined in specified assembly
/// </summary>
/// <param name="assemblyName">Name of assembly containing classes that inherit from DictionarySteps</param>
/// <returns>Array of StepInfo, which contains DictrionaryStep properties: Name, StepNumber and State</returns>

static StepInfo [] GetSteps(string assemblyName)
{
    var appDomain = new ContinuousDatabaseAppDomainProxy();
    return appDomain.GetAllSteps(assemblyName);
}

/// <summary>
/// Applies all DictionarySteps defined in the 'steps' parameter.
/// </summary>
/// <param name="assembly">Name of assembly containing classes that inherit from DictionarySteps</param>
/// <param name="steps">Array of StepInfo that defines the name, stepnumber of DictionarySteps that are applied to a database.</param>
/// <param name="uninstall">Optional parameter that when false, only applies DictionarySteps whose StepNumber is less than int.MaxValue.</param>
/// <returns>Array of StepInfo that were successfully applied.</returns>
static StepInfo [] ApplySteps(string assembly, StepInfo[] steps, bool uninstall = false)
{
    var appDomain = new ContinuousDatabaseAppDomainProxy();
    return appDomain.ApplySteps(assembly, ConnectionString, Prefix, Major, Minor, null, steps.ToArray(), uninstall);
}
```

## LearnContinuousDatabase.csproj

Demonstrates how to use SuperOffice.DevNet.CDD to create a proxy AppDomain for performing database changes.

``` CSharp
static void Main(string[] args)
{

    //Populate Database Connection Properties
    ConnectionString = "Server=localhost;Database=SuperOffice;USER ID=username;Password=password";
    Major = "MSSQL";
    Minor = "12";
    Prefix = "CRM7";

    // define assembly to load into AppDomain

    var assembly = "ExampleDictionaryStep.dll";

    // get all dictionary steps from the assembly

    var steps   = GetSteps(assembly);

    // apply all steps with StepNumber != int.MaxValue

    var applied = ApplySteps(assembly, steps);

    // apply all steps with StepNumber == int.MaxValue

    var dropped = ApplySteps(assembly, steps, true);
}
```