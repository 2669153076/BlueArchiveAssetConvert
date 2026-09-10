using MemoryPack;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using static BlueArchiveAssetConvert.Utils.Utils;
using static BlueArchiveAssetConvert.Utils.Ask;
using BlueArchiveAssetConvert.BlueArchiveConvert;

namespace Blue_Archive_Classes
{
    public class BlueArchive
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Blue Archive 资源提取器");
            Console.WriteLine();

            DisplayHelpIfArgIsHelp(
                args,
                "用法: [CatalogType资源目录类型] [资源所在文件夹] " +
                "[Catalog目录二进制文件路径] [想要输出到XX文件夹]" +
                newLineStr +
                "例: MediaCatalog E:\\MediaPatch E:\\MediaPatch\\MediaCatalog.bytes .\\output"
            );

            var specifiedCatalogType = CatalogType.MediaCatalog;

            var assetFolderPath = "";
            var catalogBinPath = "";
            var outputFolderPath = "";

            string modeIndex = "1";

            ParseOrAskENumValue<CatalogType>(
                "请输入希望转换的目录" +
                newLineStr +
                "请从上面的目录列表中选择" +
                newLineStr +
                "你可以按数字选择，但也可以按照它输入名称",
                ref specifiedCatalogType,
                0,
                args
            );

            if (specifiedCatalogType == CatalogType.MediaCatalog)
            {
                Console.WriteLine(
                    "版本更新后，角色的语音单独放在了" +
                    "MediaPatch\\Catalog\\MediaCatalog.bytes中"
                );

                Console.WriteLine(
                    "选择模式：1.提取所有\t2.提取语音压缩包"
                );

                modeIndex = Console.ReadLine() ?? "1";
            }

            assetFolderPath = ParseOrAskAndValidPath(
                false,
                "输入资源所在文件夹",
                2,
                args
            );

            catalogBinPath = ParseOrAskAndValidPath(
                true,
                "输入对应的Catalog.bytes文件路径",
                3,
                args
            );

            outputFolderPath = ParseOrAskAndValidPath(
                false,
                "输入资源输出路径",
                4,
                args
            );

            // 确保输出目录存在
            Directory.CreateDirectory(outputFolderPath);

            Console.WriteLine();
            Console.WriteLine("开始拷贝");
            Console.WriteLine("Logs are not displayed to improve processing speed");
            Console.WriteLine();

            switch (specifiedCatalogType)
            {
                case CatalogType.MediaCatalog:

                    CopyFilesWithCatalog(
                        assetFolderPath,
                        catalogBinPath,
                        outputFolderPath,
                        modeIndex
                    );

                    break;

                case CatalogType.TableCatalog:

                    CopyFilesWithCatalog(
                        assetFolderPath,
                        catalogBinPath,
                        outputFolderPath,
                        "3"
                    );

                    break;

                default:

                    Console.WriteLine(
                        $"Error: The specified CatalogType " +
                        $"{specifiedCatalogType} is invalid"
                    );

                    break;
            }

            Console.WriteLine();
            Console.WriteLine(
                "拷贝完成" +
                newLineStr +
                "是否将Catalog转换为Json格式？(y/n)"
            );

            string userConfirm = Console.ReadLine() ?? "";

