// Copyright © Transeric Solutions 2011.  All rights reserved.
// Licensed under Code Project Open License (see http://www.codeproject.com/info/cpol10.aspx).
// Author: Eric David Lynch.
using System;

namespace WPFControl
{
    /// <summary>
    /// An enumeration of possible change types.
    /// </summary>
    public enum TextChangedType
    {
        /// <summary>
        /// An assignment to the Text property.
        /// </summary>
        /// <remarks>
        /// Not currently supported.
        /// </remarks>
        Assign,
        /// <summary>
        /// A deletion of characters initiated by the user interface.
        /// </summary>
        Delete,
        /// <summary>
        /// An insertion of characters initiated by the user interface.
        /// </summary>
        Insert,
        /// <summary>
        /// A replacement of characters initiated by the user interface.
        /// </summary>
        Replace,
        /// <summary>
        /// An Undo command is changing the text.
        /// </summary>
        /// <remarks>
        /// Not supported by default.  Messy support is available via the PreviewUndoEnabled property.
        /// </remarks>
        Undo,
        /// <summary>
        /// A Redo command is changing the text.
        /// </summary>
        /// <remarks>
        /// Not supported by default.  Messy support is available via the PreviewUndoEnabled property.
        /// </remarks>
        Redo
    }
}
