using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using KarnaughMap.Events.EventArgs;
using KarnaughMap.Events.EventHandlers;
using KarnaughMap.Exceptions;

namespace KarnaughMap
{
    [DesignerSerializer(typeof(KarnaughMapSerializer), typeof(CodeDomSerializer))]
    public partial class KarnaughMap : UserControl
    {
        public KarnaughMap()
        {
            InitializeComponent();
            DoubleBuffered = true;

            InputVariables = new MintPlayer.ObservableCollection.ObservableCollection<string>();
            InputVariables.CollectionChanged += InputVariables_CollectionChanged;
            loops_ones = new MintPlayer.ObservableCollection.ObservableCollection<QuineMcCluskey.RequiredLoop>();
            loops_zeros = new MintPlayer.ObservableCollection.ObservableCollection<QuineMcCluskey.RequiredLoop>();

            EventHandler invalidateDelegate = (sender, e) => Invalidate();
            GotFocus += invalidateDelegate;
            LostFocus += invalidateDelegate;
        }

        #region Constants
        /// <summary>Defines the width of a cell in the Karnaugh map.</summary>
        const int gridSize = 40;
        #endregion
        #region Private fields
        /// <summary>Holds the variable names that are projected on top of the Karnaugh map.</summary>
        private string[] varsX;
        /// <summary>Holds the variable names that are projected on the left of the Karnaugh map.</summary>
        private string[] varsY;
        /// <summary>The number of actual rows in the Karnaugh map.</summary>
        private int rowCount;
        /// <summary>The number of actual columns in the Karnaugh map.</summary>
        private int columnCount;
        /// <summary>The position of the currently focused cell.</summary>
        private Point focusedCell = new Point();
        /// <summary>Holds the minterms for "high".</summary>
        private List<int> ones = new List<int>();
        /// <summary>Holds the minterms for "low".</summary>
        private List<int> zeros = new List<int>();
        /// <summary>Holds the minterms that are selected</summary>
        private List<int> selectedCells = new List<int>();
        /// <summary>Holds the required loops for "high".</summary>
        private readonly MintPlayer.ObservableCollection.ObservableCollection<QuineMcCluskey.RequiredLoop> loops_ones;
        /// <summary>Holds the required loops for "low".</summary>
        private readonly MintPlayer.ObservableCollection.ObservableCollection<QuineMcCluskey.RequiredLoop> loops_zeros;
        #endregion
        #region Private methods
        /// <summary>Gets the minterm value for a specified grid position</summary>
        /// <param name="gridIndex"></param>
        /// <returns></returns>
        private int GridPositionToMinterm(Point gridIndex)
        {
            return GridPositionToMinterm(gridIndex.X, gridIndex.Y);
        }
        private int GridPositionToMinterm(int x, int y)
        {
            var x_gray = GrayCodeConverter.Decimal2Gray(x);
            var y_gray = GrayCodeConverter.Decimal2Gray(y);

            var result = y_gray * (1 << varsX.Length) + x_gray;
            return result;
        }

