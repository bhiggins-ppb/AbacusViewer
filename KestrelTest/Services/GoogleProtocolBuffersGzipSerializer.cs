using System;
//using BTS.Infrastructure.Core.Compression;
//using BTS.Infrastructure.DotNetExtensions.Diagnostics;

namespace AbacusViewer.Services
{
    public sealed class GoogleProtocolBuffersGzipSerializer
    {
        //private readonly ICompressor _compressor;
        //private readonly GoogleProtocolBuffersSerializer _innerGoogleProtocolBuffersSerializer;
        public string ContentType => "application/protobuf";
        public string ContentEncoding => "gzip";

        public GoogleProtocolBuffersGzipSerializer()//ICompressor inputCompressor
            //, GoogleProtocolBuffersSerializer inputInnerSerializer)
        {
            //_compressor = ArgCheck.IsNotNull(inputCompressor, nameof(inputCompressor));
            //_innerGoogleProtocolBuffersSerializer = ArgCheck.IsNotNull(inputInnerSerializer, nameof(inputInnerSerializer));
        }

        public T GetObject<T>(byte[] bytes) where T : Google.Protobuf.IMessage<T>, new()
        {
            /*if (bytes == null || bytes.Length == 0)
                return default(T);

            var obj = null;
            default(T);

            try
            {
                obj = _innerGoogleProtocolBuffersSerializer.GetObject<T>(_compressor.Decompress(bytes));
            }
            catch(Exception e)
            {
                //MvcApplication.Logger.Error("Couldn't deserialise message", e);
            }

            return obj;*/
            return default(T);
        }

        public byte[] GetBytes<TObject>(TObject message)
        {
            //return _compressor.Compress(_innerGoogleProtocolBuffersSerializer.GetBytes(message));
            return null;
        }
    }
}
