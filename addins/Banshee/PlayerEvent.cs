// 
// PlayerEvent.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2006-2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Banshee.MediaEngine
{
    public delegate void DBusPlayerStateHandler (string state);
    public delegate void DBusPlayerEventHandler (string evnt, string message, double bufferingPercent);
    public delegate void PlayerEventHandler (PlayerEventArgs args);
    
    public class PlayerEventArgs : EventArgs
    {
        private PlayerEvent @event;
        public PlayerEvent Event {
            get { return @event; }
        }
        
        public PlayerEventArgs (PlayerEvent @event)
        {
            this.@event = @event;
        }
    }
    
    public class PlayerEventStateChangeArgs : PlayerEventArgs
    {
        private PlayerState previous;
        public PlayerState Previous {
            get { return previous; }
        }
        
        private PlayerState current;
        public PlayerState Current {
            get { return current; }
        }
        
        public PlayerEventStateChangeArgs (PlayerState previous, PlayerState current) : base (PlayerEvent.StateChange)
        {
            this.previous = previous;
            this.current = current;
        }
    }
    
    public class PlayerEventErrorArgs : PlayerEventArgs
    {
        private string message;
        public string Message {
            get { return message; }
        }
        
        public PlayerEventErrorArgs (string message) : base (PlayerEvent.Error)
        {
            this.message = message;
        }
    }
    
    public sealed class PlayerEventBufferingArgs : PlayerEventArgs
    {
        private double progress;
        public double Progress {
            get { return progress; }
        }
        
        public PlayerEventBufferingArgs (double progress) : base (PlayerEvent.Buffering)
        {
            this.progress = progress;
        }
    }
    
    // WARNING: If you add events to the list below, you MUST update the 
    // "all" mask in PlayerEngineService.cs to reflect your addition!
    
    [Flags]
    public enum PlayerEvent
    {
        None = 0,
        Iterate = 1,
        StateChange = 2,
        StartOfStream = 4,
        EndOfStream = 8,
        Buffering = 16,
        Seek = 32,
        Error = 64,
        Volume = 128,
        Metadata = 256,
        TrackInfoUpdated = 512
    }
    
    public enum PlayerState 
    {
        NotReady,
        Ready,
        Idle,
        Contacting,
        Loading,
        Loaded,
        Playing,
        Paused
    }
}
