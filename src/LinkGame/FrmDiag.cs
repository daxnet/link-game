using LinkGame.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkGame
{
    public partial class FrmDiag : Form
    {

        #region Private Fields

        private Map _map;

        #endregion Private Fields

        #region Public Constructors

        public FrmDiag()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Internal Properties

        internal Map Map
        {
            get => _map;
            set
            {
                _map = value;
                ShowDiagnosticInformation();
            }
        }

        #endregion Internal Properties

        #region Public Methods

        public void ShowDiagnosticInformation()
        {
            if (_map != null && Visible)
            {
                var resolutionPaths = _map.GetPossibleResolutions();
                lst.Items.Clear();
                foreach (var resolutionPath in resolutionPaths)
                {
                    lst.Items.Add(resolutionPath.ToString());
                }
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            ShowDiagnosticInformation();
        }

        #endregion Protected Methods

    }
}
