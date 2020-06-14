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
        public SignGenLauncher(string inputFileName, IEnumerable<string> templates, string logoFileName, string targetDirectory = null, string imageDirectory = null, string defaultEncoding = "UTF-8", string imageFileType = ".jpg", bool overwriteExisting = false)
        {
            if (string.IsNullOrEmpty(inputFileName))
            {
                throw new ArgumentException("Ein Pfad für die einzulesende Konfiguration muss gegeben sein", nameof(inputFileName));
            }
            if (templates == null || !templates.Any())
            {
                throw new ArgumentException("Mindestens eine Vorlage muss definiert sein", nameof(logoFileName));
            }

            OverwriteExisting = overwriteExisting;
            InputFileName = inputFileName;
            LogoFileName = logoFileName ?? string.Empty;
            TargetDirectory = targetDirectory ?? string.Empty;
            ImageDirectory = imageDirectory ?? string.Empty;
            DefaultEncoding = defaultEncoding ?? string.Empty;
            ImageFileType = imageFileType ?? string.Empty;
            TemplatePathes = templates.ToList();

            if (!(string.IsNullOrEmpty(ImageFileType) || ImageFileType.StartsWith(".")))
            {
                ImageFileType = "." + ImageFileType;
            }
        }

        protected virtual bool OverwriteExisting { get; set; }
        protected virtual string InputFileName { get; set; }
        protected virtual string LogoFileName { get; set; }
        protected virtual string TargetDirectory { get; set; }
        protected virtual string ImageDirectory { get; set; }
        protected virtual string DefaultEncoding { get; set; }
        protected virtual string ImageFileType { get; set; }
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

            var entries = fileHandler.ReadInputFile(InputFileName, DefaultEncoding);
            if (!entries.Any())
            {
                return new SignGenResult("In der eingelesenen Konfiguration konnten keine Einträge gefunden werden. Bitte prüfen sie die einzulesende Konfiguration.");
            }

            foreach (var entry in entries)
            {
                string dirName = string.Empty;
                string accountName = string.Empty;
                string imageName = string.Empty;
                string targetDir = string.Empty;
                Tuple<string, string> logoTuple;
                Tuple<string, string> imageTuple;
                IEnumerable<Tuple<string, string, string>> templateTuples;
                if (!entry.TryGetValue("FILENAME", out dirName))
                {
                    dirName = $"unbekannt{++missingFileNameCount}";
                }

                if (entry.TryGetValue("ACCOUNTNAME", out accountName))
                {
                    imageName = fileHandler.GetFileName(ImageDirectory, accountName, ImageFileType);
                }

                targetDir = fileHandler.GetTargetDirectory(TargetDirectory, dirName);
                if (string.IsNullOrEmpty(targetDir))
                {
                    completeMessage += $"\nKonnte Ordner \"{dirName}\" nicht erstellen. Datensatz wurde übersprungen.";
                    continue;
                }

                imageTuple = fileHandler.GetTargetFileName(targetDir, imageName);
                logoTuple = fileHandler.GetTargetFileName(targetDir, LogoFileName);
                templateTuples = TemplatePathes.Select(p => fileHandler.GetTargetFileName(targetDir, p))
                                         .Select(t => Tuple.Create(t.Item1, t.Item2, GetCopyResult(fileHandler, t, "Eine Vorlage", OverwriteExisting)))
                                         .ToList();

                completeMessage += GetCopyResult(fileHandler, logoTuple, "Das Firmenlogo", OverwriteExisting);
                completeMessage += GetCopyResult(fileHandler, imageTuple, "Das Benutzerfoto", OverwriteExisting);

                foreach (var tuple in templateTuples)
                {
                    if (!string.IsNullOrEmpty(tuple.Item3))
                    {
                        completeMessage += tuple.Item3;
                    }
                    else if (!fileHandler.ReplaceKeysInFile(tuple.Item2, entry, DefaultEncoding))
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

        internal virtual string GetCopyResult(SignGenFileHandler handler, Tuple<string, string> copyTuple, string objectName, bool overwriteExisting)
        {
            if (!handler.CopyToTarget(copyTuple, overwriteExisting))
            {
                return $"\n{objectName} konnte nicht kopiert werden. (\"{copyTuple.Item1}\" => \"{copyTuple.Item2}\")";
            }

            return string.Empty;
        }
    }
}
