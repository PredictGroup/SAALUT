<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TSDJourLines.aspx.cs" Inherits="Saalut.TSD.TSDJourLines" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table style="width: 100%;">
                <tr>
                    <td colspan="3">
                        <asp:Label ID="PriceLabel1" runat="server" Text="" Font-Size="Small"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>Ш/К:</td>
                    <td>
                        <asp:TextBox ID="BarcodeTextBox1" runat="server" AutoPostBack="True" OnTextChanged="BarcodeTextBox1_TextChanged"></asp:TextBox>
                    </td>
                    <td>
                        <asp:ImageButton ID="DeleteBCImageButton1" runat="server"
                            ImageUrl="~/Images/Close.png" OnClick="DeleteBCImageButton1_Click" />
                    </td>
                </tr>
                <tr>
                    <td colspan="3">
                        <asp:Button ID="AddToJournalButton1" runat="server" Text="+ в журн." OnClick="AddToJournalButton1_Click"
                            Enabled="false" />
                        <asp:Button ID="PrintRightNow" runat="server" Text="Печать" OnClick="PrintRightNow_Click"
                            Enabled="false" />
                    </td>
                </tr>
            </table>
            <br />
            <asp:HyperLink ID="BackHyperLink1" NavigateUrl="~/TSD/Default.aspx" runat="server" Font-Size="X-Small">Вернуться к списку журналов</asp:HyperLink>
            <h5>
                <asp:Label ID="JourNumLabel1" runat="server" Text=""></asp:Label></h5>
            <asp:GridView ID="JourLinesGridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
                DataKeyNames="ID" DataSourceID="JourLinesLinqDataSource1" Width="100%">
                <Columns>
                    <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"
                        SortExpression="ID" Visible="False" />
                    <asp:BoundField DataField="JournalID" HeaderText="JournalID" SortExpression="JournalID"
                        Visible="False" />
                    <asp:BoundField DataField="ItemID_UKM" HeaderText="Арт." SortExpression="ItemID_UKM">
                        <HeaderStyle Font-Size="XX-Small" />
                        <ItemStyle Font-Size="XX-Small" />
                    </asp:BoundField>
                    <asp:TemplateField HeaderText="Наименование" SortExpression="GoodID">
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Good.Name") %>'></asp:Label>
                        </ItemTemplate>
                        <HeaderStyle Font-Size="XX-Small" />
                        <ItemStyle Font-Size="XX-Small" Width="100px" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="NewPrice" HeaderText="Ц." SortExpression="NewPrice"
                        DataFormatString="{0:f2}">
                        <HeaderStyle Font-Size="XX-Small" />
                        <ItemStyle HorizontalAlign="Right" Font-Size="XX-Small" />
                    </asp:BoundField>
                    <asp:BoundField DataField="TimeStamp" HeaderText="TimeStamp" SortExpression="TimeStamp"
                        Visible="False" />
                    <asp:CheckBoxField DataField="Active" HeaderText="Active" SortExpression="Active"
                        Visible="False" />
                    <asp:CommandField DeleteText="" ShowDeleteButton="True" ButtonType="Image" DeleteImageUrl="~/Images/Close.png">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:CommandField>
                </Columns>
            </asp:GridView>
            <asp:LinqDataSource ID="JourLinesLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" OrderBy="TimeStamp desc" TableName="PriceChangeLine" Where="JournalID == @JournalID &amp;&amp; Active == @Active">
                <WhereParameters>
                    <asp:Parameter Name="JournalID" Type="Int32" />
                    <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                </WhereParameters>
            </asp:LinqDataSource>
        </div>
    </form>
</body>
</html>
