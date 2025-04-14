using System;
using System.Buffers;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    public class MediaCatalog : IMemoryPackable<MediaCatalog>, IMemoryPackFormatterRegister
    {
        // Field
        private Dictionary<string, Media> _table;

        // Properties
        public Dictionary<string, Media> Table
        {
            get { return _table; }
            set { _table = value; }
        }

        // Static constructor
        static MediaCatalog()
        {
            MemoryPackFormatterProvider.Register<MediaCatalog>();
        }


        public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref MediaCatalog? value) where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(1);
            writer.WriteValue(value.Table);
        }

        public static void Deserialize(ref MemoryPackReader reader, scoped ref MediaCatalog? value)
        {
            byte objectHeader;
            if (!reader.TryReadObjectHeader(out objectHeader))
            {
                value = null;
                return;
            }

            if (objectHeader == 1)
            {
                value ??= new MediaCatalog();
                value.Table = reader.ReadValue<Dictionary<string, Media>>();
            }
        }

        public static void RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<MediaCatalog>())
            {
                MemoryPackFormatterProvider.Register(new MemoryPackableFormatter<MediaCatalog>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<MediaCatalog[]>())
            {
                MemoryPackFormatterProvider.Register(new ArrayFormatter<MediaCatalog>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, Media>>())
            {
                MemoryPackFormatterProvider.Register(new DictionaryFormatter<string, Media>());
            }
        }
    }
}
