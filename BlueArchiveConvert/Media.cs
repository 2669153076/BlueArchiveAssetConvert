using MemoryPack;
using MemoryPack.Formatters;
using System;
using System.Buffers;

namespace BlueArchiveAssetConvert.BlueArchiveConvert
{
    [MemoryPackable]
    public partial class Media : IMemoryPackable<Media>, IMemoryPackFormatterRegister
    {
        private string _path;
        private string _fileName;
        private long _bytes;
        private long _crc;
        private bool _isPrologue;
        private bool _isSplitDownload;
        private MediaType _mediaType;

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

        public Media()
        {
        }

        static Media()
        {
            MemoryPackFormatterProvider.Register<Media>();
        }

        static void IMemoryPackable<Media>.Serialize<TBufferWriter>(
            ref MemoryPackWriter<TBufferWriter> writer,
            scoped ref Media? value)
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

            writer.WriteUnmanaged(
                ref bytes,
                ref crc,
                ref isPrologue,
                ref isSplitDownload,
                ref mediaType
            );
        }

        static void IMemoryPackable<Media>.Deserialize(
            ref MemoryPackReader reader,
            scoped ref Media? value)
        {
            if (!reader.TryReadObjectHeader(out byte header))
            {
                value = null;
                return;
            }

            if (header > 7)
            {
                MemoryPackSerializationException.ThrowInvalidPropertyCount(
                    typeof(Media),
                    7,
                    header
                );

                return;
            }

            string path = null;
            string fileName = null;

            long bytes = 0;
            long crc = 0;

            bool isPrologue = false;
            bool isSplitDownload = false;

            MediaType mediaType = default;

            // 如果是更新已有对象，则保留没有被当前数据覆盖的字段
            if (value != null)
            {
                path = value.Path;
                fileName = value.FileName;
                bytes = value.Bytes;
                crc = value.Crc;
                isPrologue = value.IsPrologue;
                isSplitDownload = value.IsSplitDownload;
                mediaType = value.MediaType;
            }

            if (header >= 1)
            {
                path = reader.ReadString();
            }

            if (header >= 2)
            {
                fileName = reader.ReadString();
            }

            if (header >= 3)
            {
                reader.ReadUnmanaged(out bytes);
            }

            if (header >= 4)
            {
                reader.ReadUnmanaged(out crc);
            }

            if (header >= 5)
            {
                reader.ReadUnmanaged(out isPrologue);
            }

            if (header >= 6)
            {
                reader.ReadUnmanaged(out isSplitDownload);
            }

            if (header >= 7)
            {
                reader.ReadUnmanaged(out mediaType);
            }

            value ??= new Media();

            value.Path = path;
            value.FileName = fileName;
            value.Bytes = bytes;
            value.Crc = crc;
            value.IsPrologue = isPrologue;
            value.IsSplitDownload = isSplitDownload;
            value.MediaType = mediaType;
        }

        static void IMemoryPackFormatterRegister.RegisterFormatter()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<Media>())
            {
                MemoryPackFormatterProvider.Register(
                    new MemoryPackableFormatter<Media>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<Media[]>())
            {
                MemoryPackFormatterProvider.Register(
                    new ArrayFormatter<Media>()
                );
            }

            if (!MemoryPackFormatterProvider.IsRegistered<MediaType>())
            {
                MemoryPackFormatterProvider.Register(
                    new UnmanagedFormatter<MediaType>()
                );
            }
        }
    }
}