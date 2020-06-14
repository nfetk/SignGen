using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignGen.Logic
{
    internal static class Extension
    {
        /// <summary>
        /// Fügt alle Items eines <see cref="IEnumerable{T}"/> zu einer bestehenden Instanz, welche <see cref="IList{T}"/> implementiert, hinzu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l">Die zu erweiternde <see cref="IList{T}"/></param>
        /// <param name="additions">Eine hinzuzufügende Instanz, welche <see cref="IEnumerable{T}"/> implementiert</param>
        public static void AddMulti<T>(this IList<T> l, IEnumerable<T> additions)
        {
            foreach (var item in additions)
            {
                l.Add(item);
            }
        }
    }

    public static class SignGenFileInfoProvider
    {
        public static IEnumerable<string> GetEncodings()
        {
            return Encoding.GetEncodings().Select(e => e.Name.ToLower()).ToList();
        }
        public static string GetDefaultEncoding()
        {
            return Encoding.Default?.HeaderName?.ToLower();
        }
    }

    /// <summary>
    /// Hilfsklasse für alle IO-Aktionen mit Dateien. Vorerst nicht static, damit für Erweiterungen überschrieben werden kann
    /// </summary>
    internal class SignGenFileHandler
    {
        /// <summary>
        /// Liest zeilenweise die Konfiguration ein und gibt Schlüssel-Wert-Paare pro Zeile wieder
        /// </summary>
        /// <param name="path">Der Pfad zur Konfigurationsdatei</param>
        /// <param name="encoding">Ein optinaler Parameter bei abweichendem Encoding</param>
        /// <returns></returns>
        public virtual IEnumerable<IDictionary<string, string>> ReadInputFile(string path, string encoding = "UTF-8")
        {
            var entries = new List<IDictionary<string, string>>();
            var lines = ReadFileLines(path, encoding);
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
        /// <param name="encoding">Ein optinaler Parameter bei abweichendem Encoding</param>
        /// <returns></returns>
        public virtual IList<string> ReadFileLines(string path, string encoding = "UTF-8")
        {
            try
            {
                return File.ReadAllLines(path, GetEncoding(encoding)).ToList();
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
        /// <param name="encoding">Ein optinaler Parameter bei abweichendem Encoding</param>
        /// <returns></returns>
        public virtual string ReadFileText(string path, string encoding = "UTF-8")
        {
            try
            {
                return File.ReadAllText(path, GetEncoding(encoding));
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
        /// <param name="encoding">Ein optinaler Parameter bei abweichendem Encoding</param>
        /// <returns></returns>
        public virtual bool WriteFileText(string path, string text, string encoding = "UTF-8")
        {
            try
            {
                File.WriteAllText(path, text, GetEncoding(encoding));
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
        /// <param name="overwrite">Ein optionaler Parameter, welcher angibt ob vorhandene Dateien überschrieben werden sollen</param>
        /// <returns></returns>
        public virtual bool CopyToTarget(string source, string target, bool overwrite = false)
        {
            try
            {
                File.Copy(source, target, overwrite);
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
        /// <param name="overwrite">Ein optionaler Parameter, welcher angibt ob vorhandene Dateien überschrieben werden sollen</param>
        /// <returns></returns>
        public virtual bool CopyToTarget(Tuple<string, string> st, bool overwrite = false)
        {
            return CopyToTarget(st.Item1, st.Item2, overwrite);
        }

        /// <summary>
        /// Erlaubt es Parameter in einer Datei mit eingelesenen Werten zu füllen.
        /// </summary>
        /// <param name="filePath">Der Pfad zur einzulesenden Datei</param>
        /// <param name="replacements">Die einzufügenden Schlüssel-Wert-Paare</param>
        /// <param name="encoding">Ein optinaler Parameter bei abweichendem Encoding</param>
        /// <returns></returns>
        public virtual bool ReplaceKeysInFile(string filePath, IDictionary<string,string> replacements, string encoding = "UTF-8")
        {
            var text = ReadFileText(filePath, encoding);
            if (string.IsNullOrEmpty(text))
                return false;
            var newText = SignGenTextHelper.ReplaceKeysInText(text, replacements);

            return WriteFileText(filePath, newText, encoding);
        }

        /// <summary>
        /// Verbindet zwei Pfade (Gedacht für Pfad + Dateiname) und hängt einen Suffix an, sofern angegeben
        /// </summary>
        /// <param name="path">Der Pfad, an den angehangen werden soll</param>
        /// <param name="name">Der anzuhängende Pfad</param>
        /// <param name="suffix">Optionaler Suffix für den Pfad (z.B. Dateiendung)</param>
        /// <returns></returns>
        public virtual string GetFileName(string path, string name, string suffix = null)
        {
            return Path.Combine(path, name + (suffix ?? string.Empty));
        }

        /// <summary>
        /// Versucht abhängig von <paramref name="enc"/> das am besten passende Encoding zu ermitteln und gibt dieses zurück
        /// </summary>
        /// <param name="enc">Der Schlüssel für das gewünschte Encoding</param>
        /// <returns></returns>
        public virtual Encoding GetEncoding(string enc)
        {
            int e = 0;
            if (int.TryParse(enc, out e))
            {
                return Encoding.GetEncoding(e);
            }
            else if (!string.IsNullOrEmpty(enc))
            {
                return Encoding.GetEncodings().FirstOrDefault(e => e.Name.ToUpper() == enc.ToUpper())?.GetEncoding();
            }
            else
            {
                return Encoding.Default;
            }
        }
    }
}
