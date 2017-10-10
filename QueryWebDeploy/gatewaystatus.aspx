<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPageSingleMenu.Master" CodeBehind="gatewaystatus.aspx.cs" Inherits="TelerikWebApp3.gatewaystatus" %>

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
                        <telerik:RadLabel runat="server" Text="GatewayStaus:&nbsp;&nbsp;Gateway Mac" ID="lbMac" />
                        <telerik:RadTextBox runat="server" ID="txtMac" />
                    </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="4">
                        <telerik:RadLabel runat="server" Text="Start Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateStart" Width="240px" />
                    </telerik:LayoutColumn>

                    <telerik:LayoutColumn Span="4">
                        <telerik:RadLabel runat="server" Text="End Date" />
                        <telerik:RadDateTimePicker runat="server" ID="dateEnd" Width="240px" />
                        <telerik:RadButton runat="server" ID="btnQuery" OnClick="btnQuery_Click" Text="Query" />
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
                <CommandItemSettings ShowAddNewRecordButton="false"  ShowExportToExcelButton="true" ShowExportToPdfButton="true" />
                <Columns>
                    <telerik:GridBoundColumn DataField="ID" HeaderText="ID" SortExpression="ID" UniqueName="ID">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="DeviceMac" HeaderText="Device Mac">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="SerialNo" HeaderText="Serial No">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="SnCalc" HeaderText="SN Diff"/>
                    <telerik:GridBoundColumn DataField="SystemDate" HeaderText="SystemDate" DataFormatString="{0:MM-dd HH:mm:ss.fff}">
                    </telerik:GridBoundColumn>

                    <telerik:GridBoundColumn DataField="GatewayTransDateTime" HeaderText="Trans Time" DataFormatString="{0:MM-dd HH:mm:ss}">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="GatewayVoltage" HeaderText="Volt">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="ACPower" HeaderText="AC">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="RamCount" HeaderText="Ram">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="RomCount" HeaderText="Rom">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="LastSuccessNumber" HeaderText="Trans Num">
                    </telerik:GridBoundColumn>
                    <telerik:GridBoundColumn DataField="GSMSignal" HeaderText="CSQ" />
                    <telerik:GridBoundColumn DataField="TransStrategy" HeaderText="Trans Strategy" />
                     <telerik:GridBoundColumn DataField="BindingNumber" HeaderText="Bind Num" />
                     <telerik:GridBoundColumn DataField="TransforNumber" HeaderText="Trans Num" />
                     <telerik:GridBoundColumn DataField="SimNumber" HeaderText="Sim Num" />
                     <telerik:GridBoundColumn DataField="LastStatus" HeaderText="Status" />
                     <telerik:GridBoundColumn DataField="DeviceType" HeaderText="Device Type" />
                    <telerik:GridBoundColumn DataField="SoftwareVersion" HeaderText="Soft Ver" />
                    <telerik:GridBoundColumn DataField="ClientID" HeaderText="Client ID" />
                    
                     





                </Columns>
            </MasterTableView>
            <ClientSettings AllowColumnsReorder="true" AllowColumnHide="true" AllowDragToGroup="true">
                <Selecting AllowRowSelect="true" />
                <Scrolling AllowScroll="false" UseStaticHeaders="true" />
            </ClientSettings>
        </telerik:RadGrid>
    </telerik:RadAjaxPanel>
</asp:Content>
