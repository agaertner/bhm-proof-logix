using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Extended;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Nekres.ProofLogix.Core.UI {
    public class StandardTable<T> : Container where T : IEquatable<T> {

        private enum SortOrder {

            Ascending,
            Descending

        }

        private BitmapFont _font = GameService.Content.DefaultFont12;
        public BitmapFont Font {
            get => _font;
            set {
                if (!SetProperty(ref _font, value) || value == null)
                {
                    return;
                }
                _maxTextureHeight = (int)Math.Round(_font.MeasureString(".").Height);
            }
        }

        // Define a 2D array to represent the data table
        private object[]                          _header;
        private ConcurrentDictionary<T, object[]> _data;

        // Individual cell sizes. Height is equal font height plus padding.
        private int[]     _rowWidths;
        private int[]     _colHeights;

        // Padding
        private int       _cellPadding      = 10;

        // Max height for a Texture2D drawn in a cell. Should correspond to font height.
        private int       _maxTextureHeight = 13;

        private int       _sortColumn       = -1;
        private SortOrder _sortOrder        = SortOrder.Ascending;

        public StandardTable(object[] headerRow) {
            _header = headerRow;
            _data   = new ConcurrentDictionary<T, object[]>();
        }

        protected override void DisposeControl() {
            base.DisposeControl();
        }

        public void ChangeHeader(object[] headerRow) {
            foreach (object o in _header) {
                if (o is Control ctrl) {
                    ctrl.Dispose();
                }
            }
            Interlocked.Exchange(ref _header, headerRow);
        } 

        public void ChangeData(T key, object[] row) {
            if (row.Length != _header.Length) {
                return;
            }
            _data.AddOrUpdate(key, row, (_, oldRow) => {
                foreach (object o in oldRow) {
                    if (o is Control ctrl) {
                        ctrl.Dispose();
                    }
                }
                return row;
            });
        }

        public void RemoveData(T key) {
            _data.TryRemove(key, out _);
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            // Determine which column was clicked
            int x = e.MousePosition.X - this.AbsoluteBounds.X - _cellPadding;
            int y = e.MousePosition.Y - this.AbsoluteBounds.Y - _cellPadding;

            int col      = 0;
            int cumWidth = 0;

            while (col < _header.Length && cumWidth <= x) {
                cumWidth += _rowWidths[col] + _cellPadding * 2;
                col++;
            }

            col--;

            if (col >= 0 && col < _header.Length && y <= _colHeights[0] + _cellPadding) {
                // Sort the table based on the clicked column
                SortTable(col);
            }
            
        }

        private void SortTable(int column) {

            // Determine the sort order (ascending or descending)
            if (_sortColumn == column) {
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            } else {
                _sortColumn = column;
                _sortOrder  = SortOrder.Ascending;
            }

            // Sort the data based on the values in the specified column
            var sortedData = _data.OrderBy(row => {
                                    var value = row.Value[column];
                                    if (value is not IComparable) {
                                        return _sortOrder == SortOrder.Ascending ? int.MinValue : int.MaxValue;
                                    }
                                    return value;
                                }, Comparer<object>.Create((x, y) => {
                                    int compareResult;
                                    if (x is string && y is string) {
                                        compareResult = StringComparer.InvariantCultureIgnoreCase.Compare(y, x);
                                    } else {
                                        compareResult = ((IComparable)y).CompareTo(x);
                                    }
                                    return _sortOrder == SortOrder.Descending ? -compareResult : compareResult;
                                }));

            // Update the data table with the sorted values
            var newData = new ConcurrentDictionary<T, object[]>(sortedData);
            Interlocked.Exchange(ref _data, newData);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Calculate the maximum width and height of each column and row
            var dataTable = new List<object[]> { _header }.Concat(_data.Values).ToList();
            var colLength = dataTable.Count;
            var rowLength = _header.Length;

            _rowWidths = new int[rowLength];
            _colHeights = new int[colLength];

            for (int row = 0; row < rowLength; row++) {
                for (int col = 0; col < colLength; col++) {
                    object cellContent = dataTable[col][row];

                    if (cellContent is AsyncTexture2D texture) {
                        var maxWidth  = Math.Min(texture.Bounds.Width,  _maxTextureHeight);
                        var maxHeight = Math.Min(texture.Bounds.Height, _maxTextureHeight);
                        _rowWidths[row]  = Math.Max(_rowWidths[row],  maxWidth); // In case there is no text to define cell width.
                        _colHeights[col] = Math.Max(_colHeights[col], maxHeight);

                    } else if (cellContent is Control ctrl) {

                        _rowWidths[row]  = Math.Max(_rowWidths[row],  ctrl.Size.X);
                        _colHeights[col] = Math.Max(_colHeights[col], ctrl.Size.Y);

                    } else {
                        var str = cellContent.ToString();
                        var size = this.Font.MeasureString(str);
                        _rowWidths[row]  = Math.Max(_rowWidths[row],  (int)size.Width + -_cellPadding / 2);
                        _colHeights[col] = Math.Max(_colHeights[col], (int)size.Height);
                    }
                }
            }

            // Calculate the total width of the table
            int totalWidth = _rowWidths.Sum(w => w + _cellPadding * 2);
            // Calculate the total height of the table
            int totalHeight = _colHeights.Sum(h => h + _cellPadding * 2);

            // Determine the remaining width and height available for cells to occupy
            int remainingWidth = Math.Max(bounds.Width - totalWidth, 0);
            int remainingHeight = Math.Max(bounds.Height - totalHeight, 0);

            // Calculate the number of cells that can occupy the remaining width and height
            int numCols = Math.Max(_rowWidths.Count(w => w == 0), 1);
            int numRows = Math.Max(_colHeights.Count(h => h == 0), 1);

            // Calculate the width and height of each cell
            int cellWidth = remainingWidth / numCols;
            int cellHeight = remainingHeight / numRows;

            // Draw each cell of the table
            int x = bounds.X + _cellPadding;
            int y = bounds.Y + _cellPadding;

            for (int row = 0; row < dataTable.Count; row++) {
                x = bounds.X + _cellPadding;

                for (int col = 0; col < dataTable[0].Length; col++) {
                    object cellContent = dataTable[row][col];

                    // Calculate cell size
                    var currentCellWidth = _rowWidths[col] + _cellPadding * 2;
                    var currentCellHeight = _colHeights[row] + _cellPadding * 2;
                    // If the current cell size is zero, use the calculated cell size
                    if (currentCellWidth == 0) {
                        currentCellWidth = cellWidth;
                    }
                    if (currentCellHeight == 0) {
                        currentCellHeight = cellHeight;
                    }
                    
                    var contentRect = new Rectangle(x, y, currentCellWidth, currentCellHeight);

                    if (cellContent is AsyncTexture2D texture) {

                        var target = Math.Min(_maxTextureHeight, currentCellWidth);

                        // Calculate the target width based on the aspect ratio of the texture.
                        var targetSize = (int) (target * ((float) texture.Bounds.Width / texture.Bounds.Height));

                        // Center texture in the cell.
                        var textureRect = new Rectangle(x + (currentCellWidth  - targetSize) / 2,
                                                        y + (currentCellHeight - targetSize) / 2,
                                                        targetSize,
                                                        targetSize);

                        spriteBatch.DrawOnCtrl(this, texture, textureRect, Color.White);

                    } else if (cellContent is Control ctrl) {

                        ctrl.Location = new Point(x + (currentCellWidth - ctrl.Size.X) / 2, 
                                                  y + (currentCellHeight - ctrl.Size.Y) / 2);
                        
                    } else {
                        var str = cellContent.ToString();
                        spriteBatch.DrawStringOnCtrl(this, str, this.Font, contentRect, Color.White, false, row == 0, 1, HorizontalAlignment.Center);
                    }

                    spriteBatch.DrawRectangleOnCtrl(this, contentRect, 1);

                    x += currentCellWidth;
                }

                y += _colHeights[row] + _cellPadding * 2;
            }
        }

    }
}
