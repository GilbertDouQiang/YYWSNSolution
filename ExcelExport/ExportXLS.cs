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
using NPOI.SS.Util;

namespace ExcelExport
{
    public class ExportXLS
    {
        public int ExportWPFDataGrid(DataGrid SrcDataGrid, String FileName, ObservableCollection<M1> data)
        {
            if (SrcDataGrid == null || SrcDataGrid.Items.Count == 0)
            {
                return -1;
            }

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;            

            ISheet sheet = hssfworkbook.CreateSheet("M1");

            IRow row;
            ICell cell;
            //Header
            int r, c;
            row = sheet.CreateRow(0);
            List<String> bingdingPath = new List<string>();
            for (c = 0; c < SrcDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SrcDataGrid.Columns[c].Header.ToString());
                //row.Cells[c].CellStyle todo 控制Cell的宽度
                row.Cells[c].CellStyle = cellStyle;
                sheet.SetColumnWidth(c,Convert.ToInt32(SrcDataGrid.Columns[c].ActualWidth*40));
                //CellRangeAddress c = CellRangeAddress.ValueOf();
                // TODO: 自动筛选：http://blog.csdn.net/u011981242/article/details/50516328
            }

            for (r = 0; r < data.Count; r++)
            {
                row = sheet.CreateRow(r + 1);

                //用对象循环，而不是用DATAGrid中的数据循环
                for (c = 0; c < SrcDataGrid.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);
                    DataGridTextColumn dgcol = SrcDataGrid.Columns[c] as DataGridTextColumn;
                    Binding binding = (Binding)dgcol.Binding;
                    string path = binding.Path.Path;  //对象属性的名称拿到了
                    PropertyInfo info1= data[r].GetType().GetProperty(path);
                    switch (info1.PropertyType.Name)
                    {
                        case "Int32":
                            Int32 intValue = (Int32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(intValue);
                            break;
                        case "Double":
                            double doubleValue = (double)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(doubleValue);
                            break;
                        default:
                            string cellString = info1.GetValue(data[r], null).ToString();
                            row.CreateCell(c).SetCellValue(cellString);
                            break;
                    }
                    row.Cells[c].CellStyle = cellStyle;
                }
            }

            r = 0;

            FileStream file = null;
            try
            {
                file = new FileStream(FileName, FileMode.Create);
                hssfworkbook.Write(file);
            }
            catch (Exception)
            {
                r = 1;
            }

            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return 0;
        }

        public int ExportWPFDataGridM2(DataGrid SrcDataGrid, String FileName, ObservableCollection<M2> data)
        {
            if (SrcDataGrid == null || SrcDataGrid.Items.Count == 0)
            {
                return -1;
            }

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

            ISheet sheet = hssfworkbook.CreateSheet("Sheet1");

            IRow row;
            ICell cell;
            //Header
            int r, c;
            row = sheet.CreateRow(0);
            List<String> bingdingPath = new List<string>();
            for (c = 0; c < SrcDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SrcDataGrid.Columns[c].Header.ToString());
                //row.Cells[c].CellStyle todo 控制Cell的宽度
                row.Cells[c].CellStyle = cellStyle;
                sheet.SetColumnWidth(c, Convert.ToInt32(SrcDataGrid.Columns[c].ActualWidth * 40));
                //CellRangeAddress c = CellRangeAddress.ValueOf();
                // TODO: 自动筛选：http://blog.csdn.net/u011981242/article/details/50516328
            }

            for (r = 0; r < data.Count; r++)
            {
                row = sheet.CreateRow(r + 1);

                //用对象循环，而不是用DATAGrid中的数据循环
                for (c = 0; c < SrcDataGrid.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);
                    DataGridTextColumn dgcol = SrcDataGrid.Columns[c] as DataGridTextColumn;
                    Binding binding = (Binding)dgcol.Binding;
                    string path = binding.Path.Path;  //对象属性的名称拿到了
                    PropertyInfo info1 = data[r].GetType().GetProperty(path);
                    switch (info1.PropertyType.Name)
                    {
                        case "Int32":
                            Int32 intValue = (Int32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(intValue);
                            break;
                        case "Double":
                            double doubleValue = (double)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(doubleValue);
                            break;

                        default:
                            string cellString = info1.GetValue(data[r], null).ToString();
                            row.CreateCell(c).SetCellValue(cellString);
                            break;
                    }
                    row.Cells[c].CellStyle = cellStyle;
                }
            }

            r = 0;

            FileStream file = null;
            try
            {
                file = new FileStream(FileName, FileMode.Create);
                hssfworkbook.Write(file);
            }
            catch (Exception)
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

        public Int16 ExportWPFDataGridFromM9(DataGrid SrcDataGrid, String FileName, ObservableCollection<M9> data)
        {
            if (SrcDataGrid == null || SrcDataGrid.Items.Count == 0)
            {
                return -1;
            }

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;

            ISheet sheet = hssfworkbook.CreateSheet("M9");

            IRow row;
            ICell cell;

            //Header
            int r, c;
            row = sheet.CreateRow(0);
            List<String> bingdingPath = new List<string>();
            for (c = 0; c < SrcDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SrcDataGrid.Columns[c].Header.ToString());
                //row.Cells[c].CellStyle todo 控制Cell的宽度
                row.Cells[c].CellStyle = cellStyle;
                sheet.SetColumnWidth(c, Convert.ToInt32(SrcDataGrid.Columns[c].ActualWidth * 40));
                //CellRangeAddress c = CellRangeAddress.ValueOf();
                // TODO: 自动筛选：http://blog.csdn.net/u011981242/article/details/50516328
            }

            for (r = 0; r < data.Count; r++)
            {
                row = sheet.CreateRow(r + 1);

                //用对象循环，而不是用DATAGrid中的数据循环
                for (c = 0; c < SrcDataGrid.Columns.Count; c++)
                {
                    cell = row.CreateCell(c);
                    DataGridTextColumn dgcol = SrcDataGrid.Columns[c] as DataGridTextColumn;
                    Binding binding = (Binding)dgcol.Binding;
                    string path = binding.Path.Path;  //对象属性的名称拿到了
                    PropertyInfo info1 = data[r].GetType().GetProperty(path);
                    switch (info1.PropertyType.Name)
                    {
                        case "Int32":
                            Int32 intValue = (Int32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(intValue);
                            break;
                        case "Double":
                            double doubleValue = (double)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(doubleValue);
                            break;
                        default:
                            string cellString = info1.GetValue(data[r], null).ToString();
                            row.CreateCell(c).SetCellValue(cellString);
                            break;
                    }
                    row.Cells[c].CellStyle = cellStyle;
                }
            }

            r = 0;

            FileStream file = null;
            try
            {
                file = new FileStream(FileName, FileMode.Create);
                hssfworkbook.Write(file);
            }
            catch (Exception)
            {
                r = 1;
            }

            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return 0;
        }
    }
}
