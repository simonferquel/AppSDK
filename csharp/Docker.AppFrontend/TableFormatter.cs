using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Docker.AppFrontend
{
    struct ColumnDefinition<T>
    {
        public string Title { get; set; }
        public Func<T,string> ValueProvider { get; set; }

        public ColumnDefinition( string title, Func<T, string> valueProvider)
        {
            Title = title;
            ValueProvider = valueProvider;
        }
    }
    class TableFormatter<T>
    {
        public TableFormatter(params ColumnDefinition<T>[] columns)
        {
            _columns = columns;
        }

        private ColumnDefinition<T>[] _columns;

        public void Print(ICollection<T> items, TextWriter w, int minSpacing = 3)
        {
            var columnSizes = new List<int>();
            foreach(var c in _columns) {
                var s = c.Title?.Length ?? 0;
                foreach(var i in items) {
                    var l = c.ValueProvider(i)?.Length ?? 0;
                    if (l > s) {
                        s = l;
                    }
                }
                columnSizes.Add(s+minSpacing);
            }

            // write headers
            for(int i= 0; i < columnSizes.Count; ++i) {
                w.Write(_columns[i].Title.PadRight(columnSizes[i], ' '));
            }
            w.WriteLine();

            // write body
            foreach(var item in items) {
                for (int i = 0; i < columnSizes.Count; ++i) {
                    var value = _columns[i].ValueProvider(item) ?? "";
                    w.Write(value.PadRight(columnSizes[i], ' '));
                }
                w.WriteLine();
            }
        }
    }
}
