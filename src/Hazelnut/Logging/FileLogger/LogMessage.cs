﻿using System;

namespace Hazelnut.Logging
{
    public struct LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public string Stack { get; set; }
        public int Severity { set; get; }
    }
}