        private Point MintermToGridPosition(int minterm)
        {
            var y_gray = minterm >> varsX.Length;
            var x_gray = minterm - (y_gray << varsX.Length);

            var pos = new Point(
                GrayCodeConverter.Gray2Decimal(x_gray),
                GrayCodeConverter.Gray2Decimal(y_gray)
            );
            return pos;
        }
        private void ToggleNumber(int minterm)
        {
            if (mode == Enums.eEditMode.Edit)
            {
                if (zeros.Contains(minterm))
                {
                    if (ones.Contains(minterm))
                    {
                        ones.Remove(minterm);
                        zeros.Remove(minterm);
                    }
                    else
                    {
                        zeros.Remove(minterm);
                        ones.Add(minterm);
                    }
                }
                else
                {
                    if (ones.Contains(minterm))
                    {
                        zeros.Add(minterm);
                    }
                    else
                    {
                        zeros.Add(minterm);
                    }
                }
            }
            else
            {
                if (selectedCells.Contains(minterm))
                {
                    selectedCells.RemoveAll(m => m == minterm);
                }
                else
                {
                    selectedCells.Add(minterm);
                }
            }
        }
        private void SetValue(int minterm, Enums.eCellValue value)
        {
            if (mode == Enums.eEditMode.Edit)
            {
                switch (value)
                {
                    case Enums.eCellValue.Zero:
                        if (!zeros.Contains(minterm))
                        {
                            zeros.Add(minterm);
                        }

                        if (ones.Contains(minterm))
                        {
                            ones.Remove(minterm);
                        }

                        break;
                    case Enums.eCellValue.One:
                        if (zeros.Contains(minterm))
                        {
                            zeros.Remove(minterm);
                        }

                        if (!ones.Contains(minterm))
                        {
                            ones.Add(minterm);
                        }

                        break;
                    case Enums.eCellValue.DontCare:
                        if (!zeros.Contains(minterm))
                        {
                            zeros.Add(minterm);
                        }

                        if (!ones.Contains(minterm))
                        {
                            ones.Add(minterm);
                        }

                        break;
                    case Enums.eCellValue.Undefined:
                        if (zeros.Contains(minterm))
                        {
                            zeros.Remove(minterm);
                        }

                        if (ones.Contains(minterm))
                        {
                            ones.Remove(minterm);
                        }

                        break;
                }
            }
        }
        private async Task<List<int>> CalculateRandomNumbers(int max)
        {
            var random = new Random();
            var list = new List<int>();
            await Task.Run(() =>
            {
                for (int i = 0; i < max; i++)
                {
                    var num = random.Next(max);
                    if (!list.Contains(num))
                    {
                        list.Add(num);
                    }
                }
            }).ConfigureAwait(false);
            return list;
        }
        #endregion
        #region Events
        /// <summary>Raised after the Quine McCluskey algorithm is finished solving the Karnaugh map.</summary>
        public event EventHandler<KarnaughMapSolvedEventArgs> KarnaughMapSolved;
        public event EventHandler<KarnaughLoopAddedEventArgs> KarnaughLoopAdded;
        #endregion
        #region Public methods
        /// <summary>Fill the Karnaugh map randomly.</summary>
        public async Task RandomFill()
        {
            if (mode == Enums.eEditMode.Edit)
            {
                ones = await CalculateRandomNumbers(1 << InputVariables.Count).ConfigureAwait(false);
                zeros = await CalculateRandomNumbers(1 << InputVariables.Count).ConfigureAwait(false);
                Invalidate();
            }
        }
        public Task SolveSelection()
        {
            try
            {
                if (mode == Enums.eEditMode.Solve)
                {
                    SuspendLayout();
                    
                    // Check if there are selected cells
                    if (!selectedCells.Any())
                    {
                        throw new MinificationException("Please select some cells to join.");
                    }

                    var selected_ones = ones.Except(zeros).Intersect(selectedCells);
                    var selected_zeros = zeros.Except(ones).Intersect(selectedCells);
                    var selected_dontcares = zeros.Intersect(ones).Intersect(selectedCells);

                    // Find out the value of the loop
                    bool value;
                    if (selected_ones.Any())
                    {
                        if (selected_zeros.Any())
                        {
                            throw new MinificationException("Selected minterms must have the same value.");
                        }
                        else
                        {
                            value = true;
                        }
                    }
                    else
                    {
                        if (selected_zeros.Any())
                        {
                            value = false;
                        }
                        else
                        {
                            throw new MinificationException("Selected minterms cannot all be don't cares.");
                        }
                    }

                    var result = QuineMcCluskey.QuineMcCluskeySolver.QMC_Solve(value ? selected_ones : selected_zeros, selected_dontcares).ToList();

                    // Check if selection resolves to one loop.
                    if (result.Count != 1)
                    {
                        throw new MinificationException("Selected minterms cannot be simplified.");
                    }

                    if (value)
                    {
                        loops_ones.Add(result.First());
                    }
                    else
                    {
                        loops_zeros.Add(result.First());
                    }

                    selectedCells.Clear();

                    if (KarnaughLoopAdded != null)
                    {
                        KarnaughLoopAdded(this, new KarnaughLoopAddedEventArgs(result.First(), value));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ResumeLayout(true);
                Invalidate();
            }

            return Task.CompletedTask;
        }
        /// <summary>Solve the Karnaugh map using the Quine McCluskey algorithm.</summary>
        public Task SolveAutomatically()
        {
            try
            {
                if (mode == Enums.eEditMode.Solve)
                {
                    SuspendLayout();

                    var dontcares = ones.Intersect(zeros);
                    var solved_loops_ones = QuineMcCluskey.QuineMcCluskeySolver.QMC_Solve(ones, dontcares);
                    var solved_loops_zeros = QuineMcCluskey.QuineMcCluskeySolver.QMC_Solve(zeros, dontcares);

                    loops_ones.Clear();
                    loops_ones.AddRange(solved_loops_ones);

                    loops_zeros.Clear();
                    loops_zeros.AddRange(solved_loops_zeros);

                    if (KarnaughMapSolved != null)
                    {
                        KarnaughMapSolved(this, new KarnaughMapSolvedEventArgs(solved_loops_ones.ToList(), solved_loops_zeros.ToList()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ResumeLayout(true);
                Invalidate();
            }

            return Task.CompletedTask;
        }
        #endregion
        #region Properties

        #region InputVariables
        /// <summary>Names of the input variables.</summary>
        public MintPlayer.ObservableCollection.ObservableCollection<string> InputVariables { get; private set; }
        #endregion
        #region OutputVariable
        private string outputVariable;
        /// <summary>Name of the output variable.</summary>
        public string OutputVariable
        {
            get => outputVariable;
            set
            {
                outputVariable = value;
                Invalidate();
            }
        }
        #endregion
        #region Mode
        private Enums.eEditMode mode;
        /// <summary>Defines whether you want to edit/solve the Karnaugh map.</summary>
        public Enums.eEditMode Mode
        {
            get => mode;
            set
            {
                var args = new ModeChangingEventArgs(mode, value);
                ModeChanging?.Invoke(this, args);

                if (!args.Cancel)
                {
                    mode = value;

                    if (mode == Enums.eEditMode.Edit)
                    {
                        loops_ones.Clear();
                        loops_zeros.Clear();
                        selectedCells.Clear();
                    }

                    Invalidate();
                }
            }
        }
        public event ModeChangingEventHandler ModeChanging;
        #endregion
        #region HasLoops
        public bool HasLoops
        {
            get => loops_ones.Any() & loops_zeros.Any();
        }
        #endregion
        #region SelectedLoop
        private QuineMcCluskey.RequiredLoop selected_loop;
        public QuineMcCluskey.RequiredLoop SelectedLoop
        {
            get => selected_loop;
            set
            {
                selected_loop = value;
                selectedCells = selected_loop == null ? new List<int>() : value.MinTerms.ToList();
                Invalidate();
            }
        }
        #endregion

        #endregion
        #region Event handlers
        private void InputVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var varsYcount = InputVariables.Count >> 1;
            var varsXcount = InputVariables.Count - varsYcount;

            varsY = InputVariables.Take(varsYcount).ToArray();
            varsX = InputVariables.Skip(varsYcount).ToArray();

            rowCount = 1 << varsYcount;
            columnCount = 1 << varsXcount;

            Invalidate();
        }
        private void KarnaughMap_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (--focusedCell.X < 0)
                    {
                        focusedCell.X = columnCount - 1;
                    }
                    break;
                case Keys.Right:
                    if (++focusedCell.X >= columnCount)
                    {
                        focusedCell.X = 0;
                    }
                    break;
                case Keys.Up:
                    if (--focusedCell.Y < 0)
                    {
                        focusedCell.Y = rowCount - 1;
                    }
                    break;
                case Keys.Down:
                    if (++focusedCell.Y >= rowCount)
                    {
                        focusedCell.Y = 0;
                    }
                    break;
                case Keys.Space:
                    ToggleNumber(GridPositionToMinterm(focusedCell));
                    break;


                case Keys.D0:
                case Keys.NumPad0:
                    SetValue(GridPositionToMinterm(focusedCell), Enums.eCellValue.Zero);
                    break;
                case Keys.D1:
                case Keys.NumPad1:
                    SetValue(GridPositionToMinterm(focusedCell), Enums.eCellValue.One);
                    break;
                case Keys.X:
                    SetValue(GridPositionToMinterm(focusedCell), Enums.eCellValue.DontCare);
                    break;
                case Keys.OemMinus:
                    SetValue(GridPositionToMinterm(focusedCell), Enums.eCellValue.Undefined);
                    break;


                default:
                    return;
            }
            Invalidate();
        }
        private void KarnaughMap_Paint(object sender, PaintEventArgs e)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            // Set control size
            Width = (columnCount + 1) * gridSize + 1 + Font.Height;
            Height = (rowCount + 1) * gridSize + 1 + Font.Height;

            // Clear the background
            e.Graphics.Clear(BackColor);

            // Draw the output variable
            e.Graphics.DrawString(OutputVariable, Font, Brushes.Black, 0, 0);

            // Translate the transform for the titles
            e.Graphics.TranslateTransform(Font.Height, Font.Height);

            // Draw the x-axis title
            e.Graphics.DrawString(string.Join(", ", varsX), Font, Brushes.Black, new RectangleF(gridSize, -Font.Height, columnCount * gridSize, Font.Height), sf);

            // Draw the grid
            for (int i = 0; i <= columnCount; i++)
            {
                e.Graphics.DrawLine(System.Drawing.Pens.Black, gridSize * (i + 1), gridSize, gridSize * (i + 1), gridSize * (rowCount + 1));
            }
            for (int j = 0; j <= rowCount; j++)
            {
                e.Graphics.DrawLine(System.Drawing.Pens.Black, gridSize, gridSize * (j + 1), gridSize * (columnCount + 1), gridSize * (j + 1));
            }

            // Draw the diagonal line
            e.Graphics.DrawLine(System.Drawing.Pens.Black, 0, 0, gridSize, gridSize);

            // Draw the y-axis binary codes
            for (int i = 0; i < rowCount; i++)
            {
                var gray = GrayCodeConverter.Decimal2Gray(i);
                e.Graphics.DrawString(Convert.ToString(gray, 2).PadLeft(varsY.Length, '0'), Font, Brushes.Black, new RectangleF(0, (i + 1) * gridSize, gridSize, gridSize), sf);
            }

            // Save the transform, rotate
            var grid_transform = e.Graphics.Transform;
            e.Graphics.RotateTransform(-90);

            // Draw the y-axis title
            e.Graphics.DrawString(string.Join(", ", varsY), base.Font, Brushes.Black, new RectangleF(-Height + Font.Height, -Font.Height, Height - Font.Height - gridSize, Font.Height), sf);

            // Draw the x-axis binary codes
            for (int i = 0; i < columnCount; i++)
            {
                var gray = GrayCodeConverter.Decimal2Gray(i);
                e.Graphics.DrawString(Convert.ToString(gray, 2).PadLeft(varsX.Length, '0'), Font, Brushes.Black, new RectangleF(-gridSize, (i + 1) * gridSize, gridSize, gridSize), sf);
            }

            // Restore the transform
            e.Graphics.Transform = grid_transform;

            // Fill the boxes
            foreach (var minterm in loops_ones.SelectMany(l => l.MinTerms).Distinct())
            {
                Point pos = MintermToGridPosition(minterm);

                var br = zeros.Contains(minterm)
                    ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Olive, Color.Transparent)
                    : Brushes.Olive;
                e.Graphics.FillRectangle(br, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
            }
            foreach (var minterm in loops_zeros.SelectMany(l => l.MinTerms).Distinct())
            {
                var pos = MintermToGridPosition(minterm);

                var br = ones.Contains(minterm)
                    ? new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent50, Color.Transparent, Color.OrangeRed)
                    : Brushes.OrangeRed;
                e.Graphics.FillRectangle(br, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
            }

            // Fill the selected boxes
            if (mode == Enums.eEditMode.Solve)
            {
                foreach (var minterm in selectedCells)
                {
                    var pos = MintermToGridPosition(minterm);
                    e.Graphics.FillRectangle(Brushes.Yellow, (pos.X + 1) * gridSize + 1, (pos.Y + 1) * gridSize + 1, gridSize - 1, gridSize - 1);
                }
            }

            // Draw the box values
            for (int i = 0; i < columnCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    var index = GridPositionToMinterm(i, j);
                    string text;
                    if (ones.Contains(index))
                    {
                        text = zeros.Contains(index) ? "X" : "1";
                    }
                    else
                    {
                        text = zeros.Contains(index) ? "0" : "-";
                    }

                    e.Graphics.DrawString(text, Font, Brushes.Black, new RectangleF((i + 1) * gridSize, (j + 1) * gridSize, gridSize, gridSize), sf);
                }
            }

            // Draw the focused cell
            if (Focused)
            {
                var rct = new Rectangle((focusedCell.X + 1) * gridSize, (focusedCell.Y + 1) * gridSize, gridSize, gridSize);
                rct.Inflate(-2, -2);
                var pen = new Pen(Color.Black, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
                e.Graphics.DrawRectangle(pen, rct);
            }
        }

        private void KarnaughMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.X < gridSize + Font.Height)
            {
                return;
            }
            if (e.Y < gridSize + Font.Height)
            {
                return;
            }

            var x = (e.X - gridSize - Font.Height) / gridSize;
            var y = (e.Y - gridSize - Font.Height) / gridSize;
            var index = GridPositionToMinterm(x, y);

            focusedCell = new Point(x, y);

            ToggleNumber(index);

            Invalidate();
        }
        #endregion

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }
    }
}
