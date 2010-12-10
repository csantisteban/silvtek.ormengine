using System;
using System.Text.RegularExpressions;

namespace Silvtek.ORMEngine.QueryLanguage
{
    internal class QueryLanguageParserUtility
    {
        private QueryLanguageParserUtility()
        {
            
        }

        internal static string ExtractSingleTypeName(string text)
        {
            const string extractTypeName = @"(?:\{{1}([\w\d]*)\}{1})(?>(?i)[\s]*as{1}[\s]*){0,1}";

            Regex regex = new Regex(extractTypeName);
            MatchCollection matches = regex.Matches(text);
            if (matches.Count == 0) return String.Empty;

            foreach (Match match in matches)
            {
                if (!match.Success) continue;

                if (match.Groups.Count > 0)
                {
                    return match.Groups[1].Value;
                }
            }

            return String.Empty;
        }
    }
}
