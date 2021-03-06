﻿using Microsoft.Extensions.Configuration;
using SignGen.Logic;
using System;
using System.Linq;

namespace SignGen.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var config = GetConfiguration(args);
            Console.WriteLine("Wilkommen bei SignGen!");
            Console.WriteLine("Möchten Sie Signaturen generieren? (j/n)");
            string entered = Console.ReadLine();
            while (entered == "j" || entered == "J")
            {
                Execute(config);
                Console.WriteLine("Möchten Sie erneut generieren? (j/n)");
                entered = Console.ReadLine();
            }
            Console.Write("Drücken Sie eine beliebige Taste, um SignGen zu beenden...");
            Console.ReadKey();
        }

        private static void Execute(IConfiguration config)
        {
            SignGenLauncher launcher = null;
            bool overwrite = false;
            var input = config.GetSection("InputConfigPath").Value;
            var logo = config.GetSection("CompanyLogoPath").Value;
            var target = config.GetSection("TargetDirectory").Value;
            var image = config.GetSection("AccountImageDirectory").Value;
            var encoding = config.GetSection("DefaultEncoding").Value;
            var imageFileType = config.GetSection("DefaultImageFileType").Value;
            var templates = config.GetSection("TemplatePathes").GetChildren().Select(c => c.Value).ToList();
            bool.TryParse(config.GetSection("OverwriteExisting").Value, out overwrite);
            try
            {
                launcher = new SignGenLauncher(input, templates, logo, target, image, encoding, imageFileType, overwrite);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Die angegebenen Parameter stimmen nicht mit der Erwartung überein. Bitte prüfen Sie Ihre Konfiguration.");
                Console.WriteLine("Der Fehler lautet: " + e.Message);
                launcher = null;
            }

            if (launcher != null)
            {
                Console.WriteLine("Ihre Signaturen werden generiert. Bitte haben Sie einen Moment Geduld...");
                var result = launcher.Run();
                if (result.Succeeded)
                {
                    Console.WriteLine("Ihre Signaturen wurden erfolgreich generiert.");
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        Console.WriteLine("Folgende Warnungen sind dabei zu beachten: " + result.Message);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        Console.WriteLine(result.Message);
                    }
                    Console.WriteLine("Beim Generieren der Signaturen ist ein Fehler aufgetreten.");
                }
            }
        }

        private static IConfiguration GetConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
