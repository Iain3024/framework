﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Utilities;
using System.Windows.Controls;
using System.Windows;

namespace Signum.Windows
{
    public static class ShortcutHelper
    {
        public static void SetLabelShortcuts(Window windows)
        {
            List<LineBase> labels = windows.Children<LineBase>(l => l.IsSet(LineBase.LabelTextProperty)).ToList();

            SetShortcuts(labels, l => l.LabelText, (l, s) => l.LabelText = s);
        }

        public static void SetMenuItemShortcuts(MenuItem menuItem)
        {
            List<MenuItem> menus = menuItem.Items.OfType<MenuItem>().Where(a => a.Header is string).ToList();

            SetShortcuts(menus, m => (string)m.Header, (m, s) => m.Header = s);
        }

        static void SetShortcuts<T>(List<T> elements, Func<T, string> getLabel, Action<T,string> setLabel)
        {
            if (elements.Count == 0)
                return;

            var labelList = elements.Select(getLabel).ToList();

            var commonWords = (from str in labelList
                               from word in str.Replace("_", "").Split(' ').Distinct()
                               where word.Length > 0
                               group word by word into g
                               where g.Count() > 1
                               select g.Key).ToDictionary(a => a, a => new string('*', a.Length));

            var usedChars = labelList.Select(s =>
            {
                int index = s.IndexOf('_');
                if (index == -1 || index == s.Length - 1)
                    return (char?)null;
                char result = s[index + 1];
                if (char.IsLetter(result) || char.IsNumber(result))
                    return result;
                return (char?)null;
            }).NotNull().ToHashSet();

            foreach (var item in elements)
            {
                string lab = getLabel(item);
                string lab2 = Shortcut(lab, commonWords, usedChars);
                if (lab != lab2)
                    setLabel(item, lab2); 
            }
        }

        static string Shortcut(string header, Dictionary<string, string> commonWords, HashSet<char> usedChars)
        {
            if (header.Contains('_'))
                return header;

            string header2 = header.Replace(commonWords); //replace common words by ****(n) so we don't use it, preserving length!

            int? index =
                FindReplacementIndex(header2, usedChars) ??
                FindReplacementIndex(header, usedChars) ??
                FindReplacementIndex(header, new HashSet<char>());

            if (index == null)
                return header;

            usedChars.Add(header[index.Value]);

            return header.Insert(index.Value, "_");
        }

        static int? FindReplacementIndex(string text, HashSet<char> usedchars)
        {
            bool lastSpace = true;
            int? j = null;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == ' ')
                    lastSpace = true;
                else
                {
                    if (char.IsLetter(c) || char.IsNumber(c) && !usedchars.Contains(c))
                        if (lastSpace)
                            return i;
                        else if (j == null)
                            j = i;
                    lastSpace = false;
                }
            }
            return j;
        }


    }
}
