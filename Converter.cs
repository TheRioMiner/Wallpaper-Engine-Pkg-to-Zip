using System;
using System.IO;
using System.IO.Compression;

namespace Wallpaper_Engine_Pkg_To_Zip
{
    public class Converter
    {
        /// <summary>
        /// Получить информацию об пакете
        /// </summary>
        /// <param name="pkgFilePath">Путь к файлу пакета .pkg/param>
        /// <returns></returns>
        public static PkgInfo ReadPkgInfo(string pkgFilePath)
        {
            PkgInfo pkgInfo = new PkgInfo(pkgFilePath);
            using (var pkgFileStream = new FileStream(pkgFilePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(pkgFileStream))
            {
                //Читаем сигнатуру файла
                int maybeSignatureLenght = br.ReadInt32();
                pkgInfo.Signature = new string(br.ReadChars(8));

                if (pkgInfo.Signature != "PKGV0001") //Проверяем что это PKGV0001?
                    throw new InvalidDataException("Given file is not 'PKGV0001'!"); //Нас обманули!

                //Читаем кол. файлов в пакете
                pkgInfo.FilesCount = br.ReadInt32();

                //Сквозь все файлы в пакете
                for (int i = 0; i < pkgInfo.FilesCount; i++)
                {
                    string path = new string(br.ReadChars(br.ReadInt32()));
                    int offset = br.ReadInt32();
                    int lenght = br.ReadInt32();

                    pkgInfo.Files.Add(new PkgInfo.FileInfo() { Path = path, Offset = offset, Lenght = lenght });
                }

                //Получаем начало содержимого файлов
                pkgInfo.Offset = (int)(br.BaseStream.Position);
            }

            //Возвращяем полученную информацию об пакете
            return pkgInfo;
        }



        public static void PkgToZip(PkgInfo pkgInfo, string zipFilePath)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            using (FileStream pkgFileStream = new FileStream(pkgInfo.FilePath, FileMode.Open, FileAccess.Read))
            using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                int filesPacked = 0;
                foreach (var file in pkgInfo.Files)
                {
                    try
                    {
                        //Создаем новое вхождение в архиве с нужным названием
                        var fileEntry = zipArchive.CreateEntry(file.Path, CompressionLevel.NoCompression);
                        using (Stream writer = Stream.Synchronized(fileEntry.Open()))
                        {
                            byte[] binBytes = new byte[file.Lenght];

                            //Переходим в нужную позицию в пакете
                            pkgFileStream.Seek(pkgInfo.Offset + file.Offset, SeekOrigin.Begin);
                            int readedCount = pkgFileStream.Read(binBytes, 0, file.Lenght);

                            if (readedCount != file.Lenght) //Кидаемься молотком, если вдруг насокячили с чтением
                                throw new ArgumentOutOfRangeException($"File lenght: {file.Lenght}, but readed: {readedCount}");

                            //Записываем в архив
                            writer.Write(binBytes, 0, readedCount);
                            writer.Flush();
                        }

                        //Успешно перепаковали
                        filesPacked++;
                        Console.WriteLine($"{filesPacked}:> {file.Path}");
                    }
                    catch (Exception ex)
                    {
                        //Словили молоток!
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"!> Exception {ex.Message} catched at file: {file.Path}!");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                }

                //Сообщаем об результате
                if (filesPacked == pkgInfo.FilesCount)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"All {pkgInfo.FilesCount} pkg files repacked to zip successfully!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Only {filesPacked} of {pkgInfo.FilesCount} pkg files repacked to zip!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }   
            }
        }





        /// <summary>
        /// Создать информацию о пакете для его последуещего создания из zip архива
        /// </summary>
        /// <param name="zipFilePath">Путь к zip файлу</param>
        /// <param name="pkgFilePath">Путь к пакету который будет создан</param>
        /// <returns></returns>
        public static PkgInfo CreatePkgInfoFromZip(string zipFilePath, string pkgFilePath)
        {
            PkgInfo pkgInfo = new PkgInfo();
            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
            {
                pkgInfo.Signature = "PKGV0001";
                pkgInfo.FilePath = pkgFilePath;
                pkgInfo.FilesCount = zipArchive.Entries.Count;

                //Прекомпутируем оффсет начального файла
                pkgInfo.Offset += 4 + pkgInfo.Signature.Length + 4; //signatureStringLenght + "signatureString" + filesCountInt
                foreach (var entry in zipArchive.Entries)
                    pkgInfo.Offset += (4 + entry.FullName.Length + 4 + 4); //pathStringLenght + "pathString" + offsetInt + lenghtInt

                //Генерируем дерево файлов
                int filesOffset = 0;
                foreach (var entry in zipArchive.Entries)
                {
                    pkgInfo.Files.Add(new PkgInfo.FileInfo() { Path = entry.FullName, Lenght = (int)(entry.Length), Offset = filesOffset, });
                    filesOffset += (int)(entry.Length);
                }
            }

            //Возвращяем готовую информацию об пакете
            return pkgInfo;
        }



        public static void ZipToPkg(PkgInfo pkgInfo, string zipFilePath)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            using (var pkgFileStream = new FileStream(pkgInfo.FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
            using (var pkgBinaryWriter = new BinaryWriter(pkgFileStream))
            {
                //Сообщаем об прогрессе
                Console.WriteLine($"Writing main signature and files count...");

                pkgBinaryWriter.Write(pkgInfo.Signature.Length); //Длина строки сигнатуры (наверное)
                pkgBinaryWriter.Write(pkgInfo.Signature.ToCharArray()); //Сигнатура файла (!Обязательно как массив символов!)

                //Записываем кол. файлов в архиве
                pkgBinaryWriter.Write(pkgInfo.FilesCount);

                Console.WriteLine($"Writing files tree...");

                //Создаем дерево файлов
                foreach (var file in pkgInfo.Files)
                {
                    //Записываем длину строки пути файла и саму строку
                    pkgBinaryWriter.Write(file.Path.Length);
                    pkgBinaryWriter.Write(file.Path.ToCharArray()); //(!Обязательно как массив символов!)

                    //Записываем оффсет этого файла в пакете
                    pkgBinaryWriter.Write(file.Offset);

                    //Записываем длину файла
                    pkgBinaryWriter.Write(file.Lenght);
                }

                Console.WriteLine($"Starting writing files data to pkg...");
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                //Наконец все файлы впихываем
                int filesPacked = 0;
                for (int i = 0; i < pkgInfo.Files.Count; i++)
                {
                    var entry = zipArchive.Entries[i];
                    using (var stream = Stream.Synchronized(entry.Open()))
                    {
                        byte[] readedBytes = new byte[entry.Length];
                        int readedCount = stream.Read(readedBytes, 0, readedBytes.Length);

                        if (readedCount != entry.Length) //Кидаемься молотком, если вдруг насокячили с чтением
                            throw new ArgumentOutOfRangeException($"File lenght: {entry.Length}, but readed: {readedCount}");

                        //Пихуем файл в пакет
                        pkgBinaryWriter.Write(readedBytes, 0, readedCount);
                    }

                    filesPacked++;
                    Console.WriteLine($"{filesPacked}:> {entry.FullName}");
                }

                //Сообщаем об результате
                if (filesPacked == pkgInfo.Files.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"All {pkgInfo.FilesCount} zip files repacked to pkg successfully!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Only {filesPacked} of {pkgInfo.FilesCount} packed... Something went wrong!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

    }
}
