﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

// Based on https://github.com/madskristensen/TextmateBundleInstaller

namespace SLangPlugin.Comment
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommentUncommentCommand))]
    [ContentType(Constants.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class CommentUncommentCommand : ICommandHandler<CommentSelectionCommandArgs>, ICommandHandler<UncommentSelectionCommandArgs>
    {
        public string DisplayName => "Comment/Uncomment selection";

        public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            return Execute(AddComment, args.TextView);
        }

        public bool ExecuteCommand(UncommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            return Execute(RemoveComment, args.TextView);
        }

        public CommandState GetCommandState(CommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }

        public CommandState GetCommandState(UncommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }

        private bool Execute(Action<StringBuilder, string[]> callback, ITextView textView)
        {
            var sb = new StringBuilder();
            SnapshotSpan span = GetSpan(textView);
            string[] lines = GetLines(span);

            callback(sb, lines);

            UpdateTextBuffer(span, sb.ToString().TrimEnd(), textView);

            return true;
        }

        private void AddComment(StringBuilder sb, string[] lines)
        {
            foreach (string line in lines)
            {
                sb.AppendLine(Constants.SingleCommentChars + line);
            }
        }

        private void RemoveComment(StringBuilder sb, string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith(Constants.SingleCommentChars, StringComparison.Ordinal))
                {
                    sb.AppendLine(line.Substring(Constants.SingleCommentChars.Length));
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
        }

        private string[] GetLines(SnapshotSpan span)
        {
            ITextSnapshotLine startLine = span.Start.GetContainingLine();
            ITextSnapshotLine endLine = span.End.GetContainingLine();

            string[] lines;

            if (startLine.LineNumber != endLine.LineNumber)
            {
                IEnumerable<ITextSnapshotLine> rawLines = span.Snapshot.Lines.Where(l => l.LineNumber >= startLine.LineNumber && l.LineNumber <= endLine.LineNumber);
                lines = rawLines.Select(l => l.GetText()).ToArray();
            }
            else
            {
                lines = span.GetText().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }

            return lines;
        }

        private void UpdateTextBuffer(SnapshotSpan span, string text, ITextView textview)
        {
            try
            {
                span.Snapshot.TextBuffer.Replace(span.Span, text);

                var newSpan = new SnapshotSpan(span.Snapshot.TextBuffer.CurrentSnapshot, span.Start, text.Length);
                textview.Selection.Select(newSpan, false);
            }
            catch (Exception)
            {
                // nothing
            }
        }

        private SnapshotSpan GetSpan(ITextView textview)
        {
            VirtualSnapshotSpan sel = textview.Selection.StreamSelectionSpan;
            ITextSnapshotLine startLine = new SnapshotPoint(textview.TextSnapshot, sel.Start.Position).GetContainingLine();
            ITextSnapshotLine endLine = new SnapshotPoint(textview.TextSnapshot, sel.End.Position).GetContainingLine();

            if (textview.Selection.IsEmpty || startLine.LineNumber != endLine.LineNumber)
            {
                return new SnapshotSpan(startLine.Start, endLine.End);
            }
            else
            {
                return new SnapshotSpan(sel.Start.Position, sel.Length);
            }
        }
    }
}
