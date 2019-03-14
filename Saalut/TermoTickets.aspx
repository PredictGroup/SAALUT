<%@ Page Title="Печать термоэтикетки" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="TermoTickets.aspx.cs" Inherits="Saalut.TermoTickets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <hgroup class="title">
        <h1>
            <%: Page.Title %>.</h1>
    </hgroup>
    <article>
        <p>
            Указать артикул:<br />
            <asp:TextBox ID="ForFindTextBox1" runat="server"></asp:TextBox>
            <asp:Button ID="FindButton1" runat="server" Text="Поиск" />
        </p>
        <p>
            <asp:GridView ID="GoodsGridView1" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="GoodsLinqDataSource1"
                PageSize="25" Width="95%">
                <Columns>
                    <asp:CheckBoxField DataField="Active" HeaderText="Удал." SortExpression="Active" />
                    <asp:TemplateField HeaderText="ID" InsertVisible="False" SortExpression="ID" Visible="False">
                        <ItemTemplate>
                            <asp:Label ID="GoodIDLabel1" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Articul" HeaderText="Артикул" SortExpression="Articul" />
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" Visible="False" />
                    <asp:BoundField DataField="Descr" HeaderText="Описание" SortExpression="Descr" />
                    <asp:BoundField DataField="Barcode" HeaderText="Штрих-код" SortExpression="Barcode" />
                    <asp:BoundField DataField="Edinic" HeaderText="Ед." SortExpression="Edinic" />
                    <asp:BoundField DataField="GroupID" HeaderText="GroupID" SortExpression="GroupID"
                        Visible="False" />
                    <asp:BoundField DataField="DepartmentID" HeaderText="DepartmentID" SortExpression="DepartmentID"
                        Visible="False" />
                    <asp:BoundField DataField="PrintTemplateID" HeaderText="PrintTemplateID" SortExpression="PrintTemplateID"
                        Visible="False" />
                    <asp:BoundField DataField="TimeStamp" DataFormatString="{0:d}" HeaderText="Изменение"
                        SortExpression="TimeStamp" />
                </Columns>
            </asp:GridView>
            <asp:LinqDataSource ID="GoodsLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" TableName="Goods" Where="Articul == @Articul">
                <WhereParameters>
                    <asp:ControlParameter ControlID="ForFindTextBox1" Name="Articul" PropertyName="Text"
                        Type="String" />
                </WhereParameters>
            </asp:LinqDataSource>
        </p>
    </article>
    <aside>
        <h3>Действия</h3>
        <p>
            Дата, время:
            <asp:TextBox ID="DataVremTextBox1" runat="server" MaxLength="25" Width="100px"></asp:TextBox>
            <br />
            Кол.:<asp:TextBox ID="QtyTextBox1" runat="server" Text="1" MaxLength="5" Width="50px"></asp:TextBox>
            <asp:DropDownList ID="TypeDropDownList1" runat="server" DataSourceID="LabelsLinqDataSource1"
                DataTextField="Name" DataValueField="ID">
            </asp:DropDownList>
            <asp:LinqDataSource ID="LabelsLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" TableName="ThermoLabels" Where="Active == @Active">
                <WhereParameters>
                    <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                </WhereParameters>
            </asp:LinqDataSource>
            <br />
            <asp:DropDownList ID="TermPrinterDropDownList1" runat="server" DataSourceID="TermPrintersLinqDataSource1"
                DataTextField="Name" DataValueField="ID">
            </asp:DropDownList>
            <asp:LinqDataSource ID="TermPrintersLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" TableName="TermoPrinters" Where="Active == @Active">
                <WhereParameters>
                    <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                </WhereParameters>
            </asp:LinqDataSource>
            <br />
            <asp:Button ID="CennicButton1" runat="server" Text="Печать" OnClick="CennicButton1_Click" />
        </p>
        <br />
        <h3>Инструкция</h3>
        <p>
            Дата, время - текстовое поле для этикеток где используется дата и время, по умолчанию
            текущая дата и время.
        </p>
        <p>
            Этикетки можно напечатать только по 1 товару. Укажите артикул, выполните поиск и
            распечатайте этикетки. При выводе на печать в окне настроек печати укажите количество
            копий этикетки.
        </p>
    </aside>
</asp:Content>
