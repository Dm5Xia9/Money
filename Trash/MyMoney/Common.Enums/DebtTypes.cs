﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Enums
{
    public enum DebtTypes
    {
        /// <summary>
        /// Нужно забрать.
        /// </summary>
        [Description("Нужно забрать")]
        Plus = 1,

        /// <summary>
        /// Нужно отдать.
        /// </summary>
        [Description("Нужно отдать")]
        Minus = 2,
    }
}
