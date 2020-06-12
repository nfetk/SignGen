using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignGen.Logic
{
    internal static class Extension
    {
        public static void AddMulti<T>(this IList<T> l, IEnumerable<T> additions)
        {
            foreach (var item in additions)
            {
                l.Add(item);
            }
        }
    }

    /// <summary>
    /// Hilfsklasse für alle IO-Aktionen mit Dateien. Vorerst nicht static, damit für Erweiterungen überschrieben werden kann
    /// </summary>
    internal class SignGenFileHandler
    {
        public virtual IEnumerable<IDictionary<string, string>> ReadInputFile(string path)
        {
            var entries = new List<IDictionary<string, string>>();
            var lines = ReadFileLines(path);
            if (lines.Any())
            {
                var template = lines.First().Split(";")
                                            .Where(s => s != null)
                                            .Select(s => s.ToUpper())
                                            .ToArray();
                lines.RemoveAt(0);
                entries.AddMulti(lines.Select(l =>
                {
                    var props = l.Split(";");
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < template.Length; i++)
                    {
                        if (i > props.Length)
                        {
                            break;
                        }
                        
                        // Key nur einmal hinzufügen. Doppelte Spalten werden ignoriert.
                        if (!dict.ContainsKey(template[i]))
                            dict.Add(template[i], props[i]);
                    }

                    return dict;
                }));
            }

            return entries;
        }

        /// <summary>
        /// Liest eine komplette Datei Zeile für Zeile ein, wenn sie gefunden wird
        /// </summary>
        /// <param name="path">Der Pfad zur einzulesenden Datei</param>
        /// <returns></returns>
        public virtual IList<string> ReadFileLines(string path)
        {
            try
            {
                return File.ReadAllLines(path).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Liest den kompletten Inhalt einer Datei als einen Text ein, wenn sie gefunden wird
        /// </summary>
        /// <param name="path">Der Pfad zur einzulesenden Datei</param>
        /// <returns></returns>
        public virtual string ReadFileText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Schreibt einen Text in eine Datei, wenn sie gefunden wird
        /// </summary>
        /// <param name="path">Der Pfad zur zu beschreibenden Datei</param>
        /// <param name="text">Der zu schreibende Text</param>
        /// <returns></returns>
        public virtual bool WriteFileText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Prüft für jedes Element aus <paramref name="pathes"/>, ob die entsprechende Datei existiert. Gibt true zurück, wenn alle Dateien existieren.
        /// </summary>
        /// <param name="pathes"></param>
        /// <returns></returns>
        public virtual string CheckInvalidFiles(IEnumerable<string> pathes)
        {
            foreach (var item in pathes)
            {
                if (!File.Exists(item))
                    return item;
            }

            return string.Empty;
        }

        /// <summary>
        /// Erstellt einen vollständigen Zielpfad sowie die dazugehörige Ordnerstruktur und gibt den Pfad zurück. Gibt eine leere Instanz vom Typ <see cref="string"/> zurück, wenn kein Pfad erstellt werden konnte.
        /// </summary>
        /// <param name="targetPath">Der Zielpfad für erstellte Dateien, wenn vorhanden</param>
        /// <param name="directoryName">Der Name des zu erstellenden Ordners</param>
        /// <returns></returns>
        public virtual string GetTargetDirectory(string targetPath, string directoryName)
        {
            string fullPath = Path.Combine(targetPath, directoryName);
            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Gibt ein Tuple aus zwei Elementen vom Typ <see cref="string"/> zurück. Item1 ist der Quellpfad und Item2 der Zielpfad.
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public virtual Tuple<string, string> GetTargetFileName(string targetDirectory, string sourceFilePath)
        {
            var fileName = Path.GetFileName(sourceFilePath);
            return Tuple.Create(sourceFilePath, Path.Combine(targetDirectory, fileName));
        }

        /// <summary>
        /// Kopiert eine Datei von Quelle zu Ziel und gibt einen Wert zurück der den Erfolg der Aktion in Form eines <see cref="bool"/> widerspiegelt
        /// </summary>
        /// <param name="source">Der Quellpfad der Datei</param>
        /// <param name="target">Der Zielpfad der Datei</param>
        /// <returns></returns>
        public virtual bool CopyToTarget(string source, string target)
        {
            try
            {
                File.Copy(source, target);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Kopiert eine Datei von Quelle zu Ziel und gibt einen Wert zurück der den Erfolg der Aktion in Form eines <see cref="bool"/> widerspiegelt
        /// </summary>
        /// <param name="st">Ein aus <see cref="GetTargetFileName(string, string)"/> generiertes <see cref="Tuple{string,string}"/> mit Quell- und Zielpfad.</param>
        /// <returns></returns>
        public virtual bool CopyToTarget(Tuple<string, string> st)
        {
            return CopyToTarget(st.Item1, st.Item2);
        }

        /// <summary>
        /// Erlaubt es Parameter in einer Datei mit eingelesenen Werten zu füllen.
        /// </summary>
        /// <param name="filePath">Der Pfad zur einzulesenden Datei</param>
        /// <param name="replacements">Die einzufügenden Schlüssel-Wert-Paare</param>
        /// <returns></returns>
        public virtual bool ReplaceKeysInFile(string filePath, IDictionary<string,string> replacements)
        {
            var text = ReadFileText(filePath);
            if (string.IsNullOrEmpty(text))
                return false;
            var newText = SignGenTextHelper.ReplaceKeysInText(text, replacements);

            return WriteFileText(filePath, newText);
        }
    }
}
