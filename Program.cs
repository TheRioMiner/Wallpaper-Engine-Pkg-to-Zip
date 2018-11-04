using System;
using System.IO;

namespace Wallpaper_Engine_Pkg_To_Zip
{
    class Program
    {
        static void Main(string[] args)
        {
            //Стартовое приветствие
            Console.ForegroundColor = ConsoleColor.Yellow;  
            Console.WriteLine("\n<-  Wallpaper Engine PKGV0001 to Zip and back  ->");
            Console.WriteLine("<-              by TheRioMiner                 ->\n");
            Console.ForegroundColor = ConsoleColor.Gray;

            //Нам пихнули нормальные аргументы?
            if (args.Length >= 3)
            {
                string mode = args[0].ToLower();
                string pkg = args[1];
                string zip = args[2];

                //Pkg в zip
                if (mode == "-pkg2zip" || mode == "-pkgtozip")
                {
                    if (File.Exists(pkg))
                    {
                        Console.WriteLine($"Reading pkg: {pkg}");

                        var pkgInfo = new PkgInfo();
                        try
                        {
                            //Читаем инфо о пакете!
                            pkgInfo = Converter.ReadPkgInfo(pkg);
                        }
                        catch (InvalidDataException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Given pkg file is not pkg file format! Readed signature: '{pkgInfo.Signature}'");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            return;
                        }

                        //Пишем сколько файлов в архиве и начинаем упаковку в zip архив
                        Console.WriteLine($"Files in pkg: {pkgInfo.FilesCount}");
                        Console.WriteLine($"Starting repacking to zip: {zip}");

                        //Конвертируем пакет в zip архив!
                        Converter.PkgToZip(pkgInfo, zip);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Pkg file: {pkg} not found! You are sure about correctness of this path?");
                    }
                }
                else
                {
                    if (File.Exists(zip))
                    {
                        Console.WriteLine($"Reading zip: {zip}");

                        //Создаем инфо о пакете
                        var pkgInfo = Converter.CreatePkgInfoFromZip(zip, pkg);

                        //Пишем сколько файлов в архиве и начинаем упаковку в пакет
                        Console.WriteLine($"Files in zip: {pkgInfo.FilesCount}");
                        Console.WriteLine($"Starting repacking to pkg: {pkg}");

                        try
                        {
                            //Конвертируем из zip архива в пакет!
                            Converter.ZipToPkg(pkgInfo, zip);
                        }
                        catch (Exception ex)
                        {
                            //Словили молоток!
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"> Critical exception {ex.Message} catched at repacking zip to pkg!");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Zip file: {zip} not found! You are sure about correctness of this path?");
                    }
                }
            }
            else
            {
                //Показываем как правильно использовать программу
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Usage: \"Wallpaper Engine Pkg To Zip.exe\" [-mode] [pkgFile] [zipFile]");
                Console.WriteLine("-mode        \"-pkg2zip\" or \"-zip2pkg\"");
                Console.WriteLine("pkgFile      Wallpaper Engine \".pkg\" file path");
                Console.WriteLine("zipFile      Archive \".zip\" file path");
                Console.WriteLine("Example: \"Wallpaper Engine Pkg To Zip.exe\" -pkg2zip scene.pkg result.zip");
                //Console.ReadKey();
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