            if (userConfirm.Equals(
                "y",
                StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (specifiedCatalogType == CatalogType.MediaCatalog)
                    {
                        ConvertMediaCatalogToJson(
                            catalogBinPath,
                            outputFolderPath
                        );
                    }
                    else if (specifiedCatalogType == CatalogType.TableCatalog)
                    {
                        ConvertTableCatalogToJson(
                            catalogBinPath,
                            outputFolderPath
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("Catalog 转换 JSON 失败：");
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine();
            Console.WriteLine(
                "Done" +
                newLineStr +
                "Press enter key to exit..."
            );

            Console.ReadLine();
        }

        // ============================================================
        // Catalog 资源复制
        // ============================================================

        private static void CopyFilesWithCatalog(
            string assetFolderPath,
            string catalogBinPath,
            string outputFolderPath,
            string modeIndex)
        {
            if (!File.Exists(catalogBinPath))
            {
                Console.WriteLine(
                    $"Catalog binary file not found: {catalogBinPath}"
                );

                return;
            }

            byte[] catalogBin;

            try
            {
                catalogBin = File.ReadAllBytes(catalogBinPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取 Catalog.bytes 失败：");
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine(
                $"Catalog 大小: {catalogBin.Length:N0} bytes"
            );

            switch (modeIndex)
            {
                // ====================================================
                // Mode 1
                // MediaCatalog：提取所有资源
                // ====================================================

                case "1":
                    {
                        MediaCatalog? mediaCatalog;

                        try
                        {
                            mediaCatalog =
                                MemoryPackSerializer.Deserialize<MediaCatalog>(
                                    catalogBin
                                );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                "MediaCatalog 反序列化失败："
                            );

                            Console.WriteLine(ex);
                            return;
                        }

                        if (mediaCatalog?.Table == null)
                        {
                            Console.WriteLine(
                                "MediaCatalog.Table 为空"
                            );

                            return;
                        }

                        Console.WriteLine(
                            $"Catalog 记录数量: {mediaCatalog.Table.Count:N0}"
                        );

                        int total = mediaCatalog.Table.Count;
                        int copied = 0;
                        int notFound = 0;
                        int invalidPath = 0;
                        int current = 0;

                        foreach (var pair in mediaCatalog.Table)
                        {
                            current++;

                            Media? media = pair.Value;

                            if (media == null)
                            {
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(media.Path))
                            {
                                invalidPath++;

                                Console.WriteLine(
                                    $"跳过空 Path，CRC: {media.Crc}"
                                );

                                continue;
                            }

                            // ------------------------------------------------
                            // 查找资源
                            // ------------------------------------------------

                            string searchPattern =
                                "*_" + media.Crc;

                            string[] srcFileArray;

                            try
                            {
                                srcFileArray = Directory.GetFiles(
                                    assetFolderPath,
                                    searchPattern,
                                    SearchOption.TopDirectoryOnly
                                );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"搜索资源失败: {searchPattern}"
                                );

                                Console.WriteLine(ex.Message);

                                continue;
                            }

                            if (srcFileArray.Length == 0)
                            {
                                notFound++;
                                continue;
                            }

                            // ------------------------------------------------
                            // 处理 Catalog 路径
                            // ------------------------------------------------

                            string normalizedPath =
                                NormalizeCatalogPath(media.Path);

                            if (string.IsNullOrWhiteSpace(normalizedPath))
                            {
                                invalidPath++;

                                Console.WriteLine(
                                    $"无效 Path: {media.Path}"
                                );

                                continue;
                            }

                            // ------------------------------------------------
                            // 输出路径
                            // ------------------------------------------------

                            string destinationPath =
                                Path.Combine(
                                    outputFolderPath,
                                    normalizedPath
                                );

                            // 防止目标路径目录不存在
                            string? destinationDirectory =
                                Path.GetDirectoryName(destinationPath);

                            if (!string.IsNullOrEmpty(
                                destinationDirectory))
                            {
                                Directory.CreateDirectory(
                                    destinationDirectory
                                );
                            }

                            // ------------------------------------------------
                            // 拷贝
                            // ------------------------------------------------

                            try
                            {
                                File.Copy(
                                    srcFileArray[0],
                                    destinationPath,
                                    true
                                );

                                copied++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    $"复制失败:"
                                );

                                Console.WriteLine(
                                    $"源: {srcFileArray[0]}"
                                );

                                Console.WriteLine(
                                    $"目标: {destinationPath}"
                                );

                                Console.WriteLine(
                                    ex.Message
                                );
                            }

                            // 每 1000 个显示一次进度
                            if (current % 1000 == 0)
                            {
                                Console.WriteLine(
                                    $"进度: {current:N0}/{total:N0} " +
                                    $"已复制: {copied:N0}"
                                );
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("========== 复制统计 ==========");
                        Console.WriteLine(
                            $"Catalog 总记录: {total:N0}"
                        );

                        Console.WriteLine(
                            $"成功复制: {copied:N0}"
                        );

                        Console.WriteLine(
                            $"资源不存在: {notFound:N0}"
                        );

                        Console.WriteLine(
                            $"无效路径: {invalidPath:N0}"
                        );

                        Console.WriteLine("==============================");

                        break;
                    }

                // ====================================================
                // Mode 2
                // MediaCatalog：提取语音压缩包
                // ====================================================

                case "2":
                    {
                        MediaCatalog? mediaCatalog;

                        try
                        {
                            mediaCatalog =
                                MemoryPackSerializer.Deserialize<MediaCatalog>(
                                    catalogBin
                                );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                "MediaCatalog 反序列化失败："
                            );

                            Console.WriteLine(ex);
                            return;
                        }

                        if (mediaCatalog?.Table == null)
                        {
                            Console.WriteLine(
                                "MediaCatalog.Table 为空"
                            );

                            return;
                        }

                        int total = mediaCatalog.Table.Count;
                        int copied = 0;
                        int notFound = 0;

                        foreach (var pair in mediaCatalog.Table)
                        {
                            Media? media = pair.Value;

                            if (media == null)
                            {
                                continue;
                            }

                            string searchPattern =
                                "*_" + media.Crc;

                            string[] srcFileArray =
                                Directory.GetFiles(
                                    assetFolderPath,
                                    searchPattern,
                                    SearchOption.TopDirectoryOnly
                                );

                            if (srcFileArray.Length == 0)
                            {
                                notFound++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(media.Path))
                            {
                                continue;
                            }

                            string normalizedPath =
                                NormalizeCatalogPath(media.Path);

                            string destinationPath =
                                Path.Combine(
                                    outputFolderPath,
                                    normalizedPath
                                );

                            string? destinationDirectory =
                                Path.GetDirectoryName(destinationPath);

                            if (!string.IsNullOrEmpty(
                                destinationDirectory))
                            {
                                Directory.CreateDirectory(
                                    destinationDirectory
                                );
                            }

                            try
                            {
                                File.Copy(
                                    srcFileArray[0],
                                    destinationPath,
                                    true
                                );

                                copied++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"复制失败: {destinationPath}"
                                );

                                Console.WriteLine(ex.Message);
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("========== 复制统计 ==========");
                        Console.WriteLine(
                            $"Catalog 总记录: {total:N0}"
                        );

                        Console.WriteLine(
                            $"成功复制: {copied:N0}"
                        );

                        Console.WriteLine(
                            $"资源不存在: {notFound:N0}"
                        );

                        Console.WriteLine("==============================");

                        break;
                    }

                // ====================================================
                // Mode 3
                // TableCatalog
                // ====================================================

                case "3":
                    {
                        TableCatalog? tableCatalog;

                        try
                        {
                            tableCatalog =
                                MemoryPackSerializer.Deserialize<TableCatalog>(
                                    catalogBin
                                );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                "TableCatalog 反序列化失败："
                            );

                            Console.WriteLine(ex);
                            return;
                        }

                        if (tableCatalog?.Table == null)
                        {
                            Console.WriteLine(
                                "TableCatalog.Table 为空"
                            );

                            return;
                        }

                        int total = tableCatalog.Table.Count;
                        int copied = 0;
                        int notFound = 0;

                        foreach (var pair in tableCatalog.Table)
                        {
                            TableBundle? tableBundle =
                                pair.Value;

                            if (tableBundle == null)
                            {
                                continue;
                            }

                            string searchPattern =
                                "*_" + tableBundle.Crc;

                            string[] srcFileArray =
                                Directory.GetFiles(
                                    assetFolderPath,
                                    searchPattern,
                                    SearchOption.TopDirectoryOnly
                                );

                            if (srcFileArray.Length == 0)
                            {
                                notFound++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(
                                tableBundle.Name))
                            {
                                continue;
                            }

                            string normalizedName =
                                NormalizeCatalogPath(
                                    tableBundle.Name
                                );

                            string destinationPath =
                                Path.Combine(
                                    outputFolderPath,
                                    normalizedName
                                );

                            string? destinationDirectory =
                                Path.GetDirectoryName(
                                    destinationPath
                                );

                            if (!string.IsNullOrEmpty(
                                destinationDirectory))
                            {
                                Directory.CreateDirectory(
                                    destinationDirectory
                                );
                            }

                            try
                            {
                                File.Copy(
                                    srcFileArray[0],
                                    destinationPath,
                                    true
                                );

                                copied++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"复制失败: {destinationPath}"
                                );

                                Console.WriteLine(
                                    ex.Message
                                );
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("========== 复制统计 ==========");
                        Console.WriteLine(
                            $"Catalog 总记录: {total:N0}"
                        );

                        Console.WriteLine(
                            $"成功复制: {copied:N0}"
                        );

                        Console.WriteLine(
                            $"资源不存在: {notFound:N0}"
                        );

                        Console.WriteLine("==============================");

                        break;
                    }

                default:

                    Console.WriteLine(
                        $"未知模式: {modeIndex}"
                    );

                    break;
            }
        }

        // ============================================================
        // Catalog 路径标准化
        // ============================================================

        private static string NormalizeCatalogPath(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string result = path.Trim();

            // Catalog 里面可能出现 / 或 \
            result = result.Replace(
                '\\',
                Path.DirectorySeparatorChar
            );

            result = result.Replace(
                '/',
                Path.DirectorySeparatorChar
            );

            // 删除开头的分隔符
            result = result.TrimStart(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );

            return result;
        }

        // ============================================================
        // MediaCatalog → JSON
        // ============================================================

        private static void ConvertMediaCatalogToJson(
            string catalogBinPath,
            string outputFolderPath)
        {
            Console.WriteLine();
            Console.WriteLine(
                "正在将 MediaCatalog.bytes 转换为 JSON..."
            );

            byte[] bytes =
                File.ReadAllBytes(catalogBinPath);

            Console.WriteLine(
                $"读取: {bytes.Length:N0} bytes"
            );

            MediaCatalog? catalog =
                MemoryPackSerializer.Deserialize<MediaCatalog>(
                    bytes
                );

            if (catalog == null)
            {
                Console.WriteLine(
                    "MediaCatalog 反序列化结果为空。"
                );

                return;
            }

            int count =
                catalog.Table?.Count ?? 0;

            Console.WriteLine(
                $"Media 数量: {count:N0}"
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,

                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json =
                JsonSerializer.Serialize(
                    catalog,
                    options
                );

            string outputPath =
                Path.Combine(
                    outputFolderPath,
                    "MediaCatalog.json"
                );

            File.WriteAllText(
                outputPath,
                json
            );

            Console.WriteLine();
            Console.WriteLine(
                $"JSON 已生成：{outputPath}"
            );
        }

        // ============================================================
        // TableCatalog → JSON
        // ============================================================

        private static void ConvertTableCatalogToJson(
            string catalogBinPath,
            string outputFolderPath)
        {
            Console.WriteLine();
            Console.WriteLine(
                "正在将 TableCatalog.bytes 转换为 JSON..."
            );

            byte[] bytes =
                File.ReadAllBytes(catalogBinPath);

            Console.WriteLine(
                $"读取: {bytes.Length:N0} bytes"
            );

            TableCatalog? catalog =
                MemoryPackSerializer.Deserialize<TableCatalog>(
                    bytes
                );

            if (catalog == null)
            {
                Console.WriteLine(
                    "TableCatalog 反序列化结果为空。"
                );

                return;
            }

            int count =
                catalog.Table?.Count ?? 0;

            Console.WriteLine(
                $"TableBundle 数量: {count:N0}"
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,

                Encoder =
                    JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json =
                JsonSerializer.Serialize(
                    catalog,
                    options
                );

            string outputPath =
                Path.Combine(
                    outputFolderPath,
                    "TableCatalog.json"
                );

            File.WriteAllText(
                outputPath,
                json
            );

            Console.WriteLine();
            Console.WriteLine(
                $"JSON 已生成：{outputPath}"
            );
        }

        // ============================================================
        // Catalog 类型
        // ============================================================

        enum CatalogType
        {
            MediaCatalog,
            TableCatalog
        }
    }
}