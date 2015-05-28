// Copyright © Transeric Solutions 2011.  All rights reserved.
// Licensed under Code Project Open License (see http://www.codeproject.com/info/cpol10.aspx).
// Author: Eric David Lynch.
using System;

namespace WPFControl
{
    /// <summary>
    /// Represents the method that will handle the PreviewTextBox.PreviewTextChanged event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void PreviewTextChangedEventHandler(
        object sender,
        PreviewTextChangedEventArgs e);
}
