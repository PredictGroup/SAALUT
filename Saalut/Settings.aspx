<%@ Page Title="Настройки и технические функции" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="Saalut.Settings" %>

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
            <asp:DetailsView ID="StoreInfoDetailsView1" runat="server" Height="50px" Width="100%"
                AutoGenerateRows="False" DataKeyNames="ID" DataSourceID="StoreInfoLinqDataSource1">
                <Fields>
                    <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"
                        SortExpression="ID" Visible="False" />
                    <asp:TemplateField HeaderText="Название магазина" SortExpression="StoreName">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("StoreName") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("StoreName") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("StoreName") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Юр. лицо" SortExpression="Company">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Company") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Company") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("Company") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Факт. адрес" SortExpression="AddressFact">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("AddressFact") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("AddressFact") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("AddressFact") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Юр. адрес" SortExpression="AddressJur">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("AddressJur") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("AddressJur") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label4" runat="server" Text='<%# Bind("AddressJur") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="StoreID_UKM" HeaderText="Номер магазина в УКМ" ReadOnly="True"
                        SortExpression="StoreID_UKM" />
                    <asp:BoundField DataField="PriceType_ID_UKM" HeaderText="Тип прайс-листа УКМ" ReadOnly="True"
                        SortExpression="PriceType_ID_UKM" />
                    <asp:BoundField DataField="PriceList_ID_UKM" HeaderText="ИД прайс-листа УКМ" ReadOnly="True"
                        SortExpression="PriceList_ID_UKM" />
                    <asp:BoundField DataField="PriceListName_UKM" HeaderText="Текущий прайс-лист УКМ"
                        ReadOnly="True" SortExpression="PriceListName_UKM" />
                    <asp:BoundField DataField="TimeStamp" HeaderText="Последнее изменение" ReadOnly="True"
                        SortExpression="TimeStamp" />
                    <asp:CheckBoxField DataField="Active" HeaderText="Активно" ReadOnly="True" SortExpression="Active" />
                    <asp:CommandField CancelText="Отмена" EditText="Изменить" ShowEditButton="True" UpdateText="Обновить" />
                </Fields>
            </asp:DetailsView>
            <asp:LinqDataSource ID="StoreInfoLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EnableUpdate="True" EntityTypeName="" TableName="StoreInfos">
            </asp:LinqDataSource>
        </p>
    </article>
    <aside>
        <h3>Инструкция</h3>
        <p>
            Указанные настройки используются в ценниках. Для весов используется шаблон, в котором
            указываются данные текущего магазина.
        </p>
        <br />
        <h3>Настройка SAP ERP</h3>
        <p>
            Настройка соединения с SAP ERP 
            <a href="SettinsSAPERP.aspx">здесь</a>
        </p>
    </aside>
</asp:Content>
