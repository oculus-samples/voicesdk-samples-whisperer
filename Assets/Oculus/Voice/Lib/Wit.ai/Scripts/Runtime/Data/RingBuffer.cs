/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using UnityEngine;

namespace Facebook.WitAi.Data
{
    public class RingBuffer<T>
    {
        public delegate void ByteDataWriter(T[] buffer, int offset, int length);

        public delegate void OnDataAdded(T[] data, int offset, int length);

        private readonly T[] buffer;
        private long bufferDataLength;
        private int bufferIndex;


        public OnDataAdded OnDataAddedEvent;

        public RingBuffer(int capacity)
        {
            buffer = new T[capacity];
        }

        public int Capacity => buffer.Length;

        public T this[long bufferDataIndex] => buffer[GetBufferArrayIndex(bufferDataIndex)];

        public int GetBufferArrayIndex(long bufferDataIndex)
        {
            if (bufferDataLength <= bufferDataIndex) return -1;
            if (bufferDataLength - bufferDataIndex > buffer.Length) return -1;

            var endOffset = bufferDataLength - bufferDataIndex;
            var index = bufferIndex - endOffset;
            if (index < 0) index = buffer.Length + index;
            return (int)index;
        }

        public void Clear(bool eraseData = false)
        {
            bufferIndex = 0;
            bufferDataLength = 0;

            if (eraseData)
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = default;
        }

