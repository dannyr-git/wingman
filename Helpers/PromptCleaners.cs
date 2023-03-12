using System;

namespace wingman.Helpers
{
    public static class PromptCleaners
    {
        public static string CleanBlockIdentifiers(string input)
        {
            // Remove code block identifier at the beginning of the string, if any
            var identifiers = new string[] { "c", "cpp", "java", "python", "ruby", "perl", "php", "html", "css", "javascript", "typescript", "swift", "objective-c", "go", "kotlin", "rust", "scala", "sql", "sh", "bash", "json", "yaml", "xml", "markdown" };

            var index = input.IndexOf("```");

            if (index == -1)
                return input;

            input = input.Substring(index + 1);

            foreach (string id in identifiers)
            {
                if (input.StartsWith(id) && input.Length > (input.Length - id.Length))
                {
                    input = input.Substring(id.Length);
                    break;
                }
            }

            // Remove code block identifier at the end of the string, if any
            index = input.LastIndexOf("```");
            if (index != -1)
            {
                // decrement index while there are whitespace characters
                while (index > 0 && char.IsWhiteSpace(input[index - 1]))
                {
                    index--;
                }
                input = input.Substring(0, index);
            }

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
            // Replace all occurrences of newline characters with a single space
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", lines);
        }


    }
}