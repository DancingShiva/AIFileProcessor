//-----------------------------------------------------------------------
// <copyright file="AudioReorder.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Response
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AudioReorder
    {
        public string VisualString { get; set; } = string.Empty;

        public int StartIndex { get; set; } = -1;

        public int EndIndex { get; set; } = -1;
    }
}
