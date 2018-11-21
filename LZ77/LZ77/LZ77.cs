using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace LZ77
{
    class LZ77
    {
        private byte _referencePrefix;
        private int _referenceIntBase;
        private int _referenceIntFloorCode;
        private int _referenceIntCeilCode;
        private int _maxStringDistance;
        private int _minStringLength;
        private int _maxStringLength;
        private int _defaultWindowLength;
        private int _maxWindowLength;

        public LZ77()
        {
            _referencePrefix = (byte)'`';
            _referenceIntBase = 96;
            _referenceIntFloorCode = 32;
            _referenceIntCeilCode = 128;
            _maxStringDistance = (int)Math.Pow(_referenceIntBase, 2) - 1;
            _minStringLength = 5;
            _maxStringLength = (int)Math.Pow(_referenceIntBase, 1) - 1 + _minStringLength;
            _defaultWindowLength = (int)CompressionLevel.Fastest;
            _maxWindowLength = _maxStringDistance + _minStringLength;
        }
        public byte[] Compress(byte[] data, int windowLength)
        {
            if (windowLength == -1)
            {
                windowLength = _defaultWindowLength;
            }

            if (windowLength > _maxWindowLength)
            {
                throw new ArgumentException("Window length is too large.");
            }

            List<byte> compressed = new List<byte>();

            int pos = 0;
            int lastPos = data.Length - _minStringLength;

            while (pos < lastPos)
            {
                //Stopwatch w = Stopwatch.StartNew();

                int searchStart = Math.Max(pos - windowLength, 0);
                int matchLength = _minStringLength;
                bool foundMatch = false;
                int bestMatchDistance = _maxStringDistance;
                int bestMatchLength = 0;
                List<byte> newCompressed = new List<byte>();

                while ((searchStart + matchLength) < pos)
                {
                    int sourceWindowEnd = Math.Min(searchStart + matchLength, data.Length);
                    int targetWindowEnd = Math.Min(pos + matchLength, data.Length);

                    int m1Length = sourceWindowEnd - searchStart;
                    int m2Length = targetWindowEnd - pos;

                    byte[] m1 = new byte[m1Length];
                    byte[] m2 = new byte[m2Length];

                    Array.Copy(data, searchStart, m1, 0, m1Length);
                    Array.Copy(data, pos, m2, 0, m2Length);

                    bool isValidMatch = m1.SequenceEqual(m2) && matchLength < _maxStringLength;

                    if (isValidMatch)
                    {
                        matchLength++;
                        foundMatch = true;
                    }
                    else
                    {
                        int realMatchLength = matchLength - 1;

                        if (foundMatch && (realMatchLength > bestMatchLength))
                        {
                            bestMatchDistance = pos - searchStart - realMatchLength;
                            bestMatchLength = realMatchLength;
                        }

                        matchLength = _minStringLength;
                        searchStart++;
                        foundMatch = false;
                    }
                }

                if (bestMatchLength != 0)
                {
                    newCompressed.Add(_referencePrefix);
                    newCompressed.AddRange(EncodeReferenceInt(bestMatchDistance, 2));
                    newCompressed.AddRange(EncodeReferenceLength(bestMatchLength));

                    pos += bestMatchLength;
                }
                else
                {
                    if (data[pos] != _referencePrefix)
                    {
                        newCompressed = new List<byte>(new byte[] { data[pos] });
                    }
                    else
                    {
                        newCompressed = new List<byte>(new byte[] { _referencePrefix, _referencePrefix });
                    }

                    pos++;
                }

                compressed.AddRange(newCompressed);

                //Console.WriteLine(w.ElapsedMilliseconds);
            }

            var lasts = data.Skip(pos).Take(data.Length - pos);
            lasts
                .Where(x => x == _referencePrefix)
                .ToList()
                .ForEach
                (
                    x =>
                        compressed
                            .AddRange
                            (
                                new byte[]
                                {
                                _referencePrefix,
                                _referencePrefix
                                }
                            )
                );

            return compressed.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            List<byte> decompressed = new List<byte>();
            int pos = 0;

            while (pos < data.Length)
            {
                byte currentByte = data[pos];

                if (currentByte != _referencePrefix)
                {
                    decompressed.Add(currentByte);
                    pos++;
                }
                else
                {
                    byte nextChar = data[pos + 1];

                    if (nextChar != _referencePrefix)
                    {
                        int distance = DecodeReferenceInt(data.Skip(pos + 1).Take(2).ToList(), 2);
                        int length = DecodeReferenceLength(data.Skip(pos + 3).Take(1).ToList());
                        int start = decompressed.Count - distance - length;
                        int end = start + length;

                        var slice = decompressed.Skip(start).Take(end - start).ToList();
                        decompressed.AddRange(slice);
                        pos += _minStringLength - 1;
                    }
                    else
                    {
                        decompressed.Add((byte)_referencePrefix);
                        pos += 2;
                    }
                }
            }

            return decompressed.ToArray();
        }
        private IList<byte> EncodeReferenceInt(int value, int width)
        {
            if ((value >= 0) && (value < (Math.Pow(_referenceIntBase, width) - 1)))
            {
                IList<byte> encoded = new List<byte>();

                while (value > 0)
                {
                    byte b = (byte)((value % _referenceIntBase) + _referenceIntFloorCode);
                    encoded.Insert(0, b);
                    value = (int)Math.Floor((double)value / _referenceIntBase);
                }

                int missingLength = width - encoded.Count;

                for (int i = 0; i < missingLength; i++)
                {
                    byte b = (byte)_referenceIntFloorCode;
                    encoded.Insert(0, b);
                }

                return encoded;
            }
            else
            {
                throw new ArgumentException(string.Format("Reference int out of range: {0} (width = {1})", value, width));
            }
        }

        private IList<byte> EncodeReferenceLength(int length)
        {
            return EncodeReferenceInt(length - _minStringLength, 1);
        }

        private int DecodeReferenceInt(IList<byte> data, int width)
        {
            int value = 0;

            for (int i = 0; i < width; i++)
            {
                value *= _referenceIntBase;

                int charCode = (int)data[i];

                if ((charCode >= _referenceIntFloorCode) && (charCode <= _referenceIntCeilCode))
                {
                    value += charCode - _referenceIntFloorCode;
                }
                else
                {
                    throw new ArgumentException("Invalid char code in reference int: " + charCode);
                }
            }

            return value;
        }

        private int DecodeReferenceLength(IList<byte> data)
        {
            return DecodeReferenceInt(data, 1) + _minStringLength;
        }
    }
}
