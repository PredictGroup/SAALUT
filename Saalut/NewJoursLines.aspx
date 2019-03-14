<%@ Page Title="Печать ценников по журналу изменения цен УКМ" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="NewJoursLines.aspx.cs" Inherits="Saalut.NewJoursLines" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <hgroup class="title">
        <h3>
            <%: Page.Title %>.</h3>
        <h4>
            <asp:Label ID="JournalNumLabel1" runat="server" Text=""></asp:Label></h4>
    </hgroup>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <article>
                <p>
                    <asp:GridView ID="NewJourLinesGridView1" runat="server" AllowPaging="True" AllowSorting="True"
                        AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="NewJourLineLinqDataSource1"
                        Width="100%" OnRowDataBound="NewJourLinesGridView1_RowDataBound" OnSelectedIndexChanged="NewJourLinesGridView1_SelectedIndexChanged"
                        PageSize="25" OnPageIndexChanging="NewJourLinesGridView1_PageIndexChanging">
                        <Columns>
                            <asp:CheckBoxField DataField="Active" HeaderText="A" SortExpression="Active" Visible="false" />
                            <asp:TemplateField HeaderText="ID" InsertVisible="False" SortExpression="ID" Visible="False">
                                <ItemTemplate>
                                    <asp:Label ID="IDLabel1" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ItemID_UKM" HeaderText="Артикул" SortExpression="ItemID_UKM" />
                            <asp:BoundField DataField="GoodID" HeaderText="Номенклатура" SortExpression="GoodID"
                                Visible="False" >
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="Номенклатура" SortExpression="GoodID">
                                <ItemTemplate>
                                    <%--<asp:Label ID="GoodNameLabel1" runat="server" Text="Материал"></asp:Label>--%>
                                    <asp:TextBox ID="GoodNameTextBox1" Width="170" runat="server" OnTextChanged="GoodNameTextBox1_TextChanged" AutoPostBack="true" Rows="2" TextMode="MultiLine" ></asp:TextBox>
                                </ItemTemplate>
                                <ItemStyle Font-Size="X-Small" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Страна" SortExpression="GoodID">
                                <ItemTemplate>
                                    <asp:TextBox ID="ProizvoditelTextBox1" Width="100" runat="server" OnTextChanged="ProizvoditelTextBox1_TextChanged" AutoPostBack="true" ></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CheckBoxField DataField="Akcionniy" HeaderText="Aкц" SortExpression="Akcionniy" />
                            <asp:BoundField DataField="NewPrice" HeaderText="Новая цена" SortExpression="NewPrice"
                                DataFormatString="{0:N2}">
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="TimeStamp" HeaderText="Время" SortExpression="TimeStamp" Visible="false">
                                <ItemStyle HorizontalAlign="Justify" />
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="Шаблон ценника">
                                <ItemTemplate>
                                    <asp:DropDownList ID="PrintTemplateDropDownList1" runat="server" DataTextField="TemplateName"
                                        DataValueField="ID" AutoPostBack="true" OnSelectedIndexChanged="PrintTemplateDropDownList1_SelectedIndexChanged">
                                    </asp:DropDownList>
                                    <asp:Label ID="IDLineDDLLabel1" Visible="false" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ShowHeader="False">
                                <ItemTemplate>
                                    <asp:LinkButton ID="SelectLinkButton1" runat="server" CausesValidation="False" CommandName="Select"
                                        Text="Включить"></asp:LinkButton>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:LinqDataSource ID="NewJourLineLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                        EntityTypeName="" TableName="PriceChangeLine" Where="JournalID == @JournalID">
                        <WhereParameters>
                            <asp:Parameter DefaultValue="0" Name="JournalID" Type="Int32" />
                        </WhereParameters>
                    </asp:LinqDataSource>
                </p>
            </article>
            <aside>
                <h3>Действия</h3>
                <p>
                    <asp:CheckBox ID="CheckBox1WhithoutPrice" Text="Не печатать без цены" runat="server" />
                </p>
                <p>
                    <asp:Button ID="SetAllButton1" runat="server" Text="Вкл/Искл все" OnClick="SetAllButton1_Click" />
                </p>
                <p>
                    <asp:Button ID="CennicButton1" runat="server" Text="Ценники" OnClick="CennicButton1_Click" />
                </p>
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
                <br />
                <p>
                    <asp:Button ID="WeightLoadButton1" runat="server" Text="Загрузить весы" OnClick="WeightLoadButton1_Click" />
                </p>
                <br />
                <br />


            </aside>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
