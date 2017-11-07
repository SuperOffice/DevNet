using SuperOffice.CD.DSL;
using SuperOffice.CD.DSL.PhysicalDatabase;
using SuperOffice.CD.DSL.V1.DatabaseModel;
using SuperOffice.CD.DSL.V1.StepModel;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperOffice.DevNet.CDD
{
    /// <summary>
    /// Contains implementation to get, add, modify and remove DictionarySteps.
    /// </summary>
    internal class ContinuousDatabaseAppDomain : MarshalByRefObject
    {
        public ContinuousDatabaseAppDomain() { }

        /// <summary>
        /// Given an assembly containing one ore more DictionarySteps, applies specified dictionary steps into database model and physical database.
        /// </summary>
        /// <param name="assembly">Assembly containing dictionary steps</param>
        /// <param name="connectionString">Connection string to SuperOffice database where steps are to be applied.</param>
        /// <param name="tablePrefix">SuperOffice table prefix</param>
        /// <param name="dbMajor">Major version of SuperOffice database.</param>
        /// <param name="dbMinor">Minor version of SuperOffice database.</param>
        /// <param name="stepsToInclude">Name of the DictionaryStep to apply. Useful when an assembly contains more then one.</param>
        /// <param name="uninstall">Indicates whether this call should only apply DictionarySteps with a StepNumber equals to int.MaxValue</param>
        /// <param name="progress">Optional progress implementation for reporting progress.</param>
        /// <returns>Array of StepInfo indicating which dictionary steps were applied.</returns>
        internal StepInfo[] ApplySteps(
            string assembly,
            string connectionString,
            string tablePrefix,
            string dbMajor,
            string dbMinor,
            StepInfo[] stepsToInclude,
            bool uninstall,
            IProgressNotification progress)
        {
            DictionaryStepInfo[] result = null;

            try
            {
                // load the assembly into this AppDomain
                 
                var stepAssembly = LoadAssembly(assembly);

                var fileLogger = new SuperOffice.CD.DSL.Logging.FileLogger(Environment.CurrentDirectory + "\\appliedstep.log");

                using (var connection = DbConnectionProvider.GetConnection(connectionString, dbMajor, dbMinor))
                {
                    using (var dbm = DatabaseManagement.CreateInstance(tablePrefix, connection, fileLogger, progress))
                    {

                        var model = dbm.ReadDatabaseModel();
                        var dbState = dbm.InspectDatabase();

                        // ensure we are talking to a SuperOffice database version >= 8.1

                        if (dbState == DatabaseManagement.DatabaseContent.SuperOfficeCdd)
                        {

                            // get all DictionarySteps in this AppDomain - filter out any .net & SuperOffice assemblies

                            var steps = GetDictionarySteps(stepsToInclude, uninstall);


                            //make sure we only apply steps that were passed in stepsToInclude argument

                            var desiredSteps = new List<DictionaryStep>();

                            foreach (var item in stepsToInclude.OrderBy(st => st.Name).ThenBy(st => st.StepNumber))
                            {
                                var dicStep = steps.Find(s => s.GetAttribute().DictionaryName.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase)
                                                           && s.GetAttribute().StepNumber == item.StepNumber);

                                if (dicStep != null)
                                    desiredSteps.Add(dicStep);
                            }

                            // apply DictionarySteps in the Database
                            if (desiredSteps.Count > 0)
                            {
                                result = dbm.ApplyDictionarySteps(new LinkedList<DictionaryStep>(desiredSteps), model);
                            }
                        }

                        if (result == null)
                            return null;

                        return result.Select(s => new StepInfo { Name = s.Name, StepNumber = s.StepNumber, State = s.State.ToString() }).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gets all DictionarySteps defined in an assembly.
        /// </summary>
        /// <param name="assembly">An assembly that contains one or more DictionarySteps.</param>
        /// <returns>An array of StepInfo, indicating all DictionarySteps the specified assembly contains.</returns>
        internal StepInfo [] GetAllSteps(string dictionaryStepAssembly)
        {
            var steps = new List<StepInfo>();

            var stepAssembly = LoadAssembly(dictionaryStepAssembly);

            var assemblies = GetAssemblies();

            foreach (var asm in assemblies)
            {
                try
                {
                    var dictionaryAttributes = asm.GetTypes().
                            Where(t => t.IsClass && !t.IsAbstract
                            && typeof(DictionaryStep).IsAssignableFrom(t)
                            && t.GetCustomAttributes<DictionaryAttribute>().FirstOrDefault() != null)
                            .Select(t => t.GetCustomAttributes<DictionaryAttribute>().FirstOrDefault());

                    steps.AddRange(dictionaryAttributes.Select(c => new StepInfo { Name = c.DictionaryName, StepNumber = c.StepNumber, State = c.State.ToString() }));

                }
                catch { }
            }

            return steps.ToArray();
        }

        /// <summary>
        /// Gets all dictionary steps loaded in appdomain.  
        /// </summary>
        /// <param name="stepInfos">Optional filter to only get DictionaryStep from StepInfo.</param>
        /// <param name="uninstallOnly">Optional filter to only get DictionaryStep with StepNumber equal to int.MaxValue.</param>
        /// <returns>List of DictionarySteps</returns>
        private List<DictionaryStep> GetDictionarySteps(StepInfo[] stepInfos = null, bool uninstallOnly = false)
        {
            var steps = new List<DictionaryStep>();
            var stepNames = stepInfos != null ? stepInfos.Select(s => s.Name).ToArray() : null;

            var assemblies = GetAssemblies();

            foreach (var asm in assemblies)
            {
                try
                {
                    var dictionarySteps = asm.GetTypes().
                            Where(t => t.IsClass && !t.IsAbstract
                            && typeof(DictionaryStep).IsAssignableFrom(t)
                            && t.GetCustomAttributes<DictionaryAttribute>().FirstOrDefault() != null)
                            .Select(t => t.GetConstructor(new Type[0]).Invoke(new object[0]) as DictionaryStep);

                    if (dictionarySteps != null && stepNames != null)
                    {

                        dictionarySteps = dictionarySteps.Where(d => uninstallOnly ? d.GetAttribute().StepNumber == int.MaxValue : d.GetAttribute().StepNumber != int.MaxValue
                                                     && stepNames.Contains(d.GetAttribute().DictionaryName, StringComparer.InvariantCultureIgnoreCase));
                    }

                    steps.AddRange(dictionarySteps);

                }
                catch { }
            }

            return steps;
        }

        /// <summary>
        /// Loads specified assembly into current AppDomain.
        /// </summary>
        /// <param name="assembly">Assembly containing one or more DictionarySteps.</param>
        /// <returns>Assembly that was loaded.</returns>
        private Assembly LoadAssembly(string assembly)
        {
            var fullPath = Path.GetFullPath(assembly);
            return Assembly.LoadFrom(fullPath);
        }

        /// <summary>
        /// Gets a filtered list of assemblies loaed in the AppDomain. 
        /// </summary>
        /// <returns>All assemblies in AppDomain except the executing assembly and those that start with: SuperOffice, Newtonsotf.Json, Microsoft, mscor and System.</returns>
        private Assembly[] GetAssemblies()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies().
                Where(a => !a.FullName.StartsWith("SuperOffice.")
                        && !a.FullName.StartsWith("Newtonsoft.Json")
                        && !a.FullName.StartsWith("Microsoft.")
                        && !a.FullName.StartsWith("mscor")
                        && !a.FullName.StartsWith("System")
                        && !a.FullName.Equals(Assembly.GetExecutingAssembly().FullName))
                .ToArray();
        }

        
    }
}
