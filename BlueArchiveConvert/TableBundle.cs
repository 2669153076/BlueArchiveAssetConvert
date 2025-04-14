using System;
using System.Collections.Generic;
using MemoryPack;
using System.Buffers;
using MemoryPack.Internal;
using MemoryPack.Formatters;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class TableBundle : IMemoryPackable<TableBundle>, IMemoryPackFormatterRegister
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
            get { return name;}
            set { name = value; }
        }

        public long Size
        {
            get { return size; } set { size = value; }
        }

        public long Crc
        {
            get { return crc;  } set { crc = value; }
        }

        public bool IsInbuild
        {
            get { return isInbuild; }
            set { isInbuild = value; }
        }

        public bool IsChanged
        {
            get { return isChanged; } set { isChanged = value; }
        }

        public bool IsPrologue
        {
           get { return isPrologue; }
            set { isPrologue = value; }
        }

        public bool IsSplitDownload
        {
            get { return isSplitDownload; }
            set { isSplitDownload = value; }
        }

        public List<string> Includes
        {
            get { return includes; }
            set { includes = value; }
        }

        static TableBundle()
        {
            MemoryPackFormatterProvider.Register<TableBundle>();
        }

        [Preserve]
        static void IMemoryPackFormatterRegister.RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<TableBundle>())
            {
                MemoryPackFormatterProvider.Register(new MemoryPackableFormatter<TableBundle>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<TableBundle[]>())
            {
                MemoryPackFormatterProvider.Register(new ArrayFormatter<TableBundle>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
            {
                MemoryPackFormatterProvider.Register(new ListFormatter<string>());
            }
        }

        [Preserve]
        static void IMemoryPackable<TableBundle>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref TableBundle? value) 
        {
            if (value == null)
            {
                writer.WriteNullObjectHeader();
                return;
            }

            writer.WriteObjectHeader(8);
            writer.WriteString(value.Name);
            writer.WriteUnmanaged(value.Size, value.Crc, value.isInbuild, value.isChanged, value.IsPrologue, value.IsSplitDownload);
            writer.WriteValue(value.Includes);
        }

        [Preserve]
        static void IMemoryPackable<TableBundle>.Deserialize(ref MemoryPackReader reader, scoped ref TableBundle? value)
        {
            if (!reader.TryReadObjectHeader(out var count))
            {
                value = null;
                return;
            }

            if (count == 8)
            {
                if (value == null)
                {
                    string name = reader.ReadString();
                    long size = 0;
                    long crc = 0;
                    bool isInbuild = false;
                    bool isChanged = false;
                    bool isPrologue = false;
                    bool isSplitDownload = false;
                    reader.ReadUnmanaged(out size, out crc, out isInbuild, out isChanged, out isPrologue, out isSplitDownload);
                    List<string> includes = reader.ReadValue<List<string>>();

                    value = new TableBundle
                    {
                        Name = name,
                        Size = size,
                        Crc = crc,
                        isInbuild = isInbuild,
                        isChanged = isChanged,
                        IsPrologue = isPrologue,
                        IsSplitDownload = isSplitDownload,
                        Includes = includes
                    };
                }
                else
                {
                    value.Name = reader.ReadString();
                    reader.ReadUnmanaged(out value.size);
                    reader.ReadUnmanaged(out value.crc);
                    reader.ReadUnmanaged(out value.isInbuild);
                    reader.ReadUnmanaged(out value.isChanged);
                    reader.ReadUnmanaged(out value.isPrologue);
                    reader.ReadUnmanaged(out value.isSplitDownload);
                    reader.ReadValue(ref value.includes);
                }
            }
            else
            {
                if (count > 8)
                {
                    MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TableBundle), 8, count);
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
                    isInbuild = value.isInbuild;
                    isChanged = value.isChanged;
                    isPrologue = value.IsPrologue;
                    isSplitDownload = value.IsSplitDownload;
                    includes = value.Includes;
                }

                if (count >= 1)
                {
                    name = reader.ReadString();
                    if (count >= 2)
                    {
                        reader.ReadUnmanaged(out size);
                        if (count >= 3)
                        {
                            reader.ReadUnmanaged(out crc);
                            if (count >= 4)
                            {
                                reader.ReadUnmanaged(out isInbuild);
                                if (count >= 5)
                                {
                                    reader.ReadUnmanaged(out isChanged);
                                    if (count >= 6)
                                    {
                                        reader.ReadUnmanaged(out isPrologue);
                                        if (count >= 7)
                                        {
                                            reader.ReadUnmanaged(out isSplitDownload);
                                            if (count >= 8)
                                            {
                                                reader.ReadValue(ref includes);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (value == null)
                {
                    value = new TableBundle
                    {
                        Name = name,
                        Size = size,
                        Crc = crc,
                        isInbuild = isInbuild,
                        isChanged = isChanged,
                        IsPrologue = isPrologue,
                        IsSplitDownload = isSplitDownload,
                        Includes = includes
                    };
                }
                else
                {
                    value.Name = name;
                    value.Size = size;
                    value.Crc = crc;
                    value.isInbuild = isInbuild;
                    value.isChanged = isChanged;
                    value.IsPrologue = isPrologue;
                    value.IsSplitDownload = isSplitDownload;
                    value.Includes = includes;
                }
            }
        }

        public TableBundle()
        {
        }
    }
}