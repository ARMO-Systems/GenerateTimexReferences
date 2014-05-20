using CommandLine;

namespace ArmoSystems.ArmoGet.GenerateTimexReferences
{
    internal sealed class Options
    {
        public Options()
        {
            ProjectName = "*";
            FileName = "devxref";
        }

        [Option( 'p', "projectName", HelpText = "Project file name." )]
        public string ProjectName { get; set; }

        [Option( 'f', "fileName", HelpText = "Output file name." )]
        public string FileName { get; set; }

        [Option( 'd' )]
        public bool SearchDeadFiles { get; set; }
    }
}