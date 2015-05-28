// Copyright © Transeric Solutions 2011.  All rights reserved.
// Licensed under Code Project Open License (see http://www.codeproject.com/info/cpol10.aspx).
// Author: Eric David Lynch.
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WPFControl
{
    /// <summary>
    /// A text box that implements a PreviewTextChanged event to allow filtering of user input.
    /// </summary>
    /// <remarks>
    /// Currently, this control does not raise PreviewTextChanged events of type TextChangedType.Assign.
    /// 
    /// Generally, it should not be necessary to preview the results of Undo / Redo.  Since, with otherwise
    /// correct validation, neither can result in an invalid value.  However, messy support of these features
    /// is included via the PreviewUndoEnabled property.  The support is messy because access to the Undo / Redo
    /// stacks is almost non-existent.
    /// </remarks>
    public class PreviewTextBox : TextBox
    {
        #region Event identifiers
        /// <summary>
        /// Identifies the PreviewTextChanged event.
        /// </summary>
        public static readonly RoutedEvent PreviewTextChangedEvent =
            EventManager.RegisterRoutedEvent("PreviewTextChanged", RoutingStrategy.Tunnel,
            typeof(PreviewTextChangedEventHandler), typeof(PreviewTextBox));
        #endregion // Event identifiers

        #region Events
        /// <summary>
        /// Occurs when content changes in the text element.
        /// </summary>
        public event PreviewTextChangedEventHandler PreviewTextChanged
        {
            add { AddHandler(PreviewTextChangedEvent, value); }
            remove { RemoveHandler(PreviewTextChangedEvent, value); }
        }
        #endregion // Events

        #region Properties
        /// <summary>
        /// A value indicating if PreviewTextChanged events are raised for Undo / Redo commands.
        /// </summary>
        public bool PreviewUndoEnabled { get; set; }
        #endregion // Properties

        #region Protected methods
        /// <summary>
        /// Occurs when the control is initializing.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(
            EventArgs e)
        {
            // Catch application and editing commands for this control
            AddHandler(CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler(previewExecutedEvent), true);

            // Perform base logic
            base.OnInitialized(e);
        }

        /// <summary>
        /// Raise a PreviewKeyDown event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnPreviewKeyDown(
            KeyEventArgs e)
        {
            // If white space (which does NOT trigger PreviewTextInput), handle it
            if (e.Key == Key.Space)
                textInput(e, " ");
            
            // Perform base logic
            base.OnPreviewKeyDown(e);
        }
 
        /// <summary>
        /// Raise a PreviewTextInput event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewTextInput(
            TextCompositionEventArgs e)
        {
            // Process the text input
            textInput(e, e.Text);

            // Perform base logic
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// Raise a PreviewTextChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnPreviewTextChanged(
            PreviewTextChangedEventArgs e)
        {
            RaiseEvent(e);
        }
        #endregion // Protected methods

        #region Private methods
        /// <summary>
        /// Handle PreviewExecutedEvent for this control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void previewExecutedEvent(
            object sender,
            ExecutedRoutedEventArgs e)
        {
            // If this is a cut or delete command...
            if (e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Delete)
            {
                // Get the start of the selection (caret position) and length of selection
                int start = SelectionStart;
                int length = SelectionLength;

                // If we have something to delete, delete it
                if (length > 0)
                    textDelete(e, start, length);

                // Return to caller
                return;
            } // If this is a cut or delete command...

            // If this is a paste command...
            if (e.Command == ApplicationCommands.Paste)
            {
                // Perform the paste (used past text as input)
                textInput(e, (string)Clipboard.GetDataObject().GetData(
                    typeof(string)));

                // Return to caller
                return;
            } // If this is a paste command...

            // If messy Undo / Redo support is enabled...
            if (PreviewUndoEnabled)
            {
                // If this is an Undo command...
                if (e.Command == ApplicationCommands.Undo)
                {
                    // If we can't undo, just return
                    if (!Undo())
                        return;

                    // Here is the messy bit (redo the undo)
                    string text = Text;
                    Redo();

                    // Check if we should let the handler undo it again
                    textChange(e, TextChangedType.Undo, text);

                    // Return to caller
                    return;
                } // If this is an Undo command...

                // If this is a Redo command...
                if (e.Command == ApplicationCommands.Redo)
                {
                    // If we can't redo, just return
                    if (!Redo())
                        return;

                    // Here is the messy bit (undo the redo)
                    string text = Text;
                    Undo();

                    // Check if we should let the handler redo it again
                    textChange(e, TextChangedType.Redo, text);

                    // Return to caller
                    return;
                } // If this is a Redo command...
            } // If messy Undo / Redo support is enabled...

            // If this is a backspace editing command...
            if (e.Command == EditingCommands.Backspace)
            {
                // Get the start of the selection (caret position) and length of selection
                int start = SelectionStart;
                int length = SelectionLength;

                // If we are deleting something...
                if (length > 0 || start > 0)
                {
                    // If deleting selection, delete it; otherwise, delete previous character
                    if (length > 0)
                        textDelete(e, start, length);
                    else
                        textDelete(e, start - 1, 1);
                } // If we are deleting something...

                // Return to caller
                return;
            } // If this is a backspace editing command...

            // If this is a delete editing command...
            if (e.Command == EditingCommands.Delete)
            {
                // Get the start of the selection (caret position) and length of selection
                int start = SelectionStart;
                int length = SelectionLength;

                // If we are deleting something...
                if (length > 0 || start < Text.Length)
                {
                    // If deleting selection, delete it; otherwise, delete previous character
                    if (length > 0)
                        textDelete(e, start, length);
                    else
                        textDelete(e, start, 1);
                } // If we are deleting something...

                // Return to caller
                return;
            } // If this is a delete editing command...

            // If this is a delete next word command...
            if (e.Command == EditingCommands.DeleteNextWord)
            {
                // Get text and text length
                string text = Text;
                int length = text.Length;

                // Prepare for loop
                int start = CaretIndex;
                int end = start;

                // Loop to get to end of current word (if in word)...
                while (end < length && !Char.IsWhiteSpace(text[end]))
                    end++;

                // Loop to get to end of white space preceeding subsequent word (if any)...
                while (end < length && Char.IsWhiteSpace(text[end]))
                    end++;

                // If we have something to delete, delete it
                if (end > start)
                    textDelete(e, start, end - start);

                // Return to caller
                return;
            } // If this is a delete next word command...

            // If this is a delete previous word command...
            if (e.Command == EditingCommands.DeletePreviousWord)
            {
                // Get text and text length
                string text = Text;
                int length = text.Length;

                // Prepare for loop
                int end = CaretIndex;
                int start = end;

                // Loop to get to end of previous word...
                while(start > 0 && Char.IsWhiteSpace(text[start - 1]))
                    start--;

                // Loop to get to start of previous word...
                while(start > 0 && !Char.IsWhiteSpace(text[start - 1]))
                    start--;

                // If we have something to delete, delete it
                if (end > start)
                    textDelete(e, start, end - start);

                // Return to caller
                return;
            } // If this is a delete previous word command...
        }

        /// <summary>
        /// Process text input.
        /// </summary>
        /// <param name="e">The arguments for the routed event that triggered the change.</param>
        /// <param name="text">The text.</param>
        private void textInput(
            RoutedEventArgs e,
            string text)
        {
            // Get the start of the selection (caret position) and length of selection
            int start = SelectionStart;
            int length = SelectionLength;

            // If we are replacing selected text, replace it; otherwise, insert it
            if (length > 0)
                textReplace(e, start, length, text);
            else
                textInsert(e, start, text);
        }

        /// <summary>
        /// Delete text.
        /// </summary>
        /// <param name="e">The arguments for the routed event that triggered the change.</param>
        /// <param name="startIndex">The zero-based position to begin deleting characters.</param>
        /// <param name="count">The number of characters to delete.</param>
        private void textDelete(
            RoutedEventArgs e,
            int startIndex,
            int count)
        {
            textChange(e, TextChangedType.Delete, Text.Remove(startIndex, count));
        }

        /// <summary>
        /// Insert text.
        /// </summary>
        /// <param name="e">The event that triggered the change.</param>
        /// <param name="text">The text to be inserted.</param>
        private void textInsert(
            RoutedEventArgs e,
            int startIndex,
            string text)
        {
            textChange(e, TextChangedType.Insert, Text.Insert(startIndex, text));
        }

        /// <summary>
        /// Replace text (delete and insert).
        /// </summary>
        /// <param name="e">The event that triggered the change.</param>
        /// <param name="startIndex">The zero-based position to begin deleting characters.</param>
        /// <param name="count">The number of characters to delete.</param>
        /// <param name="text">The text to be inserted.</param>
        private void textReplace(
            RoutedEventArgs e,
            int startIndex,
            int count,
            string text)
        {
            textChange(e, TextChangedType.Replace, Text.Remove(startIndex, count).Insert(startIndex, text));
        }

        /// <summary>
        /// Change text.
        /// </summary>
        /// <param name="e">The event that triggered the change.</param>
        /// <param name="type">The type of text change that will occur.</param>
        /// <param name="text">The new value of the text.</param>
        private void textChange(
            RoutedEventArgs e,
            TextChangedType type,
            string text)
        {
            // If no change, just return
            if (Text == text)
                return;

            // If this event was handled, handle the triggering event
            if (textChange(type, text))
                e.Handled = true;
        }

        /// <summary>
        /// Change text.
        /// </summary>
        /// <param name="type">The type of text change that will occur.</param>
        /// <param name="text">The new value of the text.</param>
        /// <returns>A value indicating if the event was handled.</returns>
        private bool textChange(
            TextChangedType type,
            string text)
        {
            // Raise the PreviewTextChanged event
            PreviewTextChangedEventArgs e = new PreviewTextChangedEventArgs(
                PreviewTextChangedEvent, type, text);
            OnPreviewTextChanged(e);

            // Return a value indicating if the event was handled
            return e.Handled;
        }
        #endregion // Private methods
    }
}
