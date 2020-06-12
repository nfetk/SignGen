using System;
using System.Collections.Generic;
using System.Linq;

namespace SignGen.Logic
{
    public class SignGenResult
    {
        private bool? _succeeded;

        internal SignGenResult(string error, bool? succeeded = null)
        {
            Message = error;
            _succeeded = succeeded;
        }

        public virtual bool Succeeded => _succeeded.HasValue ? _succeeded.Value : string.IsNullOrEmpty(Message);
        public virtual string Message { get; }
    }

    public class SignGenLauncher
    {
        public SignGenLauncher(string inputFileName, IEnumerable<string> templates, string logoFileName, string targetDirectory = null)
        {
            if (string.IsNullOrEmpty(inputFileName))
            {
                throw new ArgumentException("Ein Pfad für die einzulesende Konfiguration muss gegeben sein", nameof(inputFileName));
            }
            if (string.IsNullOrEmpty(logoFileName))
            {
                throw new ArgumentException("Ein Pfad zum Firmenlogo muss gegeben sein", nameof(logoFileName));
            }
            if (templates == null || !templates.Any())
            {
                throw new ArgumentException("Mindestens eine Vorlage muss definiert sein", nameof(logoFileName));
            }

            InputFileName = inputFileName;
            LogoFileName = logoFileName;
            TargetDirectory = targetDirectory;
            TemplatePathes = templates.ToList();
        }

        protected virtual string InputFileName { get; set; }
        protected virtual string LogoFileName { get; set; }
        protected virtual string TargetDirectory { get; set; }
        protected virtual IList<string> TemplatePathes { get; set; }

        public virtual SignGenResult Run()
        {
            string completeMessage = string.Empty;
            int missingFileNameCount = 0;
            bool result = false;
            var fileHandler = new SignGenFileHandler();
            var invalidFile = fileHandler.CheckInvalidFiles(TemplatePathes.Concat(new List<string>() { InputFileName, LogoFileName }));
            if (!string.IsNullOrEmpty(invalidFile))
            {
                return new SignGenResult($"Kann folgende Datei nicht finden: {invalidFile}\nDie Ausführung wird abgebrochen.");
            }

            var entries = fileHandler.ReadInputFile(InputFileName);
            if (!entries.Any())
            {
                return new SignGenResult("In der eingelesenen Konfiguration konnten keine Einträge gefunden werden. Bitte prüfen sie die einzulesende Konfiguration.");
            }

            foreach (var entry in entries)
            {
                string dirName = string.Empty;
                string targetDir = string.Empty;
                Tuple<string, string> logoTuple;
                IEnumerable<Tuple<string, string>> templateTuples;
                IEnumerable<Tuple<string, string>> successfulCopies;
                if (!entry.TryGetValue("FILENAME", out dirName))
                {
                    dirName = $"unbekannt{++missingFileNameCount}";
                }

                targetDir = fileHandler.GetTargetDirectory(TargetDirectory, dirName);
                if (string.IsNullOrEmpty(targetDir))
                {
                    completeMessage += $"\nKonnte Ordner \"{dirName}\" nicht erstellen. Datensatz wurde übersprungen.";
                    continue;
                }

                logoTuple = fileHandler.GetTargetFileName(targetDir, LogoFileName);
                templateTuples = TemplatePathes.Select(p => fileHandler.GetTargetFileName(targetDir, p));

                if (!fileHandler.CopyToTarget(logoTuple))
                {
                    completeMessage += $"\nDas Firmenlogo konnte nicht kopiert werden. (\"{logoTuple.Item1}\" => \"{logoTuple.Item2}\")";
                }
                successfulCopies = templateTuples.Where(fileHandler.CopyToTarget).ToList();
                foreach (var tuple in templateTuples.Where(t => !successfulCopies.Contains(t)))
                {
                    completeMessage += $"\nEine Vorlage konnte nicht kopiert werden: (\"{tuple.Item1}\" => \"{tuple.Item2}\")";
                }

                foreach (var tuple in successfulCopies)
                {
                    if (!fileHandler.ReplaceKeysInFile(tuple.Item2, entry))
                    {
                        completeMessage += $"\nBeim Einfügen der Werte in die Datei \"{tuple.Item2}\" ist ein Fehler aufgetreten.";
                    }
                    else
                    {
                        result = true;
                    }
                } 
            }

            // result zeigt ob mindestens eine Signatur erfolgreich erstellt wurde
            return new SignGenResult(completeMessage, result);
        }
    }
}
