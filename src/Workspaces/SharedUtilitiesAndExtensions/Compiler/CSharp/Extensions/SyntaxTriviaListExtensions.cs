﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static class SyntaxTriviaListExtensions
    {
        public static bool Any(this SyntaxTriviaList triviaList, params SyntaxKind[] kinds)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.MatchesKind(kinds))
                {
                    return true;
                }
            }

            return false;
        }

        public static SyntaxTrivia? GetFirstNewLine(this SyntaxTriviaList triviaList)
        {
            return triviaList
                .Where(t => t.Kind() == SyntaxKind.EndOfLineTrivia)
                .FirstOrNull();
        }

        public static SyntaxTrivia? GetLastComment(this SyntaxTriviaList triviaList)
        {
            return triviaList
                .Where(t => t.IsRegularComment())
                .LastOrNull();
        }

        public static SyntaxTrivia? GetLastCommentOrWhitespace(this SyntaxTriviaList triviaList)
        {
            return triviaList
                .Where(t => t.MatchesKind(SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia, SyntaxKind.WhitespaceTrivia))
                .LastOrNull();
        }

        public static IEnumerable<SyntaxTrivia> SkipInitialWhitespace(this SyntaxTriviaList triviaList)
            => triviaList.SkipWhile(t => t.Kind() == SyntaxKind.WhitespaceTrivia);

        private static ImmutableArray<ImmutableArray<SyntaxTrivia>> GetLeadingBlankLines(SyntaxTriviaList triviaList)
        {
            var result = ArrayBuilder<ImmutableArray<SyntaxTrivia>>.GetInstance();
            var currentLine = ArrayBuilder<SyntaxTrivia>.GetInstance();
            foreach (var trivia in triviaList)
            {
                currentLine.Add(trivia);
                if (trivia.Kind() == SyntaxKind.EndOfLineTrivia)
                {
                    var currentLineIsBlank = currentLine.All(t =>
                        t.Kind() == SyntaxKind.EndOfLineTrivia ||
                        t.Kind() == SyntaxKind.WhitespaceTrivia);
                    if (!currentLineIsBlank)
                    {
                        break;
                    }

                    result.Add(currentLine.ToImmutableAndFree());
                    currentLine = ArrayBuilder<SyntaxTrivia>.GetInstance();
                }
            }

            return result.ToImmutableAndFree();
        }

        public static SyntaxTriviaList WithoutLeadingBlankLines(this SyntaxTriviaList triviaList)
        {
            var triviaInLeadingBlankLines = GetLeadingBlankLines(triviaList).SelectMany(l => l);
            return new SyntaxTriviaList(triviaList.Skip(triviaInLeadingBlankLines.Count()));
        }

        /// <summary>
        /// Takes an INCLUSIVE range of trivia from the trivia list. 
        /// </summary>
        public static IEnumerable<SyntaxTrivia> TakeRange(this SyntaxTriviaList triviaList, int start, int end)
        {
            while (start <= end)
            {
                yield return triviaList[start++];
            }
        }

        /// <summary>
        /// Returns modified <paramref name="triviaList"/> with removed multiple <see cref="SyntaxToken"/>
        /// with kind <see cref="SyntaxKind.WhitespaceTrivia"/> arranged in a sequence 
        /// preserving only the first one: [space1][space2][space3] -> [space1]. 
        /// </summary>
        public static SyntaxTriviaList CollapseSequentialWhitespaceTrivia(this SyntaxTriviaList triviaList)
        {
            var result = new SyntaxTriviaList();
            var previous = default(SyntaxTrivia);
            foreach (var current in triviaList)
            {
                if (!(previous.IsWhitespace() && current.IsWhitespace()))
                    result = result.Add(current);
                previous = current;
            }

            return result;
        }
    }
}
