using SuperOffice.CD.DSL;
using System;
using System.Reflection;

namespace SuperOffice.DevNet.CDD
{
    /// <summary>
    /// Creates an isolated AppDomain for applying DictionarySteps to a SuperOffice Database.
    /// </summary>
    public class ContinuousDatabaseAppDomainProxy : IDisposable
    {
        System.AppDomain _domain;
        ContinuousDatabaseAppDomain _remoteClass;

        public ContinuousDatabaseAppDomainProxy()
        {
            _domain = System.AppDomain.CreateDomain("ContinuousDatabaseActivity");
            _remoteClass = _domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ContinuousDatabaseAppDomain).FullName) as ContinuousDatabaseAppDomain;
        }

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
        public StepInfo[] ApplySteps(
            string assembly,
            string connectionString,
            string tablePrefix,
            string dbMajor,
            string dbMinor,
            IProgressNotification progress,
            StepInfo[] stepsToInclude,
            bool uninstall = false)
        {
            return _remoteClass.ApplySteps(assembly, connectionString, tablePrefix, dbMajor, dbMinor, stepsToInclude, uninstall, progress);
        }

        /// <summary>
        /// Gets all DictionarySteps defined in an assembly.
        /// </summary>
        /// <param name="assembly">An assembly that contains one or more DictionarySteps.</param>
        /// <returns>An array of StepInfo, indicating all DictionarySteps the specified assembly contains.</returns>
        public StepInfo[] GetAllSteps(string assembly)
        {
            return _remoteClass.GetAllSteps(assembly);
        }

        /// <summary>
        /// Unloads the AppDomain and cleans up any residual artifacts.
        /// </summary>
        public void Dispose()
        {
            System.AppDomain.Unload(_domain);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
