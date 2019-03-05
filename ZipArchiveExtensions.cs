using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.IO.Compression;

namespace Wallpaper_Engine_Pkg_To_Zip
{
    public static class ZipArchiveExtensions
    {
        public static string GetComment(this ZipArchive archive, Encoding encoding)
        {
            //Getting binary reader of archive
            var type = archive.GetType();
            var field = type.GetField("_archiveReader", BindingFlags.NonPublic | BindingFlags.Instance);
            var binaryReader = field.GetValue(archive) as BinaryReader;
            {
                long savedPos = binaryReader.BaseStream.Position; //Saving pos of stream to prevent errors
                if (SeekBackwardsToSignature32(binaryReader.BaseStream, 101010256)) //EndOfCentralDirectoryBlock
                {
                    binaryReader.BaseStream.Seek(20, SeekOrigin.Current); //Skipping don't need data

                    ushort count = binaryReader.ReadUInt16(); //ArchiveCommentLength
                    byte[] archiveComment = binaryReader.ReadBytes(count); //ArchiveComment

                    return Encoding.UTF8.GetString(archiveComment); //Returning the result
                }
                binaryReader.BaseStream.Seek(savedPos, SeekOrigin.Begin); //Restore original pos of stream
            }
            return "";
        }


        public static void SetComment(this ZipArchive archive, string comment, Encoding encoding)
        {
            var type = archive.GetType();
            var field = type.GetField("_archiveComment", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(archive, encoding.GetBytes(comment));
        }



        private static bool SeekBackwardsToSignature32(Stream stream, uint signatureToFind)
        {
            long savedPos = stream.Position;
            for (long i = stream.Length; i >= 4; i--)
            {
                stream.Seek(i, SeekOrigin.Begin);

                //Read
                byte[] buff = new byte[4];
                stream.Read(buff, 0, 4);
                uint val = BitConverter.ToUInt32(buff, 0);
                if (val == signatureToFind)
                {
                    stream.Seek(i, SeekOrigin.Begin); //Seeking into signature
                    return true;
                }
            }
            stream.Seek(savedPos, SeekOrigin.Begin);
            return false;
        }
    }
}
