using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SignGen.Logic
{
    internal class SignGenTextHelper
    {
        /// <summary>
        /// Gibt eine Sammlung von Schlüssel-Wert-Paaren zurück, die ein Mapping ohne Berücksichtigung von Groß- und Kleinschreibung ermöglicht.
        /// Die Parameter werden aus einem eingegebenen Text ermittelt
        /// </summary>
        /// <param name="text">Der zu durchsuchende Text</param>
        /// <returns></returns>
        public static IDictionary<string, string> GetParameters(string text)
        {
            Regex regex = new Regex("@[^@]*@");
            var param = regex.Matches(text).Select(m => m.Value).Distinct();
            return param.ToDictionary(p => p, v => v.Replace("@", "").ToUpper());
        }

        /// <summary>
        /// Ersetzt alle im Text gefundenen Parameter mit Werten, sofern Werte vorhanden sind
        /// </summary>
        /// <param name="input">Der zu füllende Text</param>
        /// <param name="replacements">Die einzusetzenden Werte als Schlüssel-Wert-Paare</param>
        /// <returns></returns>
        public static string ReplaceKeysInText(string input, IDictionary<string,string> replacements)
        {
            string newText = input;
            var paramInfo = GetParameters(newText);
            foreach (var item in paramInfo)
            {
                string value = string.Empty;
                if (replacements.TryGetValue(item.Value, out value))
                {
                    newText = newText.Replace(item.Key, value);
                }
            }

            return newText;
        }
    }
}
