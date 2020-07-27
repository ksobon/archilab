using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using WeCantSpell.Hunspell;
// ReSharper disable UnusedMember.Global

namespace archilab.SpellCheck
{
    /// <summary>
    /// 
    /// </summary>
    public class Speller
    {
        internal WordList Dictionary { get; set; }
        internal WordList CustomDictionary { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Speller()
        {
            var dllPath = Assembly.GetExecutingAssembly().Location;
            var binPath = Path.GetDirectoryName(dllPath);
            if (binPath == null)
                return;

            var packagePath = Path.Combine(binPath, @"..\");
            var extraPath = Path.Combine(packagePath, "extra");
            var dicPath = Path.Combine(extraPath, "en_US.dic");
            var affPath = Path.Combine(extraPath, "en_US.aff");

            Dictionary = WordList.CreateFromFiles(dicPath, affPath);
            CustomDictionary = WordList.CreateFromWords(new List<string> {"RFI"});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicPath"></param>
        /// <param name="affPath"></param>
        public Speller(string dicPath, string affPath)
        {
            Dictionary = WordList.CreateFromFiles(dicPath, affPath);
            CustomDictionary = WordList.CreateFromWords(new List<string> { "RFI" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicPath"></param>
        /// <param name="affPath"></param>
        /// <param name="additions"></param>
        public Speller(string dicPath, string affPath, IReadOnlyCollection<string> additions = null)
        {
            Dictionary = WordList.CreateFromFiles(dicPath, affPath);

            if (additions != null && additions.Any())
            {
                CustomDictionary = WordList.CreateFromWords(additions);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [MultiReturn("Misspelled", "Suggestions")]
        public Dictionary<string, object> CheckSpelling(string text)
        {
            // (Konrad) Clean the text.
            var invalidChars = Path.GetInvalidFileNameChars();
            text = invalidChars.Aggregate(text, (current, c) => current.Replace(c.ToString(), ""));

            // (Konrad) Split it up into words.
            var delimiters = new[] { ' ', '.', ',', ':', ';', '\r' };
            var parts = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            // (Konrad) Check spelling for each word.
            var misspelled = new List<string>();
            var suggestions = new List<IEnumerable<string>>();
            foreach (var word in parts)
            {
                var customOk = CustomDictionary.Check(word);
                if (customOk)
                    continue;

                var ok = Dictionary.Check(word);
                if (ok)
                    continue;

                misspelled.Add(word);
                suggestions.Add(Dictionary.Suggest(word));
            }

            return new Dictionary<string, object>
            {
                {"Misspelled", misspelled},
                {"Suggestions", suggestions}
            };
        }
    }
}