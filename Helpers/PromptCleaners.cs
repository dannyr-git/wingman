using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace wingman.Helpers
{
    public static class PromptCleaners
    {
        public static string CleanBlockIdentifiers(string input)
        {
            string[] identifiers = Enum.GetNames(typeof(CodeBlockIdentifiers))
                .Select(name => name == "cplusplus" ? "c++" : name)
                .ToArray();

            //foreach (string identifier in identifiers)
            //{
            string pattern = @"(\n)?(```(csharp|cpp|c\+\+|c|python|ruby|perl|php|html|css|javascript|java|typescript|swift|objectivec|go|kotlin|rust|scala|sql|sh|bash|json|yaml|xml|markdown))(\n)?|(\n)?(```)(\n)?";


            //string pattern = $@"(\n)?(```{identifier})(\n)?";
            //string pattern = $@"(\n)?(```{identifier})(\n)?|(\n)?(```)(\n)?";
            input = Regex.Replace(input, pattern, "");
            //pattern = $@"(\n)?(```)(\n)?";
            //input = Regex.Replace(input, pattern, "");
            //}

            return input;
        }


        public static string TrimWhitespaces(string input)
        {
            // Split the input string into lines using all line-ending characters
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Trim whitespace characters at the beginning and end of each line
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // Join the lines back into a single string using the original line-ending character(s)
            return string.Join(input.Contains("\r\n") ? "\r\n" : input.Contains("\r") ? "\r" : "\n", lines);
        }


        public static string TrimNewlines(string input)
        {
            return Regex.Replace(input, @"(\r\n|\r|\n)+", " ");
        }


    }
}