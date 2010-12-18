﻿/***************************************************************************
 *                            QueuedMemoryWriter.cs
 *                            -------------------
 *   begin                : December 16, 2010
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: QueuedMemoryWriter.cs 37 2006-06-19 17:28:24Z asayre $
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Server
{

	public sealed class QueuedMemoryWriter : BinaryFileWriter
	{
		private struct IndexInfo
		{
			public int size;
			public int typeCode;
			public int serial;
		}

		private MemoryStream _memStream;
		private Queue<IndexInfo> _indexQueue = new Queue<IndexInfo>();	//TODO: Pick a more optimal starting size

		protected override int BufferSize
		{
			get { return 4096; }
		}

		public QueuedMemoryWriter()
			: base(new MemoryStream(1024 * 1024), true)
		{
			this._memStream = this.UnderlyingStream as MemoryStream;
		}

		public void QueueForIndex(ISerializable serializable, int size)
		{
			IndexInfo info;

			info.size = size;

			info.typeCode = serializable.TypeReference;	//For guilds, this will automagically be zero.
			info.serial = serializable.SerialIdentity;

			_indexQueue.Enqueue(info);
		}

		public int CommitTo(SequentialFileWriter dataFile, SequentialFileWriter indexFile)
		{
			this.Flush();

			byte[] memBuffer = _memStream.GetBuffer();
			int memLength = (int)_memStream.Position;

			long actualPosition = dataFile.Position;

			dataFile.Write(memBuffer, 0, memLength);	//The buffer contains the data from many items.

			//Console.WriteLine("Writing {0} bytes starting at {1}", memLength, actualPosition);

			byte[] indexBuffer = new byte[20];

			int indexWritten = _indexQueue.Count * indexBuffer.Length;

			while (_indexQueue.Count > 0)
			{
				IndexInfo info = _indexQueue.Dequeue();

				int typeCode = info.typeCode;
				int serial = info.serial;
				int length = info.size;


				indexBuffer[0] = (byte)(info.typeCode);
				indexBuffer[1] = (byte)(info.typeCode >> 8);
				indexBuffer[2] = (byte)(info.typeCode >> 16);
				indexBuffer[3] = (byte)(info.typeCode >> 24);

				indexBuffer[4] = (byte)(info.serial);
				indexBuffer[5] = (byte)(info.serial >> 8);
				indexBuffer[6] = (byte)(info.serial >> 16);
				indexBuffer[7] = (byte)(info.serial >> 24);

				indexBuffer[8] = (byte)(actualPosition);
				indexBuffer[9] = (byte)(actualPosition >> 8);
				indexBuffer[10] = (byte)(actualPosition >> 16);
				indexBuffer[11] = (byte)(actualPosition >> 24);
				indexBuffer[12] = (byte)(actualPosition >> 32);
				indexBuffer[13] = (byte)(actualPosition >> 40);
				indexBuffer[14] = (byte)(actualPosition >> 48);
				indexBuffer[15] = (byte)(actualPosition >> 56);

				indexBuffer[16] = (byte)(info.size);
				indexBuffer[17] = (byte)(info.size >> 8);
				indexBuffer[18] = (byte)(info.size >> 16);
				indexBuffer[19] = (byte)(info.size >> 24);

				indexFile.Write(indexBuffer, 0, indexBuffer.Length);

				actualPosition += info.size;
			}

			this.Close();	//We're donezo with this writer.

			return memLength + indexWritten;
		}
	}
}