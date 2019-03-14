<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Saalut.TSD.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h5>Печать ценников с ТСД</h5>
            Принтер №<asp:DropDownList ID="ddlMobilePrinterNum" runat="server">
                <asp:ListItem Text="0" Value="0"></asp:ListItem>
                <asp:ListItem Text="1" Value="1"></asp:ListItem>
                <asp:ListItem Text="2" Value="2"></asp:ListItem>
                <asp:ListItem Text="3" Value="3"></asp:ListItem>
                <asp:ListItem Text="4" Value="4"></asp:ListItem>
                <asp:ListItem Text="5" Value="5"></asp:ListItem>
            </asp:DropDownList>
            <br />
            <asp:Button ID="CreateNewButton1" runat="server" Text="Создать новый журнал"
                OnClick="CreateNewButton1_Click" />
            <br />
            <asp:GridView ID="Gridview1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
                DataKeyNames="ID" DataSourceID="PriceJoursLinqDataSource1" Width="100%" OnRowDeleted="Gridview1_RowDeleted"
                OnRowDeleting="Gridview1_RowDeleting"
                OnRowDataBound="Gridview1_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:ImageButton ID="GoToLinesImageButton1" runat="server" ImageUrl="~/Images/Open.png" />
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="ID" HeaderText="N" InsertVisible="False" ReadOnly="True"
                        SortExpression="ID">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TimeStamp" HeaderText="Время"
                        SortExpression="TimeStamp" Visible="False">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:CheckBoxField DataField="InUse" HeaderText="Блок." SortExpression="InUse"
                        Visible="False">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:CheckBoxField>
                    <asp:CheckBoxField DataField="Active" HeaderText="Актив." SortExpression="Active"
                        Visible="False" />
                    <asp:CommandField DeleteText="" ShowDeleteButton="True" ButtonType="Image"
                        DeleteImageUrl="~/Images/Close.png">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:CommandField>
                </Columns>
            </asp:GridView>
            <asp:LinqDataSource ID="PriceJoursLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" OrderBy="TimeStamp desc" TableName="PriceChangeJours" Where="FromTerminal == @FromTerminal &amp;&amp; Active == @Active">
                <WhereParameters>
                    <asp:Parameter DefaultValue="True" Name="FromTerminal" Type="Boolean" />
                    <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                </WhereParameters>
            </asp:LinqDataSource>
        </div>
    </form>
</body>
</html>
