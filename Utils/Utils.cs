namespace BlueArchiveAssetConvert.Utils
{
    public static class Utils
    {
        public static string newLineStr;

        static Utils()
        {
            newLineStr = "\r\n";
        }

        public static void DisplayHelpIfArgIsHelp(string[] args, string helpText)
        {
            if (args != null && args.Length > 0 && args[0] == "--help")
            {
                Console.WriteLine(helpText);
                Environment.Exit(0);
            }
        }
    }
}