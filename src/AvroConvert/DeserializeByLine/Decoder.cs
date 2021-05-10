﻿using System.IO;
using System.Linq;
using SolTechnology.Avro.Exceptions;
using SolTechnology.Avro.FileHeader;
using SolTechnology.Avro.FileHeader.Codec;
using SolTechnology.Avro.Read;
using SolTechnology.Avro.Schema;
using SolTechnology.Avro.Schema.Abstract;

namespace SolTechnology.Avro.DeserializeByLine
{
    internal class Decoder
    {
        internal static ILineReader<T> OpenReader<T>(Stream stream, TypeSchema readSchema)
        {
            var reader = new Reader(stream);

            // validate header 
            byte[] firstBytes = new byte[DataFileConstants.AvroHeader.Length];

            try
            {
                reader.ReadFixed(firstBytes);
            }
            catch (EndOfStreamException)
            {
                //stream shorter than AvroHeader
            }

            //does not contain header
            if (!firstBytes.SequenceEqual(DataFileConstants.AvroHeader))
            {
                if (readSchema == null)
                {
                    throw new MissingSchemaException("Provide valid schema for the Avro data");
                }
                var resolver = new Resolver(readSchema, readSchema);
                stream.Seek(0, SeekOrigin.Begin);
                return new ListLineReader<T>(reader, resolver);
            }
            else
            {
                var header = reader.ReadHeader();

                readSchema = readSchema ?? BuildSchema.Schema.Create(header.GetMetadata(DataFileConstants.SchemaMetadataKey));
                TypeSchema writeSchema = BuildSchema.Schema.Create(header.GetMetadata(DataFileConstants.SchemaMetadataKey));

                var resolver = new Resolver(writeSchema, readSchema);

                // read in sync data 
                reader.ReadFixed(header.SyncData);
                var codec = AbstractCodec.CreateCodecFromString(header.GetMetadata(DataFileConstants.CodecMetadataKey));

                var remainingBlocks = reader.ReadLong();

                var dataBlock = reader.ReadDataBlock();

                reader.ReadAndValidateSync(header.SyncData);

                dataBlock = codec.Decompress(dataBlock);
                reader = new Reader(new MemoryStream(dataBlock));


                if (remainingBlocks > 1)
                {
                    return new BlockLineReader<T>(reader, resolver, remainingBlocks);
                }

                if (writeSchema.Type == AvroType.Array)
                {
                    return new ListLineReader<T>(reader, new Resolver(((ArraySchema)writeSchema).ItemSchema, readSchema));
                }

                return new ListLineReader<T>(reader, new Resolver(writeSchema, readSchema));
            }
        }
    }
}