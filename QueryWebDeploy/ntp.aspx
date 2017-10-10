<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPageSingleMenu.Master" CodeBehind="ntp.aspx.cs" Inherits="TelerikWebApp3.ntp" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="styles/grid.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <telerik:RadPageLayout runat="server" ID="JumbotronLayout" CssClass="jumbotron" GridType="Fluid">
        <Rows>
            <telerik:LayoutRow>
                <Columns>
                    <telerik:LayoutColumn Span="3">
                        <telerik:RadLabel runat="server" Text="Gateway Mac" ID="lbMac" />
                        <telerik:RadTextBox runat="server" ID="txtMac" />
                     </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="3">
                        <telerik:RadLabel runat="server" Text="Start Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateStart" Width="240px"/>
                    </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="4">
                         <telerik:RadLabel runat="server" Text="End Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateEnd"   Width="240px"/>
                        <telerik:RadButton runat="server" ID="btnQuery" OnClick="btnQuery_Click" Text="Query" />
                        </telerik:LayoutColumn>

                </Columns>
            </telerik:LayoutRow>

        </Rows>
    </telerik:RadPageLayout>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <telerik:RadAjaxPanel ID="RadAjaxPanel1" runat="server" CssClass="grid_wrapper">
        <telerik:RadGrid  ID="RadGrid1" runat="server" PageSize="10" PagerStyle-PageButtonCount="5"
            OnNeedDataSource="RadGrid1_NeedDataSource"  
            AllowPaging="True" AllowSorting="false" ShowGroupPanel="true" RenderMode="Auto">
            <ExportSettings ExportOnlyData="true" IgnorePaging="true"></ExportSettings>
            <MasterTableView AutoGenerateColumns="False"
                AllowFilteringByColumn="true" TableLayout="Auto"
                DataKeyNames="ID" CommandItemDisplay="Top"
                InsertItemPageIndexAction="ShowItemOnFirstPage">
                <CommandItemSettings ShowExportToExcelButton="true" ShowExportToPdfButton="true" />
                <Columns >
                    <telerik:GridBoundColumn DataField="ID" HeaderText="ID" SortExpression="ID"  UniqueName="ID">
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                     <telerik:GridBoundColumn DataField="DeviceMac" HeaderText="Device Mac"  >
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                      <telerik:GridBoundColumn DataField="SerialNo" HeaderText="Seria lNo"  >
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                      <telerik:GridBoundColumn DataField="NTPStatus" HeaderText="NTP Status"  >
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                      <telerik:GridBoundColumn DataField="RequestDateTime" HeaderText="RequestDateTime"  >
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                     <telerik:GridBoundColumn DataField="SendDateTime" HeaderText="Response Date"  >
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>
                     <telerik:GridBoundColumn DataField="SystemDate" HeaderText="SystemDate" DataFormatString="{0:MM-dd HH:mm:ss.fff}">
                        <HeaderStyle Width="150px" />
                    </telerik:GridBoundColumn>



                </Columns>
            </MasterTableView>
            <ClientSettings AllowColumnsReorder="true" AllowColumnHide="true" AllowDragToGroup="true">
                <Selecting AllowRowSelect="true" />
                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
            </ClientSettings>
        </telerik:RadGrid>
    </telerik:RadAjaxPanel>
</asp:Content>
