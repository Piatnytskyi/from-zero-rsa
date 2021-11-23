using System;

namespace MD5
{
    public class HashingProgressEventArgs : EventArgs
    {
        private readonly int _done, _outOf;

        public HashingProgressEventArgs(int progress, int outOf = 100)
        {
            _done = progress;
            _outOf = outOf;
        }

        public int Done { get { return _done; } }
        public int OutOf { get { return _outOf; } }
    }
}
