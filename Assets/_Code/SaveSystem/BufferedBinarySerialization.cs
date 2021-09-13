using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KaizerWaldCode
{
	/*
    public class BufferedBinarySerialization : IDisposable
    {
		private readonly Stream stream;
		private readonly byte[] buffer;
		private readonly int bufferSize;
		private int bufferOffset;
		private int numBufferedBytes;

		public BufferedBinarySerialization(Stream stream, int bufferSize)
		{
			this.stream = stream;
			this.bufferSize = bufferSize;
			buffer = new byte[bufferSize];
			bufferOffset = bufferSize;
		}

		public int NumBytesAvailable { get { return Math.Max(0, numBufferedBytes - bufferOffset); } }

		public bool FillBuffer()
		{
			var numBytesUnread = bufferSize - bufferOffset;
			var numBytesToRead = bufferSize - numBytesUnread;
			bufferOffset = 0;
			numBufferedBytes = numBytesUnread;
			if (numBytesUnread > 0)
			{
				Buffer.BlockCopy(buffer, numBytesToRead, buffer, 0, numBytesUnread);
			}
			while (numBytesToRead > 0)
			{
				var numBytesRead = stream.Read(buffer, numBytesUnread, numBytesToRead);
				if (numBytesRead == 0)
				{
					return false;
				}
				numBufferedBytes += numBytesRead;
				numBytesToRead -= numBytesRead;
				numBytesUnread += numBytesRead;
			}
			return true;
		}

		public ushort ReadUInt16()
		{
			var val = (ushort)((int)buffer[bufferOffset] | (int)buffer[bufferOffset + 1] << 8);
			bufferOffset += 2;
			return val;
		}

		public void Dispose()
		{
			stream.Close();
		}
	}

	//Use exemple

	public class TestScript : MonoBehaviour
	{
		private const int FileSize = 10 * 1024 * 1024;

		void Start()
		{
			var path = Path.Combine(Application.persistentDataPath, "bigfile.dat");
			try
			{
				File.WriteAllBytes(path, new byte[FileSize]);
				TestBinaryReaders(path);
			}
			finally
			{
				File.Delete(path);
			}
		}
		private void TestBinaryReaders(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open))
			{
				var stopwatch = System.Diagnostics.Stopwatch.StartNew();
				var log = "Reader,Time\n";
				var numValues = FileSize / sizeof(ushort);
				var readValues = new ushort[numValues];
				var reader = new BinaryReader(stream);
				stopwatch.Reset();
				stopwatch.Start();
				for (var i = 0; i < numValues; ++i)
				{
					readValues[i] = reader.ReadUInt16();
				}
				var time = stopwatch.ElapsedMilliseconds;
				log += "BinaryReader," + time + "\n";

				stream.Position = 0;

				var bufferedReader = new BufferedBinarySerialization(stream, 4096);
				stopwatch.Reset();
				stopwatch.Start();
				while (bufferedReader.FillBuffer())
				{
					var readValsIndex = 0;
					for (
						var numReads = bufferedReader.NumBytesAvailable / sizeof(ushort);
						numReads > 0;
						--numReads
					)
					{
						readValues[readValsIndex++] = bufferedReader.ReadUInt16();
					}
				}
				time = stopwatch.ElapsedMilliseconds;
				log += "BufferedBinaryReader," + time + "\n";
				Debug.Log(log);
			}
		}
	}
	*/
}
