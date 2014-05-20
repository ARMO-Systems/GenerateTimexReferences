using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArmoSystems.ArmoGet.GenerateTimexReferences
{
    internal class DeadFiles
    {
        public static void Search( string parentDir, List< string > projectFiles )
        {
            var dependentProjects = projectFiles.SelectMany( GetIncludedFileList ).Union( projectFiles ).Select( i => i.ToLowerInvariant() ).Distinct().ToList();

            var allFiles = Directory.GetFiles( parentDir, "*.*", SearchOption.AllDirectories ).Select( i => i.ToLowerInvariant() );
            var s = allFiles.Where( file => !Path.GetDirectoryName( file ).Split( Path.DirectorySeparatorChar ).Contains( "obj" ) ).Except( dependentProjects ).ToArray();
            var types = new List< string > { "png", "ico", "resx", "cs", "bak", "suo", "xsd", "wsdl", "resx", "pfx", "cache" };
            var twithdot = types.Select( t => "." + t );
            var files = s.Where( f => twithdot.Any( f.Contains ) ).ToList();
// ReSharper disable once AssignNullToNotNullAttribute
            using ( TextWriter writer = File.CreateText( Path.Combine( Environment.GetEnvironmentVariable( "TimexCommonTempPath" ), "DeadFiles.txt" ) ) )
                files.ForEach( writer.WriteLine );
            //files.ForEach( File.Delete );
        }

        private static IEnumerable< string > GetIncludedFileList( string proj )
        {
            var dir = Path.GetDirectoryName( proj );
            const string last = " Include=\"(.*)\"";
            const string first = "<";
            var list = new List< string > { "Compile", "EmbeddedResource", "None", "Content" };

            var fileText = File.ReadAllText( proj );
            Func< Regex, List< string > > getMatches = regex =>
                                                       {
                                                           var matchResult = regex.Match( fileText );
                                                           var ret = new List< string >();
                                                           while ( matchResult.Success )
                                                           {
                                                               ret.Add( matchResult.Groups[ 1 ].Value );
                                                               matchResult = matchResult.NextMatch();
                                                           }
                                                           return ret;
                                                       };

// ReSharper disable once AssignNullToNotNullAttribute
            return list.Select( l => new Regex( first + l + last ) ).SelectMany( getMatches ).Select( file => Path.GetFullPath( Path.Combine( dir, file ) ) );
        }
    }
}