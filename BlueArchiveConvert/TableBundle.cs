using System;
using System.Buffers;
using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class TableBundle
        : IMemoryPackable<TableBundle>,
          IMemoryPackFormatterRegister
    {
        private string name;
        private long size;
        private long crc;
        private bool isInbuild;
        private bool isChanged;
        private bool isPrologue;
        private bool isSplitDownload;
        private List<string> includes;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public long Size
        {
            get => size;
            set => size = value;
        }

        public long Crc
        {
            get => crc;
            set => crc = value;
        }

        public bool IsInbuild
        {
            get => isInbuild;
            set => isInbuild = value;
        }

        public bool IsChanged
        {
            get => isChanged;
            set => isChanged = value;
        }

        public bool IsPrologue
        {
            get => isPrologue;
            set => isPrologue = value;
        }

        public bool IsSplitDownload
        {
            get => isSplitDownload;
            set => isSplitDownload = value;
        }

        public List<string> Includes
        {
            get => includes;
            set => includes = value;
        }

        public TableBundle()
        {
        }

        static TableBundle()
        {
            MemoryPackFormatterProvider.Register<TableBundle>();
        }

        static void IMemoryPackable<TableBundle>.Serialize<TBufferWriter>(
            ref MemoryPackWriter<TBufferWriter> writer,
            scoped ref TableBundle? value)
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(8);

            writer.WriteString(value.Name);

            long size = value.Size;
            long crc = value.Crc;

            bool isInbuild = value.IsInbuild;
            bool isChanged = value.IsChanged;
            bool isPrologue = value.IsPrologue;
            bool isSplitDownload = value.IsSplitDownload;

            writer.WriteUnmanaged(
                ref size,
                ref crc,
                ref isInbuild,
                ref isChanged,
                ref isPrologue,
                ref isSplitDownload
            );

            writer.WriteValue(value.Includes);
        }

        static void IMemoryPackable<TableBundle>.Deserialize(
            ref MemoryPackReader reader,
            scoped ref TableBundle? value)
        {
            if (!reader.TryReadObjectHeader(out byte count))
            {
                value = null;
                return;
            }

            if (count > 8)
            {
                MemoryPackSerializationException.ThrowInvalidPropertyCount(
                    typeof(TableBundle),
                    8,
                    count
                );

                return;
            }

            string name = null;

            long size = 0;
            long crc = 0;

            bool isInbuild = false;
            bool isChanged = false;
            bool isPrologue = false;
            bool isSplitDownload = false;

            List<string> includes = null;

            if (value != null)
            {
                name = value.Name;
                size = value.Size;
                crc = value.Crc;
                isInbuild = value.IsInbuild;
                isChanged = value.IsChanged;
                isPrologue = value.IsPrologue;
                isSplitDownload = value.IsSplitDownload;
                includes = value.Includes;
            }

            if (count >= 1)
            {
                name = reader.ReadString();
            }

            if (count >= 2)
            {
                reader.ReadUnmanaged(out size);
            }

            if (count >= 3)
            {
                reader.ReadUnmanaged(out crc);
            }

            if (count >= 4)
            {
                reader.ReadUnmanaged(out isInbuild);
            }

            if (count >= 5)
            {
                reader.ReadUnmanaged(out isChanged);
            }

            if (count >= 6)
            {
                reader.ReadUnmanaged(out isPrologue);
            }

            if (count >= 7)
            {
                reader.ReadUnmanaged(out isSplitDownload);
            }

            if (count >= 8)
            {
                includes = reader.ReadValue<List<string>>();
            }

            value ??= new TableBundle();

            value.Name = name;
            value.Size = size;
            value.Crc = crc;
            value.IsInbuild = isInbuild;
            value.IsChanged = isChanged;
            value.IsPrologue = isPrologue;
            value.IsSplitDownload = isSplitDownload;
            value.Includes = includes;
        }

        static void IMemoryPackFormatterRegister.RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<TableBundle>())
            {
                MemoryPackFormatterProvider.Register(
                    new MemoryPackableFormatter<TableBundle>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<TableBundle[]>())
            {
                MemoryPackFormatterProvider.Register(
                    new ArrayFormatter<TableBundle>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
            {
                MemoryPackFormatterProvider.Register(
                    new ListFormatter<string>()
                );
            }
        }
    }
}