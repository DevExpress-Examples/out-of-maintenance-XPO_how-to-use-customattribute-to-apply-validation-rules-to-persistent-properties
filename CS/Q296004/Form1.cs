using System;
using DXSample;
using DevExpress.Xpo;
using DevExpress.XtraGrid;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Controls;

namespace Q296004 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }

        private void gridView1_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e) {
            GridView view = (GridView)sender;
            XPBaseObject row = (XPBaseObject)view.GetFocusedRow();
            Dictionary<string, object> customValues = new Dictionary<string, object>();
            customValues.Add(view.FocusedColumn.FieldName, e.Value);
            e.ErrorText = new Validator(row, customValues).Validate(view.FocusedColumn.FieldName);
            e.Valid = string.IsNullOrEmpty(e.ErrorText);
        }

        private void gridView1_ValidateRow(object sender, ValidateRowEventArgs e) {
            if (e.RowHandle != GridControl.NewItemRowHandle) return;
            Dictionary<string, object> customValues = new Dictionary<string, object>();
            GridView view = (GridView)sender;
            foreach (GridColumn col in view.Columns)
                customValues.Add(col.FieldName, view.GetRowCellValue(e.RowHandle, col));
            Validator validator = new Validator((XPBaseObject)e.Row, customValues);
            foreach (GridColumn col in ((GridView)sender).Columns) {
                e.ErrorText = validator.Validate(col.FieldName);
                if (!string.IsNullOrEmpty(e.ErrorText)) {
                    e.Valid = false;
                    return;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            unitOfWork1.CommitChanges();
        }
    }
}