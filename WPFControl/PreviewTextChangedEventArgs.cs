// Copyright © Transeric Solutions 2011.  All rights reserved.
// Licensed under Code Project Open License (see http://www.codeproject.com/info/cpol10.aspx).
// Author: Eric David Lynch.
using System;
using System.Windows;
using System.Windows.Input;

namespace WPFControl
{
    /// <summary>
    /// The arguments for the PreviewTextChangedEventArgs.
    /// </summary>
    public class PreviewTextChangedEventArgs : RoutedEventArgs
    {
        #region Constructors
        /// <summary>
        /// Constructor for PreviewTextChangedEventArgs.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        /// <param name="type">The type of text change that will occur.</param>
        /// <param name="text">The new value of the text.</param>
        public PreviewTextChangedEventArgs(
            RoutedEvent routedEvent,
            TextChangedType type,
            string text)
            : base(routedEvent)
        {
            Type = type;
            Text = text;
        }

        /// <summary>
        /// Constructor for PreviewTextChangedEventArgs.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        /// <param name="source">An alternative source that will be reported when this event is handled.</param>
        /// <param name="type">The type of text change that will occur.</param>
        /// <param name="text">The new value of the text.</param>
        public PreviewTextChangedEventArgs(
            RoutedEvent routedEvent,
            object source,
            TextChangedType type,
            string text)
            : base(routedEvent, source)
        {
            Type = type;
            Text = text;
        }
        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Get the new value of the text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Get the type of text change that will occur.
        /// </summary>
        public TextChangedType Type { get; private set; }
        #endregion // Properties
    }
}
