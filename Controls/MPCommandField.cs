using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MPN.Framework.Controls
{
    /// <summary>
    /// An extended <see cref="CommandField"/> that allows deletions
    /// to be confirmed by the user.
    /// </summary>
    public class MpCommandField : CommandField
    {
        /// <summary>
        /// Initialise the cell.
        /// </summary>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            if (string.IsNullOrEmpty(DeleteConfirmationText) || !ShowDeleteButton) return;
            foreach (var control in from Control control in cell.Controls
                                        let button = control as IButtonControl
                                        where button != null && button.CommandName == "Delete"
                                        select control)
            {
                ((WebControl)control).Attributes.Add("onclick", string.Format("if (!confirm('{0}')) return false;", DeleteConfirmationText));
            }
        }

        #region Properties

        #region DeleteConfirmationText
        /// <summary>
        /// Delete confirmation text.
        /// </summary>
        [Category("Behavior")]
        [Description("The text shown to the user to confirm the deletion.")]
        public string DeleteConfirmationText
        {
            get { return ViewState["DeleteConfirmationText"] as string; }
            set { ViewState["DeleteConfirmationText"] = value; }
        }
        #endregion

        #endregion
    }
}