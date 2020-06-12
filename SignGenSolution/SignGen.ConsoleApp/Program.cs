using SignGen.ConsoleApp.Properties;
using SignGen.Logic;
using System;
using System.Collections.Generic;

namespace SignGen.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Wilkommen bei SignGen!");
            SignGenLauncher launcher = null;
            var input = Resources.InputConfigPath;
            var logo = Resources.CompanyLogoPath;
            var templatesString = Resources.TemplatePathes;
            var target = Resources.TargetDirectory;
            IEnumerable<string> templates = null;
            if (!string.IsNullOrEmpty(templatesString))
            {
                templates = templatesString.Split(";");
            }
            try
            {
                launcher = new SignGenLauncher(input, templates, logo, target);
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
