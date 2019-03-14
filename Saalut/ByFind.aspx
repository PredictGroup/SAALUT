<%@ Page Title="Печать ценников по списку артикулов" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ByFind.aspx.cs" Inherits="Saalut.ByFind" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <hgroup class="title">
        <h1>
            <%: Page.Title %>.</h1>
    </hgroup>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <article>
                <p>
                    Указать артикулы или штрихкоды (каждый в отдельной строке):<br />
                    <asp:TextBox ID="ForFindTextBox1" runat="server" Rows="7" TextMode="MultiLine"></asp:TextBox>
                    <asp:Button ID="FindButton1" runat="server" Text="Поиск" OnClick="FindButton1_Click" />
                </p>
                <p>
                    <asp:GridView ID="GoodsGridView1" runat="server" AllowPaging="True" AllowSorting="True"
                        AutoGenerateColumns="False" DataKeyNames="ID" PageSize="25" Width="95%" OnRowDataBound="GoodsGridView1_RowDataBound"
                        OnPageIndexChanging="GoodsGridView1_PageIndexChanging">
                        <Columns>
                            <asp:CheckBoxField DataField="Active" HeaderText="Удал." SortExpression="Active" Visible="false" />
                            <asp:TemplateField HeaderText="ID" InsertVisible="False" SortExpression="ID" Visible="False">
                                <ItemTemplate>
                                    <asp:Label ID="GoodIDLabel1" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Articul" HeaderText="Артикул" SortExpression="Articul" />
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" Visible="False" />
                            <%--<asp:BoundField DataField="Descr" HeaderText="Описание" SortExpression="Descr">
                                <ItemStyle Font-Size="X-Small" />
                            </asp:BoundField>--%>
                            <asp:TemplateField HeaderText="Описание" SortExpression="Descr">
                                <ItemTemplate>
                                    <asp:TextBox ID="GoodNameTextBox1" Width="170" runat="server" OnTextChanged="GoodNameTextBox1_TextChanged" AutoPostBack="true" Rows="2" TextMode="MultiLine" ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Страна" SortExpression="GoodID">
                                <ItemTemplate>
                                    <asp:TextBox ID="ProizvoditelTextBox1" Width="110" runat="server" OnTextChanged="ProizvoditelTextBox1_TextChanged" AutoPostBack="true"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Barcode" HeaderText="Штрих-код" SortExpression="Barcode" />
                            <asp:BoundField DataField="Edinic" HeaderText="Ед." SortExpression="Edinic" />
                            <asp:BoundField DataField="GroupID" HeaderText="GroupID" SortExpression="GroupID"
                                Visible="False" />
                            <asp:BoundField DataField="DepartmentID" HeaderText="DepartmentID" SortExpression="DepartmentID"
                                Visible="False" />
                            <asp:BoundField DataField="PrintTemplateID" HeaderText="PrintTemplateID" SortExpression="PrintTemplateID"
                                Visible="False" />
                            <asp:BoundField DataField="TimeStamp" HeaderText="Изменение" SortExpression="TimeStamp"
                                DataFormatString="{0:d}" />
                            <asp:TemplateField HeaderText="Цена">
                                <ItemTemplate>
                                    <asp:Label ID="GoodPriceLabel1" runat="server"></asp:Label>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Шаблон ценника">
                                <ItemTemplate>
                                    <asp:DropDownList ID="PrintTemplateDropDownList1" runat="server" DataTextField="TemplateName"
                                        DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="PrintTemplateDropDownList1_SelectedIndexChanged">
                                    </asp:DropDownList>
                                    <asp:Label ID="IDLineDDLLabel1" Visible="false" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </p>
            </article>
            <aside>
                <h3>Действия</h3>
                <p>
                    <asp:CheckBox ID="CheckBox1WhithoutPrice" Text="Не печатать без цены" runat="server" />
                </p>
                <p>
                    <asp:Button ID="CennicButton1" runat="server" Text="Ценники" OnClick="CennicButton1_Click" />
                </p>
                <br />
                <p>
                    Количество:
                    <asp:TextBox ID="QtyTermoPTextBox1" Text="1" Width="30" runat="server"></asp:TextBox>
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
                    <asp:Button ID="TermoCennicButton1" runat="server" Text="Термо ценник" OnClick="TermoCennicButton1_Click" />
                </p>
                <br />
                <p>
                    <asp:DropDownList ID="SomeCennicsDropDownList1" runat="server" DataSourceID="SomeCennicsLinqDataSource1"
                        DataTextField="TemplateName" DataValueField="ID">
                    </asp:DropDownList>
                    <asp:LinqDataSource ID="SomeCennicsLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                        EntityTypeName="" TableName="PrintTemplates" Where="Active == @Active">
                        <WhereParameters>
                            <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                        </WhereParameters>
                    </asp:LinqDataSource>
                    <asp:Button ID="SetAllDDLButton1" runat="server" Text="Установить" OnClick="SetAllDDLButton1_Click" />
                </p>
                <br />
                <h3>Инструкция</h3>
                <p>
                    Артикулы товаров указываются каждый в своей строке, скопируйте из SAP или из другого
                    места и нажмите поиск, далее выберите формат ценника или оставьте указанный и нажмите
                    кнопку печать.
                </p>
            </aside>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
