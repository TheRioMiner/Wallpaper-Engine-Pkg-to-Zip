using System;

namespace Wallpaper_Engine_Pkg_To_Zip
{
    class Program
    {
        public static string Version = "v2.0";
        public static string Greetings = $"\n┌──────────────────────────────────────────┬──────┐\n│   Wallpaper Engine Pkg to Zip and back   │ {Version} │\n├──────────────────────────────────────────┴──────┤\n│             Supported pkg versions:             │\n│                   \"PKGV0001\"                    │\n├─────────────────────────────────────────────────┤\n│                 by TheRioMiner                  │\n╘═════════════════════════════════════════════════╛\n";
        //public static string Greetingsv21 = $"\n┌──────────────────────────────────────────┬──────┐\n│   Wallpaper Engine Pkg to Zip and back   │ {Version} │\n├──────────────────────────────────────────┴──────┤\n│             Supported pkg versions:             │\n│             \"PKGV0001\", \"PKGV0002\"              │\n├─────────────────────────────────────────────────┤\n│                 by TheRioMiner                  │\n╘═════════════════════════════════════════════════╛\n";
        public static string ZipComment = $"┌─────────────────────────────────────────────────┐\n│        This zip was created by program:         │\n├──────────────────────────────────────────┬──────┤\n│   Wallpaper Engine Pkg to Zip and back   │ {Version} │\n├──────────────────────────────────────────┴──────┤\n│                 by TheRioMiner                  │\n╘═════════════════════════════════════════════════╛\n";

        static void Main(string[] args)
        {
            //Starting greeting
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Greetings);
            Console.ForegroundColor = ConsoleColor.Gray;

            //Нам пихнули нормальные аргументы?
            if (args.Length >= 3)
            {
                string mode = args[0].ToLower();
                string pkg = args[1];
                string zip = args[2];

                PkgConverter converter = null;

                bool bMode;
                if (mode == "-pkg2zip" || mode == "-pkgtozip" || mode == "-pkg_to_zip" || mode == "-pkg-to-zip")
                    bMode = true;  //Pkg2Zip
                else if (mode == "-zip2pkg" || mode == "-ziptopkg" || mode == "-zip_to_pkg" || mode == "-zip-to-pkg")
                    bMode = false; //Zip2Pkg
                else
                {
                    ShowUsage(); //Invalid mode putted, show how usage program
                    return;
                }


                try
                {
                    converter = new PkgConverter(pkg, zip, bMode);
                }
                catch (PkgConverter.PkgConverterException ex)
                {
                    //Error handling
                    Console.ForegroundColor = ConsoleColor.Red;
                    {
                        switch (ex.Error)
                        {
                            case PkgConverter.Error.PKG_FILE_NOT_FOUND:
                                Console.WriteLine($"Pkg file: '{pkg}' not found! You are sure about correctness of this path?");
                                break;
                            case PkgConverter.Error.FAILED_TO_CREATE_FILE_STREAM:
                                Console.WriteLine($"Failed to create file streams for pkg: '{pkg}' and zip: '{zip}' - Message:[{ex.SrcMsg}]");
                                break;
                            case PkgConverter.Error.FAILED_TO_OPEN_ZIP_ARCHIVE:
                                Console.WriteLine($"Failed to open zip archive: '{zip}' - Message:[{ex.SrcMsg}]");
                                break;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }


                //Convert!
                try
                {
                    converter.Convert();
                }
                catch (PkgConverter.PkgConverterException ex)
                {
                    //Error handling
                    Console.ForegroundColor = ConsoleColor.Red;
                    switch (ex.Error)
                    {
                        case PkgConverter.Error.UNHANDLED_EXCEPTION:
                            Console.WriteLine($"Unhandled exception occured! - Message:[{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.PKG_FILE_CORRUPTED:
                            Console.WriteLine($"Pkg file: '{pkg}' corrupted or unhandled error! - Message:[{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.INVALID_PKG_FILE_SIGNATURE:
                            Console.WriteLine($"Unknown pkg version - {ex.SrcMsg}");
                            break;
                        case PkgConverter.Error.FAILED_SEEKING_PKG_FILE:
                            Console.WriteLine($"Failed seeking in pkg file - [{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.FAILED_READING_PKG_FILE:
                            Console.WriteLine($"Failed reading pkg file! - Message:[{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.READED_LENGHT_NOT_EQUALS_NEED_LENGHT:
                            Console.WriteLine($"Readed length != Need length - Message:[{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.FAILED_WRITING_INTO_ZIP_FILE:
                            Console.WriteLine($"Failed writing into zip file! - Message:[{ex.SrcMsg}]");
                            break;
                        case PkgConverter.Error.STREAM_COPYTO_EXCEPTION:
                            Console.WriteLine($"Failed copying stream to another stream! - Message:[{ex.SrcMsg}]");
                            break;
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.ReadLine();
                    return;
                }
            }
            else
                ShowUsage();

            Console.ForegroundColor = ConsoleColor.Gray;
        }



        private static void ShowUsage()
        {
            //Показываем как правильно использовать программу
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Usage: \"Wallpaper Engine Pkg To Zip.exe\" [-mode] [pkgFile] [zipFile]");
            Console.WriteLine("-mode        \"-pkg2zip\" or \"-zip2pkg\"");
            Console.WriteLine("pkgFile      Wallpaper Engine \".pkg\" file path");
            Console.WriteLine("zipFile      Archive \".zip\" file path");
            Console.WriteLine("Example: \"Wallpaper Engine Pkg To Zip.exe\" -pkg2zip scene.pkg result.zip");
            Console.ReadKey();
        }
    }
}
