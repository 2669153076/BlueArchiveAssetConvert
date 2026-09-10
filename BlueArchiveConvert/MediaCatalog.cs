using System;
using System.Buffers;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class MediaCatalog
        : IMemoryPackable<MediaCatalog>,
          IMemoryPackFormatterRegister
    {
        private Dictionary<string, Media> _table;

        public Dictionary<string, Media> Table
        {
            get => _table;
            set => _table = value;
        }

        public MediaCatalog()
        {
        }

        public static void Serialize<TBufferWriter>(
            ref MemoryPackWriter<TBufferWriter> writer,
            scoped ref MediaCatalog? value)
            where TBufferWriter : IBufferWriter<byte>
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(1);
            writer.WriteValue(value.Table);
        }

        public static void Deserialize(
            ref MemoryPackReader reader,
            scoped ref MediaCatalog? value)
        {
            if (!reader.TryReadObjectHeader(out byte header))
            {
                value = null;
                return;
            }

            if (header > 1)
            {
                MemoryPackSerializationException.ThrowInvalidPropertyCount(
                    typeof(MediaCatalog),
                    1,
                    header
                );

                return;
            }

            value ??= new MediaCatalog();

            if (header >= 1)
            {
                value.Table =
                    reader.ReadValue<Dictionary<string, Media>>();
            }
        }

        public static void RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<MediaCatalog>())
            {
                MemoryPackFormatterProvider.Register(
                    new MemoryPackableFormatter<MediaCatalog>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<MediaCatalog[]>())
            {
                MemoryPackFormatterProvider.Register(
                    new ArrayFormatter<MediaCatalog>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, Media>>())
            {
                MemoryPackFormatterProvider.Register(
                    new DictionaryFormatter<string, Media>()
                );
            }
        }
    }
}