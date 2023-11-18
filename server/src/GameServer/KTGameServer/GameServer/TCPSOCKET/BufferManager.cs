using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.TCP
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/bb517542.aspx
    /// This class creates a single large buffer which can be divided up 
    /// and assigned to SocketAsyncEventArgs objects for use with each 
    /// socket I/O operation.  
    /// This enables bufffers to be easily reused and guards against 
    /// fragmenting heap memory.
    /// </summary>
    /// <remarks>The operations exposed on the BufferManager class are not thread safe.</remarks>
    internal sealed class BufferManager
    {
        /// <summary>
        /// The underlying Byte array maintained by the Buffer Manager.
        /// </summary>
        private Byte[] buffer;                

        /// <summary>
        /// Size of the underlying Byte array.
        /// </summary>
        private Int32 bufferSize;

        /// <summary>
        /// Current index of the underlying Byte array.
        /// </summary>
        private Int32 currentIndex;

        /// <summary>
        /// Pool of indexes for the Buffer Manager.
        /// </summary>
        private Stack<Int32> freeIndexPool;     

        /// <summary>
        /// The total number of bytes controlled by the buffer pool.
        /// </summary>
        private Int32 numBytes;

        /// <summary>
        /// Instantiates a buffer manager.
        /// </summary>
        /// <param name="totalBytes">The total number of bytes for the buffer pool.</param>
        /// <param name="bufferSize">Size of the buffer pool.</param>
        internal BufferManager(Int32 totalBytes, Int32 bufferSize)
        {
            this.numBytes = totalBytes;
            this.currentIndex = 0;
            this.bufferSize = bufferSize;
            this.freeIndexPool = new Stack<Int32>();
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object. 
        /// This frees the buffer back to the buffer pool.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs where is the buffer to be removed.</param>
        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            this.freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }

        /// <summary>
        ///  Allocates buffer space used by the buffer pool. 
        /// </summary>
        internal void InitBuffer()
        {
            // Create one big large buffer and divide that out to each SocketAsyncEventArg object.
            this.buffer = new Byte[this.numBytes];
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs where is the buffer to be allocated.</param>
        /// <returns>True if the buffer was successfully set, else false.</returns>
        internal Boolean SetBuffer(SocketAsyncEventArgs args)
        {
            if (this.freeIndexPool.Count > 0)
            {
                args.SetBuffer(this.buffer, this.freeIndexPool.Pop(), this.bufferSize);
            }
            else
            {
                if ((this.numBytes - this.bufferSize) < this.currentIndex)
                {
                    return false;
                }
                args.SetBuffer(this.buffer, this.currentIndex, this.bufferSize);
                this.currentIndex += this.bufferSize;
            }

            return true;
        }
    }
}
