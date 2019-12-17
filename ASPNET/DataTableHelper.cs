using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;//Need reference to System.Web
using System.Web.UI.WebControls;
using System.Collections;//Need reference to System.Web

namespace Common
{   
    /// <summary>
    /// A Collection of common DataTable related methods.
    /// <para>Be careful when modifying this class since it might be used in multiple locations.</para>
    /// </summary>
    public static class DataTableHelper
    {
        /// <summary>
        /// Get the column index of a column from the DataTable. Up to three compare strings can be used.
        /// <para>The compare strings are used in sequence. Ex: In order to use the 3rd string the first two cannot have a length of zero.</para>
        /// <para>Returns the first result that is found or -1 if the column is not found.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="strCompare1"></param>
        /// <param name="strCompare2"></param>
        /// <param name="strCompare3"></param>
        /// <param name="listStringsToIgnore">If a column name contains any of these strings then it will be ignored.</param>
        /// <returns></returns>
        public static Int32 GetIndexOfColumnFromDataTable(DataTable dtbl, String strCompare1, String strCompare2, String strCompare3, List<string> listStringsToIgnore)
        {
            Int32 result = -1;

            if (listStringsToIgnore == null)
                listStringsToIgnore = new List<string>();

            foreach (DataColumn dc in dtbl.Columns)
            {
                string colName = dc.ColumnName.ToString().ToUpper();

                bool ignoreThisColumn = false;
                foreach (string strToIgnore in listStringsToIgnore)
                {
                    if (colName.Contains(strToIgnore.ToUpper()))
                    {
                        ignoreThisColumn = true;
                        break;
                    }
                }

                if (ignoreThisColumn)
                    continue;

                if (colName.Contains(strCompare1.ToUpper()))
                {
                    if (strCompare2.Length > 0)
                    {
                        if (colName.Contains(strCompare2.ToUpper()))
                        {
                            if (strCompare3.Length > 0)
                            {
                                if (colName.Contains(strCompare3.ToUpper()))
                                {
                                    result = dc.Ordinal;
                                    break;//Return first result found.
                                }
                            }
                            else
                            {
                                result = dc.Ordinal;
                                break;//Return first result found.
                            }
                        }
                    }
                    else
                    {
                        result = dc.Ordinal;
                        break;//Return first result found.
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get the column indexes of column(s) that match the search strings from the DataTable. Up to three compare strings can be used.
        /// <para>The compare strings are used in sequence. Ex: In order to use the 3rd string the first two cannot have a length of zero.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="strCompare1"></param>
        /// <param name="strCompare2"></param>
        /// <param name="strCompare3"></param>
        /// <param name="listStringsToIgnore">If a column name contains any of these strings then it will be ignored.</param>
        /// <returns></returns>
        public static List<Int32> GetIndexesOfColumnFromDataTable(DataTable dtbl, String strCompare1, String strCompare2, String strCompare3, List<string> listStringsToIgnore)
        {
            List<Int32> listResult = new List<Int32>();

            if (listStringsToIgnore == null)
                listStringsToIgnore = new List<string>();

            foreach (DataColumn dc in dtbl.Columns)
            {
                string colName = dc.ColumnName.ToString().ToUpper();

                bool ignoreThisColumn = false;
                foreach (string strToIgnore in listStringsToIgnore)
                {
                    if (colName.Contains(strToIgnore.ToUpper()))
                    {
                        ignoreThisColumn = true;
                        break;
                    }
                }

                if (ignoreThisColumn)
                    continue;

                if (colName.Contains(strCompare1.ToUpper()))
                {
                    if (strCompare2.Length > 0)
                    {
                        if (colName.Contains(strCompare2.ToUpper()))
                        {
                            if (strCompare3.Length > 0)
                            {
                                if (colName.Contains(strCompare3.ToUpper()))
                                {
                                    listResult.Add(dc.Ordinal);
                                }
                            }
                            else
                            {
                                listResult.Add(dc.Ordinal);
                            }
                        }
                    }
                    else
                    {
                        listResult.Add(dc.Ordinal);
                    }
                }
            }

            return listResult;
        }

        /// <summary>
        /// Attemps to find one object value from a DataTable. 
        /// <para>You must know what type to convert it to after retrieval.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="SearchCol">Column of unique identifiers.</param>
        /// <param name="SearchCrit">The search criteria for the search column.</param>
        /// <param name="ValueCol">The column containing the desired value(s).</param>
        /// <returns></returns>
        public static Object GetObjectFromDataTable(DataTable dtbl, String SearchCol, String SearchCrit, String ValueCol)
        {
            Object result = null;
            DataRow[] drows;

            try
            {
                SearchCol = SearchCol.Replace("[", "").Replace("]", "");

                if (dtbl.Columns.Contains(SearchCol))
                {
                    String strFilter = dtbl.SafeGetFilterPart(SearchCol, "=", SearchCrit);

                    drows = dtbl.Select(strFilter, "", DataViewRowState.CurrentRows);
                    if (drows.Length > 0)
                    {
                        result = drows[0][ValueCol];
                    }
                }
                else
                {
                    throw new Exception("Column(" + SearchCol + ") does not exist in DataTable(" + dtbl.TableName + ").");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Attemps to find one or more object values from a DataTable. 
        /// <para>You must know what type to convert it to after retrieval.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="SearchCol">Column of unique identifiers.</param>
        /// <param name="SearchCrit">The search criteria for the search column.</param>
        /// <param name="ValueCol">The column containing the desired value(s).</param>
        /// <returns></returns>
        public static Object[] GetObjectsFromDataTable(DataTable dtbl, String SearchCol, String SearchCrit, String ValueCol)
        {
            Object[] results = null;
            DataRow[] drows;

            try
            {
                SearchCol = SearchCol.Replace("[", "").Replace("]", "");

                if (dtbl.Columns.Contains(SearchCol))
                {
                    String strFilter = dtbl.SafeGetFilterPart(SearchCol, "=", SearchCrit);

                    drows = dtbl.Select(strFilter, "", DataViewRowState.CurrentRows);
                    if (drows.Length > 0)
                    {
                        results = new Object[drows.Length];

                        for (Int32 i = 0; i < drows.Length; i++)
                        {
                            results[i] = drows[i][ValueCol];
                        }
                    }
                }
                else
                {
                    throw new Exception("Column(" + SearchCol + ") does not exist in DataTable(" + dtbl.TableName + ").");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
            }

            return results;
        }

        /// <summary>
        /// Exports the DataTable to a csv file.
        /// <para>Creates the directory strFilePath if it does not exist.</para>
        /// <para>Checks strFileName for the .csv extension and adds it if it is missing.</para>
        /// <para>Handles fields with leading zeros and fields containing commas.</para>
        /// <para>Returns True on success, False on failure. A log is generated on failure.</para>
        /// <para>Modified from http://dotnetguts.blogspot.com/2007/01/exporting-datatable-to-csv-file-format.html </para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="strDestinationDirectory"></param>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static Boolean ExportDataTableToCSV(DataTable dtbl, String strDestinationDirectory, String strFileName)
        {
            Boolean success = false;

            //Make sure directy exists, do not clear it if it does exist.
            if (MiscUtility.CreateDirectory(strDestinationDirectory, false))
            {
                // Add the .csv extension if necessary.
                strFileName = strFileName.Contains(".csv") ? strFileName : strFileName + ".csv";

                // Create the StreamWriter
                StreamWriter sw = new StreamWriter(Path.Combine(strDestinationDirectory, strFileName), false);

                try
                {
                    String strColDelim = ",";
                    String strCellDelim = @"""";//same as "/""
                    String strEscapedCellDelim = @"""""";

                    // Write the headers.
                    Int32 iColCount = dtbl.Columns.Count;
                    for (Int32 i = 0; i < iColCount; i++)
                    {
                        //Double quoting all of the column headers in case they have spaces and because excel
                        //does not like to have the first word in a .csv file to be ID (in some tables the first column is ID)
                        //Note that if you use dtbl.Columns[i] and a column has an expression then the expression will also be displayed so use the ColumName property.
                        sw.Write(strCellDelim + dtbl.Columns[i].ColumnName + strCellDelim);
                        if (i < iColCount - 1) sw.Write(strColDelim);
                    }
                    sw.Write(sw.NewLine);

                    // Write rows.
                    foreach (DataRow dr in dtbl.Rows)
                    {
                        for (Int32 i = 0; i < iColCount; i++)
                        {
                            String value = "";

                            if (!Convert.IsDBNull(dr[i]))
                            {
                                value = SafeConvert.SafeString(dr[i]);

                                if (value.Contains(strCellDelim))
                                {
                                    value = value.ToString().Replace(strCellDelim, strEscapedCellDelim);//Replace the cell delimiter with its escaped variant.
                                }
                                value = (strCellDelim + value + strCellDelim);//double quote everything!!!
                            }
                            else
                            {
                                value = (strCellDelim + strCellDelim);//double quote everything!!!
                            }

                            sw.Write(value);

                            if (i < iColCount - 1) sw.Write(strColDelim);
                        }
                        sw.Write(sw.NewLine);
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
                }

                sw.Close();
            }
            else
            {
                LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), "MiscUtility.CreateDirectory failed for some reason.");
            }

            return success;
        }

        /// <summary>
        /// Loop through a list of DataTables and export them all to csv files.
        /// <para>Note that this function deletes all csv files in strDestinationDirectory prior to exporting unless onlyDeleteConflictingCSV is true.</para>
        /// </summary>
        /// <param name="listDtbls"></param>
        /// <param name="strDestinationDirectory"></param>
        public static Boolean ExportDataTablesToCSV(List<DataTable> listDtbls, String strDestinationDirectory)
        {
            return ExportDataTablesToCSV(listDtbls, strDestinationDirectory, false);
        }

        /// <summary>
        /// Loop through a list of DataTables and export them all to csv files.
        /// <para>Note that this function deletes all csv files in strDestinationDirectory prior to exporting unless onlyDeleteConflictingCSV is true.<para>
        /// <para>If onlyDeleteConflictingCSV is true then only the csv files with conflicting names will be deleted.</para>
        /// </summary>
        /// <param name="listDtbls"></param>
        /// <param name="strDestinationDirectory"></param>
        /// <param name="onlyDeleteConflictingCSV">false to delete every CSV</param>
        public static Boolean ExportDataTablesToCSV(List<DataTable> listDtbls, String strDestinationDirectory, bool onlyDeleteConflictingCSV)
        {
            bool success = true;
            int count = 0;

            //Make sure directy exists, do not clear it if it does exist.
            if (MiscUtility.CreateDirectory(strDestinationDirectory, false))
            {
                //Delete all csv files in the folder
                string[] fileList = System.IO.Directory.GetFiles(strDestinationDirectory, "*.csv", SearchOption.TopDirectoryOnly);
                foreach (string strFn in fileList)
                {
                    bool deleteThisFile = true;

                    if (onlyDeleteConflictingCSV)
                    {
                        deleteThisFile = false;
                        foreach (DataTable dtbl in listDtbls)
                        {
                            if (strFn.Contains(dtbl.TableName))
                            {
                                deleteThisFile = true;
                                break;
                            }
                        }
                    }

                    if (deleteThisFile)
                    {
                        try
                        {
                            //prevent things like "c:\"
                            if (strFn.Length > 5)
                            {
                                File.Delete(strFn);
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
                        }
                    }
                }

                if (count > 0)
                    LogHelper.AddLog("", "", MethodInfo.GetCurrentMethod().Name + ": " + count + " CSV files deleted.");

                //Export each dataTable to csv file. If any of the exports fail, set success to false.
                foreach (DataTable dtbl in listDtbls)
                {
                    if (!ExportDataTableToCSV(dtbl, strDestinationDirectory, dtbl.TableName))
                        success = false;
                }
            }
            else
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/1104121/how-to-convert-a-datatable-to-a-string-in-c
        /// </summary>
        public class DataTableStringifier
        {
            public bool IsOuterBordersPresent { get; set; } //Whether outer borders of table needed
            public bool IsHeaderHorizontalSeparatorPresent { get; set; } // Whether horizontal line separator between table title and data is needed. Useful to set 'false' if you expect only 1 or 2 rows of data - no need for additional lines then
            public char ValueSeparator { get; set; } //Vertical line character
            public char HorizontalLinePadChar { get; set; } // Horizontal line character
            public char HorizontalLineSeparator { get; set; } // Horizontal border (between header and data) column separator (crossing of horizontal and vertical borders)
            public int ValueMargin { get; set; } // Horizontal margin from table borders (inner and outer) to cell values
            public int MaxColumnWidth { get; set; } // To avoid too wide columns with thousands of characters. Longer values will be cropped in the center
            public string LongValuesEllipses { get; set; } // Cropped values wil be inserted this string in the middle to mark the point of cropping

            //public DataTableStringifier(int maxColumnWidth = int.MaxValue, bool isHeaderHorizontalSeparatorPresent = true, char valueSeparator = '|', int valueMargin = 1,
            //    char horizontalLinePadChar = '-', char horizontalLineSeparator = '+', string longValuesEllipses = "...", bool isOuterBordersPresent = false)
            //{
            //    MaxColumnWidth = maxColumnWidth;
            //    IsHeaderHorizontalSeparatorPresent = isHeaderHorizontalSeparatorPresent;
            //    ValueSeparator = valueSeparator;
            //    ValueMargin = valueMargin;
            //    HorizontalLinePadChar = horizontalLinePadChar;
            //    HorizontalLineSeparator = horizontalLineSeparator;
            //    LongValuesEllipses = longValuesEllipses;
            //    IsOuterBordersPresent = isOuterBordersPresent;
            //}

            public DataTableStringifier()
            {
                MaxColumnWidth = int.MaxValue;
                IsHeaderHorizontalSeparatorPresent = true;
                ValueSeparator = '|';
                ValueMargin = 1;
                HorizontalLinePadChar = '-';
                HorizontalLineSeparator = '+';
                LongValuesEllipses = "...";
                IsOuterBordersPresent = false;
            }

            public string StringifyDataTable(DataTable table)
            {
                int colCount = table.Columns.Count;
                int rowCount = table.Rows.Count;
                string[] colHeaders = new string[colCount];
                string[,] cells = new string[rowCount, colCount];
                int[] colWidth = new int[colCount];

                for (int i = 0; i < colCount; i++)
                {
                    var column = table.Columns[i];
                    var colName = ValueToLimitedLengthString(column.ColumnName);
                    colHeaders[i] = colName;
                    if (colWidth[i] < colName.Length)
                    {
                        colWidth[i] = colName.Length;
                    }
                }

                for (int i = 0; i < rowCount; i++)
                {
                    DataRow row = table.Rows[i];
                    for (int j = 0; j < colCount; j++)
                    {
                        var valStr = ValueToLimitedLengthString(row[j]);
                        cells[i, j] = valStr;
                        if (colWidth[j] < valStr.Length)
                        {
                            colWidth[j] = valStr.Length;
                        }
                    }
                }

                string valueSeparatorWithMargin = string.Concat(new string(' ', ValueMargin), ValueSeparator, new string(' ', ValueMargin));
                string leftBorder = IsOuterBordersPresent ? string.Concat(ValueSeparator, new string(' ', ValueMargin)) : "";
                string rightBorder = IsOuterBordersPresent ? string.Concat(new string(' ', ValueMargin), ValueSeparator) : "";
                string horizLine = new string(HorizontalLinePadChar, colWidth.Sum() + (colCount - 1) * (ValueMargin * 2 + 1) + (IsOuterBordersPresent ? (ValueMargin + 1) * 2 : 0));

                StringBuilder tableBuilder = new StringBuilder();

                if (String.IsNullOrEmpty(table.TableName))
                {
                    tableBuilder.AppendLine("==Table==");
                }
                else
                {
                    tableBuilder.AppendLine("==" + table.TableName + "==");
                }

                if (IsOuterBordersPresent)
                {
                    tableBuilder.AppendLine(horizLine);
                }

                tableBuilder.Append(leftBorder);
                for (int i = 0; i < colCount; i++)
                {
                    tableBuilder.Append(colHeaders[i].PadRight(colWidth[i]));
                    if (i < colCount - 1)
                    {
                        tableBuilder.Append(valueSeparatorWithMargin);
                    }
                }
                tableBuilder.AppendLine(rightBorder);

                if (IsHeaderHorizontalSeparatorPresent)
                {
                    if (IsOuterBordersPresent)
                    {
                        tableBuilder.Append(ValueSeparator);
                        tableBuilder.Append(HorizontalLinePadChar, ValueMargin);
                    }
                    for (int i = 0; i < colCount; i++)
                    {
                        tableBuilder.Append(new string(HorizontalLinePadChar, colWidth[i]));
                        if (i < colCount - 1)
                        {
                            tableBuilder.Append(HorizontalLinePadChar, ValueMargin);
                            tableBuilder.Append(HorizontalLineSeparator);
                            tableBuilder.Append(HorizontalLinePadChar, ValueMargin);
                        }
                    }
                    if (IsOuterBordersPresent)
                    {
                        tableBuilder.Append(HorizontalLinePadChar, ValueMargin);
                        tableBuilder.Append(ValueSeparator);
                    }
                    tableBuilder.AppendLine();
                }

                for (int i = 0; i < rowCount; i++)
                {
                    tableBuilder.Append(leftBorder);
                    for (int j = 0; j < colCount; j++)
                    {
                        tableBuilder.Append(cells[i, j].PadRight(colWidth[j]));
                        if (j < colCount - 1)
                        {
                            tableBuilder.Append(valueSeparatorWithMargin);
                        }
                    }
                    tableBuilder.AppendLine(rightBorder);
                }

                if (IsOuterBordersPresent)
                {
                    tableBuilder.AppendLine(horizLine);
                }

                return tableBuilder.ToString(0, tableBuilder.Length - 1); //Trim last enter char
            }

            private string ValueToLimitedLengthString(object value)
            {
                string strValue = value.ToString();
                if (strValue.Length > MaxColumnWidth)
                {
                    int beginningLength = (MaxColumnWidth) / 2;
                    int endingLength = (MaxColumnWidth + 1) / 2 - LongValuesEllipses.Length;
                    return string.Concat(strValue.Substring(0, beginningLength), LongValuesEllipses, strValue.Substring(strValue.Length - endingLength, endingLength));
                }
                else
                {
                    return strValue;
                }
            }
        }

        /// <summary>
        /// Tries to convert a DataTable to a display string.
        /// </summary>
        /// <param name="dtbl"></param>
        /// <returns></returns>
        public static string ParseDtblToString(DataTable dtbl)
        {
            if (dtbl != null)
            {
                try
                {
                    string result = "Did not fail but something still went wrong. Maybe the DataTable was null?";

                    DataTableHelper.DataTableStringifier stringifier = new DataTableHelper.DataTableStringifier();
                    result = stringifier.StringifyDataTable(dtbl);

                    return result;
                }
                catch
                {
                    return "Error converting DataTable " + dtbl.TableName + " to string.";
                }
            }
            else
            {
                return "DataTable is null!";
            }
        }

        public static string ParseDtblToHtmlString(DataTable dtbl)
        {
            GridView gridView = new GridView();//Need reference to System.Web
            StringBuilder result = new StringBuilder();
            StringWriter writer = new StringWriter(result);
            HtmlTextWriter htmlWriter = new HtmlTextWriter(writer);//Need reference to System.Web
            gridView.DataSource = dtbl;
            gridView.DataBind();

            gridView.RenderControl(htmlWriter);

            return result.ToString();
        }

        /// <summary>
        /// Used with DataTableHelper.JoinTwoDataTablesOnOneColumn
        /// </summary>
        public enum JoinType
        {
            /// <summary>
            /// Same as regular join. Inner join produces only the set of records that match in both Table A and Table B.
            /// </summary>
            Inner = 0,
            /// <summary>
            /// Same as Left Outer join. Left outer join produces a complete set of records from Table A, with the matching records (where available) in Table B. If there is no match, the right side will contain null.
            /// </summary>
            Left = 1
        }

        /// <summary>
        /// Joins the passed in DataTables on the colToJoinOn.
        /// <para>Returns an appropriate DataTable with zero rows if the colToJoinOn does not exist in both tables.</para>
        /// </summary>
        /// <param name="dtblLeft"></param>
        /// <param name="dtblRight"></param>
        /// <param name="colToJoinOn"></param>
        /// <param name="joinType"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>http://stackoverflow.com/questions/2379747/create-combined-datatable-from-two-datatables-joined-with-linq-c-sharp?rq=1</para>
        /// <para>http://msdn.microsoft.com/en-us/library/vstudio/bb397895.aspx</para>
        /// <para>http://www.codinghorror.com/blog/2007/10/a-visual-explanation-of-sql-joins.html</para>
        /// <para>http://stackoverflow.com/questions/406294/left-join-and-left-outer-join-in-sql-server</para>
        /// </remarks>
        public static DataTable JoinTwoDataTablesOnOneColumn(DataTable dtblLeft, DataTable dtblRight, string colToJoinOn, JoinType joinType)
        {
            //Change column name to a temp name so the LINQ for getting row data will work properly.
            string strTempColName = colToJoinOn + "_2";
            if (dtblRight.Columns.Contains(colToJoinOn))
                dtblRight.Columns[colToJoinOn].ColumnName = strTempColName;

            //Get columns from dtblA
            DataTable dtblResult = dtblLeft.Clone();

            //Get columns from dtblB
            var dt2Columns = dtblRight.Columns.OfType<DataColumn>().Select(dc => new DataColumn(dc.ColumnName, dc.DataType, dc.Expression, dc.ColumnMapping));

            //Get columns from dtblB that are not in dtblA
            var dt2FinalColumns = from dc in dt2Columns.AsEnumerable()
                                  where !dtblResult.Columns.Contains(dc.ColumnName)
                                  select dc;

            //Add the rest of the columns to dtblResult
            dtblResult.Columns.AddRange(dt2FinalColumns.ToArray());

            //No reason to continue if the colToJoinOn does not exist in both DataTables.
            if (!dtblLeft.Columns.Contains(colToJoinOn) || (!dtblRight.Columns.Contains(colToJoinOn) && !dtblRight.Columns.Contains(strTempColName)))
            {
                if (!dtblResult.Columns.Contains(colToJoinOn))
                    dtblResult.Columns.Add(colToJoinOn);
                return dtblResult;
            }

            switch (joinType)
            {

                default:
                case JoinType.Inner:
                    #region Inner
                    //get row data
                    //To use the DataTable.AsEnumerable() extension method you need to add a reference to the System.Data.DataSetExtension assembly in your project. 
                    var rowDataLeftInner = from rowLeft in dtblLeft.AsEnumerable()
                                           join rowRight in dtblRight.AsEnumerable() on rowLeft[colToJoinOn] equals rowRight[strTempColName]
                                           select rowLeft.ItemArray.Concat(rowRight.ItemArray).ToArray();


                    //Add row data to dtblResult
                    foreach (object[] values in rowDataLeftInner)
                        dtblResult.Rows.Add(values);

                    #endregion
                    break;
                case JoinType.Left:
                    #region Left
                    var rowDataLeftOuter = from rowLeft in dtblLeft.AsEnumerable()
                                           join rowRight in dtblRight.AsEnumerable() on rowLeft[colToJoinOn] equals rowRight[strTempColName] into gj
                                           from subRight in gj.DefaultIfEmpty()
                                           select rowLeft.ItemArray.Concat((subRight == null) ? (dtblRight.NewRow().ItemArray) : subRight.ItemArray).ToArray();


                    //Add row data to dtblResult
                    foreach (object[] values in rowDataLeftOuter)
                        dtblResult.Rows.Add(values);

                    #endregion
                    break;
            }

            //Change column name back to original
            dtblRight.Columns[strTempColName].ColumnName = colToJoinOn;

            //Remove extra column from result
            dtblResult.Columns.Remove(strTempColName);

            return dtblResult;
        }

        /// <summary>
        /// <para>- Gets DataTable primary keys if they exist.</para>
        /// <para>- Applies the filter, sort and DataViewRowState to a DataView with the passed in DataTable. (This step will wipe out any primary keys )</para>
        /// <para>- Converts the result back to a DataTable.</para>
        /// <para>- Reapplies any primary keys to the new DataTable.</para>
        /// <para>- Then returns the new DataTable ( it does not change the original DataTable ). If it fails, then the original dataTable is returned.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="filter">The filter is case insensitive!</param>
        /// <param name="sort">Defaults to ""</param>
        /// <param name="rowState">Defaults to DataViewRowState.CurrentRows</param>
        /// <returns>Returns a new DataTable.</returns>
        public static DataTable SafeFilterDataTable(DataTable dtbl, string filter, string sort, DataViewRowState rowState)
        {
            try
            {
                DataColumn[] dcolPrimaryKeys = dtbl.PrimaryKey;//Save the current primary keys

                DataTable dtblResult = (new DataView(dtbl, filter, sort, rowState)).ToTable();//It is case insensitive!****Converting to DataView removes PrimaryKeys!

                List<DataColumn> listPrimaryKeysNew = new List<DataColumn>();
                foreach (DataColumn dcol in dcolPrimaryKeys)
                {
                    if (dtblResult.Columns.Contains(dcol.ColumnName))
                        listPrimaryKeysNew.Add(dtblResult.Columns[dcol.ColumnName]);
                }

                if (listPrimaryKeysNew.Count > 0)
                    dtblResult.PrimaryKey = listPrimaryKeysNew.ToArray();//This is because converting to dataview will not copy the primary keys into the new table

                return dtblResult;
            }
            catch (Exception ex)
            {
                LogHelper.ExceptionLog(MethodInfo.GetCurrentMethod(), ex.Message);
                return dtbl;
            }
        }

        /// <summary>
        /// <para>If a pattern in a LIKE clause contains any of these special characters * % [ ], those characters must be escaped in brackets [ ] like this [*], [%], [[] or []].</para>
        /// <para>Examples:</para>
        /// <para>- strFilter = "[UnitName] LIKE '%" + DataTableHelper.EscapeLikePattern(SomeString) + "%'";</para>
        /// <para></para>
        /// </summary>
        /// <param name="LikePattern">This should not be the entire filter string... just the pattern that is being compared.</param>
        /// <returns></returns>
        /// <remarks>http://www.csharp-examples.net/dataview-rowfilter/</remarks>
        public static string EscapeLikePattern(string LikePattern)
        {
            string lb = "~~LeftBracket~~";
            string rb = "~~RightBracket~~";
            LikePattern = LikePattern.Replace("[", lb).Replace("]", rb).Replace("*", "[*]").Replace("%", "[%]");
            return LikePattern.Replace(lb, "[[]").Replace(rb, "[]]").Replace("'", "''");
        }
    }

    public static class DataTableExtensions
    {
        /// <summary>
        /// Only add the column if it does not already exist. Reuturn true if the column was added, false otherwise.
        /// <para>False indicates that either the column already existed or it could not be added.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static bool SafeColumnAdd(this DataTable dtbl, string colName)
        {
            return SafeColumnAdd(dtbl, colName, null, null, "");
        }

        /// <summary>
        /// Only add the column if it does not already exist. Reuturn true if the column was added, false otherwise.
        /// <para>False indicates that either the column already existed or it could not be added.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="colName"></param>
        /// <param name="colType"></param>
        /// <returns></returns>
        public static bool SafeColumnAdd(this DataTable dtbl, string colName, Type colType)
        {
            return SafeColumnAdd(dtbl, colName, colType, null, "");
        }

        /// <summary>
        /// Only add the column if it does not already exist. Reuturn true if the column was added, false otherwise.
        /// <para>False indicates that either the column already existed or it could not be added.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="colName"></param>
        /// <param name="colType"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool SafeColumnAdd(this DataTable dtbl, string colName, Type colType, object defaultValue)
        {
            return SafeColumnAdd(dtbl, colName, colType, defaultValue, "");
        }

        /// <summary>
        /// Only add the column if it does not already exist. Reuturn true if the column was added, false otherwise.
        /// <para>False indicates that either the column already existed or it could not be added.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="colName"></param>
        /// <param name="colType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool SafeColumnAdd(this DataTable dtbl, string colName, Type colType, object defaultValue, string expression)
        {
            bool success = false;

            if (dtbl != null)
            {
                if (!dtbl.Columns.Contains(colName))
                {
                    try
                    {
                        DataColumn dcol = new DataColumn(colName);

                        if (colType != null)
                        {
                            dcol.DataType = colType;
                        }

                        if (defaultValue != null)
                        {
                            dcol.DefaultValue = defaultValue;
                        }

                        if (!String.IsNullOrEmpty(expression))
                        {
                            dcol.Expression = expression;
                        }

                        dtbl.Columns.Add(dcol);
                        success = true;
                    }
                    catch
                    {
                        success = false;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Calls RemoveColumnByName foreach strin in columnNames. Only returns true if All columns are removed. False only indicates that 1 or more columns could not be removed.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnNames"></param>
        /// <returns></returns>
        public static Boolean SafeColumnRemoveRangeByNames(this DataTable dtbl, params String[] columnNames)
        {
            bool result = true;

            foreach (string colName in columnNames)
            {
                if (!dtbl.SafeColumnRemoveByName(colName))
                {
                    result = false;//If any column could not be removed then result should be false.
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to remove the column from the DataTable.
        /// <para>Tests to see if the column exits and if it can be removed.</para>
        /// <para>-</para>
        /// <para>-</para>
        /// </summary>
        /// <param name="dt">The DataTable to remove the column from.</param>
        /// <param name="columnName">Name of the column to remove.</param>
        /// <returns>Returns true on success, false otherwise.</returns>
        /// <remarks>Modified from http://msdn.microsoft.com/en-us/library/tcdez2aw.aspx</remarks>
        public static Boolean SafeColumnRemoveByName(this DataTable dtbl, String columnName)
        {
            Boolean result = false;

            if (dtbl.Columns.Contains(columnName))
            {
                if (dtbl.Columns.CanRemove(dtbl.Columns[columnName]))
                {
                    dtbl.Columns.Remove(columnName);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// SetOrdinal of table columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static void SetColumnsOrder(this DataTable dtbl, params String[] columnNames)
        {
            if (dtbl == null)
                return;

            List<string> listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (string colName in columnNames)
            {
                if (!dtbl.Columns.Contains(colName))
                {
                    listColNames.Remove(colName);
                }
            }

            foreach (string colName in listColNames)
            {
                dtbl.Columns[colName].SetOrdinal(listColNames.IndexOf(colName));
            }
        }

        /// <summary>
        /// Add a prefix to each column name as long as it does not already have the prefix.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="prefix"></param>
        public static void SetColumnNamesPrefix(this DataTable dtbl, String prefix)
        {
            int changedCount = 0;

            foreach (DataColumn dcol in dtbl.Columns)
            {
                string colName = prefix + dcol.ColumnName;
                if (!dcol.ColumnName.StartsWith(prefix) && !dtbl.Columns.Contains(colName))
                {
                    dcol.ColumnName = colName;
                    changedCount++;
                }
                else
                {
                    //column already starts with prefix or column name already exists.
                }
            }
        }

        /// <summary>
        /// Only changes the column name of colNameExisting to colNameNew if colNameExisting actually exists.
        /// Returns true on name change. False, otherwise.
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="colNameExisting"></param>
        /// <param name="colNameNew"></param>
        /// <returns></returns>
        public static bool SafeColumnRename(this DataTable dtbl, string colNameExisting, string colNameNew)
        {
            bool success = false;

            if (dtbl.Columns.Contains(colNameExisting))
            {
                dtbl.Columns[colNameExisting].ColumnName = colNameNew;
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Attempt to convert all values in the column to the type.
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="columnName"></param>
        /// <param name="type"></param>
        /// <param name="useSafeConvert">TRUE if you want the fields to be defaulted when the conversion fails. FALSE if you want to keep the original values for the entire column if any conversion fails.</param>
        public static void SafeConvertColumn(this DataTable dtbl, string columnName, Type type, bool useSafeConvert)
        {
            if (dtbl != null && dtbl.Columns.Contains(columnName))
            {
                if (dtbl.Columns[columnName].DataType != type)
                {
                    string colNameTemp = columnName + "_temp";

                    try
                    {
                        switch (type.Name.ToLower())
                        {
                            case "datetime":
                            case "system.datetime":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(decimal));
                                foreach (DataRow drow in dtbl.Rows)
                                {
                                    drow[colNameTemp] = Convert.ToDateTime(drow[columnName]);
                                }
                                #endregion
                                break;
                            case "decimal":
                            case "system.decimal":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(decimal));
                                if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeDecimal(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToDecimal(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                            case "double":
                            case "system.double":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(double)); if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeDouble(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToDouble(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                            case "short":
                            case "int16":
                            case "system.int16":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(short));
                                if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeInt16(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToInt16(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                            case "int":
                            case "int32":
                            case "system.int32":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(int));
                                if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeInt32(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToInt32(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                            case "long":
                            case "int64":
                            case "system.int64":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(long));
                                if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeInt64(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToInt64(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                            case "string":
                            case "system.string":
                                #region
                                dtbl.Columns.Add(colNameTemp, typeof(string));
                                if (useSafeConvert)
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = SafeConvert.SafeString(drow[columnName]);
                                    }
                                }
                                else
                                {
                                    foreach (DataRow drow in dtbl.Rows)
                                    {
                                        drow[colNameTemp] = Convert.ToString(drow[columnName]);
                                    }
                                }
                                #endregion
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        dtbl.SafeColumnRemoveByName(colNameTemp);//Remove the temp column and leave the DataTable unchanged if something went wrong.

                        LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
                    }

                    if (dtbl.Columns.Contains(colNameTemp))
                    {
                        //Only remove and rename if the temp column exists.
                        if (dtbl.SafeColumnRemoveByName(columnName))
                        {
                            dtbl.Columns[colNameTemp].ColumnName = columnName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="dictFilterParams">Key = column that contains search criteria. Value = search criteria for column.</param>
        /// <param name="columnToSum"></param>
        /// <returns></returns>
        public static Decimal SafeComputeColumnSum(this DataTable dtbl, string columnToSum, Dictionary<string, string> dictFilterParams)
        {
            if (dtbl == null)
                return 0.0m;
            if (dtbl.Rows.Count == 0)
                return 0.0m;

            Decimal result = 0.0m;
            string strFilter = "";
            List<string> listFilterParts = new List<string>();

            if (dictFilterParams == null)
            {
                dictFilterParams = new Dictionary<string, string>();
            }

            //Create a list of filter parts.
            foreach (KeyValuePair<string, string> kvp in dictFilterParams)
            {
                string filterPart = dtbl.SafeGetFilterPart(kvp.Key, "=", kvp.Value);

                if (!String.IsNullOrEmpty(filterPart) && !listFilterParts.Contains(filterPart))
                {
                    listFilterParts.Add(filterPart);
                }
            }

            //Loop the list of filter parts to create the filter string and concatenate " AND " where necessary.
            foreach (string filterPart in listFilterParts)
            {
                strFilter += filterPart;
                if (listFilterParts.IndexOf(filterPart) != listFilterParts.Count - 1)
                {
                    strFilter += " AND ";
                }
            }

            //Apply the filter and compute sum of columnToSum.
            if (dtbl.Columns.Contains(columnToSum))
            {
                try
                {
                    result = SafeConvert.SafeDecimal(dtbl.Compute("SUM([" + columnToSum + "])", strFilter));
                }
                catch
                {
                    result = 0.0m;//columnToSum was probably an invalid type for Sum().
                }
            }

            return result;
        }

        /// <summary>
        /// Get a simple DataTable filter that consists of the search column, operand and search criteria. 
        /// <para>This function inserts the proper escape characters based on the search column's data type.</para>
        /// <para>Returns empty string if the filter part can not be formed.</para>
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="searchCol"></param>
        /// <param name="comparisonOperator">Leave blank for equals.</param>
        /// <param name="searchCrit"></param>
        /// <returns></returns>
        public static string SafeGetFilterPart(this DataTable dtbl, string searchCol, string comparisonOperator, string searchCrit)
        {
            string filterPart = String.Empty;

            if (String.IsNullOrEmpty(comparisonOperator))
            {
                comparisonOperator = "=";
            }

            switch (comparisonOperator.Trim())
            {
                case "=":
                case "<":
                case ">":
                case "<=":
                case ">=":
                case "<>":
                    //Valid comparison operator.
                    //http://www.csharp-examples.net/dataview-rowfilter/
                    break;
                case "IN"://This one should not be used with this function unless searchCrit is already formed correctly.
                case "NOT IN"://This one should not be used with this function unless searchCrit is already formed correctly.
                case "LIKE"://This one should not be used with this function unless searchCrit is already formed correctly.
                    filterPart = "[" + searchCol + "]" + comparisonOperator + searchCrit;
                    return filterPart;//Exit function here.
                //break;
                default:
                    //Invalid comparison operator.
                    comparisonOperator = string.Empty;
                    break;
            }

            if (dtbl.Columns.Contains(searchCol) && !String.IsNullOrEmpty(comparisonOperator))
            {
                filterPart = "[" + searchCol + "]" + comparisonOperator + searchCrit;

                if (dtbl.Columns[searchCol].DataType == typeof(DateTime))
                {
                    filterPart = "[" + searchCol + "]" + comparisonOperator + "'#" + Convert.ToDateTime(searchCrit) + "#'";
                }
                else if (dtbl.Columns[searchCol].DataType == typeof(String) || dtbl.Columns[searchCol].DataType == typeof(Guid))
                {
                    filterPart = "[" + searchCol + "]" + comparisonOperator + "'" + searchCrit + "'";
                }
            }

            return filterPart;
        }

        /// <summary>
        /// This function does a simple select with 1 column, the equals(=) operator and 1 search criteria.
        /// It auto formats the search criteria based on the data type of the column.
        /// It does not sort the results.
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="searchCol"></param>
        /// <param name="searchCrit"></param>
        /// <returns></returns>
        public static DataRow[] SafeSelect(this DataTable dtbl, string searchCol, string searchCrit)
        {
            string strFilter = dtbl.SafeGetFilterPart(searchCol, "=", searchCrit);
            return dtbl.Select(strFilter);
        }

        /// <summary>
        /// Removes duplicate rows from given DataTable
        /// </summary>
        /// <param name="tbl">Table to scan for duplicate rows</param>
        /// <param name="keyColumns">An array of DataColumns
        ///   containing the columns to match for duplicates</param>
        /// <remarks>http://geekswithblogs.net/ajohns/archive/2004/06/24/7191.aspx</remarks>
        public static void RemoveDuplicates(this DataTable tbl, String[] keyColumns)
        {
            List<DataColumn> listKeyColumns = new List<DataColumn>();
            foreach (string str in keyColumns)
            {
                if (tbl.Columns[str] != null)
                {
                    listKeyColumns.Add(tbl.Columns[str]);
                }
            }

            if (listKeyColumns.Count > 0)
            {
                tbl.RemoveDuplicates(listKeyColumns.ToArray());
            }
        }

        /// <summary>
        /// Removes duplicate rows from given DataTable
        /// </summary>
        /// <param name="tbl">Table to scan for duplicate rows</param>
        /// <param name="KeyColumns">An array of DataColumns
        ///   containing the columns to match for duplicates</param>
        /// <remarks>http://geekswithblogs.net/ajohns/archive/2004/06/24/7191.aspx</remarks>
        public static void RemoveDuplicates(this DataTable tbl, DataColumn[] keyColumns)
        {
            int rowNdx = 0;

            while (rowNdx < tbl.Rows.Count - 1)
            {
                DataRow[] dups = FindDups(tbl, rowNdx, keyColumns);

                if (dups.Length > 0)
                {
                    foreach (DataRow dup in dups)
                    {
                        tbl.Rows.Remove(dup);
                    }
                }
                else
                {
                    rowNdx++;
                }
            }
        }

        /// <summary>
        /// Find duplicates in DataTable.
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="sourceNdx"></param>
        /// <param name="keyColumns"></param>
        /// <returns></returns>
        /// <remarks>http://geekswithblogs.net/ajohns/archive/2004/06/24/7191.aspx</remarks>
        public static DataRow[] FindDups(this DataTable tbl, int sourceNdx, DataColumn[] keyColumns)
        {
            ArrayList retVal = new ArrayList();
            DataRow sourceRow = tbl.Rows[sourceNdx];

            for (int i = sourceNdx + 1; i < tbl.Rows.Count; i++)
            {
                DataRow targetRow = tbl.Rows[i];

                if (IsDup(sourceRow, targetRow, keyColumns))
                {
                    retVal.Add(targetRow);
                }
            }

            return (DataRow[])retVal.ToArray(typeof(DataRow));
        }

        /// <summary>
        /// Compare two DataRows on the keyColumns to see if they are duplicates.
        /// </summary>
        /// <param name="sourceRow"></param>
        /// <param name="targetRow"></param>
        /// <param name="keyColumns"></param>
        /// <returns></returns>
        /// <remarks>http://geekswithblogs.net/ajohns/archive/2004/06/24/7191.aspx</remarks>
        public static bool IsDup(this DataRow sourceRow, DataRow targetRow, DataColumn[] keyColumns)
        {
            bool retVal = true;

            foreach (DataColumn column in keyColumns)
            {
                retVal = retVal && sourceRow[column].Equals(targetRow[column]);
                if (!retVal) break;
            }
            return retVal;
        }
    
        /// <summary>
        /// Returns true if both data tables have the same column names and column data types. False, otherwise.
        /// </summary>
        /// <param name="dtbl"></param>
        /// <param name="dtblToCompare"></param>
        /// <returns></returns>
        public static bool SchemaEquals(this DataTable dtbl, DataTable dtblToCompare)
        {
            bool result = false;

            DataTable dt1 = dtbl.Clone();
            DataTable dt2 = dtblToCompare.Clone();
            dt1.TableName = dt2.TableName = "Table";

            if (dt1.Columns.Count == dt2.Columns.Count)
            {
                #region This one works if you assume the columns are already in alphabetical order or you assume that order matters.
                //DataSet ds1 = new DataSet();
                //DataSet ds2 = new DataSet();

                //ds1.Tables.Add(dt1);
                //ds2.Tables.Add(dt2);

                //ds1.Tables[0].TableName = "Table";
                //ds2.Tables[0].TableName = "Table";

                //string xml1 = ds1.GetXmlSchema();
                //string xml2 = ds2.GetXmlSchema();

                //result = xml1.Equals(xml2);
                #endregion

                #region Compare ColumnName and DataType by using LINQ and IEqualityComparer.

                var dt1Cols = dt1.Columns.Cast<DataColumn>();
                var dt2Cols = dt2.Columns.Cast<DataColumn>();

                var exceptCount = dt1Cols.Except(dt2Cols, DataColumnEqualityComparer.Instance).Count();
                return (exceptCount == 0);

                #endregion
            }

            return result;
        }
    }

    public static class DataTableCollectionExtensions
    {
        /// <summary>
        /// Only removes the table if the table exists and it can be removed.
        /// <para>Returns true on successful removal or if the tableName does not exist in the collection, false otherwise.</para>
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool SafeRemove(this DataTableCollection tables, string tableName)
        {
            bool success = false;
            if (tables != null && tables.Contains(tableName))
            {
                if (tables.CanRemove(tables[tableName]))
                {
                    tables.Remove(tableName);
                    success = true;
                }
            }
            else
            {
                success = true;
            }
            return success;
        }

        /// <summary>
        /// First call SafeRemove. If SafeRemove returns true then Add the table to the collection.
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool SafeAdd(this DataTableCollection tables, DataTable table)
        {
            bool success = false;
            if (tables != null && tables.SafeRemove(table.TableName))
            {
                tables.Add(table);
            }
            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static List<DataTable> ToList(this DataTableCollection tables)
        {
            List<DataTable> result = new List<DataTable>();

            foreach (DataTable dtbl in tables)
            {
                result.Add(dtbl);
            }

            return result;
        }
    }

    public static class DataRowExtensions
    {
        /// <summary>
        /// If the DataRow's table contains columnName then return the value in columnName. Otherwise, return null.
        /// NOTE that this returns an OBJECT so do not assume that is a string.
        /// </summary>
        /// <param name="drow"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static object SafeGetValue(this DataRow drow, string columnName)
        {
            return drow.SafeGetValue(columnName, null);
        }

        /// <summary>
        /// If the DataRow's table contains columnName then return the value in columnName. Otherwise, return null.
        ///  NOTE that this returns an OBJECT so do not assume that is a string.
        /// </summary>
        /// <param name="drow"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static object SafeGetValue(this DataRow drow, string columnName, object defaultValue)
        {
            if (drow.Table.Columns.Contains(columnName))
            {
                return drow[columnName];
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// This function will get all indexes of columns that match the compare strings then add up the values in drow for each of those columns.
        /// </summary>
        /// <param name="drow"></param>
        /// <param name="strCompare1"></param>
        /// <param name="strCompare2"></param>
        /// <param name="strCompare3"></param>
        /// <param name="listStringsToIgnore"></param>
        /// <returns></returns>
        public static Decimal GetSumOfColumnValues(this DataRow drow, string strCompare1, string strCompare2, string strCompare3, List<string> listStringsToIgnore)
        {
            Decimal result = 0.0m;

            List<int> listIndexes = DataTableHelper.GetIndexesOfColumnFromDataTable(drow.Table, strCompare1, strCompare2, strCompare3, listStringsToIgnore);
            foreach (int colIndex in listIndexes)
            {
                result += colIndex < 0 ? 0.00m : SafeConvert.SafeDecimal(drow[colIndex]);
            }

            return result;
        }
    }

    /// <summary>
    /// Compare DataColumn by ColumName and DataType.
    /// </summary>
    /// <remarks> http://stackoverflow.com/questions/7313282/check-to-see-if-2-datatable-have-same-schema </remarks>
    public class DataColumnEqualityComparer : IEqualityComparer<DataColumn>
    {
        #region IEqualityComparer Members

        private DataColumnEqualityComparer() { }
        public static DataColumnEqualityComparer Instance = new DataColumnEqualityComparer();

        public bool Equals(DataColumn x, DataColumn y)
        {
            if (x.ColumnName != y.ColumnName)
                return false;
            if (x.DataType != y.DataType)
                return false;

            return true;
        }

        public int GetHashCode(DataColumn obj)
        {
            int hash = 17;
            hash = 31 * hash + obj.ColumnName.GetHashCode();
            hash = 31 * hash + obj.DataType.GetHashCode();

            return hash;
        }

        #endregion
    }
}
