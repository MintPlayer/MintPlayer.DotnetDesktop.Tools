using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Actions;
using System.ComponentModel;

namespace MintPlayer.KarnaughMap;

public sealed class KarnaughMapDesigner : ControlDesigner
{
    private bool _actionListAdded;

    protected override bool GetHitTest(Point screenCoordinates)
    {
        if (Control is KarnaughMap karnaughMap)
        {
            var offset = karnaughMap.Font.Height + KarnaughMap.gridSize;
            var clientPoint = karnaughMap.PointToClient(screenCoordinates);
            if (clientPoint.Y >= offset && clientPoint.X >= offset)
                return true;
        }
        return base.GetHitTest(screenCoordinates);
    }

    public override DesignerActionListCollection ActionLists
    {
        get
        {
            var lists = base.ActionLists;
            if (!_actionListAdded)
            {
                lists.Add(new KarnaughMapDesignerActionList(this));
                _actionListAdded = true;
            }
            return lists;
        }
    }

    public override SelectionRules SelectionRules => SelectionRules.Moveable;

    private sealed class KarnaughMapDesignerActionList : DesignerActionList
    {
        private readonly KarnaughMapDesigner _designer;
        private readonly KarnaughMap _control;
        public KarnaughMapDesignerActionList(KarnaughMapDesigner designer)
            : base(designer.Component)
        {
            _designer = designer;
            _control = (KarnaughMap)designer.Control;
        }

        public void EditInputVariables()
        {
            var pd = TypeDescriptor.GetProperties(_control)[nameof(KarnaughMap.InputVariables)];
            if (pd == null) return;

            if (pd.GetValue(_control) is ObservableCollection.ObservableCollection<string> observableCollection)
            {
                var currentArray = observableCollection.ToArray() ?? [];
                using var dlg = new InputVariablesEditorForm(currentArray);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var newValues = dlg.GetValues();
                    if (!currentArray.SequenceEqual(newValues))
                    {
                        _designer.RaiseComponentChanging(pd);
                        //pd.SetValue(_control, newValues);
                        observableCollection.Enabled = false;
                        observableCollection.Clear();
                        observableCollection.Enabled = true;
                        observableCollection.AddRange(newValues);

                        _designer.RaiseComponentChanged(pd, observableCollection, newValues);
                        TypeDescriptor.Refresh(_control);
                    }
                }
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection
            {
                new DesignerActionHeaderItem("Configuration"),
                new DesignerActionMethodItem(this,
                    nameof(EditInputVariables),
                    "Edit Input Variables",
                    "Configuration",
                    "Edit the Karnaugh map input variable names.",
                    true)
            };
            return items;
        }
    }

    private sealed class InputVariablesEditorForm : Form
    {
        private readonly ListBox _list;
        private readonly TextBox _txt;
        private readonly Button _btnAdd;
        private readonly Button _btnRemove;
        private readonly Button _btnRename;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public InputVariablesEditorForm(string[] existing)
        {
            Text = "Edit Input Variables";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(320, 300);

            _list = new ListBox { Left = 10, Top = 10, Width = 200, Height = 240 };
            _list.Items.AddRange(existing);

            _txt = new TextBox { Left = 10, Top = 260, Width = 200 };

            _btnAdd = new Button { Left = 220, Top = 10, Width = 90, Text = "Add" };
            _btnRemove = new Button { Left = 220, Top = 45, Width = 90, Text = "Remove" };
            _btnRename = new Button { Left = 220, Top = 80, Width = 90, Text = "Rename" };
            _btnOk = new Button { Left = 220, Top = 215, Width = 90, Text = "OK", DialogResult = DialogResult.OK };
            _btnCancel = new Button { Left = 220, Top = 250, Width = 90, Text = "Cancel", DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { _list, _txt, _btnAdd, _btnRemove, _btnRename, _btnOk, _btnCancel });

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            _btnAdd.Click += (_, _) =>
            {
                var name = _txt.Text.Trim();
                if (name.Length == 0) return;
                if (!_list.Items.Contains(name))
                {
                    _list.Items.Add(name);
                    _txt.Clear();
                }
                else
                {
                    MessageBox.Show("Variable already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            _btnRemove.Click += (_, _) =>
            {
                if (_list.SelectedItem is string sel)
                    _list.Items.Remove(sel);
            };

            _btnRename.Click += (_, _) =>
            {
                if (_list.SelectedItem is string sel)
                {
                    var name = _txt.Text.Trim();
                    if (name.Length == 0) return;
                    if (_list.Items.Contains(name))
                    {
                        MessageBox.Show("Variable already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    var idx = _list.Items.IndexOf(sel);
                    _list.Items[idx] = name;
                    _txt.Clear();
                }
            };

            _list.SelectedIndexChanged += (_, _) =>
            {
                if (_list.SelectedItem is string sel)
                    _txt.Text = sel;
            };
        }

        public string[] GetValues() => _list.Items.Cast<string>().ToArray();
    }
}
