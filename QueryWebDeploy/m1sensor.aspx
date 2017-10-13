<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPageSingleMenu.Master" CodeBehind="m1sensor.aspx.cs" Inherits="TelerikWebApp3.m1sensor" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="styles/grid.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <telerik:RadPageLayout runat="server" ID="JumbotronLayout" CssClass="jumbotron" GridType="Fluid">
        <Rows>
            <telerik:LayoutRow>
                <Columns>
                    <telerik:LayoutColumn Span="4">
                        
                        <telerik:RadLabel runat="server" Text="M1 Query :Sensor Mac" ID="lbMac" />
                        <telerik:RadTextBox runat="server" ID="txtMac" />
                    </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="3">
                        <telerik:RadLabel runat="server" Text="Start Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateStart" Width="240px" />
                    </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="5">
                        <telerik:RadLabel runat="server" Text="End Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateEnd" Width="240px" />
                        <telerik:RadComboBox runat="server" ID ="comboDatetime" >
                            <Items>
                                <telerik:RadComboBoxItem Text="Collect Datetime" Selected="true"/>
                                <telerik:RadComboBoxItem Text="Server Datetime" />
                            </Items>
                        </telerik:RadComboBox>
                        <telerik:RadButton runat="server" ID="btnQuery" OnClick="btnQuery_Click" Text="Query" />
                        <telerik:RadButton runat="server" ID="btnExport" OnClick="btnExport_Click" Text="Export" />
                    </telerik:LayoutColumn>

                </Columns>
            </telerik:LayoutRow>

        </Rows>
    </telerik:RadPageLayout>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <telerik:RadAjaxPanel ID="RadAjaxPanel1" runat="server" CssClass="grid_wrapper">
        <telerik:RadGrid ID="RadGrid1" runat="server" PageSize="10" PagerStyle-PageButtonCount="5" PagerStyle-AlwaysVisible="true"
            OnNeedDataSource="RadGrid1_NeedDataSource" OnItemCommand="RadGrid1_ItemCommand"
            AllowPaging="True" AllowSorting="true" ShowGroupPanel="false" RenderMode="Auto">
            
            <ExportSettings ExportOnlyData="true" IgnorePaging="true"></ExportSettings>
            <MasterTableView AutoGenerateColumns="False"
                AllowFilteringByColumn="false" TableLayout="Auto"
                DataKeyNames="ID" CommandItemDisplay="Top"
                InsertItemPageIndexAction="ShowItemOnFirstPage">
                <CommandItemSettings  ShowAddNewRecordButton="false"  ShowExportToExcelButton="true" ShowExportToPdfButton="true" />
                <Columns>
                    <telerik:GridBoundColumn DataField="ID" HeaderText="ID" SortExpression="ID" UniqueName="ID">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="SensorMac" HeaderText="Sensor Mac"/>
                    
                    <telerik:GridBoundColumn DataField="SensorSN" HeaderText="Serial No"/>                    
                    <telerik:GridBoundColumn DataField="SensorStatic" HeaderText="Current"/>
                    <telerik:GridBoundColumn DataField="SensorRAMCount" HeaderText="RAM"/>
                    <telerik:GridBoundColumn DataField="SensorROMCount" HeaderText="ROM"/>

                    <telerik:GridBoundColumn DataField="ICTemperature" HeaderText="IC Temp"/>                    
                    <telerik:GridBoundColumn DataField="SensorStatic" HeaderText="Current"/>
                    <telerik:GridBoundColumn DataField="SensorTemperature" HeaderText="Temp"/>
                    <telerik:GridBoundColumn DataField="SensorHumidity" HeaderText="Humi"/>


                    <telerik:GridBoundColumn DataField="SensorRSSI" HeaderText="SensorRSSI"/>

                    <telerik:GridBoundColumn DataField="SensorCollectDatetime" HeaderText="Collect" DataFormatString="{0:MM-dd HH:mm:ss}"/>
                    <telerik:GridBoundColumn DataField="SensorTransforDatetime" HeaderText="Trans" DataFormatString="{0:MM-dd HH:mm:ss}"/>
                    <telerik:GridBoundColumn DataField="SystemDate" HeaderText="SystemDate" DataFormatString="{0:MM-dd HH:mm:ss.fff}"/>
                </Columns>
            </MasterTableView>
            <ClientSettings AllowColumnsReorder="true" AllowColumnHide="true" AllowDragToGroup="true">
                <Selecting AllowRowSelect="true" />
                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
            </ClientSettings>
        </telerik:RadGrid>
    </telerik:RadAjaxPanel>
</asp:Content>

