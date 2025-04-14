using System;
using System.Buffers;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class TableCatalog : IMemoryPackable<TableCatalog>, IMemoryPackFormatterRegister
    {
        // 字段
        private Dictionary<string, TableBundle> _table;

        // 属性
        public Dictionary<string, TableBundle> Table
        {
            get => _table;
            set => _table = value;
        }

        // 构造函数
        public TableCatalog()
        {
        }

        public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TableCatalog? value) where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(1);
            //var table = value.Table;
            writer.WriteValue(value.Table);
        }

        public static void Deserialize(ref MemoryPackReader reader, scoped ref TableCatalog? value)
        {
            byte header;
            if (!reader.TryReadObjectHeader(out header))
            {
                value = null;
                return;
            }

            if (header == 1)
            {
                if (value == null)
                {
                    value = new TableCatalog();
                }

                //var table = value.Table;
                //reader.ReadValue(ref table);
                //value.Table = table;

                value.Table = reader.ReadValue<Dictionary<string, TableBundle>>();
            }
            //else if (header > 1)
            //{
            //    var type = typeof(TableCatalog);
            //    throw new MemoryPackSerializationException($"Invalid property count for type {type.Name}. Expected {1}, got {header}");
            //}
            //else
            //{
            //    if (value == null)
            //    {
            //        value = new TableCatalog();
            //    }

            //    var table = value.Table;
            //    if (header > 0)
            //    {
            //        reader.ReadValue(ref table);
            //        value.Table = table;
            //    }
            //}
        }

        public static void RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<TableCatalog>())
            {
                MemoryPackFormatterProvider.Register(new MemoryPackableFormatter<TableCatalog>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<TableCatalog[]>())
            {
                MemoryPackFormatterProvider.Register(new ArrayFormatter<TableCatalog>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, TableBundle>>())
            {
                MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, TableBundle>());
            }
        }
    }
}