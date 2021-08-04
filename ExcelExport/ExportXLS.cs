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

            // 字体
            IFont font = hssfworkbook.CreateFont();
            font.FontName = "Courier New";
            font.FontHeightInPoints = 10;

            // 单元格边框
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.SetFont(font);

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

                double GridColumnActualWidth = SrcDataGrid.Columns[c].ActualWidth;
                double ExcelColumnActualWidth = GridColumnActualWidth * 40.0f;

                Int32 ExcelColumnActualWidth_i = Convert.ToInt32(ExcelColumnActualWidth);

                if(ExcelColumnActualWidth_i > 30000)
                {
                    sheet.SetColumnWidth(c, 30000);
                }
                else
                {
                    sheet.SetColumnWidth(c, ExcelColumnActualWidth_i);
                }

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
                    if (info1 == null)
                    {
                        continue;
                    }

                    switch (info1.PropertyType.Name)
                    {
                        case "Byte":
                            byte byteValue = (byte)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(byteValue);
                            break;
                        case "UInt16":
                            UInt16 UInt16Value = (UInt16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt16Value);
                            break;
                        case "Int16":
                            Int16 Int16Value = (Int16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(Int16Value);
                            break;
                        case "UInt32":
                            UInt32 UInt32Value = (UInt32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt32Value);
                            break;
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

        /// <summary>
        /// 带有备注
        /// </summary>
        /// <param name="SrcDataGrid"></param>
        /// <param name="FileName"></param>
        /// <param name="data"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public int ExportWPFDataGrid(DataGrid SrcDataGrid, String FileName, ObservableCollection<M1> data, string comment)
        {
            if (SrcDataGrid == null || SrcDataGrid.Items.Count == 0)
            {
                return -1;
            }

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();

            // 字体
            IFont font = hssfworkbook.CreateFont();
            font.FontName = "Courier New";
            font.FontHeightInPoints = 10;

            // 单元格边框
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.SetFont(font);

            // 备注
            ISheet sheetOfComment = hssfworkbook.CreateSheet("备注");

            IRow row;
            ICell cell;
            int r = 0;
            int c = 0;

            string[] RowText = comment.Split('\n');

            for (r = 0; r < RowText.Length; r++)
            {
                c = 0;

                row = sheetOfComment.CreateRow(r);
                cell = row.CreateCell(c);

                row.CreateCell(c).SetCellValue(RowText[r]);
                row.Cells[c].CellStyle = cellStyle;
            }        

            // 数据
            ISheet sheet = hssfworkbook.CreateSheet("数据");

            row = sheet.CreateRow(0);
            List<String> bingdingPath = new List<string>();
            for (c = 0; c < SrcDataGrid.Columns.Count; c++)
            {
                row.CreateCell(c).SetCellValue(SrcDataGrid.Columns[c].Header.ToString());
                row.Cells[c].CellStyle = cellStyle;

                double GridColumnActualWidth = SrcDataGrid.Columns[c].ActualWidth;
                double ExcelColumnActualWidth = GridColumnActualWidth * 40.0f;

                Int32 ExcelColumnActualWidth_i = Convert.ToInt32(ExcelColumnActualWidth);

                if (ExcelColumnActualWidth_i > 30000)
                {
                    sheet.SetColumnWidth(c, 30000);
                }
                else
                {
                    sheet.SetColumnWidth(c, ExcelColumnActualWidth_i);
                }

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
                    if (info1 == null)
                    {
                        continue;
                    }

                    switch (info1.PropertyType.Name)
                    {
                        case "Byte":
                            byte byteValue = (byte)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(byteValue);
                            break;
                        case "UInt16":
                            UInt16 UInt16Value = (UInt16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt16Value);
                            break;
                        case "Int16":
                            Int16 Int16Value = (Int16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(Int16Value);
                            break;
                        case "UInt32":
                            UInt32 UInt32Value = (UInt32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt32Value);
                            break;
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

            // 字体
            IFont font = hssfworkbook.CreateFont();
            font.FontName = "Courier New";
            font.FontHeightInPoints = 10;

            // 单元格边框
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.SetFont(font);

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
                    if (info1 == null)
                    {
                        continue;
                    }

                    switch (info1.PropertyType.Name)
                    {
                        case "Byte":
                            byte byteValue = (byte)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(byteValue);
                            break;
                        case "UInt16":
                            UInt16 UInt16Value = (UInt16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt16Value);
                            break;
                        case "Int16":
                            Int16 Int16Value = (Int16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(Int16Value);
                            break;
                        case "UInt32":
                            UInt32 UInt32Value = (UInt32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt32Value);
                            break;
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

            // 字体
            IFont font = hssfworkbook.CreateFont();
            font.FontName = "Courier New";
            font.FontHeightInPoints = 10;

            // 单元格边框
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.SetFont(font);

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
                    if (info1 == null)
                    {
                        continue;
                    }

                    switch (info1.PropertyType.Name)
                    {
                        case "Byte":
                            byte byteValue = (byte)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(byteValue);
                            break;
                        case "UInt16":
                            UInt16 UInt16Value = (UInt16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt16Value);
                            break;
                        case "Int16":
                            Int16 Int16Value = (Int16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(Int16Value);
                            break;
                        case "UInt32":
                            UInt32 UInt32Value = (UInt32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt32Value);
                            break;
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

        public Int16 ExportWPFDataGridFromAdhoc(DataGrid SrcDataGrid, String FileName, ObservableCollection<WP> data)
        {
            if (SrcDataGrid == null || SrcDataGrid.Items.Count == 0)
            {
                return -1;
            }

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();

            // 字体
            IFont font = hssfworkbook.CreateFont();
            font.FontName = "Courier New";
            font.FontHeightInPoints = 10;

            // 单元格边框
            ICellStyle cellStyle = hssfworkbook.CreateCellStyle();
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            cellStyle.SetFont(font);

            ISheet sheet = hssfworkbook.CreateSheet("ADHOC");            

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

                double GridColumnActualWidth = SrcDataGrid.Columns[c].ActualWidth;
                double ExcelColumnActualWidth = GridColumnActualWidth * 40.0f;

                Int32 ExcelColumnActualWidth_i = Convert.ToInt32(ExcelColumnActualWidth);

                if (ExcelColumnActualWidth_i > 30000)
                {
                    sheet.SetColumnWidth(c, 30000);
                }
                else
                {
                    sheet.SetColumnWidth(c, ExcelColumnActualWidth_i);
                }

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
                    row.Cells[c].CellStyle = cellStyle;
                    DataGridTextColumn dgcol = SrcDataGrid.Columns[c] as DataGridTextColumn;
                    Binding binding = (Binding)dgcol.Binding;
                    if (binding == null)
                    {
                        continue;
                    }

                    string path = binding.Path.Path;  //对象属性的名称拿到了
                    PropertyInfo info1 = data[r].GetType().GetProperty(path);
                    if (info1 == null)
                    {
                        continue;
                    }

                    switch (info1.PropertyType.Name)
                    {
                        case "Byte":
                            byte byteValue = (byte)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(byteValue);
                            break;
                        case "UInt16":
                            UInt16 UInt16Value = (UInt16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt16Value);
                            break;
                        case "Int16":
                            Int16 Int16Value = (Int16)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(Int16Value);
                            break;
                        case "UInt32":
                            UInt32 UInt32Value = (UInt32)info1.GetValue(data[r], null);
                            row.CreateCell(c).SetCellValue(UInt32Value);
                            break;
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
