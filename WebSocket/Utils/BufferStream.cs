using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebSocket.Utils
{
    /// <summary>
    /// Enumerator that represents the size of each data type supported by the BufferStream class.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum BufferTypeSize
    {
        None = 0, Bool = 1, Byte = 1, SByte = 1,
        UInt16 = 2, Int16 = 2, UInt32 = 4, Int32 = 4,
        Single = 4, Double = 8, String = -1, Bytes = -1
    }

    /// <summary>
    /// Provides a stream designed for reading TCP packets and UDP datagrams by type.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public class BufferStream
    {
        private const byte BTrue = 1, BFalse = 0;
        private readonly int _fastAlign;
        private readonly int _fastAlignNot;
        private int _iterator, _length, _alignment;
        private byte[] _memory;

        /// <summary>
        /// Gets the underlying array of memory for this BufferStream.
        /// </summary>
        public byte[] Memory => _memory;

        /// <summary>
        /// Gets the length of the buffer in bytes.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Gets the current iterator(read/write position) for this BufferStream.
        /// </summary>
        public int Iterator => _iterator;


        /// <summary>
        /// Instantiates an instance of the BufferStream class with the specified stream length and alignment.
        /// </summary>
        /// <param name="length">Length of the BufferStream in bytes.</param>
        /// <param name="alignment">Alignment of the BufferStream in bytes.</param>
        public BufferStream(int length, int alignment)
        {
            _fastAlign = alignment - 1;
            _fastAlignNot = ~_fastAlign;
            this._length = length;
            this._alignment = alignment;
            _memory = new byte[AlignedIterator(length, alignment)];
            _iterator = 0;
        }

        /// <summary>
        /// (forced inline) Takes an iterator and aligns it to the specified alignment size.
        /// </summary>
        /// <param name="iterator">Read/write position.</param>
        /// <param name="alignment">Size in bytes to align the iterator too.</param>
        /// <returns>The aligned iterator value.</returns>
        [MethodImpl(256)]
        public int AlignedIterator(int iterator, int alignment)
        {
            return ((iterator + (alignment - 1)) & ~(alignment - 1));
        }

        /// <summary>
        /// (forced inline) Checks if the specified index with the specified length in bytes is within bounds of the buffer.
        /// </summary>
        /// <param name="iterator">Read/write position.</param>
        /// <param name="length">Index to check the bounds of.</param>
        /// <param name="align"></param>
        /// <returns></returns>
        [MethodImpl(256)]
        public bool IsWithinMemoryBounds(int iterator, int length, bool align = true)
        {
            int iteratorBegin = (align) ? ((iterator + _fastAlign) & _fastAlignNot) : iterator;
            int iteratorEnd = iteratorBegin + length;
            return (iteratorBegin < 0 || iteratorEnd >= length);
        }

        /// <summary>
        /// Allocates a new block of memory for the BufferStream with the specified length and alignment--freeing the old one if it exists.
        /// </summary>
        /// <param name="length">Length in bytes of the new block of memory.</param>
        /// <param name="alignment">Alignment in bytes to align the block of memory too.</param>
        public void Allocate(int length, int alignment)
        {
            if (_memory != null) Deallocate();

            _memory = new byte[AlignedIterator(length, alignment)];
            this._alignment = alignment;
            this._length = length;
        }

        /// <summary>
        /// Frees up the existing block of memory and sets the iterator to 0.
        /// </summary>
        public void Deallocate()
        {
            _memory = null;
            _iterator = 0;
        }

        public void ReassignMemory(byte[] mem)
        {
            _memory = mem;
        }

        /// <summary>
        /// Sets all elements in this buffer to 0.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"/>
        public void ZeroMemory()
        {
            Array.Clear(_memory, 0, _memory.Length);
        }

        /// <summary>
        /// Sets all elements in this buffer to the specified value.
        /// </summary>
        /// <param name="value">The value to zero the memory too.</param>
        public void ZeroMemory(byte value)
        {
            for (int i = 0; i++ < _memory.Length;) _memory[i] = value;
        }

        /// <summary>
        /// Creates a copy of this buffer and all it's contents.
        /// </summary>
        /// <returns>A new clone BufferStream.</returns>
        /// <exception cref="System.ArgumentNullException"/>
        public BufferStream CloneBufferStream()
        {
            BufferStream clone = new BufferStream(_memory.Length, _alignment);
            Array.Copy(_memory, clone.Memory, _memory.Length);
            clone._iterator = _iterator;
            return clone;
        }

        /// <summary>
        /// Copies the specified number of bytes from this buffer to the destination buffer, given the start position(s) in each buffer.
        /// </summary>
        /// <param name="destBuffer">Buffer to copy the contents of this buffer too.</param>
        /// <param name="sourceIndex">Start position to begin copying the data from in this buffer.</param>
        /// <param name="destIndex">Start position to begin copying the data to in the destination buffer.</param>
        /// <param name="length">Number of bytes to copy from this buffer to the destination buffer.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentException"/>
        public void BlockCopy(BufferStream destBuffer, int sourceIndex, int destIndex, int length)
        {
            Array.Copy(_memory, sourceIndex, destBuffer.Memory, destIndex, length);
        }

        /// <summary>
        /// Copes the specified number of bytes from this buffer to the destination buffer, given the shared start position for both buffers.
        /// </summary>
        /// <param name="destBuffer">Buffer to copy the contents of this buffer too.</param>
        /// <param name="startIndex">Shared start index of both buffers to start copying data from/to.</param>
        /// <param name="length">Number of bytes to copy from this buffer to the destination buffer.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentException"/>
        public void BlockCopy(BufferStream destBuffer, int startIndex, int length)
        {
            Array.Copy(_memory, startIndex, destBuffer.Memory, startIndex, length);
        }

        /// <summary>
        /// Copes the specified number of bytes from the start of this buffer to the start of the destination buffer.
        /// </summary>
        /// <param name="destBuffer">Buffer to copy the contents of this buffer too.</param>
        /// <param name="length">Number of bytes to copy from this buffer to the destination buffer.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentException"/>
        public void BlockCopy(BufferStream destBuffer, int length)
        {
            Array.Copy(_memory, destBuffer.Memory, length);
        }

        /// <summary>
        /// Copies the entire contents of this buffer to the destination buffer.
        /// </summary>
        /// <param name="destBuffer">Buffer to copy the contents of this buffer too.</param>
        public void BlockCopy(BufferStream destBuffer)
        {
            int length = (destBuffer.Memory.Length > _memory.Length) ? _memory.Length : destBuffer.Memory.Length;
            Array.Copy(_memory, destBuffer.Memory, length);
        }

        /// <summary>
        /// Resizes the block of memory for this buffer.
        /// </summary>
        /// <param name="size">Size in bytes of the resized block of memory.</param>
        public void ResizeBuffer(int size)
        {
            Array.Resize(ref _memory, size);
            _length = _memory.Length;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">BOOLEAN value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(bool value)
        {
            _memory[_iterator++] = (value) ? BTrue : BFalse;
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">BYTE value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(byte value)
        {
            _memory[_iterator++] = value;
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">SBYTE value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(sbyte value)
        {
            _memory[_iterator++] = (byte)value;
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">USHORT value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(ushort value)
        {
            _memory[_iterator++] = (byte)value;
            _memory[_iterator++] = (byte)(value >> 8);
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">SHORT value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(short value)
        {
            _memory[_iterator++] = (byte)value;
            _memory[_iterator++] = (byte)(value >> 8);
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">UINT value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(uint value)
        {
            _memory[_iterator++] = (byte)value;
            _memory[_iterator++] = (byte)(value >> 8);
            _memory[_iterator++] = (byte)(value >> 16);
            _memory[_iterator++] = (byte)(value >> 24);
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">INT value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(int value)
        {
            _memory[_iterator++] = (byte)value;
            _memory[_iterator++] = (byte)(value >> 8);
            _memory[_iterator++] = (byte)(value >> 16);
            _memory[_iterator++] = (byte)(value >> 24);
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">FLOAT value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = 0; i < bytes.Length; i++) _memory[_iterator++] = bytes[i];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">DOUBLE value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = 0; i < bytes.Length; i++) _memory[_iterator++] = bytes[i];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">STRING value to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++) { _memory[_iterator++] = bytes[i]; }
            _memory[_iterator++] = 0;
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Writes a value of the specified type to this buffer.
        /// </summary>
        /// <param name="value">BYTE[] array to be written.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Write(byte[] value)
        {
            for (int i = 0; i < value.Length; i++) _memory[_iterator++] = value[i];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the BOOL value in.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Read(out bool value)
        {
            value = _memory[_iterator++] > 0;
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the BYTE value in.</param>
        /// /// <exception cref="System.IndexOutOfRangeException"/>
        public void Read(out byte value)
        {
            value = _memory[_iterator++];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the SBYTE value in.</param>
        /// <exception cref="System.IndexOutOfRangeException"/>
        public void Read(out sbyte value)
        {
            value = (sbyte)_memory[_iterator++];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the USHORT value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out ushort value)
        {
            value = BitConverter.ToUInt16(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.UInt16 + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the SHORT value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out short value)
        {
            value = BitConverter.ToInt16(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.Int16 + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the UINT value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out uint value)
        {
            value = BitConverter.ToUInt32(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.Int32 + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the INT value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out int value)
        {
            value = BitConverter.ToInt32(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.Int32 + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the FLOAT value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out float value)
        {
            value = BitConverter.ToSingle(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.Single + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the DOUBLE value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out double value)
        {
            value = BitConverter.ToUInt16(_memory, _iterator);
            _iterator = (_iterator + (int)BufferTypeSize.Double + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the BYTE[] value in.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out string value)
        {
            int index = 0;
            int startIt = _iterator;

            for (char c; _iterator < _length;)
            {
                c = (char)_memory[_iterator++];
                if (c == '\0') break;
                index++;
            }

            value = Encoding.UTF8.GetString(_memory, startIt, index);
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Reads the value of the specified type from the buffer.
        /// </summary>
        /// <param name="value">The OUT variable to store the BYTE[] value in.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        public void Read(out byte[] value, int length)
        {
            value = new byte[length];
            for (int i = 0; i < length; i++) value[i] = _memory[_iterator++];
            _iterator = (_iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Sets the iterator(read/write position) to the specified index, aligned to this buffer's alignment.
        /// </summary>
        /// <param name="iterator">Index to set the iterator to.</param>
        public void Seek(int iterator)
        {
            this._iterator = (iterator + _fastAlign) & _fastAlignNot;
        }

        /// <summary>
        /// Sets the iterator(read/write position) to the specified index, aligned to this buffer's alignment if alignment is specified as true.
        /// </summary>
        /// <param name="iterator">Index to set the iterator to.</param>
        /// <param name="align">Whether to align the iterator or not.</param>
        public void Seek(int iterator, bool align)
        {
            this._iterator = (align) ? (iterator + _fastAlign) & _fastAlignNot : iterator;
        }
    }
}