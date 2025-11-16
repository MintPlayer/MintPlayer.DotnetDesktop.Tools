using MintPlayer.QuineMcCluskey.Abstractions;
using MintPlayer.QuineMcCluskey;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MintPlayer.KarnaughMap.Enums;
using MintPlayer.KarnaughMap.Events.EventArgs;

namespace KarnaughMap.Test
{
    public partial class Form1 : Form
    {
        private readonly IQuineMcCluskeySolver solver = new QuineMcCluskeySolver();
        public Form1()
        {
            InitializeComponent();
            karnaughMap1.Solver = solver; // inject solver instance
        }

        private async void BtnRandomFill_Click(object sender, System.EventArgs e)
        {
            await karnaughMap1.RandomFill();
        }

        private async void BtnSolve_Click(object sender, System.EventArgs e)
        {
            await karnaughMap1.SolveAutomatically();
        }

        private async void BtnSolveSelection_Click(object sender, System.EventArgs e)
        {
            await karnaughMap1.SolveSelection();
        }

        private List<IRequiredLoop> loopsOnes;
        private List<IRequiredLoop> loopsZeros;

        private void KarnaughMap1_KarnaughLoopAdded(object sender, KarnaughLoopAddedEventArgs e)
        {
            if (loopsOnes == null)
            {
                loopsOnes = new List<IRequiredLoop>();
            }

            if (loopsZeros == null)
            {
                loopsZeros = new List<IRequiredLoop>();
            }

            if (e.Value)
            {
                loopsOnes.Add(e.Loop);
                lstLoopOnes.Items.Add(e.Loop.ToString(karnaughMap1.InputVariables.ToArray()));
            }
            else
            {
                loopsZeros.Add(e.Loop);
                lstLoopZeros.Items.Add(e.Loop.ToString(karnaughMap1.InputVariables.ToArray()));
            }
        }
        private void KarnaughMap1_KarnaughMapSolved(object sender, KarnaughMapSolvedEventArgs e)
        {
            loopsOnes = e.LoopsOnes;
            loopsZeros = e.LoopsZeros;

            lstLoopOnes.Items.Clear();
            lstLoopOnes.Items.AddRange(e.LoopsOnes.Select(l => l.ToString(karnaughMap1.InputVariables.ToArray())).ToArray());
            lstLoopZeros.Items.Clear();
            lstLoopZeros.Items.AddRange(e.LoopsZeros.Select(l => l.ToString(karnaughMap1.InputVariables.ToArray())).ToArray());
        }

        private bool ignoreSelectedModeChanging;
        private void cmbMode_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!ignoreSelectedModeChanging)
            {
                karnaughMap1.Mode = (EEditMode)cmbMode.SelectedIndex;
                btnRandomFill.Enabled = cmbMode.SelectedIndex == 0;
                btnSolve.Enabled = cmbMode.SelectedIndex == 1;
                lstLoopOnes.Items.Clear();
                lstLoopZeros.Items.Clear();
            }
        }

        private void KarnaughMap1_ModeChanging(object sender, ModeChangingEventArgs e)
        {
            if (karnaughMap1.HasLoops)
            {
                if (e.NewValue == EEditMode.Edit)
                {
                    if (MessageBox.Show("This will remove all loops. Are you sure?", "Edit mode", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        ignoreSelectedModeChanging = true;
                        cmbMode.SelectedIndex = (int)e.OldValue;
                        ignoreSelectedModeChanging = false;
                    }
                }
            }
        }

        private void LstLoopZeros_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            karnaughMap1.SelectedLoop = loopsZeros[lstLoopZeros.SelectedIndex];
        }

        private void LstLoopOnes_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            karnaughMap1.SelectedLoop = lstLoopOnes.SelectedIndex == -1 ? null : loopsOnes[lstLoopOnes.SelectedIndex];
        }
    }
}
