using FlubuCore.Context;
using FlubuCore.Scripting;
using System;
using FlubuCore.Context.Attributes.BuildProperties;
using System.Collections.Generic;
using FlubuCore.Tasks.Solution.VSSolutionBrowsing;
using System.IO;
using Newtonsoft.Json;

namespace BuildScript
{
    public class BuildScript : DefaultBuildScript
    {
        [SolutionFileName]
        public string SolutionFileName { get; set; } = "FHT.Nullify.sln";

        protected override void ConfigureTargets(ITaskContext context)
        {
            var compile = context.CreateTarget("compile")
                .SetDescription("Compiles the solution.")
                .AddCoreTask(x => x.Build());
            var vsSolution = context.GetVsSolution();

            var libProjs = new List<VSProject>();
            vsSolution.ForEachProject(proj =>
            {
                var d = proj.ProjectDetails;
                var isNetStandardLib = proj.TargetFramework?.Contains("netstandard2.", StringComparison.OrdinalIgnoreCase) ?? false;
                if (isNetStandardLib)
                {
                    libProjs.Add(proj);
                }
            });
            var rawPkg = File.ReadAllText("./package.json");
            var pkg = JsonConvert.DeserializeAnonymousType(rawPkg, new { version = "" });
            if (string.IsNullOrEmpty(pkg.version)) throw new InvalidOperationException("gotta have package.json and version");
            var outputDirectory = "./out";
            var pack = context.CreateTarget("pack")
                         .SetDescription("pack packages")
                           .Do(c =>
                           {
                               if (Directory.Exists(outputDirectory))
                               {
                                   Directory.Delete(outputDirectory, true);
                               }
                               Console.WriteLine($"Removed {outputDirectory}");
                           })
                         .ForEach(libProjs, (project, target) =>
                         {
                             target.AddCoreTask(x => x
                             .Pack()
                             .Project(project.ProjectName)
                             .PackageVersion(pkg.version)
                             .OutputDirectory(outputDirectory));
                         })
                         .ForEach(libProjs, (project, target) =>
                         {
                             var nupkgPath = Path.Combine(outputDirectory, $"{project.ProjectName}.{pkg.version}.nupkg");
                             target.AddCoreTask(x => x
                                 .NugetPush(nupkgPath)
                                 .ServerUrl("https://api.nuget.org/v3/index.json")
                                 .ApiKey(Environment.GetEnvironmentVariable("NUGET_API_KEY"))
                             );
                         });
        }
    }
}
