using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Microsoft.CSharp;


namespace NetFrameworkDataTableTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DataSet DataSet_FirstSet = new DataSet();

            DataTable DataTable_FirstParent = new DataTable();
            DataTable DataTable_FirstChild = new DataTable();
            DataTable DataTable_SecondChild = new DataTable();
            DataTable DataTable_ThirdChild = new DataTable();
            DataTable DataTable_FourthChild = new DataTable();
            DataTable DataTable_FifthChild = new DataTable();
            DataTable DataTable_Parent = new DataTable();

            DataSet_FirstSet.ReadXmlSchema("C://Users/hurricane/source/repos/NetFrameworkDataTableTest/NetFrameworkDataTableTest/Parent.xsd");

            /*Part of the substitutionGroup alternative implementation.   *
             *Create two new DataTables using the existing schema         *
             *And rename them to insure they are completely new DataTables*/
            DataTable DataTable_InsertDataTable = DataSet_FirstSet.Tables["ParentTable"].Copy();
            DataTable_InsertDataTable.TableName = "ParentHeadLevel2";
            DataSet_FirstSet.Tables.Add(DataTable_InsertDataTable);

            DataTable_InsertDataTable = DataSet_FirstSet.Tables["ChildHead"].Copy();
            DataTable_InsertDataTable.TableName = "ChildHeadLevel2";
            DataSet_FirstSet.Tables.Add(DataTable_InsertDataTable);

            /*Part of the substitutionGroup alternative implementation.                                    *
             *Create the DataRelations:                                                                    *
             *DataTable "FirstParentChildHead" is the parent DataTable                                     *
             *DataTable "ChildHeadLevel2" is the child DataTable of parent "FirstParentChildHead" DataTable*
             *DataTable "ParentHeadLevel2" is the child DataTable of parent "ChildHeadLevel2" DataTable    */
            DataSet_FirstSet.Relations.Add("FirstParentChildHead_ChildHeadLevel2",
                                           DataSet_FirstSet.Tables["FirstParentChildHead"].Columns["ChildLevel2_Reference"],
                                           DataSet_FirstSet.Tables["ChildHeadLevel2"].Columns["ChildLevel2_Reference"]);

            DataSet_FirstSet.Relations.Add("ChildHeadLevel2_ParentHeadLevel2",
                                           DataSet_FirstSet.Tables["ChildHeadLevel2"].Columns["ParentLevel2_Reference"],
                                           DataSet_FirstSet.Tables["ParentHeadLevel2"].Columns["ParentLevel2_Reference"]);

            /*Create a new row in DataTable "ParentTable"                                                        *
             *Fill data fields for checking on cyclistic references with "ParentHeadLevel2" DataTable            *
             *"ParentHeadLevel2" DataTable uses the "ParentTable" as DataTable template but with a different name*/
            DataTable_Parent = DataSet_FirstSet.Tables["ParentTable"];
            DataRow DataRow_Parent = DataTable_Parent.NewRow();
            DataRow_Parent["Name"] = "PARENTTABLE";
            DataRow_Parent["Level"] = "0";
            DataSet_FirstSet.Tables["ParentTable"].Rows.Add(DataRow_Parent);

            /*Create a new row in DataTable "ParentLevel1Child_ID"                     *
             *Fill fields for checking on cyclistic references.                        *
             *Walk and Establish the linking ID between the DataTables.                *
             *"ParentTable" DataTable is the parent table.                             *
             *"ParentLevel1Child_ID" is the child DataTable to "ParentTable" DataTable.*/
            DataTable_FirstChild = DataSet_FirstSet.Tables["ParentLevel1Child_ID"];
            DataRow DataRow_FirstChild = DataTable_FirstChild.NewRow();
            DataRow_FirstChild["ParentTable_Id"] = DataRow_Parent["ParentTable_Id"];
            DataTable_FirstChild.Rows.Add(DataRow_FirstChild);

            /*Create a new row in DataTable "FirstParentChildHead".                             *
             *Fill fields for checking on cyclistic references.                                 *
             *Manually set the Key for this new row in this DataTable.                          *
             *Walk and Establish the linking ID between the DataTables.                         *
             *"ParentLevel1Child_ID" DataTable is the parent table.                             *
             *"FirstParentChildHead" is the child DataTable to "ParentLevel1Child_ID" DataTable.*/
            DataTable_ThirdChild = DataSet_FirstSet.Tables["FirstParentChildHead"];
            DataRow DataRow_ThirdChild = DataTable_ThirdChild.NewRow();
            DataRow_ThirdChild["Name"] = "FIRSTPARENTCHILDHEAD";
            DataRow_ThirdChild["Level"] = "1";
            DataRow_ThirdChild["ChildLevel2_Reference"] = 0; //Have to manually set the Key for the Child Table
            DataRow_ThirdChild["ParentLevel1Child_ID_Id"] = DataRow_FirstChild["ParentLevel1Child_ID_Id"];
            DataSet_FirstSet.Tables["FirstParentChildHead"].Rows.Add(DataRow_ThirdChild);

            /*Create a new row in DataTable "Child_Reference".                             *
             *Fill fields for checking on cyclistic references.                            *
             *Walk and Establish the linking ID between the DataTables.                    *
             *"ParentLevel1Child_ID" DataTable is the parent table.                        *
             *"Child_Reference" is the child DataTable to "ParentLevel1Child_ID" DataTable.*/
            DataTable_FourthChild = DataSet_FirstSet.Tables["Child_Reference"];
            DataRow DataRow_FourthChild = DataTable_FourthChild.NewRow();
            DataRow_FourthChild["Name"] = "CHILD_REFERENCE";
            DataRow_FourthChild["Level"] = "1";
            DataRow_FourthChild["ParentLevel1Child_ID_Id"] = DataRow_FirstChild["ParentLevel1Child_ID_Id"];
            DataSet_FirstSet.Tables["Child_Reference"].Rows.Add(DataRow_FourthChild);

            /*Using the existing ChildRelations "Child_Reference_ChildHead_Reference" to pull child DataTable *
             *"ChildHead_Reference".                                                                          *
             *Fill fields for checking on cyclistic references.                                               *
             *Walk and Establish the linking ID between the DataTables.                                       *
             *"Child_Reference" DataTable is the parent table.                                                *
             *"ChildHead_Reference" is the child DataTable to "Child_Reference" DataTable                     *
             *NOTE: BECAUSE OF THE WAY THE SCHEMA IS WRITTEN, THE .ReadXmlSchema() WILL AUTOMATICALLY CREATE  *
             *AN EXTRA COLUMN HERE FOR AN ID TO DATATABLE "FIRSTPARENTCHILDHEAD". THIS IS UNWANTED, BUT MAY   *
             *HAVE TO LIVE WITH. I THINK IT IS OKAY, PER SE SINCE THE ID LINKING DO NOT SHOW UP BETWEEN THE   *
             *THE PARENT TABLES. THIS NEEDS TO BE INVESTIGATED MORE.                                          */
            DataRelation dataRelation = DataRow_FourthChild.Table.ChildRelations["Child_Reference_ChildHead_Reference"];
            DataTable_FifthChild = dataRelation.ChildTable;
            DataRow DataRow_FifthChild = DataTable_FifthChild.NewRow();
            DataRow_FifthChild["Name"] = "CHILDHEAD_REFERENCE";
            DataRow_FifthChild["Level"] = "1";
            DataRow_FifthChild["Child_Reference_Id"] = DataRow_FourthChild["Child_Reference_Id"];
            DataTable_FifthChild.Rows.Add(DataRow_FifthChild);

            /*Using the existing ChildRelations "FirstParentChildHead_ChildHeadLevel2" to pull child DataTable *
             *"ChildHeadLevel2".                                                                               *
             *Fill fields for checking cyclistic references.                                                   *
             *Manually set the Key for this new row in this DataTable.                                         *
             *Walk and Establish the linking ID between the DataTables.                                        *
             *"FirstParentChildHead" DataTable is the parent table.                                            *
             *"ChildHeadLevel2" is the child DataTable to "FirstParentChildHead" DataTable.                    */
            DataRelation DataRelation_SixthChild = DataRow_ThirdChild.Table.ChildRelations["FirstParentChildHead_ChildHeadLevel2"];
            DataRelation_SixthChild.Nested = true;
            DataTable DataTable_SixthChild = DataRelation_SixthChild.ChildTable;
            DataRow DataRow_SixthChild = DataTable_SixthChild.NewRow();
            DataRow_SixthChild["Name"] = "CHILDHEADLEVEL2";
            DataRow_SixthChild["Level"] = "2";
            DataRow_SixthChild["ParentLevel2_Reference"] = 0; //Have to manually set the Key for the Child
            DataRow_SixthChild["ChildLevel2_Reference"] = DataRow_ThirdChild["ChildLevel2_Reference"];
            DataSet_FirstSet.Tables["ChildHeadLevel2"].Rows.Add(DataRow_SixthChild);

            /*Using the existing ChildRelations "ChildHeadLevel2_ParentHeadLevel2" to pull child DataTable     *
             *"ParentHeadLevel2".                                                                              *
             *Fill fields for checking cyclistic references.                                                   *
             *Manually set the Key for this new row in this DataTable.                                         *
             *Walk and Establish the linking ID between the DataTables.                                        *
             *"ChildHeadLevel2" DataTable is the parent table.                                                 *
             *"ParentHeadLevel2" is the child DataTable to "ChildHeadLevel2" DataTable.                        */
            DataRelation DataRelation_SeventhChild = DataRow_SixthChild.Table.ChildRelations["ChildHeadLevel2_ParentHeadLevel2"];
            DataRelation_SeventhChild.Nested = true;
            DataTable DataTable_SeventhChild = DataRelation_SeventhChild.ChildTable;
            DataRow DataRow_SeventhChild = DataTable_SeventhChild.NewRow();
            DataRow_SeventhChild["Name"] = "PARENTHEADLEVEL2";
            DataRow_SeventhChild["Level"] = "2";
            DataRow_SeventhChild["ParentLevel2_Reference"] = DataRow_SixthChild["ParentLevel2_Reference"];
            DataSet_FirstSet.Tables["ParentHeadLevel2"].Rows.Add(DataRow_SeventhChild);


            DataSet_FirstSet.AcceptChanges();

            DataSet_FirstSet.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            /*Write schema to check if the resulting schema matches the original schema with the added DataTables created in this code.*/
            DataSet_FirstSet.WriteXmlSchema("C://Users//hurricane//source//repos//NetFrameworkDataTableTest//NetFrameworkDataTableTest//DataSetWriteOut.xsd");
            /*Write XML file to check for the correct nesting of the DataTables.*/
            DataSet_FirstSet.WriteXml("C://Users//hurricane//source//repos//NetFrameworkDataTableTest//NetFrameworkDataTableTest//DataSetWriteOut.xml");


            Console.WriteLine("Stop");
        }
    }
}
