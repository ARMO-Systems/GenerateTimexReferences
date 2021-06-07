using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommandLine;

namespace ArmoSystems.ArmoGet.GenerateTimexReferences
{
    internal static class Program
    {
        private static string parentDir;

        [STAThread]
        private static void Main()
        {
            var options = new Options();

            new Parser().ParseArguments( Environment.GetCommandLineArgs(), options );

            // ReSharper disable once AssignNullToNotNullAttribute
            parentDir = Path.Combine( Environment.GetEnvironmentVariable( "TimexCommonTimexPath" ), "Source" );
            var projectFiles = FindProjects( string.Format( "{0}.csproj", options.ProjectName ) ).ToList();

            if ( options.SearchDeadFiles )
            {
                DeadFiles.Search( parentDir, projectFiles );
                return;
            }

            var devExpressReferences = projectFiles.SelectMany( GetDevExpressReferences ).Distinct().ToList();

            // ReSharper disable once AssignNullToNotNullAttribute
            using ( TextWriter writer = File.CreateText( Path.Combine( Environment.GetEnvironmentVariable( "TimexCommonTempPath" ), options.FileName + ".txt" ) ) )
                devExpressReferences.ForEach( writer.WriteLine );
        }

        private static IEnumerable< string > FindProjects( string projectName )
        {
            return Directory.GetFiles( parentDir, projectName, SearchOption.AllDirectories ).ToList();
        }

        private static List< string > GetDevExpressReferences( string projectPath )
        {
            return GetReference( projectPath, ReferenceType.Reference ).
                   Select( item =>
                           {
                               var matchResult = Regex.Match( item, @"(DevExpress.*?)," );
                               return matchResult.Success ? matchResult.Groups[ 1 ].Value.Trim() : string.Empty;
                           } ).
                   Where( item => !string.IsNullOrEmpty( item ) ).
                   ToList();
        }

        private static IEnumerable< string > GetReference( string csprojFile, ReferenceType referenceType )
        {
            try
            {
                return GetReferenceWithMSbuild( csprojFile, referenceType, "http://schemas.microsoft.com/developer/msbuild/2003" );
            }
            catch ( NullReferenceException )
            {
                return GetReferenceWithMSbuild( csprojFile, referenceType, string.Empty );
            }
        }

        private static IEnumerable< string > GetReferenceWithMSbuild( string csprojFile, ReferenceType referenceType, XNamespace msbuild )
        {
            return XDocument.Load( csprojFile ).
                             Element( msbuild + "Project" ).
                             Elements( msbuild + "ItemGroup" ).
                             Elements( msbuild + ( referenceType == ReferenceType.ProjectReference ? "ProjectReference" : "Reference" ) ).
                             Attributes( "Include" ).
                             Select( item => item.Value ).
                             ToList();
        }

        private enum ReferenceType
        {
            ProjectReference,
            Reference
        }
    }
}