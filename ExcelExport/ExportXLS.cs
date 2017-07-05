using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using System.Reflection;
using System.Collections.ObjectModel;
using YyWsnDeviceLibrary;

namespace ExcelExport
{
    public class ExportXLS
    {
        public int ExportWPFDataGrid(DataGrid SourceDataGrid, String FileName, ObservableCollection<M1> data)
        {
            /*
            if (SourceDataGrid == null || (SourceDataGrid.Rows.Count == 0 && SourceDataGrid.Columns.Count == 0))
                return 1;
                */
            if (SourceDataGrid == null || SourceDataGrid.Items.Count == 0)
                return -1;

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            ISheet sheet = hssfworkbook.CreateSheet("Sheet1");
            hssfworkbook.CreateSheet("Sheet2");
            hssfworkbook.CreateSheet("Sheet3");

            IRow row;
            ICell cell;
            //Header
            int r, c;
            row = sheet.CreateRow(1);
            List<String> bingdingPath = new List<string>();
            for (c = 0; c < SourceDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SourceDataGrid.Columns[c].Header.ToString());
               

            }


            for (r = 0; r < data.Count; r++)
            {

                //DataGridRow rowContainer = SourceDataGrid.GetRow(r);


                // DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                //DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex

                row = sheet.CreateRow(r + 2);

                //用对象循环，而不是用DATAGrid中的数据循环
                for (c = 0; c < SourceDataGrid.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);
                    DataGridTextColumn dgcol = SourceDataGrid.Columns[c] as DataGridTextColumn;
                    Binding binding = (Binding)dgcol.Binding;
                    string path = binding.Path.Path;  //对象属性的名称拿到了
                    PropertyInfo info1= data[r].GetType().GetProperty(path);
                    string cellString= info1.GetValue(data[r],null).ToString();
                    row.CreateCell(c).SetCellValue(cellString);
                 


                }


            }

            r = 0;


            FileStream file = null;
            try
            {
                file = new FileStream(FileName, FileMode.Create);
                hssfworkbook.Write(file);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                r = 1;
            }
            finally
            {
                if (file != null)
                    file.Close();
            }



            return 0;


        }
    }
}