        private int CopyToBuffer(T[] data, int offset, int length, int bufferIndex)
        {
            if (length > buffer.Length)
                throw new ArgumentException(
                    "Push data exceeds buffer size.");

            if (bufferIndex + length < buffer.Length)
            {
                Array.Copy(data, offset, buffer, bufferIndex, length);
                return bufferIndex + length;
            }

            var len = Mathf.Min(length, buffer.Length);
            var endChunkLength = buffer.Length - bufferIndex;
            var wrappedChunkLength = len - endChunkLength;
            try
            {
                Array.Copy(data, offset, buffer, bufferIndex, endChunkLength);
                Array.Copy(data, offset + endChunkLength, buffer, 0, wrappedChunkLength);
                return wrappedChunkLength;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
        }

        public void WriteFromBuffer(ByteDataWriter writer, long bufferIndex, int length)
        {
            lock (buffer)
            {
                if (bufferIndex + length < buffer.Length)
                {
                    writer(buffer, (int)bufferIndex, length);
                }
                else
                {
                    if (length > bufferDataLength) length = (int)(bufferDataLength - bufferIndex);

                    if (length > buffer.Length) length = buffer.Length;

                    var l = Math.Min(buffer.Length, length);
                    var endChunkLength = (int)(buffer.Length - bufferIndex);
                    var wrappedChunkLength = l - endChunkLength;

                    writer(buffer, (int)bufferIndex, endChunkLength);
                    writer(buffer, 0, wrappedChunkLength);
                }
            }
        }

        private int CopyFromBuffer(T[] data, int offset, int length, int bufferIndex)
        {
            if (length > buffer.Length)
                throw new ArgumentException(
                    $"Push data exceeds buffer size {length} < {buffer.Length}");

            if (bufferIndex + length < buffer.Length)
            {
                Array.Copy(buffer, bufferIndex, data, offset, length);
                return bufferIndex + length;
            }

            var l = Mathf.Min(buffer.Length, length);
            var endChunkLength = buffer.Length - bufferIndex;
            var wrappedChunkLength = l - endChunkLength;

            Array.Copy(buffer, bufferIndex, data, offset, endChunkLength);
            Array.Copy(buffer, 0, data, offset + endChunkLength, wrappedChunkLength);
            return wrappedChunkLength;
        }

        public void Push(T[] data, int offset, int length)
        {
            lock (buffer)
            {
                bufferIndex = CopyToBuffer(data, offset, length, bufferIndex);
                bufferDataLength += length;
                OnDataAddedEvent?.Invoke(data, offset, length);
            }
        }

        public void Push(T data)
        {
            lock (buffer)
            {
                buffer[bufferIndex++] = data;
                if (bufferIndex >= buffer.Length) bufferIndex = 0;
                bufferDataLength++;
            }
        }

        public int Read(T[] data, int offset, int length, long bufferDataIndex)
        {
            if (bufferIndex == 0 && bufferDataLength == 0) // The ring buffer has been cleared.
                return 0;

            lock (buffer)
            {
                var read = (int)(Math.Min(bufferDataIndex + length, bufferDataLength) -
                                 bufferDataIndex);

                var bufferIndex = this.bufferIndex - (int)(bufferDataLength - bufferDataIndex);
                if (bufferIndex < 0) bufferIndex = buffer.Length + bufferIndex;

                CopyFromBuffer(data, offset, length, bufferIndex);

                return read;
            }
        }

        public Marker CreateMarker(int offset = 0)
        {
            var markerPosition = bufferDataLength + offset;
            if (markerPosition < 0) markerPosition = 0;

            var bufIndex = bufferIndex + offset;
            if (bufIndex < 0) bufIndex = buffer.Length + bufIndex;

            if (bufIndex > buffer.Length) bufIndex -= buffer.Length;

            var marker = new Marker(this, markerPosition, bufIndex);

            return marker;
        }

        public class Marker
        {
            private int index;

            public Marker(RingBuffer<T> ringBuffer, long markerPosition, int bufIndex)
            {
                this.RingBuffer = ringBuffer;
                CurrentBufferDataIndex = markerPosition;
                index = bufIndex;
            }

            public RingBuffer<T> RingBuffer { get; }

            public bool IsValid => RingBuffer.bufferDataLength - CurrentBufferDataIndex <= RingBuffer.Capacity;
            public long AvailableByteCount => Math.Min(RingBuffer.Capacity, RequestedByteCount);
            public long RequestedByteCount => RingBuffer.bufferDataLength - CurrentBufferDataIndex;
            public long CurrentBufferDataIndex { get; private set; }

            public int Read(T[] buffer, int offset, int length, bool skipToNextValid = false)
            {
                var read = -1;
                if (!IsValid && skipToNextValid && RingBuffer.bufferDataLength > RingBuffer.Capacity)
                    CurrentBufferDataIndex = RingBuffer.bufferDataLength - RingBuffer.Capacity;

                if (IsValid)
                {
                    read = RingBuffer.Read(buffer, offset, length, CurrentBufferDataIndex);
                    CurrentBufferDataIndex += read;
                    index += read;
                    if (index > buffer.Length) index -= buffer.Length;
                }


                return read;
            }

            public void ReadIntoWriters(params ByteDataWriter[] writers)
            {
                if (!IsValid && RingBuffer.bufferDataLength > RingBuffer.Capacity)
                    CurrentBufferDataIndex = RingBuffer.bufferDataLength - RingBuffer.Capacity;

                index = RingBuffer.GetBufferArrayIndex(CurrentBufferDataIndex);
                var length = (int)(RingBuffer.bufferDataLength - CurrentBufferDataIndex);
                if (IsValid && length > 0)
                    for (var i = 0; i < writers.Length; i++)
                        RingBuffer.WriteFromBuffer(writers[i], index, length);

                CurrentBufferDataIndex += length;
                index = RingBuffer.GetBufferArrayIndex(CurrentBufferDataIndex);
            }

            public Marker Clone()
            {
                return new Marker(RingBuffer, CurrentBufferDataIndex, index);
            }

            public void Offset(int amount)
            {
                CurrentBufferDataIndex += amount;
                if (CurrentBufferDataIndex < 0) CurrentBufferDataIndex = 0;
                if (CurrentBufferDataIndex > RingBuffer.bufferDataLength)
                    CurrentBufferDataIndex = RingBuffer.bufferDataLength;

                index = RingBuffer.GetBufferArrayIndex(CurrentBufferDataIndex);
            }
        }
    }
}
