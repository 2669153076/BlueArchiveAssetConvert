using MemoryPack;
using MemoryPack.Formatters;
using System;
using System.Buffers;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
    {
        [MemoryPackable]
        public partial class Media : IMemoryPackable<Media>, IMemoryPackFormatterRegister
        {
            // 私有字段
            private string _path;
            private string _fileName;
            private long _bytes;
            private long _crc;
            private bool _isPrologue;
            private bool _isSplitDownload;
            private MediaType _mediaType;

            // 属性
            public string Path
            {
                get => _path;
                set => _path = value;
            }

            public string FileName
            {
                get => _fileName;
                set => _fileName = value;
            }

            public long Bytes
            {
                get => _bytes;
                set => _bytes = value;
            }

            public long Crc
            {
                get => _crc;
                set => _crc = value;
            }

            public bool IsPrologue
            {
                get => _isPrologue;
                set => _isPrologue = value;
            }

            public bool IsSplitDownload
            {
                get => _isSplitDownload;
                set => _isSplitDownload = value;
            }

            public MediaType MediaType
            {
                get => _mediaType;
                set => _mediaType = value;
            }

            // 构造函数
            public Media()
            {
            }

            // 静态构造函数
            static Media()
            {
                MemoryPackFormatterProvider.Register<Media>();
            }

            // 实现 IMemoryPackable<Media> 的 Serialize 方法
            static void IMemoryPackable<Media>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Media? value) 
            {
                if (value == null)
                {
                    writer.WriteNullObjectHeader();
                    return;
                }

                writer.WriteObjectHeader(7);
                writer.WriteString(value.Path);
                writer.WriteString(value.FileName);
                long bytes = value.Bytes;
                long crc = value.Crc;
                bool isPrologue = value.IsPrologue;
                bool isSplitDownload = value.IsSplitDownload;
                MediaType mediaType = value.MediaType;
                writer.WriteUnmanaged(ref bytes, ref crc, ref isPrologue, ref isSplitDownload, ref mediaType);
            }

            // 实现 IMemoryPackable<Media> 的 Deserialize 方法（静态）
            static void IMemoryPackable<Media>.Deserialize(ref MemoryPackReader reader, scoped ref Media? value)
            {
                byte header;
                if (!reader.TryReadObjectHeader(out header))
                {
                    value = null;
                    return;
                }

                if (header == 7)
                {
                    if (value == null)
                    {
                        string path = reader.ReadString();
                        string fileName = reader.ReadString();
                        long bytes;
                        long crc;
                        bool isPrologue;
                        bool isSplitDownload;
                        MediaType mediaType;
                        reader.ReadUnmanaged(out bytes, out crc, out isPrologue, out isSplitDownload, out mediaType);
                        value = new Media
                        {
                            Path = path,
                            FileName = fileName,
                            Bytes = bytes,
                            Crc = crc,
                            IsPrologue = isPrologue,
                            IsSplitDownload = isSplitDownload,
                            MediaType = mediaType
                        };
                    }
                    else
                    {
                        string path = reader.ReadString();
                        string fileName = reader.ReadString();
                        long bytes = value.Bytes;
                        long crc = value.Crc;
                        bool isPrologue = value.IsPrologue;
                        bool isSplitDownload = value.IsSplitDownload;
                        MediaType mediaType = value.MediaType;
                        value.Path = reader.ReadString();
                        value.FileName = reader.ReadString();
                        reader.ReadUnmanaged(out bytes, out crc, out isPrologue, out isSplitDownload, out mediaType);
                        value.Bytes = bytes;
                        value.Crc = crc;
                        value.IsPrologue = isPrologue;
                        value.IsSplitDownload = isSplitDownload;
                        value.MediaType = mediaType;
                    }
                }
                else if (header > 7)
                {
                    Type type = typeof(Media);
                    MemoryPackSerializationException.ThrowInvalidPropertyCount(type, (byte)7, header);
                }
                else
                {
                    long bytes = 0;
                    long crc = 0;
                    bool isPrologue = false;
                    bool isSplitDownload = false;
                    MediaType mediaType = default;

                    if (value != null)
                    {
                        bytes = value.Bytes;
                        crc = value.Crc;
                        isPrologue = value.IsPrologue;
                        isSplitDownload = value.IsSplitDownload;
                        mediaType = value.MediaType;
                    }

                    switch (header)
                    {
                        case 0:
                            break;
                        case 1:
                            if (value != null)
                            {
                                value.Path = reader.ReadString();
                            }
                            break;
                        case 2:
                            if (value != null)
                            {
                                value.FileName = reader.ReadString();
                            }
                            break;
                        case 3:
                            reader.ReadUnmanaged(out bytes);
                            break;
                        case 4:
                            reader.ReadUnmanaged(out crc);
                            break;
                        case 5:
                            reader.ReadUnmanaged(out isPrologue);
                            break;
                        case 6:
                            reader.ReadUnmanaged(out isSplitDownload);
                            break;
                        case 7:
                            reader.ReadUnmanaged(out mediaType);
                            break;
                    }

                    if (value != null)
                    {
                        value.Bytes = bytes;
                        value.Crc = crc;
                        value.IsPrologue = isPrologue;
                        value.IsSplitDownload = isSplitDownload;
                        value.MediaType = mediaType;
                    }
                    else
                    {
                        value = new Media
                        {
                            Path = null,
                            FileName = null,
                            Bytes = bytes,
                            Crc = crc,
                            IsPrologue = isPrologue,
                            IsSplitDownload = isSplitDownload,
                            MediaType = mediaType
                        };
                    }
                }
            }

            // 实现 IMemoryPackFormatterRegister 的 RegisterFormatter 方法
            static void IMemoryPackFormatterRegister.RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<Media>())
            {
                MemoryPackFormatterProvider.Register(new MemoryPackableFormatter<Media>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Media[]>())
            {
                MemoryPackFormatterProvider.Register(new ArrayFormatter<Media>());
            }

            if (!MemoryPackFormatterProvider.IsRegistered<MediaType>())
            {
                MemoryPackFormatterProvider.Register(new UnmanagedFormatter<MediaType>());
            }
        }
    }

}