using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using System.IO;

namespace ExcelExport
{
    public class ExportXLS
    {
        public int ExportWPFDataGrid(DataGrid SourceDataGrid,String FileName)
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
            for (c = 0; c < SourceDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SourceDataGrid.Columns[c].Header.ToString());
            }

            /*
            for (r = 0; r < SourceDataGrid.Items.Count; r++)
            {
                row = sheet.CreateRow(r + 2);
                for (c = 0; c < SourceDataGrid.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);

                    if (SourceDataGrid.Items[r].c.Value != null)
                        cell.SetCellValue(dgv[c, r].Value.ToString());
                }
            }
            r = 0;
            */

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
