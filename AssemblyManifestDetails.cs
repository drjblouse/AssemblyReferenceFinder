using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace AssemblyReferenceFinder
{
    public class AssemblyReferences
    {
        #region Constructors

        public AssemblyReferences()
        {
            FilePath = string.Empty;
            AssemblyName = string.Empty;
            AssembliesReferenced = new List<AssemblyReferences>();
        }

        #endregion

        #region Members

        public string FilePath { get; set; }
        public string FullAssemblyName { get; set; }
        public string AssemblyName { get; set; }
        public List<AssemblyReferences> AssembliesReferenced { get; set; } 

        #endregion
    }

    interface IAssemblyManifestDetails
    {
        List<AssemblyReferences> GetAllAssemblyDetailsFromPath(string folderPath);
        AssemblyReferences GetAssemblyReferences(string filePath);
        AssemblyReferences FindReferencedAssemblies(string folderPath, string assemblyName);
    }

    public class AssemblyManifestDetails : IAssemblyManifestDetails
    {
        #region IAssemblyManifestDetails Members

        List<AssemblyReferences> IAssemblyManifestDetails.GetAllAssemblyDetailsFromPath(string folderPath)
        {
            return GetAssemblyDetailsForLocation(folderPath);
        }

        AssemblyReferences IAssemblyManifestDetails.GetAssemblyReferences(string filePath)
        {
            return GetAssemblyDetails(filePath);
        }

        AssemblyReferences IAssemblyManifestDetails.FindReferencedAssemblies(string folderPath, string assemblyName)
        {
            return FindAllReferences(folderPath, assemblyName);
        }       

        #endregion

        #region Private Methods

        private List<AssemblyReferences> GetAssemblyDetailsForLocation(string folderPath)
        {
            var assemblyDetails = new List<AssemblyReferences>();

            if (Directory.Exists(folderPath))
            {
                var dllFiles = Directory.GetFiles(folderPath, "*.dll");
                var exeFiles = Directory.GetFiles(folderPath, "*.exe");
                var allFiles = new List<string>();
                allFiles.AddRange(dllFiles);
                allFiles.AddRange(exeFiles);
                assemblyDetails.AddRange(from file in allFiles where IsFileAssembly(file) select GetAssemblyDetails(file));
            }

            return assemblyDetails;
        }

        private static bool IsFileAssembly(string file)
        {            
            try
            {
                Assembly.ReflectionOnlyLoadFrom(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private AssemblyReferences GetAssemblyDetails(string filePath)
        {
            var assemblyDetails = new AssemblyReferences();
            var assembly = Assembly.ReflectionOnlyLoadFrom(filePath);
            assemblyDetails.FilePath = filePath;
            assemblyDetails.FullAssemblyName = assembly.FullName;
            assemblyDetails.AssemblyName = assembly.FullName.Split(',')[0];
            var references = assembly.GetReferencedAssemblies();
            foreach (var name in references)
            {
                var assemblyReference = new AssemblyReferences
                    {
                        FullAssemblyName = name.FullName,
                        AssemblyName = name.FullName.Split(',')[0]
                    };
                assemblyDetails.AssembliesReferenced.Add(assemblyReference);
            }
            return assemblyDetails;
        }

        private AssemblyReferences FindAllReferences(string folderPath, string assemblyName)
        {
            var references = new AssemblyReferences();
            
            List<AssemblyReferences> allAssemblies = GetAssemblyDetailsForLocation(folderPath);
            foreach (AssemblyReferences assembly in allAssemblies)
            {
                if (assembly.AssemblyName.ToUpper() == assemblyName.ToUpper())
                {
                    references.AssemblyName = assembly.AssemblyName;
                    references.FullAssemblyName = assembly.FullAssemblyName;
                    references.FilePath = assembly.FilePath;
                    continue;
                }

                foreach (AssemblyReferences referencedAssembly in assembly.AssembliesReferenced)
                {
                    if (referencedAssembly.AssemblyName.ToUpper() == assemblyName.ToUpper())
                    {
                        references.AssembliesReferenced.Add(assembly);
                    }
                }
            }

            return references;
        }

        #endregion
    }
}
