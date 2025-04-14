using MemoryPack;
using System;
using System.Diagnostics;
using System.IO;
using static BlueArchiveAssetConvert.Utils.Utils;
using static BlueArchiveAssetConvert.Utils.Ask;
using BlueArchiveAssetConvert.BlueArchiveConvert;
using System.Text.Json;

namespace Blue_Archive_Classes
{
    public class BlueArchive
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Blue Archive 资源提取器");
            Console.WriteLine();
            DisplayHelpIfArgIsHelp(args, "用法: [CatalogType资源目录类型] [资源所在文件夹] " +
                "[Catalog目录二进制文件路径] [想要输出到XX文件夹]" + newLineStr + "例: MediaCatalog E:\\MediaPatch E:\\MediaPatch\\MediaCatalog.bytes .\\output");


            var specifiedCatalogType = CatalogType.MediaCatalog;
            var assetFolderPath = "";
            var catalogBinPath = "";
            var outputFolderPath = "";
            string modeIndex = "1";

            ParseOrAskENumValue<CatalogType>("请输入希望转换的目录" + newLineStr + "请从上面的目录列表中选择" + newLineStr + "你可以按数字选择，但也可以按照它输入名称",
    ref specifiedCatalogType, 0, args);
            if (specifiedCatalogType == CatalogType.MediaCatalog)
            {
                Console.WriteLine("版本更新后，角色的语音单独放在了MediaPatch\\Catalog\\MediaCatalog.bytes中，模式2为单独提取角色语音压缩包，输入数字进行选择");
                Console.WriteLine("选择模式：1.提取所有\t2.提取语音压缩包");
                modeIndex = Console.ReadLine().ToString();
            }
            // 这里需要实现 ParseOrAskAndValidPath 方法
            assetFolderPath = ParseOrAskAndValidPath(false, "输入资源所在文件夹", 2, args);
            catalogBinPath = ParseOrAskAndValidPath(true, "输入对应的Catalog.bytes文件路径", 3, args);
            outputFolderPath = ParseOrAskAndValidPath(false, "输入资源输出路径", 4, args);

            Directory.SetCurrentDirectory(outputFolderPath);

            Console.WriteLine("开始拷贝" + Environment.NewLine + "Logs are not displayed to improve processing speed");

            switch (specifiedCatalogType)
            {
                case CatalogType.MediaCatalog:
                    CopyFilesWithCatalog(assetFolderPath, catalogBinPath, modeIndex);
                    break;

                case CatalogType.TableCatalog:
                    CopyFilesWithCatalog(assetFolderPath, catalogBinPath, "3");
                    break;

                default:
                    Console.WriteLine($"Error: The specified CatalogType {specifiedCatalogType} is invalid");
                    break;
            }
            
            Console.WriteLine("拷贝完成" + newLineStr + "是否将Catalog转换为Json格式？(y/n)");

            byte[] mediaCatalogBytes = File.ReadAllBytes(catalogBinPath);
            MediaCatalog mediaCatalog = MemoryPack.MemoryPackSerializer.Deserialize<MediaCatalog>(mediaCatalogBytes);
            string userConfirm = Console.ReadLine();
            if (userConfirm.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                string jsonFileName = "MediaCatalog.json";
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(mediaCatalog, jsonOptions);
                File.WriteAllText(jsonFileName, json);

            }


            Console.WriteLine("Done" + newLineStr + "Press enter key to exit...");
            Console.ReadLine();
        }
        private static void CopyFilesWithCatalog(string assetFolderPath, string catalogBinPath, string modeIndex)
        {
            byte[]? catalogBin = null;
            if (File.Exists(catalogBinPath))
            {
                catalogBin = File.ReadAllBytes(catalogBinPath);
            }
            else
            {
                Console.WriteLine("Catalog binary file not found");
                return;
            }

            if (catalogBin == null)
            {
                Console.WriteLine("Failed to load catalog binary - variable is null");
                return;
            }

            MediaCatalog mediaCatalog;
            TableCatalog tableCatalog;
            switch (modeIndex)
            {
                case "1":
                    mediaCatalog = MemoryPackSerializer.Deserialize<MediaCatalog>(catalogBin);
                    if (mediaCatalog == null)
                    {
                        Console.WriteLine("Failed to load catalog - variable is null");
                        return;
                    }
                    foreach (var pair in mediaCatalog.Table)
                    {
                        var media = pair.Value;
                        var srcFileArray = Directory.GetFiles(assetFolderPath, "*_" + media.Crc.ToString());
                        if (srcFileArray.Length > 0)
                        {
                            string[] pathArray = media.Path.Split("/");
                            string directoryPath = Path.Combine(pathArray.Take(pathArray.Length - 1).ToArray());
                            Directory.CreateDirectory(directoryPath);
                            if (directoryPath.Contains("Audio"))
                            {

                            }
                            string destinationPath = Path.Combine(directoryPath, pathArray.Last());
                            File.Copy(srcFileArray[0], destinationPath, true);
                        }
                    }
                    break;
                case "2":
                    mediaCatalog = MemoryPackSerializer.Deserialize<MediaCatalog>(catalogBin);
                    if (mediaCatalog == null)
                    {
                        Console.WriteLine("Failed to load catalog - variable is null");
                        return;
                    }
                    foreach (var pair in mediaCatalog.Table)
                    {
                        var media = pair.Value;
                        var srcFileArray = Directory.GetFiles(assetFolderPath, "*_" + media.Crc.ToString());
                        if (srcFileArray.Length > 0)
                        {
                            string[] pathArray = media.Path.Split("\\");
                            string directoryPath = Path.Combine(pathArray.Take(pathArray.Length - 1).ToArray());
                            Directory.CreateDirectory(directoryPath);
                            string destinationPath = Path.Combine(directoryPath, pathArray.Last());
                            File.Copy(srcFileArray[0], destinationPath, true);
                        }
                    }
                    break;
                case "3":
                    tableCatalog = MemoryPackSerializer.Deserialize<TableCatalog>(catalogBin);
                    if (tableCatalog == null)
                    {
                        Console.WriteLine("Failed to load catalog - variable is null");
                        return;
                    }

                    foreach (var pair in tableCatalog.Table)
                    {
                        var tableBundle = pair.Value;

                        var srcFileArray = Directory.GetFiles(assetFolderPath, "*_" + tableBundle.Crc.ToString());
                        if (srcFileArray.Length > 0)
                        {
                            File.Copy(srcFileArray[0], tableBundle.Name);
                        }
                    }

                    break;
            }
        }

        enum CatalogType
        {
            MediaCatalog,
            TableCatalog
        }
    }
}