using SuperOffice.DevNet.CDD;
using System.Linq;

namespace LearnContinuousDatabase
{
    class Program
    {
        public static string ConnectionString { get; set; }
        public static string Major { get; set; }
        public static string Minor { get; set; }
        public static string Prefix { get; set; }


        static void Main(string[] args)
        {

            //Populate Database Connection Properties
            ConnectionString = "Server=localhost;Database=SuperOffice81R;USER ID=crm7;Password=crm7myd";
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
        /// 
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
    }
}
