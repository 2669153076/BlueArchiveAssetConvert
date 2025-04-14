namespace BlueArchiveAssetConvert.Utils
{
    public static class Ask
    {
        public static string AskStringForUser(string prompt)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine($"Variable is null{Utils.newLineStr}Please enter it again");
                }
            } while (string.IsNullOrEmpty(input));
            return input;
        }

        public static string ParseStrFromArgOrAsk(int index, string prompt, string[] args)
        {
            string result = "";
            if (args != null && args.Length > index + 1)
            {
                result = args[index];
            }
            else
            {
                result = AskStringForUser(prompt);
            }
            return result;
        }

        public static void ParseOrAskENumValue<ENum>(string prompt, ref ENum enumValue, int index = -1, string[] args = null) where ENum : struct, Enum
        {
            Type enumType = typeof(ENum);
            if (!enumType.IsEnum)
            {
                return;
            }

            if (args != null && args.Length > 0 && index >= 0 && args.Length > index + 1)
            {
                if (Enum.TryParse(args[index], out enumValue) && Enum.IsDefined(enumType, enumValue))
                {
                    return;
                }
            }

            string enumFullName = enumType.FullName;
            while (true)
            {
                Console.WriteLine($"{enumFullName} List:");
                Array enumValues = Enum.GetValues(enumType);
                foreach (var value in enumValues)
                {
                    int intValue = Convert.ToInt32(value);
                    Console.WriteLine($"{intValue}: {value}");
                }
                Console.WriteLine();

                string input = AskStringForUser(prompt);
                if (Enum.TryParse(input, out enumValue) && Enum.IsDefined(enumType, enumValue))
                {
                    return;
                }

                Console.WriteLine($"Parse to {enumFullName} failed{Utils.newLineStr}Please enter it again");
                Console.WriteLine();
            }
        }

        public static string ParseOrAskAndValidPath(bool isFile, string prompt, int index = -1, string[] args = null)
        {
            string result = "";
            string pathType = isFile ? "file" : "directory";
            int attempt = 0;
            while (true)
            {
                if (args != null && index >= 0 && attempt == 0)
                {
                    result = ParseStrFromArgOrAsk(index, prompt, args);
                }
                else
                {
                    result = AskStringForUser(prompt);
                }

                result = result.Replace("\"", "");

                if (isFile && File.Exists(result) || !isFile && Directory.Exists(result))
                {
                    break;
                }

                Console.WriteLine();
                Console.WriteLine($"\"{result}\" is not a {pathType}{Utils.newLineStr}Please try again");
                attempt++;
            }
            return result;
        }
    }
}
