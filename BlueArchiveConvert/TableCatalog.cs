using System;
using System.Buffers;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class TableCatalog
        : IMemoryPackable<TableCatalog>,
          IMemoryPackFormatterRegister
    {
        private Dictionary<string, TableBundle> _table;

        public Dictionary<string, TableBundle> Table
        {
            get => _table;
            set => _table = value;
        }

        // 如果你知道新增的第 2 个字段类型（如 string 或 long），可以声明出来。
        // 如果只是为了顺利解包，可以用 object? 或直接在 Deserialize 中 Skip 掉。
        public object? ExtraField { get; set; }

        public TableCatalog()
        {
        }

        public static void Serialize<TBufferWriter>(
            ref MemoryPackWriter<TBufferWriter> writer,
            scoped ref TableCatalog? value)
            where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            // 修改为写入 2 个属性（如果需要反向打包）
            writer.WriteObjectHeader(2);
            writer.WriteValue(value.Table);
            writer.WriteValue(value.ExtraField);
        }

        public static void Deserialize(
            ref MemoryPackReader reader,
            scoped ref TableCatalog? value)
        {
            if (!reader.TryReadObjectHeader(out byte header))
            {
                value = null;
                return;
            }

            // 修改判断：支持最多 2 个属性（如果以后官方又加了，可以放宽条件）
            if (header > 2)
            {
                MemoryPackSerializationException.ThrowInvalidPropertyCount(
                    typeof(TableCatalog),
                    2,
                    header
                );
                return;
            }

            value ??= new TableCatalog();

            // 读取第 1 个属性：Table 字典
            if (header >= 1)
            {
                value.Table = reader.ReadValue<Dictionary<string, TableBundle>>();
            }

            // 读取第 2 个属性（防止 Deserialize 失败）
            if (header >= 2)
            {
                // 如果不关心第 2 个字段的内容，直接跳过该值的读取
                // 或者尝试按对应类型读取，例如 string 或 object
                try
                {
                    value.ExtraField = reader.ReadValue<object>();
                }
                catch
                {
                    // 如果 unknown 类型读取失败，忽略多余数据
                }
            }
        }

        public static void RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<TableCatalog>())
            {
                MemoryPackFormatterProvider.Register(
                    new MemoryPackableFormatter<TableCatalog>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<TableCatalog[]>())
            {
                MemoryPackFormatterProvider.Register(
                    new ArrayFormatter<TableCatalog>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, TableBundle>>())
            {
                MemoryPackFormatterProvider.Register(
                    new DictionaryFormatter<string, TableBundle>()
                );
            }
        }
    }
}