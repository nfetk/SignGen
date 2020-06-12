using Microsoft.Extensions.Configuration;
using SignGen.Logic;
using System;
using System.Linq;

namespace SignGen.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Wilkommen bei SignGen!");
            SignGenLauncher launcher = null;
            var config = GetConfiguration(args);
            var input = config.GetSection("InputConfigPath").Value;
            var logo = config.GetSection("CompanyLogoPath").Value;
            var target = config.GetSection("TargetDirectory").Value;
            var templates = config.GetSection("TemplatePathes").GetChildren().Select(c => c.Value).ToList();
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

        private static IConfiguration GetConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }
    }
}
